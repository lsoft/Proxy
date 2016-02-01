using System;

namespace PerformanceTelemetry.Tests.PerformanceTelemetryTests.SaverTests
{
    internal static class StringGenerator
    {
        public static string GetString(string suffix)
        {
            if (suffix == null)
            {
                throw new ArgumentNullException("suffix");
            }

            return
                string.Format(
                    "{0}:{1}",
                    Guid.NewGuid(),
                    suffix
                    );
        }
    }
}