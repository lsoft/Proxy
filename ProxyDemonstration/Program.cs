using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerformanceTelemetry;
using PerformanceTelemetry.Container;
using PerformanceTelemetry.Container.Saver;
using PerformanceTelemetry.Container.Saver.Item;
using PerformanceTelemetry.Container.Saver.Item.MultipleXml;
using PerformanceTelemetry.ErrorContext;
using PerformanceTelemetry.Payload;
using PerformanceTelemetry.Record;
using PerformanceTelemetry.ThreadIdProvider;
using PerformanceTelemetry.Timer;
using ProxyDemonstration.ApplicationThings.Class1;
using ProxyDemonstration.ProxyRelated.ErrorLogger;
using ProxyDemonstration.ProxyRelated.Saver;
using ProxyGenerator.Constructor;
using ProxyGenerator.Generator;
using ProxyGenerator.Payload;

namespace ProxyDemonstration
{
    class Program
    {
        static void Main(string[] args)
        {
            ITelemetryLogger logger = new ConsoleTelemetryLogger(
                );

            IErrorContextFactory errorContextFactory = new EmptyErrorContextFactory(
                );
            
            IThreadIdProvider threadIdProvider = new ThreadIdProvider(
                );
            
            IPerformanceTimerFactory performanceTimerFactory = new PerformanceTimerFactory(
                );
            
            IPerformanceRecordFactory performanceRecordFactory = new PerformanceRecordFactory(
                performanceTimerFactory
                );
            
            IItemSaverFactory itemSaverFactory = new ConsoleSaverFactory(
                );

            using (IPerformanceSaver performanceSaver = new EventBasedSaver(
                itemSaverFactory,
                logger
                ))
            {
                using (IPerformanceContainer container = new PerformanceContainer(
                    logger,
                    performanceRecordFactory,
                    performanceSaver
                    ))
                {
                    IProxyPayloadFactory payloadFactory = new PerformanceTelemetryPayloadFactory(
                        errorContextFactory,
                        logger,
                        threadIdProvider,
                        container
                        );

                    var generator = new ProxyTypeGenerator(
                        );

                    var constructor = new StandaloneProxyConstructor(
                        generator
                        );

                    // --------------======= SHOW ME THE MAGIC =======--------------

                    var proxiedObject = constructor.CreateProxy<IInterface1ThatNeedToBeProxied, Class1ThatNeedToBeProxied>(
                        payloadFactory,
                        typeof (ProxyAttribute)
                        );

                    proxiedObject.SumWithWait500Msec(1, 2);

                    try
                    {
                        proxiedObject.GenerateExceptionAfter250Msec();
                    }
                    catch
                    {
                        //nothing to do in this mini demonstation
                    }

                    //not proxied methods:
                    proxiedObject.GetCurrentDateTime_NotProxied();

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadLine();
                }
            }
        }
    }
}
