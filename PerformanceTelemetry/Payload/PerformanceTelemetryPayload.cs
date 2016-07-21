using System;
using PerformanceTelemetry.Container;
using PerformanceTelemetry.ErrorContext;
using PerformanceTelemetry.Record;
using PerformanceTelemetry.ThreadIdProvider;
using ProxyGenerator;
using ProxyGenerator.Payload;

namespace PerformanceTelemetry.Payload
{
    /// <summary>
    /// Класс замера производительности
    /// </summary>
    public class PerformanceTelemetryPayload : IProxyPayload
    {
        private readonly ITelemetryLogger _logger;
        private readonly IPerformanceContainer _container;

        private readonly IErrorContext _errorContext;
        private readonly IPerformanceRecord _record;
        private readonly int _manageThreadId;

        public PerformanceTelemetryPayload(
            IErrorContext errorContext,
            ITelemetryLogger logger,
            IThreadIdProvider threadIdProvider,
            IPerformanceContainer container,
            string className,
            string methodName)
        {
            if (errorContext == null)
            {
                throw new ArgumentNullException("errorContext");
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
            if (className == null)
            {
                throw new ArgumentNullException("className");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }

            this._errorContext = errorContext;
            this._logger = logger;
            this._container = container;

            this._manageThreadId = threadIdProvider.GetCurrentThreadId();

            this._record = _container.OpenPerformanceSession(
                this._manageThreadId,
                className,
                methodName);
        }

        /// <summary>
        /// применяется для указания, что метод завершился генерацией исключения
        /// </summary>
        /// <param name="excp"></param>
        public void SetExceptionFlag(Exception excp)
        {
            _record.SetExceptionFlag(excp);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            #region close performance session

            try
            {
                this._container.ClosePerformanceSession(this._manageThreadId, _record);
            }
            catch (Exception excp)
            {
                _logger.LogHandledException(
                    this.GetType(),
                    "Ошибка диспоуза ClosePerformanceSession",
                    excp);

                //пофигу, если телеметрия сломается, хост-приложение должно работать
            }

            #endregion

            #region error scope

            try
            {
                _errorContext.Dispose();
            }
            catch (Exception excp)
            {
                _logger.LogHandledException(
                    this.GetType(),
                    "Ошибка диспоуза _errorContext",
                    excp);

                //пофигу, если телеметрия сломается, хост-приложение должно работать
            }

            #endregion
        }

        #endregion
    }
}
