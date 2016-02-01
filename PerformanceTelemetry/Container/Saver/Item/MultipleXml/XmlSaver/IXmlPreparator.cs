using System.Text;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item.MultipleXml.XmlSaver
{
    /// <summary>
    /// Интерфейс модификации IPerformanceRecordData в строковый XML
    /// </summary>
    public interface IXmlPreparator
    {
        StringBuilder PrepareXml(IPerformanceRecordData item);
    }
}