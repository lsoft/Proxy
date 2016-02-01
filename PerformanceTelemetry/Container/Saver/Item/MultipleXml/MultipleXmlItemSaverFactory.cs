using System;
using System.Linq;
using PerformanceTelemetry.Container.Saver.Item.MultipleXml.XmlSaver;

namespace PerformanceTelemetry.Container.Saver.Item.MultipleXml
{
    public class MultipleXmlItemSaverFactory : IItemSaverFactory, IDisposable
    {
        private readonly ITelemetryLogger _logger;
        private readonly IXmlPreparator _xmlPreparator;
        private readonly IXmlSaver[] _savers;

        private bool _disposed = false;

        public MultipleXmlItemSaverFactory(
            ITelemetryLogger logger,
            IXmlPreparator xmlPreparator,
            params IXmlSaver[] savers
            )
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            if (xmlPreparator == null)
            {
                throw new ArgumentNullException("xmlPreparator");
            }
            if (savers == null)
            {
                throw new ArgumentNullException("savers");
            }
            if (savers.Length == 0)
            {
                throw new ArgumentException("savers");
            }
            if (savers.Any(j => j == null))
            {
                throw new ArgumentException("savers.Any(j => j == null)");
            }

            _logger = logger;
            _xmlPreparator = xmlPreparator;
            _savers = savers;
        }

        public IItemSaver CreateItemSaver()
        {
            return 
                new MultipleXmlItemSaver(
                    _logger,
                    _xmlPreparator,
                    _savers
                    );
        }

        public void Dispose()
        {
            if (_disposed)
            {
                foreach (var s in this._savers)
                {
                    try
                    {
                        s.Dispose();
                    }
                    catch (Exception excp)
                    {
                        _logger.LogHandledException(
                            this.GetType(),
                            string.Format(
                                "Ошибка диспоуза сейвера {0}",
                                s.GetType().Name),
                            excp);
                    }
                }

                _disposed = true;
            }
        }
    }
}