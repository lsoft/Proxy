using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyGenerator.Constructor;
using ProxyGenerator.Generator;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
#if NET47
    [TestClass]
    public class ValueTupleFixture
    {
        [TestMethod]
        public void ReturnTupleTest()
        {
            var payloadFactory = new MockPayloadFactory();

            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IValueTupleMock, ValueTupleMock>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute)
                );

            Assert.IsNotNull(proxy);

            var (i, s) = proxy.Get();

            Assert.AreEqual(12, i);
            Assert.AreEqual("123", s);
        }

        [TestMethod]
        public void ReturnGetterTupleTest()
        {
            var payloadFactory = new MockPayloadFactory();

            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IValueTupleMock2, ValueTupleMock2>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute)
                );

            Assert.IsNotNull(proxy);

            var (i, s) = proxy.Get;

            Assert.AreEqual(12, i);
            Assert.AreEqual("123", s);
        }

        [TestMethod]
        public void IncomeTupleTest()
        {
            var payloadFactory = new MockPayloadFactory();

            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IValueTupleMock3, ValueTupleMock3>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute)
                );

            Assert.IsNotNull(proxy);

            proxy.Set((12, "123"));

            Assert.AreEqual(12, proxy.I);
            Assert.AreEqual("123", proxy.S);
        }

        public interface IValueTupleMock3
        {
            int I
            {
                get;
            }

            string S
            {
                get;
            }
            
            [TestWrapWithProxy]
            void Set((int i, string s) tuple);
        }

        public class ValueTupleMock3 : IValueTupleMock3
        {
            public int I
            {
                get;
                private set;
            }

            public string S
            {
                get;
                private set;
            }

            public void Set((int i, string s) tuple)
            {
                I = tuple.i;
                S = tuple.s;
            }
        }

        public interface IValueTupleMock2
        {
            (int, string) Get
            {
                get;
            }
            
        }

        public class ValueTupleMock2 : IValueTupleMock2
        {
            public (int, string) Get
            {
                get
                {
                    return (12, "123");
                }
            }
        }

        public interface IValueTupleMock
        {
            [TestWrapWithProxy]
            (int, string) Get();
        }

        public class ValueTupleMock : IValueTupleMock
        {
            public (int, string) Get()
            {
                return (12, "123");
            }
        }

    }
#endif
}
