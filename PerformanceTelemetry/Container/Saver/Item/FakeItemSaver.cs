using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item
{
    internal class FakeItemSaver : IItemSaver
    {
        public static FakeItemSaver Instance = new FakeItemSaver();

        public void SaveItems(
            IPerformanceRecordData[] items,
            int itemCount
            )
        {
            //nothing to do
        }

        public void Commit()
        {
            //nothing to do
        }

        public void Dispose()
        {
            //nothing to do
        }
    }
}