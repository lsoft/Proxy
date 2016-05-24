using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.Generic.Parameters
{
    /// <summary>
    /// Summary description for GenericReturnFixture
    /// </summary>
    [TestClass]
    public class GenericReturnFixture
    {
        public GenericReturnFixture()
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

            var proxy = constructor.CreateProxy<IClassMock15, ClassMock15>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new  Func<Dictionary<string, int>>(() =>
                {
                    ok = true;
                    return 
                        new Dictionary<string, int>();
                }));

            Assert.IsNotNull(proxy);

            var result = proxy.X2();

            Assert.IsTrue(ok);
            Assert.IsFalse(setExceptionFlag);
            Assert.IsTrue(dispose);
        }

        public interface IClassMock15
        {
            [TestWrapWithProxy]
            Dictionary<string, int> X2();
        }

        public class ClassMock15 : IClassMock15
        {
            private Func<Dictionary<string, int>> _ok2;

            public ClassMock15(Func<Dictionary<string, int>> ok2)
            {
                _ok2 = ok2;
            }

            #region Implementation of IClassMock15

            public Dictionary<string, int> X2()
            {
                if (_ok2 != null)
                {
                    return 
                        _ok2();
                }

                return null;
            }

            #endregion
        }

    }

}
