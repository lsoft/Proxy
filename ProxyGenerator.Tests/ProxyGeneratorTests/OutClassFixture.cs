using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    /// <summary>
    /// Summary description for OutClassFixture
    /// </summary>
    [TestClass]
    public class OutClassFixture
    {
        public OutClassFixture()
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
        public void TestOutClass()
        {
            var ok = false;

            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            var proxy = constructor.CreateProxy<IClassMock5, ClassMock5>(
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok = true;
                }));

            Assert.IsNotNull(proxy);

            I5 a = null;
            proxy.X(out a);

            Assert.IsTrue(ok);
        }

        public class I5
        {
            
        }

        public interface IClassMock5
        {
            [TestWrapWithProxy]
            void X(out I5 a);
        }

        public class ClassMock5 : IClassMock5
        {
            private Action _ok;

            public ClassMock5(Action ok)
            {
                _ok = ok;
            }

            #region Implementation of IClassMock3

            public void X(out I5 a)
            {
                a = null;

                _ok();
            }

            #endregion
        }

    }

}
