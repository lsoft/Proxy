using System;
using PerformanceTelemetry;

namespace ProxyNinjectDemostration.ProxyRelated.ErrorLogger
{
    public class ConsoleTelemetryLogger : ITelemetryLogger
    {
        public void LogHandledException(Type sourceType, string message, Exception excp)
        {
            Console.WriteLine(
                "{0} {1} {2} {3}{3}{3}",
                sourceType.FullName,
                message,
                excp.Message,
                Environment.NewLine
                );
        }

        
    }
}