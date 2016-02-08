using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceTelemetry.Tests
{
    internal class EmptyLoggerAdapter : ITelemetryLogger
    {
        public void LogMessage(Type sourceType, string message)
        {
            Debug.WriteLine(
                "{0} {1} {2}{2}{2}",
                sourceType.FullName,
                message,
                Environment.NewLine
                );
        }

        public void LogHandledException(Type sourceType, string message, Exception excp)
        {
            Debug.WriteLine(
                "{0} {1} {2} {3}",
                sourceType.Name,
                message,
                excp.Message,
                excp.StackTrace
                );
        }
    }
}
