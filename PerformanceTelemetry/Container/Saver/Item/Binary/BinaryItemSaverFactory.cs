using System;

namespace PerformanceTelemetry.Container.Saver.Item.Binary
{
    public class BinaryItemSaverFactory : IItemSaverFactory
    {
        private const string FolderDefaultName = "PerfLogs";
        private const string DataFileDefaultName = "perf.data";
        private const string KeyFileDefaultName = "perf.key";

        private readonly string _folderPath;
        private readonly string _dataFileMask;
        private readonly string _keyFileMask;

        private readonly TimeSpan _border;
        private readonly ITelemetryLogger _logger;

        public BinaryItemSaverFactory(
            TimeSpan border,
            ITelemetryLogger logger
            )
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (border.TotalSeconds >= 0)
            {
                throw new ArgumentException("border must be less than zero. Recommended value is -7 days.");
            }

            _folderPath = FolderDefaultName;
            _dataFileMask = DataFileDefaultName;
            _keyFileMask = KeyFileDefaultName;

            _border = border;
            _logger = logger;
        }

        public BinaryItemSaverFactory(
            string dataFileMask,
            string keyFileMask,
            TimeSpan border,
            ITelemetryLogger logger
            )
        {
            if (dataFileMask == null)
            {
                throw new ArgumentNullException(nameof(dataFileMask));
            }

            if (keyFileMask == null)
            {
                throw new ArgumentNullException(nameof(keyFileMask));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (border.TotalSeconds >= 0)
            {
                throw new ArgumentException("border must be less than zero. Recommended value is -7 days.");
            }

            _folderPath = FolderDefaultName;
            _dataFileMask = dataFileMask;
            _keyFileMask = keyFileMask;

            _border = border;
            _logger = logger;
        }

        public BinaryItemSaverFactory(
            string folderPath,
            string dataFileMask,
            string keyFileMask,
            TimeSpan border,
            ITelemetryLogger logger
            )
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException(nameof(folderPath));
            }

            if (dataFileMask == null)
            {
                throw new ArgumentNullException(nameof(dataFileMask));
            }

            if (keyFileMask == null)
            {
                throw new ArgumentNullException(nameof(keyFileMask));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (border.TotalSeconds >= 0)
            {
                throw new ArgumentException("border must be less than zero. Recommended value is -7 days.");
            }

            _folderPath = folderPath;
            _dataFileMask = dataFileMask;
            _keyFileMask = keyFileMask;

            _border = border;
            _logger = logger;
        }

        public IItemSaver CreateItemSaver()
        {
            return
                new BinaryItemSaver(
                    _folderPath,
                    _dataFileMask,
                    _keyFileMask,
                    _border,
                    _logger
                    );
        }
    }

}
