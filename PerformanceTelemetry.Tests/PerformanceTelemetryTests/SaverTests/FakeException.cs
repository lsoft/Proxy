using System;

namespace PerformanceTelemetry.Tests.PerformanceTelemetryTests.SaverTests
{
    internal class FakeException : Exception
    {
        public FakeException(string message) : base(message)
        {
        }
    }

}
