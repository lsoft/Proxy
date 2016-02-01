using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    /// <summary>
    /// Summary description for OutStructFixture
    /// </summary>
    [TestClass]
    public class OutStructFixture
    {
        public OutStructFixture()
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
        public void TestOut()
        {
            var ok = false;

            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            var proxy = constructor.CreateProxy<IClassMock4, ClassMock4>(
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok = true;
                }));

            Assert.IsNotNull(proxy);

            int a = 0;
            proxy.X(out a);

            Assert.IsTrue(ok);

        }

        public interface IClassMock4
        {
            [TestWrapWithProxy]
            void X(out int a);
        }

        public class ClassMock4 : IClassMock4
        {
            private Action _ok;

            public ClassMock4(Action ok)
            {
                _ok = ok;
            }

            #region Implementation of IClassMock3

            public void X(out int a)
            {
                a = 1;

                _ok();
            }

            #endregion
        }

    }

}
