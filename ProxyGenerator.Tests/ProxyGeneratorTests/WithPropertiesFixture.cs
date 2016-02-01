using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    [TestClass]
    public class WithPropertiesFixture
    {
        [TestMethod]
        public void Test0WithProperties()
        {
            var payloadFactory = new MockPayloadFactory();

            var generator = new ProxyGenerator.G.ProxyTypeGenerator(payloadFactory);
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(payloadFactory, generator);

            var proxy = constructor.CreateProxy<IClassMock13, ClassMock13>(
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                }),
                new Action(() =>
                {
                }));

            Assert.IsNotNull(proxy);

        }

        public interface IClassMock13
        {
            int Y0
            {
                get;
                set;
            }

            string Y1
            {
                get;
            }

            byte Y2
            {
                set;
            }

            [TestWrapWithProxy]
            void X1();

            void X2();
        }

        public class ClassMock13 : IClassMock13
        {
            private Action _ok1;
            private Action _ok2;

            public ClassMock13(Action ok1)
            {
                _ok1 = ok1;
            }

            public ClassMock13(Action ok1, Action ok2)
            {
                _ok1 = ok1;
                _ok2 = ok2;
            }

            #region Implementation of IClassMock13

            public int Y0
            {
                get;
                set;
            }

            public string Y1
            {
                get
                {
                    return "Y1";
                }
            }

            public byte Y2
            {
                set
                {
                    //nothing to do
                }
            }

            public void X1()
            {
                _ok1();
            }

            public void X2()
            {
                if (_ok2 != null)
                {
                    _ok2();
                }
            }

            #endregion
        }

    }
}
