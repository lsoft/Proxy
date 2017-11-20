using PerformanceTelemetry.Container.Saver.Item;

namespace ProxyNinjectDemonstration.ProxyRelated.Saver
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