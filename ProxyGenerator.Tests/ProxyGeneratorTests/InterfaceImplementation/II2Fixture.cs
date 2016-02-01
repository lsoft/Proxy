
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.InterfaceImplementation
{
    /// <summary>
    /// Summary description for II2Fixture
    /// </summary>
    [TestClass]
    public class II2Fixture
    {
        public II2Fixture()
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
        public void TestII0()
        {
            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            var original = new ClassMockII2();
            var proxy = constructor.CreateProxy<IClassMockII2, ClassMockII2>(typeof(TestWrapWithProxyAttribute));

            {
                var originalImplementInterface = original.GetType().GetInterface(typeof (IDisposable).Name) != null;
                var proxyImplementInterface = proxy.GetType().GetInterface(typeof (IDisposable).Name) != null;

                Assert.IsTrue(originalImplementInterface);
                Assert.IsTrue(proxyImplementInterface);
            }

            {
                var originalImplementInterface = original.GetType().GetInterface(typeof(IClassMockII2_Base).Name) != null;
                var proxyImplementInterface = proxy.GetType().GetInterface(typeof(IClassMockII2_Base).Name) != null;

                Assert.IsTrue(originalImplementInterface);
                Assert.IsTrue(proxyImplementInterface);
            }

            {
                var originalImplementInterface = original.GetType().GetInterface(typeof(IClassMockII2).Name) != null;
                var proxyImplementInterface = proxy.GetType().GetInterface(typeof(IClassMockII2).Name) != null;

                Assert.IsTrue(originalImplementInterface);
                Assert.IsTrue(proxyImplementInterface);
            }

            {
                var originalImplementInterface = original.GetType().GetInterface(typeof(IClassMockII2_Additional).Name) != null;
                var proxyImplementInterface = proxy.GetType().GetInterface(typeof(IClassMockII2_Additional).Name) != null;

                Assert.IsTrue(originalImplementInterface);
                Assert.IsTrue(proxyImplementInterface);
            }
        }

    }

    public interface IClassMockII2_Additional
    {
        [TestWrapWithProxy]
        void DoIClassMockII2_Additional();
    }

    public interface IClassMockII2_Base
    {
        [TestWrapWithProxy]
        void DoBase();
    }

    public interface IClassMockII2 : IClassMockII2_Base
    {
        [TestWrapWithProxy]
        void Do();
    }

    public class ClassMockII2_Additional : IClassMockII2_Additional
    {
        public void DoIClassMockII2_Additional()
        {
            //nothing to do
        }
    }

    public class ClassMockII2 : ClassMockII2_Additional, IClassMockII2, IDisposable
    {
        public void Dispose()
        {
            //nothing to do
        }

        public void Do()
        {
            //nothing to do
        }

        public void DoBase()
        {
            //nothing to do
        }
    }

}
