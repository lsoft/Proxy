using System;

namespace PerformanceTelemetry.Container.Saver.Item.PlainText
{
    public class PlainTextItemSaverFactory : IItemSaverFactory
    {
        private readonly object _locker = new object();
        private readonly Action<string, object[]> _output;

        public PlainTextItemSaverFactory(
            Action<string, object[]> output
            )
        {
            _output = output ?? Console.WriteLine;
        }


        public IItemSaver CreateItemSaver()
        {
            return 
                new PlainTextItemSaver(
                    _locker,
                    _output
                    );
        }
    }
}