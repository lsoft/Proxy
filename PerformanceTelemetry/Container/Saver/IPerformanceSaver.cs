using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver
{
    public interface IPerformanceSaver : IDisposable
    {
        void Save(IPerformanceRecordData record);
    }
}
