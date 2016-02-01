using System;
using Ninject;
using PerformanceTelemetry;
using PerformanceTelemetry.Container;
using PerformanceTelemetry.Container.Saver;
using PerformanceTelemetry.Container.Saver.Item;
using PerformanceTelemetry.ErrorContext;
using PerformanceTelemetry.Payload;
using PerformanceTelemetry.Record;
using PerformanceTelemetry.ThreadIdProvider;
using PerformanceTelemetry.Timer;
using ProxyGenerator.G;
using ProxyGenerator.NInject;
using ProxyGenerator.PL;
using ProxyNinjectDemostration.ApplicationThings;
using ProxyNinjectDemostration.ApplicationThings.Logger;
using ProxyNinjectDemostration.ProxyRelated.ErrorContext;
using ProxyNinjectDemostration.ProxyRelated.ErrorLogger;
using ProxyNinjectDemostration.ProxyRelated.Saver;

namespace ProxyNinjectDemostration.ProxyRelated
{
    public class GeneratorProvider : IGeneratorProvider
    {
        private readonly IKernel _kernel;

        public GeneratorProvider(
            IKernel kernel
            )
        {
            if (kernel == null)
            {
                throw new ArgumentNullException("kernel");
            }

            _kernel = kernel;

            Bind();
        }

        private void Bind()
        {
            _kernel
                .Bind<ITelemetryLogger>()
                .To<ConsoleTelemetryLogger>()
                .InSingletonScope()
                ;

            _kernel
                .Bind<IItemSaverFactory>()
                .To<ConsoleSaverFactory>()
                .InSingletonScope()
                ;

            _kernel
                .Bind<IPerformanceTimerFactory>()
                .To<PerformanceTimerFactory>()
                .InSingletonScope()
                ;

            _kernel
                .Bind<IPerformanceRecordFactory>()
                .To<PerformanceRecordFactory>()
                .InSingletonScope()
                ;

            _kernel
                .Bind<IErrorContextFactory>()
                .To<EmptyContextFactory>()
                .InSingletonScope()
                ;

            _kernel
                .Bind<IThreadIdProvider>()
                .To<ThreadIdProvider>()
                .InSingletonScope()
                ;

            _kernel
                .Bind<IPerformanceContainer>()
                .To<PerformanceContainer>()
                .InSingletonScope()
                ;

            _kernel
                .Bind<IProxyPayloadFactory>()
                .To<PerformanceTelemetryPayloadFactory>()
                .InSingletonScope()
                ;

            _kernel
                .Bind<IProxyTypeGenerator>()
                .To<ProxyTypeGenerator>()
                .InSingletonScope()
                ;

            _kernel
                .Bind<IPerformanceSaver>()
                .To<EventBasedSaver>()
                .InSingletonScope()
                .WithConstructorArgument(
                    "logger",
                    c => new Action<string>((message) => c.Kernel.Get<IConsoleLogger>().LogMessage(message))
                )
                ;
        }

        public IProxyTypeGenerator ProvideGenerator()
        {
            return
                _kernel.Get<IProxyTypeGenerator>();
        }

    }
}