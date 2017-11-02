using System;
using PerformanceTelemetry.Container.Saver.Item;

namespace ProxyGenerator.Tests.Ninject.Fakes
{
    public class StatSaverFactory : IItemSaverFactory
    {

        public StatSaverFactory(
            )
        {
        }

        public IItemSaver CreateItemSaver()
        {
            return
                new StatSaver(
                    );
        }
    }
}