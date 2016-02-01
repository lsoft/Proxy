using System.Threading;

namespace PerformanceTelemetry.ThreadIdProvider
{
    /// <summary>
    /// Поставщик manage thread id
    /// </summary>
    public class ThreadIdProvider : IThreadIdProvider
    {
        #region Implementation of IThreadIdProvider

        public int GetCurrentThreadId()
        {
            return
                Thread.CurrentThread.ManagedThreadId;
        }

        #endregion
    }
}
