namespace PerformanceTelemetry.ErrorContext
{
    internal class EmptyErrorContext : IErrorContext
    {
        public void Dispose()
        {
            //nothing to do
        }
    }
}