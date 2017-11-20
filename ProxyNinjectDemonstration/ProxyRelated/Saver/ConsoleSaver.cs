using System;
using PerformanceTelemetry.Container.Saver.Item;
using PerformanceTelemetry.Record;

namespace ProxyNinjectDemonstration.ProxyRelated.Saver
{
    public class ConsoleSaver : IItemSaver
    {
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

    }
}