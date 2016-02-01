namespace PerformanceTelemetry.Container.Saver.Item
{
    public interface IItemSaverFactory
    {
        IItemSaver CreateItemSaver();
    }
}