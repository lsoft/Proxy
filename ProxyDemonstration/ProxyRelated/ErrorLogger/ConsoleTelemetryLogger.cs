using System;
using PerformanceTelemetry;

namespace ProxyDemonstration.ProxyRelated.ErrorLogger
{
    public class ConsoleTelemetryLogger : ITelemetryLogger
    {
        public void LogMessage(Type sourceType, string message)
        {
            Console.WriteLine(
                "{0} {1} {2}{2}{2}",
                sourceType.FullName,
                message,
                Environment.NewLine
                );
        }

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