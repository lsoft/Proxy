using System;
using System.Collections.Generic;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item
{
    public interface IItemSaver : IDisposable
    {
        void SaveItems(
            IPerformanceRecordData[] items,
            int itemCount
            );

        void Commit();
    }
}