using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.Hierarchy
{
    [TestClass]
    public class HierarchyInterface1Fixture
    {
        [TestMethod]
        public void TestWithDerivedInterfaces()
        {
            var payloadFactory = new MockPayloadFactory();

            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            var proxy = constructor.CreateProxy<IClassMock2, ClassMock2>(
                typeof(TestWrapWithProxyAttribute));

            Assert.IsNotNull(proxy);
        }

        public class ClassMock2 : IClassMock2
        {
            public void M01()
            {
                //nothing to do
            }

            public void M02()
            {
                //nothing to do
            }

            public void M11()
            {
                //nothing to do
            }

            public void M12()
            {
                //nothing to do
            }

            public void M21()
            {
                //nothing to do
            }

            public void M22()
            {
                //nothing to do
            }
        }

        public interface IClassMock2 : IClassMock1
        {
            void M21();
            void M22();
        }

        public interface IClassMock1 : IClassMock0
        {
            void M11();
            void M12();
        }

        public interface IClassMock0
        {
            void M01();
            void M02();
        }
    }
}
