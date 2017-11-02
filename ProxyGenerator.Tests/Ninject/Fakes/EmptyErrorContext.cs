using PerformanceTelemetry.ErrorContext;

namespace ProxyGenerator.Tests.Ninject.Fakes
{
    internal class EmptyErrorContext : IErrorContext
    {
        public void Dispose()
        {
            //nothing to do
        }
    }
}