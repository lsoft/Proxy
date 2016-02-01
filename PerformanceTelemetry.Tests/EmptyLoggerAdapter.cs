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
        public void LogHandledException(Type sourceType, string message, Exception excp)
        {
            Debug.WriteLine(
                string.Format(
                    "{0} {1} {2} {3}",
                    sourceType.Name,
                    message,
                    excp.Message,
                    excp.StackTrace));
        }
    }
}
