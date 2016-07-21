using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyGenerator.Constructor;
using ProxyGenerator.Generator;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.Generic.TargetInterface
{
    /// <summary>
    /// Summary description for UnitTest20
    /// </summary>
    [TestClass]
    public class TargetInterfaceGenericFixture
    {
        public TargetInterfaceGenericFixture()
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
            Execute_TestGeneric0(new MyStruct20(), new int[1]);
        }

        private void Execute_TestGeneric0(MyStruct20 t, Array u)
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

            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock20<MyStruct20, Array>, ClassMock20>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                t,
                u);

            Assert.IsNotNull(proxy);

            var result = proxy.X2(new Dictionary<string, List<int>>());

            Assert.IsFalse(setExceptionFlag);
            Assert.IsTrue(dispose);
        }

        public struct MyStruct20
        {
            
        }

        public interface IClassMock20<T, U>
            where T : struct
            where U : class
        {
            [TestWrapWithProxy]
            Dictionary<string, int> X2(Dictionary<string, List<int>> x);
        }

        public class ClassMock20 : IClassMock20<MyStruct20, Array>
        {
            private MyStruct20 _t;
            private readonly Array _u;

            public ClassMock20(MyStruct20 t, Array u)
            {
                if (u == null)
                {
                    throw new ArgumentNullException("u");
                }

                _t = t;
                _u = u;
            }

            #region Implementation of IClassMock20

            public Dictionary<string, int> X2(Dictionary<string, List<int>> x)
            {
                return null;
            }

            #endregion
        }

    }

}
