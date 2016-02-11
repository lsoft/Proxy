using System;

namespace PerformanceTelemetry.Record
{
    public interface IPerformanceRecord : IPerformanceRecordData
    {
        bool Active
        {
            get;
        }

        IPerformanceRecord GetDeepestActiveRecord();

        IPerformanceRecord CreateChild(
            string className,
            string methodName
            );

        void Close();

        void SetExceptionFlag(Exception excp);

        void ChildDied();

        IPerformanceRecordData GetPerformanceData();
    }
}