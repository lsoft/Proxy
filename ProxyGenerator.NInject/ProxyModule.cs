using Ninject.Modules;
using PerformanceTelemetry.Container;
using PerformanceTelemetry.Container.Saver;
using PerformanceTelemetry.Payload;
using PerformanceTelemetry.Record;
using PerformanceTelemetry.ThreadIdProvider;
using PerformanceTelemetry.Timer;
using ProxyGenerator.Generator;
using ProxyGenerator.Payload;

namespace ProxyGenerator.NInject
{
    public class ProxyModule<TPerformanceSaver> : NinjectModule
        where TPerformanceSaver : IPerformanceSaver
    {
        public ProxyModule(
            )
        {
        }

        public override void Load()
        {
            PerformBinding();
        }

        protected virtual void PerformBinding()
        {
            Bind<IPerformanceTimerFactory>()
                .To<PerformanceTimerFactory>()
                .InSingletonScope()
                ;

            Bind<IPerformanceRecordFactory>()
                .To<PerformanceRecordFactory>()
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
                .To<TPerformanceSaver>()
                .InSingletonScope()
                ;
        }
    }
}