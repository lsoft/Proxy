using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerformanceTelemetry.Container.Saver.Item.MultipleXml.XmlSaver;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item.MultipleXml
{
    public class MultipleXmlItemSaver : IItemSaver
    {
        private readonly ITelemetryLogger _logger;
        private readonly IXmlPreparator _xmlPreparator;
        private readonly IXmlSaver[] _savers;

        public MultipleXmlItemSaver(
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

        public void SaveItems(
            IPerformanceRecordData[] items,
            int itemCount
            )
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            for (var cc = 0; cc < itemCount; cc++)
            {
                var item = items[cc];

                SaveItem(item);
            }
        }

        public void Commit()
        {
            //nothing to do
        }

        public void Dispose()
        {
            //nothing to do
        }


        private void SaveItem(
            IPerformanceRecordData item
            )
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            //готовим xml
            var xmlString = _xmlPreparator.PrepareXml(item);

            //сохраняем в сейверы
            foreach (var saver in _savers)
            {
                try
                {
                    saver.Save(xmlString);
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        string.Format(
                            "Ошибка работы сейвера {0}; его пропускаем",
                            saver.GetType().Name),
                        excp);
                }
            }
        }

    }
}
