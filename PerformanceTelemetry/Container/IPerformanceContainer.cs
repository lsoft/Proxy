using System;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container
{
    /// <summary>
    /// Контейнер активных рекордов
    /// </summary>
    public interface IPerformanceContainer : IDisposable
    {
        IPerformanceRecord OpenPerformanceSession(
            int managedThreadId,
            string className,
            string methodName
            );
        
        void ClosePerformanceSession(int threadId, IPerformanceRecord closingRecord);
    }
}