using System;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item
{
    public interface IItemSaver : IDisposable
    {
        void SaveItem(IPerformanceRecordData item);

        void Commit();
    }
}