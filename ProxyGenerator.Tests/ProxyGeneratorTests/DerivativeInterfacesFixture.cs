using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    /// <summary>
    /// Summary description for DerivativeInterfacesFixture
    /// </summary>
    [TestClass]
    public class DerivativeInterfacesFixture
    {
        public DerivativeInterfacesFixture()
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
        public void TestDerivativeInterfaces()
        {
            var ok = false;

            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            var proxy = constructor.CreateProxy<IClassMock10_1, ClassMock10>(
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok = true;
                }));

            Assert.IsNotNull(proxy);

            int a = 1;
            long b = 2;
            proxy.X0(ref a, out b);

            Assert.IsTrue(ok);
        }

        public interface IClassMock10_0
        {
            [TestWrapWithProxy]
            void X0(ref int a, out long b);
        }

        public interface IClassMock10_1 : IClassMock10_0
        {
            [TestWrapWithProxy]
            void X1(ref int a);

            [TestWrapWithProxy]
            int X2();

            [TestWrapWithProxy]
            int X3();

            [TestWrapWithProxy]
            int X4(int a);
        }

        public class ClassMock10 : IClassMock10_1
        {
            private Action _ok;

            public ClassMock10(Action ok)
            {
                _ok = ok;
            }

            #region Implementation of IClassMock10_1

            public void X1(ref int a)
            {
                throw new NotImplementedException();
            }

            public int X2()
            {
                throw new NotImplementedException();
            }

            public int X3()
            {
                throw new NotImplementedException();
            }

            public int X4(int a)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region Implementation of IClassMock10_0

            public void X0(ref int a, out long b)
            {
                _ok();

                b = 0;
            }

            #endregion
        }

    }

}
