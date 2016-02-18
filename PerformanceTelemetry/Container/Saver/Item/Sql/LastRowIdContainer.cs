using System.Threading;

namespace PerformanceTelemetry.Container.Saver.Item.Sql
{
    public class LastRowIdContainer
    {
        //идентификатор последней существующей строки в СБД
        private long _lastRowId;

        public LastRowIdContainer(long lastRowId)
        {
            _lastRowId = lastRowId;
        }

        public long GetIdForNewRow()
        {
            return
                Interlocked.Increment(ref _lastRowId);
        }
    }
}