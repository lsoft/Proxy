using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyGenerator.Constructor;
using ProxyGenerator.Generator;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
#if VALUETUPLE
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

            (int x0, string x1, int x2, string x3, int x4, string x5, int x6, string x7, int x8, string x9, int x10) = proxy.Get();

            Assert.AreEqual(0, x0);
            Assert.AreEqual("1", x1);
            Assert.AreEqual(2, x2);
            Assert.AreEqual("3", x3);
            Assert.AreEqual(4, x4);
            Assert.AreEqual("5", x5);
            Assert.AreEqual(6, x6);
            Assert.AreEqual("7", x7);
            Assert.AreEqual(8, x8);
            Assert.AreEqual("9", x9);
            Assert.AreEqual(10, x10);
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
            (int x0, string x1, int x2, string x3, int x4, string x5, int x6, string x7, int x8, string x9, int x10) Get();
        }

        public class ValueTupleMock : IValueTupleMock
        {
            public (int x0, string x1, int x2, string x3, int x4, string x5, int x6, string x7, int x8, string x9, int x10) Get()
            {
                return (0, "1", 2, "3", 4, "5", 6, "7", 8, "9", 10);
            }
        }

    }
#endif
}
