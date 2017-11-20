using System;

namespace ProxyNinjectDemonstration.ApplicationThings.Logger
{
    public class ConsoleLogger : IConsoleLogger
    {
        
        public void LogMessage(string message)
        {
            Console.WriteLine(
                "{0} {1}{1}{1}",
                message,
                Environment.NewLine
                );
        }
    }
}