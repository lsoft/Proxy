using PerformanceTelemetry.ErrorContext;

namespace ProxyNinjectDemostration.ProxyRelated.ErrorContext
{
    internal class EmptyContextFactory : IErrorContextFactory
    {
        public IErrorContext OpenContext()
        {
            return
                new EmptyErrorContext();
        }
    }
}