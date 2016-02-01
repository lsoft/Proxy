namespace PerformanceTelemetry.ThreadIdProvider
{
    /// <summary>
    /// Поставщик идентификатора текущего трида
    /// (особенно полезен для юнит тестирования)
    /// </summary>
    public interface IThreadIdProvider
    {
        /// <summary>
        /// Вернуть идентификатор текущего трида
        /// </summary>
        int GetCurrentThreadId();
    }
}
