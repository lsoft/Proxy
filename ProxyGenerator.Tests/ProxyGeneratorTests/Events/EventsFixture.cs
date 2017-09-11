using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyGenerator.Constructor;
using ProxyGenerator.Generator;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.Events
{
    [TestClass]
    public class EventsFixture
    {
        [TestMethod]
        public void InterfaceEventTest1()
        {
            var payloadFactory = new MockPayloadFactory();

            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IVisibilityInterface1, VisibilityClass1>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute)
                );

            Assert.IsNotNull(proxy);

            var result = false;
            proxy.VisibilityEvent1 += () =>
            {
                result = true;
            };

            proxy.RaiseEvent();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void InterfaceEventTest2()
        {
            var payloadFactory = new MockPayloadFactory();

            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IVisibilityInterface2, VisibilityClass2>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute)
                );

            Assert.IsNotNull(proxy);

            const string Argument = "HHHHHH";

            string arg = null;
            proxy.VisibilityEvent2 += (s) =>
            {
                arg = s;
            };

            proxy.RaiseEvent(Argument);

            Assert.IsTrue(ReferenceEquals(arg, Argument));
        }

        [TestMethod]
        public void InterfaceEventTest3()
        {
            var payloadFactory = new MockPayloadFactory();

            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IVisibilityInterface3<string>, VisibilityClass3>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute)
                );

            Assert.IsNotNull(proxy);

            const string Argument = "HHHHHH";

            string arg = null;
            proxy.VisibilityEvent3 += (s) =>
            {
                arg = s;
            };

            proxy.RaiseEvent(Argument);

            Assert.IsTrue(ReferenceEquals(arg, Argument));
        }
    }

    #region test 3

    public delegate void VisibilityDelegate3<T>(T arg);

    public interface IVisibilityInterface3<T>
    {
        event VisibilityDelegate3<T> VisibilityEvent3;

        [TestWrapWithProxy]
        void RaiseEvent(T arg);
    }

    public class VisibilityClass3 : IVisibilityInterface3<string>
    {
        public event VisibilityDelegate3<string> VisibilityEvent3;

        public void RaiseEvent(string arg)
        {
            OnVisibilityEvent3(arg);
        }

        protected virtual void OnVisibilityEvent3(string arg)
        {
            VisibilityDelegate3<string> handler = VisibilityEvent3;
            if (handler != null)
            {
                handler(arg);
            }
        }
    }

    #endregion

    #region test 2

    public delegate void VisibilityDelegate2<T>(T arg);

    public interface IVisibilityInterface2
    {
        event VisibilityDelegate2<string> VisibilityEvent2;

        [TestWrapWithProxy]
        void RaiseEvent(string arg);
    }

    public class VisibilityClass2 : IVisibilityInterface2
    {
        public event VisibilityDelegate2<string> VisibilityEvent2;

        public void RaiseEvent(string arg)
        {
            OnVisibilityEvent2(arg);
        }

        protected virtual void OnVisibilityEvent2(string arg)
        {
            VisibilityDelegate2<string> handler = VisibilityEvent2;
            if (handler != null)
            {
                handler(arg);
            }
        }
    }

    #endregion

    #region test 1

    public delegate void VisibilityDelegate1();

    public interface IVisibilityInterface1
    {
        event VisibilityDelegate1 VisibilityEvent1;

        [TestWrapWithProxy]
        void RaiseEvent();
    }

    public class VisibilityClass1 : IVisibilityInterface1
    {
        public event VisibilityDelegate1 VisibilityEvent1;

        public void RaiseEvent()
        {
            OnVisibilityEvent1();
        }

        protected virtual void OnVisibilityEvent1()
        {
            VisibilityDelegate1 handler = VisibilityEvent1;
            if (handler != null)
            {
                handler();
            }
        }
    }

    //public class VisibilityClass2 : IVisibilityInterface1
    //{
    //    private readonly IVisibilityInterface1 _visibility;

    //    public VisibilityClass2(
    //        IVisibilityInterface1 visibility
    //        )
    //    {
    //        if (visibility == null)
    //        {
    //            throw new ArgumentNullException("visibility");
    //        }
    //        _visibility = visibility;
    //    }

    //    public event VisibilityDelegate1 VisibilityEvent1
    //    {
    //        add
    //        {
    //            _visibility.VisibilityEvent1 += value;
    //        }

    //        remove
    //        {
    //            _visibility.VisibilityEvent1 -= value;
    //        }
    //    }

    //    public void RaiseEvent()
    //    {
    //        _visibility.RaiseEvent();
    //    }
    //}

    #endregion
}
