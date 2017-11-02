using System;
using PerformanceTelemetry.Container.Saver.Item;
using PerformanceTelemetry.Record;

namespace ProxyGenerator.Tests.Ninject.Fakes
{
    public class StatSaver : IItemSaver
    {

        public StatSaver(
            )
        {
        }

        public void SaveItems(
            IPerformanceRecordData[] items,
            int itemCount
            )
        {
            //nothing to do
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