using PerformanceTelemetry;
using PerformanceTelemetry.Container.Saver;
using PerformanceTelemetry.Container.Saver.Item;
using PerformanceTelemetry.ErrorContext;
using ProxyGenerator.NInject;
using ProxyNinjectDemostration.ProxyRelated.ErrorLogger;
using ProxyNinjectDemostration.ProxyRelated.Saver;

namespace ProxyNinjectDemostration.ProxyRelated
{
    public class DemoProxyModule : ProxyModule<EventBasedSaver>
    {
        public DemoProxyModule(
            )
        {
        }

        protected override void PerformBinding()
        {
            base.PerformBinding();

            Bind<ITelemetryLogger>()
                .To<ConsoleTelemetryLogger>()
                .InSingletonScope()
                ;

            Bind<IItemSaverFactory>()
                .To<ConsoleSaverFactory>()
                .InSingletonScope()
                ;

            Bind<IErrorContextFactory>()
                .To<EmptyErrorContextFactory>()
                .InSingletonScope()
                ;
        }
    }
}