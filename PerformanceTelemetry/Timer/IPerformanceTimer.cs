using System;

namespace PerformanceTelemetry.Timer
{
    public interface IPerformanceTimer
    {
        Int64 Frequency
        {
            get;
        }

        double TimeInterval
        {
            get;
        }

        Int64 Value
        {
            get;
        }
    }
}