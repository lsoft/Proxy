using System;
using System.Text;

namespace PerformanceTelemetry.Container.Saver.Item.MultipleXml.XmlSaver
{
    /// <summary>
    /// Интерфейс сохранения XML куда-либо
    /// </summary>
    public interface IXmlSaver : IDisposable
    {
        void Save(StringBuilder xmlString);
    }
}