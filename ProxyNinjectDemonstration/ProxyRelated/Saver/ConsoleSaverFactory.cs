using PerformanceTelemetry.Container.Saver.Item;

namespace ProxyNinjectDemostration.ProxyRelated.Saver
{
    public class ConsoleSaverFactory : IItemSaverFactory
    {
        public IItemSaver CreateItemSaver()
        {
            return
                new ConsoleSaver();
        }
    }
}