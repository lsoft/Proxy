using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyGenerator.Constructor;
using ProxyGenerator.Generator;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.Hierarchy
{
    [TestClass]
    public class HierarchyInterface0Fixture
    {
        [TestMethod]
        public void TestWithDerivedInterfaces()
        {
            var payloadFactory = new MockPayloadFactory();

            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock2, ClassMock2>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute)
                );

            Assert.IsNotNull(proxy);

        }

        public class ClassMock2 : IClassMock2
        {
            public void M0()
            {
                //nothing to do
            }

            public void M1()
            {
                //nothing to do
            }

            public void M2()
            {
                //nothing to do
            }

            public void M3()
            {
                //nothing to do
            }
        }

        public interface IClassMock2 : IClassMock1
        {
            void M2();
            void M3();
        }

        public interface IClassMock1 : IClassMock0
        {
            void M1();
        }

        public interface IClassMock0
        {
            void M0();
        }
    }
}
