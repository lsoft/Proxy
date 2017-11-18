namespace PerformanceTelemetry.ErrorContext
{
    public class EmptyErrorContextFactory : IErrorContextFactory
    {
        public IErrorContext OpenContext()
        {
            return
                new EmptyErrorContext();
        }
    }
}