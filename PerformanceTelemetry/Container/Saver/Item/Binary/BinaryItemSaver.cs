using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using PerformanceTelemetry.Record;
using System.Threading;

namespace PerformanceTelemetry.Container.Saver.Item.Binary
{
    /// <summary>
    /// Сохранение итемов в сжатом виде на диск
    /// </summary>
    public class BinaryItemSaver : IItemSaver
    {
        #region constants

        private const int MaxRecordCount = 10 * 1000;
        private const long MaxFileSize = 100L * 1024L * 1024L; 

        #endregion

        private readonly string _folder;
        private readonly string _dataFileMask;
        private readonly string _keyFileMask;
        private readonly TimeSpan _border;
        private readonly ITelemetryLogger _logger;

        private readonly FileStream _dataFileStream;
        private readonly long _dataFileDefaultSize;
        private readonly ExtendedBinaryWriter _dataBinaryWriter;
        private readonly FileStream _keyFileStream;
        private readonly long _keyFileDefaultSize;

        private string _pathMask
        {
            get
            {
                return
                    Path.Combine(
                        _folder,
                        _currentFolderIndex.ToString("D12")
                        );
            }
        }

        private string _dataPathMask
        {
            get
            {
                return
                    Path.Combine(_pathMask, _dataFileMask);
            }
        }

        private string _keyPathMask
        {
            get
            {
                return
                    Path.Combine(_pathMask, _keyFileMask);
            }
        }

        private long _currentFolderIndex = 0L;

        //количество ошибок при сохранении
        private int _errorCounter = 0;

        private long _cleanuped = 0L;

        public BinaryItemSaver(
            string folder,
            string dataFileMask,
            string keyFileMask,
            TimeSpan border,
            ITelemetryLogger logger
            )
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            if (dataFileMask == null)
            {
                throw new ArgumentNullException(nameof(dataFileMask));
            }

            if (keyFileMask == null)
            {
                throw new ArgumentNullException(nameof(keyFileMask));
            }

            if (border.TotalSeconds >= 0)
            {
                throw new ArgumentException("border must be less than zero. Recommended value is -7 days.");
            }

            _folder = folder;
            _dataFileMask = dataFileMask;
            _keyFileMask = keyFileMask;
            _border = border;
            _logger = logger;

            //создаем папку, если не создано
            CreateIfNotExists();

            //удаляем старье
            DeleteOld();

            //читаем актуальные параметры
            ReadConfig();

            //проверяем, надо ли менять папку
            CheckForNextFolder();

            _dataFileStream = new FileStream(_dataPathMask, FileMode.Append, FileAccess.Write, FileShare.Read);
            _dataFileDefaultSize = _dataFileStream.Position;
            _dataBinaryWriter = DiskPerformanceRecordSerializer.CreateWriter(_dataFileStream);

            _keyFileStream = new FileStream(_keyPathMask, FileMode.Append, FileAccess.Write, FileShare.Read);
            _keyFileDefaultSize = _keyFileStream.Position;
        }

        #region Implementation of IItemSaver

        public void SaveItems(
            IPerformanceRecordData[] items,
            int itemCount
            )
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var sizes = new List<long>(itemCount);

            #region сохраняем итемы

            for (var cc = 0; cc < itemCount; cc++)
            {
                var item = items[cc];

                if (item == null)
                {
                    throw new InvalidOperationException("item");
                }

                var size = DiskPerformanceRecordSerializer.WriteOne(
                    item,
                    _dataBinaryWriter
                    );

                sizes.Add(size);
            }

            _dataFileStream.Flush();

            #endregion

            #region сохраняем размер в другой файл, чтобы быстро находить

            foreach (var size in sizes)
            {
                var bytes = BitConverter.GetBytes(size);

                _keyFileStream.Write(bytes, 0, bytes.Length);
            }

            _keyFileStream.Flush();

            #endregion
        }

        public void Commit()
        {
            CloseAll(false);
        }

        public void Dispose()
        {
            CloseAll(true);
        }

        #endregion

        #region private methods

        private void DeleteOld()
        {
            var border = DateTime.Now.Add(_border);

            var dirs = Directory.GetDirectories(_folder);

            foreach (var d in dirs)
            {
                var di = new DirectoryInfo(d);

                if (di.CreationTime < border)
                {
                    try
                    {
                        Directory.Delete(d, true);
                    }
                    catch (Exception excp)
                    {
                        _logger.LogHandledException(
                            this.GetType(),
                            string.Format(
                                "Ошибка удаления папки {0}",
                                d),
                            excp);
                    }

                }
            }
        }

        private void CheckForNextFolder()
        {
            var needToChangeDir = false;

            //проверяем, что надо менять директорию

            //по количеству записей
            if (File.Exists(_keyPathMask))
            {
                var count = new FileInfo(_keyPathMask).Length / sizeof(long);
                if (count > MaxRecordCount)
                {
                    needToChangeDir = true;
                }
            }

            //по размеру файла
            if (File.Exists(_dataPathMask))
            {
                var size = new FileInfo(_dataPathMask).Length;
                if (size > MaxFileSize)
                {
                    needToChangeDir = true;
                }
            }

            if (needToChangeDir)
            {
                //удаляем старые директории
                DeleteOld();

                //меняем директорию
                _currentFolderIndex++;

                Directory.CreateDirectory(_pathMask);
            }
        }

        private void CreateIfNotExists()
        {
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }
        }

        private void ReadConfig()
        {
            //определяем текущий индекс
            var dirs = Directory.GetDirectories(_folder);

            var emptyList = new List<int> { -1 };

            int a;
            var maxdir =
                (from d in dirs
                 let di = new DirectoryInfo(d)
                 where int.TryParse(di.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out a)
                 let parsed = int.Parse(di.Name, NumberStyles.Integer, CultureInfo.InvariantCulture)
                 select parsed).Concat(emptyList).OrderByDescending(j => j).First<int>();

            if (maxdir == -1)
            {
                //папок нет, создаем
                _currentFolderIndex = 0;

                if (!Directory.Exists(_pathMask))
                {
                    Directory.CreateDirectory(_pathMask);
                }
            }
            else
            {
                _currentFolderIndex = maxdir;
            }
        }

        private void CloseAll(bool needToRevert)
        {
            if (Interlocked.Exchange(ref _cleanuped, 1L) != 0L)
            {
                return;
            }

            try
            {
                _dataBinaryWriter.Close();
            }
            catch(Exception excp)
            {
                LogException(excp);
            }

            try
            {
                if (needToRevert)
                {
                    _dataFileStream.SetLength(_dataFileDefaultSize);
                }

                _dataFileStream.Close();
            }
            catch (Exception excp)
            {
                LogException(excp);
            }

            try
            {
                if (needToRevert)
                {
                    _keyFileStream.SetLength(_keyFileDefaultSize);
                }

                _keyFileStream.Close();
            }
            catch (Exception excp)
            {
                LogException(excp);
            }
        }

        private void LogException(Exception excp)
        {
            _errorCounter++;

            if (_errorCounter < 100 || (_errorCounter % 100) == 0)
            {
                //может быть такая ситуация, что НИ ОДНО сообщение телеметрии не сможет быть закомичено в базу
                //например что-то с базой или таблицей
                //в этом случае телеметрия лог завалит сообщениями, в которых потеряется ВСЁ и лог станет нечитаемым
                //поэтому первые сто сообщений записываются в лог, а потом записываемся КАЖДОЕ СОТОЕ

                _logger.LogHandledException(this.GetType(), "При сохранении рекорда возникла ошибка", excp);
            }
        }

        #endregion

    }

}
