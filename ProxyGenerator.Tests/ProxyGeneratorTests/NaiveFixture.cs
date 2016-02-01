using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    /// <summary>
    /// Summary description for NaiveFixture
    /// </summary>
    [TestClass]
    public class NaiveFixture
    {
        public NaiveFixture()
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
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidPair()
        {
            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            constructor.CreateProxy<IClassMock1, ClassMock0>(typeof(TestWrapWithProxyAttribute));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidClass()
        {
            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            constructor.CreateProxy<IClassMock1, IClassMock0>(typeof(TestWrapWithProxyAttribute));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInvalidInterface()
        {
            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            constructor.CreateProxy<ClassMock0, ClassMock0>(typeof(TestWrapWithProxyAttribute));
        }

        [TestMethod]
        public void TestCorrect()
        {
            var payloadFactory = new MockPayloadFactory();
            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            var proxy = constructor.CreateProxy<IClassMock0, ClassMock0>(typeof(TestWrapWithProxyAttribute));

            Assert.IsNotNull(proxy);
        }
    }

    public interface IClassMock0
    {

    }

    public interface IClassMock1
    {

    }

    public class ClassMock0 : IClassMock0
    {

    }

}
