using PerformanceTelemetry.ErrorContext;

namespace ProxyNinjectDemostration.ProxyRelated.ErrorContext
{
    internal class EmptyErrorContext : IErrorContext
    {
        public void Dispose()
        {
            //nothing to do
        }
    }
}