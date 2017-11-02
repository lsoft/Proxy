using System;
using System.Diagnostics;
using System.Runtime;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using Ninject.Modules;
using PerformanceTelemetry;
using PerformanceTelemetry.Container;
using PerformanceTelemetry.Container.Saver;
using PerformanceTelemetry.Container.Saver.Item;
using PerformanceTelemetry.ErrorContext;
using PerformanceTelemetry.Payload;
using PerformanceTelemetry.Record;
using PerformanceTelemetry.ThreadIdProvider;
using PerformanceTelemetry.Timer;
using ProxyGenerator.Generator;
using ProxyGenerator.NInject;
using ProxyGenerator.Payload;
using ProxyGenerator.Tests.Ninject.Fakes;

namespace ProxyGenerator.Tests.Ninject
{
    [TestClass]
    public class InterfaceFixture
    {
        [TestMethod]
        public void TwoInterfacesTest0()
        {
            using (var kernel = new StandardKernel())
            {
                kernel.Load(new ProxyModule(new DebugLogger()));

                kernel
                    .Bind<I1, I0>()
                    .ToProxy<I1, I0, T10>((methodInfo) => true)
                    .InSingletonScope()
                    ;

                var t0 = kernel.Get<I0>();

                Assert.IsNotNull(t0);

                var t1 = kernel.Get<I1>();

                Assert.IsNotNull(t1);

                Assert.AreSame(t1, t0);

                var value0 = t0.I0GetValue();
                var value1 = t1.I1GetValue();

                Assert.AreEqual(value0, value1);
            }

        }

        [TestMethod]
        public void TwoInterfacesTest1()
        {
            using (var kernel = new StandardKernel())
            {
                kernel.Load(new ProxyModule(new DebugLogger()));

                kernel
                    .Bind<I0, I1>()
                    .ToProxy<I0, I1, T10>((methodInfo) => true)
                    .InSingletonScope()
                    ;

                var t0 = kernel.Get<I0>();

                Assert.IsNotNull(t0);

                var t1 = kernel.Get<I1>();

                Assert.IsNotNull(t1);

                Assert.AreSame(t1, t0);

                var value0 = t0.I0GetValue();
                var value1 = t1.I1GetValue();

                Assert.AreEqual(value0, value1);
            }

        }

        public interface I0
        {
            long I0GetValue();
        }

        public interface I1 : I0
        {
            long I1GetValue();
        }

        public class T10 : I1
        {
            private readonly long _value;

            public T10()
            {
                _value = DateTime.Now.Ticks;
            }

            public long I0GetValue()
            {
                return
                    _value;
            }

            public long I1GetValue()
            {
                return
                    _value;
            }
        }
    }


    public class ProxyModule : NinjectModule
    {
        private readonly ITelemetryLogger _logger;

        public ProxyModule(
            ITelemetryLogger logger
            )
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            _logger = logger;
        }

        public override void Load()
        {
            Bind<ITelemetryLogger>()
                .ToConstant(_logger)
                .InSingletonScope()
                ;

            Bind<IItemSaverFactory>()
                .To<StatSaverFactory>()
                .InSingletonScope()
                ;

            Bind<IPerformanceTimerFactory>()
                .To<PerformanceTimerFactory>()
                .InSingletonScope()
                ;

            Bind<IPerformanceRecordFactory>()
                .To<PerformanceRecordFactory>()
                .InSingletonScope()
                ;

            Bind<IErrorContextFactory>()
                .To<EmptyContextFactory>()
                .InSingletonScope()
                ;

            Bind<IThreadIdProvider>()
                .To<ThreadIdProvider>()
                .InSingletonScope()
                ;

            Bind<IPerformanceContainer>()
                .To<PerformanceContainer>()
                .InSingletonScope()
                ;

            Bind<IProxyPayloadFactory>()
                .To<PerformanceTelemetryPayloadFactory>()
                .InSingletonScope()
                ;

            Bind<IProxyTypeGenerator>()
                .To<ProxyTypeGenerator>()
                .InSingletonScope()
                ;

            Bind<IPerformanceSaver>()
                .To<EventBasedSaver>()
                .InSingletonScope()
                ;

            //Bind<StatRecordContainer>()
            //    .ToSelf()
            //    .InSingletonScope()
            //    ;
        }
    }

    public class DebugLogger : ITelemetryLogger
    {
        public DebugLogger()
        {
        }

        #region ITelemetryLogger

        public void LogMessage(Type sourceType, string message)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException("sourceType");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Debug.WriteLine(
                "{0}:{1}{2}{2}{2}",
                sourceType.FullName,
                message,
                Environment.NewLine
                );
        }

        public void LogHandledException(Type sourceType, string message, Exception excp)
        {
            Console.WriteLine(
                "{0}:{1}{4}{2}{4}{3} {4}{4}{4}",
                sourceType.FullName,
                message,
                excp.Message,
                excp.StackTrace,
                Environment.NewLine
                );
        }

        #endregion
    }

}
