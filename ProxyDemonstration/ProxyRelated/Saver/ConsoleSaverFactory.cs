using PerformanceTelemetry.Container.Saver.Item;

namespace ProxyDemonstration.ProxyRelated.Saver
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