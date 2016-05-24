using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    /// <summary>
    /// Summary description for RefStructFixture
    /// </summary>
    [TestClass]
    public class RefStructFixture
    {
        public RefStructFixture()
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
        public void TestRef()
        {
            var ok = false;

            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyGenerator.G.ProxyTypeGenerator();
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock3, ClassMock3>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok = true;
                }));

            Assert.IsNotNull(proxy);

            int a = 0;
            proxy.X(ref a);

            Assert.IsTrue(ok);
        }

        public interface IClassMock3
        {
            [TestWrapWithProxy]
            void X(ref int a);
        }

        public class ClassMock3 : IClassMock3
        {
            private Action _ok;

            public ClassMock3(Action ok)
            {
                _ok = ok;
            }

            #region Implementation of IClassMock3

            public void X(ref int a)
            {
                _ok();
            }

            #endregion
        }

    }

}
