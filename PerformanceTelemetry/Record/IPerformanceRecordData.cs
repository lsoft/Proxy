using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PerformanceTelemetry.Record
{
    /// <summary>
    /// Контейнер результатов работы PerformanceRecord
    /// </summary>
    public interface IPerformanceRecordData
    {
        string ClassName
        {
            get;
        }

        string MethodName
        {
            get;
        }

        DateTime StartTime
        {
            get;
        }

        double TimeInterval
        {
            get;
        }

        Exception Exception
        {
            get;
        }

        string CreationStack
        {
            get;
        }

        ReadOnlyCollection<IPerformanceRecordData> Children
        {
            get;
        }
    }
}