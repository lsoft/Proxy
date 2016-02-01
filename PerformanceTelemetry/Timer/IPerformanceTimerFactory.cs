using System;

namespace PerformanceTelemetry.Timer
{
    public interface IPerformanceTimerFactory
    {
        IPerformanceTimer CreatePerformanceTimer();

        DateTime GetCurrentTime();
    }
}