using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyGenerator.Constructor;
using ProxyGenerator.Generator;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    /// <summary>
    /// Summary description for NestedNestedClass1Fixture
    /// </summary>
    [TestClass]
    public class NestedNestedClass1Fixture
    {
        public NestedNestedClass1Fixture()
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
        public void TestNestedNestedClass8()
        {
            var ok = false;

            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<I8.IClassMock8, ClassMock8>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok = true;
                }));

            Assert.IsNotNull(proxy);

            I8 a = null;
            proxy.X(ref a);

            Assert.IsTrue(ok);
        }


        public class I8
        {
            public interface IClassMock8
            {
                [TestWrapWithProxy]
                void X(ref I8 a);
            }
        }

        public class ClassMock8 : I8.IClassMock8
        {
            private Action _ok;

            public ClassMock8(Action ok)
            {
                _ok = ok;
            }

            #region Implementation of IClassMock3

            public void X(ref I8 a)
            {
                _ok();
            }

            #endregion
        }

    }

}
