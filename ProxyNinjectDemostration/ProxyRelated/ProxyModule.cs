using System;
using Ninject;
using Ninject.Modules;
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
    public class ProxyModule : NinjectModule
    {
        public ProxyModule(
            )
        {
        }

        public override void Load()
        {
            Bind<ITelemetryLogger>()
                .To<ConsoleTelemetryLogger>()
                .InSingletonScope()
                ;

            Bind<IItemSaverFactory>()
                .To<ConsoleSaverFactory>()
                .InSingletonScope()
                ;

            Bind<IPerformanceTimerFactory>()
                .To<PerformanceTimerFactory>()
                .InSingletonScope()
                ;

            Bind<IPerformanceRecordFactory>()
                .To<PerformanceRecordFactory>()
                .InSingletonScope()
                ;

            Bind<IErrorContextFactory>()
                .To<EmptyContextFactory>()
                .InSingletonScope()
                ;

            Bind<IThreadIdProvider>()
                .To<ThreadIdProvider>()
                .InSingletonScope()
                ;

            Bind<IPerformanceContainer>()
                .To<PerformanceContainer>()
                .InSingletonScope()
                ;

            Bind<IProxyPayloadFactory>()
                .To<PerformanceTelemetryPayloadFactory>()
                .InSingletonScope()
                ;

            Bind<IProxyTypeGenerator>()
                .To<ProxyTypeGenerator>()
                .InSingletonScope()
                ;

            Bind<IPerformanceSaver>()
                .To<EventBasedSaver>()
                .InSingletonScope()
                ;
        }

    }
}