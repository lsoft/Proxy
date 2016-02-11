using System;
using PerformanceTelemetry.Container.Saver.Item;
using PerformanceTelemetry.Record;

namespace ProxyNinjectDemostration.ProxyRelated.Saver
{
    public class ConsoleSaver : IItemSaver
    {
        public void SaveItem(IPerformanceRecordData item)
        {
            var children = item.GetChildren();

            Console.WriteLine(
                "[{0} - {1}] {2}.{3} || {4} || Children count = {5}",
                item.StartTime.ToString("yyyyMMdd HH:mm:ss.fff"),
                item.StartTime.AddSeconds(item.TimeInterval).ToString("yyyyMMdd HH:mm:ss.fff"),
                item.ClassName,
                item.MethodName,
                item.Exception != null ? item.Exception.Message : "-= NO EXCEPTION =-",
                children.Count
                );
        }

        public void Commit()
        {
            //nothing to do
        }

        public void Dispose()
        {
            //nothing to do
        }
    }
}