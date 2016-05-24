using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    /// <summary>
    /// Summary description for ConstructorArgumentsFixture
    /// </summary>
    [TestClass]
    public class ConstructorArgumentsFixture
    {
        public ConstructorArgumentsFixture()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestPayload0()
        {
            var setExceptionFlag = false;
            var dispose = false;
            var ok = false;

            var payloadFactory = new MockPayloadFactory(
                (excp) =>
                {
                    setExceptionFlag = true;
                },
                () =>
                {
                    dispose = true;
                });

            var generator = new ProxyGenerator.G.ProxyTypeGenerator();
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock12, ClassMock12>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok = true;
                }));

            Assert.IsNotNull(proxy);

            proxy.X1();

            Assert.IsTrue(ok);
            Assert.IsFalse(setExceptionFlag);
            Assert.IsTrue(dispose);
        }

        [TestMethod]
        public void TestPayload1()
        {
            var setExceptionFlag = false;
            var dispose = false;
            var ok = false;

            var payloadFactory = new MockPayloadFactory(
                (excp) =>
                {
                    setExceptionFlag = true;
                },
                () =>
                {
                    dispose = true;
                });

            var generator = new ProxyGenerator.G.ProxyTypeGenerator();
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock12, ClassMock12>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok = true;
                    throw new Exception("а мы сломались!");
                }));

            Assert.IsNotNull(proxy);

            try
            {
                proxy.X1();
            }
            catch
            {
            }

            Assert.IsTrue(ok);
            Assert.IsTrue(setExceptionFlag);
            Assert.IsTrue(dispose);
        }

        [TestMethod]
        public void TestPayload2()
        {
            var setExceptionFlag = false;
            var dispose = false;
            var ok = false;

            var payloadFactory = new MockPayloadFactory(
                (excp) =>
                {
                    setExceptionFlag = true;
                },
                () =>
                {
                    dispose = true;
                });

            var generator = new ProxyGenerator.G.ProxyTypeGenerator();
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock12, ClassMock12>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    throw new Exception("здесь мы ваще не должны были оказаться");
                }),
                new Action(() =>
                {
                    ok = true;
                }));

            Assert.IsNotNull(proxy);

            proxy.X2();

            Assert.IsTrue(ok);
            Assert.IsFalse(setExceptionFlag);
            Assert.IsFalse(dispose);
        }

        [TestMethod]
        public void TestPayload3()
        {
            var setExceptionFlag = false;
            var dispose = false;
            var ok = false;

            var payloadFactory = new MockPayloadFactory(
                (excp) =>
                {
                    setExceptionFlag = true;
                },
                () =>
                {
                    dispose = true;
                });

            var generator = new ProxyGenerator.G.ProxyTypeGenerator();
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock12, ClassMock12>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok = false;
                }),
                new Action(() =>
                {
                    ok = true;
                    throw new Exception("а мы сломались!");
                }));

            Assert.IsNotNull(proxy);

            try
            {
                proxy.X2();
            }
            catch
            {
            }

            Assert.IsTrue(ok);
            Assert.IsFalse(setExceptionFlag);
            Assert.IsFalse(dispose);
        }

        public interface IClassMock12
        {
            [TestWrapWithProxy]
            void X1();

            void X2();
        }

        public class ClassMock12 : IClassMock12
        {
            private Action _ok1;
            private Action _ok2;

            public ClassMock12(Action ok1)
            {
                _ok1 = ok1;
            }

            public ClassMock12(Action ok1, Action ok2)
            {
                _ok1 = ok1;
                _ok2 = ok2;
            }

            #region Implementation of IClassMock12

            public void X1()
            {
                _ok1();
            }

            public void X2()
            {
                if (_ok2 != null)
                {
                    _ok2();
                }
            }

            #endregion
        }

    }

}
