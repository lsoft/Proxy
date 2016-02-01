using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.Generic.Parameters
{
    /// <summary>
    /// Summary description for UnitTest21
    /// </summary>
    [TestClass]
    public class RefOutGenericHardcoreFixture
    {
        public RefOutGenericHardcoreFixture()
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

            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            var proxy = constructor.CreateProxy<IClassMock21, ClassMock21>(
                typeof(TestWrapWithProxyAttribute),
                new  Func<Dictionary<string, int>>(() =>
                {
                    ok = true;
                    return 
                        new Dictionary<string, int>();
                }));

            Assert.IsNotNull(proxy);

            Dictionary<string, List<int>> xl;
            var result0 = proxy.X2(out xl);
            var result1 = proxy.X1(ref xl);

            Assert.IsTrue(ok);
            Assert.IsFalse(setExceptionFlag);
            Assert.IsTrue(dispose);
        }

        public interface IClassMock21
        {
            [TestWrapWithProxy]
            Dictionary<string, int> X1(ref Dictionary<string, List<int>> x);

            [TestWrapWithProxy]
            Dictionary<string, int> X2(out Dictionary<string, List<int>> x);
        }

        public class ClassMock21 : IClassMock21
        {
            private Func<Dictionary<string, int>> _ok2;

            public ClassMock21(Func<Dictionary<string, int>> ok2)
            {
                _ok2 = ok2;
            }

            #region Implementation of IClassMock21

            public Dictionary<string, int> X1(ref Dictionary<string, List<int>> x)
            {
                _ok2();

                x = null;
                return null;
            }

            public Dictionary<string, int> X2(out Dictionary<string, List<int>> x)
            {
                _ok2();

                x = null;
                return null;
            }

            #endregion
        }

    }

}
