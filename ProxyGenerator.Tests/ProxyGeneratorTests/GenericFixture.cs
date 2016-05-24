using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    [TestClass]
    public class GenericFixture
    {
        [TestMethod]
        public void Test0WithProperties()
        {
            var payloadFactory = new MockPayloadFactory();

            var generator = new ProxyGenerator.G.ProxyTypeGenerator();
            var constructor = new ProxyGenerator.C.StandaloneProxyConstructor(generator);

            var proxy = constructor.CreateProxy<IClassMock14<string>, ClassMock14<string>>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute),
                new Action(() =>
                {
                }),
                new Action(() =>
                {
                }));

            Assert.IsNotNull(proxy);

        }

        public class JustParam<T>
        {
            public T Value
            {
                get;
                private set;
            }

            public JustParam(T value)
            {
                Value = value;
            }
        }

        public interface IClassMock14<T>
        {
            JustParam<T> Y0
            {
                get;
                set;
            }

            Dictionary<string, long> Y1
            {
                get;
            }

            JustParam<long> Y2
            {
                set;
            }

            [TestWrapWithProxy]
            void X1();

            void X2();
        }

        public class ClassMock14<T> : IClassMock14<T>
        {
            private Action _ok1;
            private Action _ok2;

            public ClassMock14(Action ok1)
            {
                _ok1 = ok1;
            }

            public ClassMock14(Action ok1, Action ok2)
            {
                _ok1 = ok1;
                _ok2 = ok2;
            }

            #region Implementation of IClassMock14

            public JustParam<T> Y0
            {
                get;
                set;
            }

            public Dictionary<string, long> Y1
            {
                get
                {
                    return null;
                }
            }

            public JustParam<long> Y2
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
