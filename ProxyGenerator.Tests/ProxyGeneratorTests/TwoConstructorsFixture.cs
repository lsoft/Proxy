using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyGenerator.Constructor;
using ProxyGenerator.Generator;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    /// <summary>
    /// Summary description for TwoConstructorsFixture
    /// </summary>
    [TestClass]
    public class TwoConstructorsFixture
    {
        public TwoConstructorsFixture()
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
        public void Test2Constructors0()
        {
            var ok = false;

            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock11, ClassMock11>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok = true;
                }));

            Assert.IsNotNull(proxy);

            proxy.X1();

            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void Test2Constructors1()
        {
            var ok1 = false;
            var ok2 = false;

            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock11, ClassMock11>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok1 = true;
                }),
                new Action(() =>
                {
                    ok2 = true;
                }));

            Assert.IsNotNull(proxy);

            proxy.X1();
            proxy.X2();

            Assert.IsTrue(ok1);
            Assert.IsTrue(ok2);
        }

        public interface IClassMock11
        {
            [TestWrapWithProxy]
            void X1();

            [TestWrapWithProxy]
            void X2();
        }

        public class ClassMock11 : IClassMock11
        {
            private Action _ok1;
            private Action _ok2;

            public ClassMock11(Action ok1)
            {
                _ok1 = ok1;
            }

            public ClassMock11(Action ok1, Action ok2)
            {
                _ok1 = ok1;
                _ok2 = ok2;
            }

            #region Implementation of IClassMock11

            public void X1()
            {
                _ok1();
            }

            public void X2()
            {
                if(_ok2 != null)
                {
                    _ok2();
                }
            }

            #endregion
        }

    }

}
