using System;
using PerformanceTelemetry.Container;
using PerformanceTelemetry.ErrorContext;
using PerformanceTelemetry.ThreadIdProvider;
using ProxyGenerator;
using ProxyGenerator.PL;

namespace PerformanceTelemetry.Payload
{
    /// <summary>
    /// Фабрика для событий захвата телеметрии
    /// </summary>
    public class PerformanceTelemetryPayloadFactory : IProxyPayloadFactory
    {
        private readonly IErrorContextFactory _errorContextFactory;
        private readonly ITelemetryLogger _logger;
        private readonly IThreadIdProvider _threadIdProvider;
        private readonly IPerformanceContainer _container;

        public PerformanceTelemetryPayloadFactory(
            IErrorContextFactory errorContextFactory,
            ITelemetryLogger logger,
            IThreadIdProvider threadIdProvider,
            IPerformanceContainer container)
        {
            if (errorContextFactory == null)
            {
                throw new ArgumentNullException("errorContextFactory");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            if (threadIdProvider == null)
            {
                throw new ArgumentNullException("threadIdProvider");
            }
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            _errorContextFactory = errorContextFactory;
            _logger = logger;
            _threadIdProvider = threadIdProvider;
            _container = container;
        }

        #region Implementation of IProxyPayloadFactory

        public IProxyPayload GetProxyPayload(string className, string methodName)
        {
            return
                new PerformanceTelemetryPayload(
                    _errorContextFactory.OpenContext(),
                    _logger,
                    _threadIdProvider,
                    _container,
                    className,
                    methodName);
        }

        #endregion
    }
}
