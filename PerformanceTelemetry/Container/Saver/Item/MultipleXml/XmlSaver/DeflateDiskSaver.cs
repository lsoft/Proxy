using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PerformanceTelemetry.Container.Saver.Item.MultipleXml.XmlSaver
{
    /// <summary>
    /// Сохранение XML в сжатом виде на диск
    /// </summary>
    public class DeflateDiskSaver : IXmlSaver
    {
        #region required interface

        public interface IDeflater
        {
            int Deflate(
                Stream destinationStream,
                byte[] data
                );
        }

        #endregion

        #region constants

        private const int MaxRecordCount = 10000;
        private const long MaxFileSize = 100000000L;
        private readonly TimeSpan _border;

        #endregion

        private readonly object _locker = new object();

        private readonly IDeflater _deflater;
        private readonly ITelemetryLogger _logger;
        private readonly string _folder;

        private string _dataFileMask
        {
            get
            {
                return
                    "perf.data";
            }
        }

        private string _keyFileMask
        {
            get
            {
                return
                    "perf.key";
            }
        }

        private string _pathMask
        {
            get
            {
                return
                    Path.Combine(
                        _folder,
                        _currentIndex.ToString("D12"));
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

        private long _currentIndex = 0L;

        public DeflateDiskSaver(
            IDeflater deflater,
            string folder,
            TimeSpan border,
            ITelemetryLogger logger
            )
        {
            if (deflater == null)
            {
                throw new ArgumentNullException("deflater");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            //folder allowed to be null

            if (border.TotalSeconds >= 0)
            {
                throw new ArgumentException("border must be less than zero. Recommended value is -1 month.");
            }

            _deflater = deflater;
            _logger = logger;

            _folder = string.IsNullOrEmpty(folder) ? "PerfLog" : folder;
            _border = border;

            //создаем папку, если не создано
            CreateIfNotExists();

            //удаляем старье
            DeleteOld();

            //читаем актуальные параметры
            ReadConfig();

            //следующая папка при каждом старте сейвера
            CheckForNextFolder();
        }

        #region Implementation of IXmlSaver

        public void Save(StringBuilder xmlString)
        {
            if (xmlString == null)
            {
                throw new ArgumentNullException("xmlString");
            }

            lock (_locker)
            {
                //жмем в бинарный вид
                var xmlBinary = Encoding.UTF8.GetBytes(xmlString.ToString());

                //сохраняем на диск данные

                //проверяем, надо ли менять папку
                CheckForNextFolder();

                int size;

                //сохраняем тело
                using (var dataFileStream = new FileStream(_dataPathMask, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    size = _deflater.Deflate(
                        dataFileStream,
                        xmlBinary);

                    dataFileStream.Flush();
                }

                //сохраняем размер в другой файл, чтобы быстро находить
                using (var keyFileStream = new FileStream(_keyPathMask, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    var bytes = BitConverter.GetBytes(size);

                    keyFileStream.Write(bytes, 0, bytes.Length);

                    keyFileStream.Flush();
                }
            }
        }

        #endregion

        #region private methods

        private void DeleteOld()
        {
            var border = DateTime.Now.Add(_border);

            var dirs = Directory.GetDirectories(_folder);

            foreach(var d in dirs)
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
                var count = new FileInfo(_keyPathMask).Length/sizeof(int);
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
                _currentIndex++;

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
                _currentIndex = 0;

                if (!Directory.Exists(_pathMask))
                {
                    Directory.CreateDirectory(_pathMask);
                }
            }
            else
            {
                _currentIndex = maxdir;
            }
        }

        #endregion

        public void Dispose()
        {
            //nothing to do
        }
    }
}
