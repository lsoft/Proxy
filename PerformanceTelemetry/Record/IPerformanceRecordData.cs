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

        /// <summary>
        /// It constructs children collection by any request, so do not call this method extensively.
        /// </summary>
        /// <returns>Children collection</returns>
        List<IPerformanceRecordData> GetChildren();
    }
}