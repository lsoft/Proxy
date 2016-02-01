using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    /// <summary>
    /// Summary description for VariantsFixture
    /// </summary>
    [TestClass]
    public class VariantsFixture
    {
        public VariantsFixture()
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
        public void TestVariants()
        {
            var ok = false;

            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            var proxy = constructor.CreateProxy<IClassMock9, ClassMock9>(
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                    ok = true;
                }));

            Assert.IsNotNull(proxy);

            proxy.X2();

            Assert.IsTrue(ok);
        }

        public class I9
        {
            
        }

        public interface IClassMock9
        {
            [TestWrapWithProxy]
            void X1(ref I9 a);

            [TestWrapWithProxy]
            I9 X2();

            [TestWrapWithProxy]
            int X3();

            [TestWrapWithProxy]
            I9 X4(I9 a);
        }

        public class ClassMock9 : IClassMock9
        {
            private Action _ok;

            public ClassMock9(Action ok)
            {
                _ok = ok;
            }

            #region Implementation of IClassMock9

            public void X1(ref I9 a)
            {
                throw new NotImplementedException();
            }

            public I9 X2()
            {
                _ok();

                return null;
            }

            public int X3()
            {
                throw new NotImplementedException();
            }

            public I9 X4(I9 a)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

    }

}
