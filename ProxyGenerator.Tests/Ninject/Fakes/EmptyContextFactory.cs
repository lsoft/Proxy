using PerformanceTelemetry.ErrorContext;

namespace ProxyGenerator.Tests.Ninject.Fakes
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