using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.Generic.TargetClassInterface
{
    /// <summary>
    /// Summary description for UnitTest19
    /// </summary>
    [TestClass]
    public class TargetClassAndInterfaceGenericHardcoreFixture
    {
        public TargetClassAndInterfaceGenericHardcoreFixture()
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
        public void TestGeneric0()
        {
            Execute_TestGeneric0<DateTime, object>(DateTime.Now, new object());
        }

        [TestMethod]
        public void TestGeneric1()
        {
            Execute_TestGeneric0<DateTime, object>(DateTime.Now, new object());
            Execute_TestGeneric0<DateTime, object>(DateTime.Now, new object());
        }

        [TestMethod]
        public void TestGeneric2()
        {
            Execute_TestGeneric0<DateTime, object>(DateTime.Now, new object());
            Execute_TestGeneric0<MyStruct19, Array>(new MyStruct19(), new int[1]);
        }

        private void Execute_TestGeneric0<T, U>(T t, U u)
            where T : struct
            where U : class
        {
            var setExceptionFlag = false;
            var dispose = false;

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

            var proxy = constructor.CreateProxy<IClassMock19<T, U>, ClassMock19<T, U>>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                t,
                u);

            Assert.IsNotNull(proxy);

            var result = proxy.X2(null);

            Assert.IsFalse(setExceptionFlag);
            Assert.IsTrue(dispose);
        }

        public struct MyStruct19
        {
            
        }

        public interface IClassMock19<T, U>
            where T : struct
            where U : class
        {
            [TestWrapWithProxy]
            Dictionary<string, int> X2(Dictionary<string, List<int>> x);
        }

        public class ClassMock19<T, U> : IClassMock19<T, U>
            where T : struct 
            where U : class
        {
            private T _t;
            private readonly U _u;

            public ClassMock19(T t, U u)
            {
                if (u == null)
                {
                    throw new ArgumentNullException("u");
                }

                _t = t;
                _u = u;
            }

            #region Implementation of IClassMock19

            public Dictionary<string, int> X2(Dictionary<string, List<int>> x)
            {
                return null;
            }

            #endregion
        }

    }

}
