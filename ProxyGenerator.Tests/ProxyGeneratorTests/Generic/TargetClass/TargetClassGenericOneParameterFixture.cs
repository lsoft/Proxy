using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.Generic.TargetClass
{
    /// <summary>
    /// Summary description for UnitTest17
    /// </summary>
    [TestClass]
    public class TargetClassGenericOneParameterFixture
    {
        public TargetClassGenericOneParameterFixture()
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
            Execute_TestGeneric0<DateTime>(DateTime.Now);
        }

        [TestMethod]
        public void TestGeneric1()
        {
            Execute_TestGeneric0<DateTime>(DateTime.Now);
            Execute_TestGeneric0<DateTime>(DateTime.Now);
        }

        [TestMethod]
        public void TestGeneric2()
        {
            Execute_TestGeneric0<DateTime>(DateTime.Now);
            Execute_TestGeneric0<MyStruct17>(new MyStruct17());
        }

        private void Execute_TestGeneric0<T>(T t)
            where T : struct
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

            var proxy = constructor.CreateProxy<IClassMock17, ClassMock17<T>>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                t);

            Assert.IsNotNull(proxy);

            var result = proxy.X2(null);

            Assert.IsFalse(setExceptionFlag);
            Assert.IsTrue(dispose);
        }

        public struct MyStruct17
        {
            
        }

        public interface IClassMock17
        {
            [TestWrapWithProxy]
            Dictionary<string, int> X2(Dictionary<string, List<int>> x);
        }

        public class ClassMock17<T> : IClassMock17
            where T : struct 
        {
            private T _t;

            public ClassMock17(T t)
            {
                _t = t;
            }

            #region Implementation of IClassMock17

            public Dictionary<string, int> X2(Dictionary<string, List<int>> x)
            {
                return null;
            }

            #endregion
        }

    }

}
