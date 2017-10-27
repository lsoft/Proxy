using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProxyGenerator.Constructor;
using ProxyGenerator.Generator;

namespace ProxyGenerator.Tests.ProxyGeneratorTests.Generic.Parameters
{
    /// <summary>
    /// Summary description for WithPropertiesFixture
    /// </summary>
    [TestClass]
    public class GenericParameter3Fixture
    {
        public GenericParameter3Fixture()
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
        public void TestGeneric0()
        {
            var setExceptionFlag = false;
            var dispose = false;
            var ok = false;

            var payloadFactory = new MockPayloadFactory(
                (excp) =>
                {
                    setExceptionFlag = true;
                },
                () =>
                {
                    dispose = true;
                });

            var generator = new ProxyTypeGenerator();
            var constructor = new StandaloneProxyConstructor(generator);

            const string WowString = "Wow";

            var proxy = constructor.CreateProxy<IExecutor<string>, Executor<string>>(
                payloadFactory,
                typeof(TestWrapWithProxyAttribute)
                , new Ex<string>(WowString)
                );

            Assert.IsNotNull(proxy);

            var result = proxy.Execute<int>(
                (a) =>
                {
                    if (a.Arg == WowString)
                    {
                        ok = true;
                    }
                });

            Assert.AreEqual(0, result);
            Assert.IsTrue(ok);
            Assert.IsFalse(setExceptionFlag);
            Assert.IsTrue(dispose);
        }

        public interface IEx<T0>
        {
            T0 Arg
            {
                get;
            }
        }

        public class Ex<T0> : IEx<T0>
        {
            public T0 Arg
            {
                get;
                private set;
            }

            public Ex(T0 arg)
            {
                Arg = arg;
            }
        }


        public interface IExecutor<T0>
        {
            [TestWrapWithProxy]
            T Execute<T>(
                Action<IEx<T0>> executeAction
                );
        }

        public class Executor<T0> : IExecutor<T0>
        {
            private readonly IEx<T0> _arg;

            public Executor(
                IEx<T0> arg
                )
            {
                if (arg == null)
                {
                    throw new ArgumentNullException("arg");
                }
                _arg = arg;
            }

            public T Execute<T>(Action<IEx<T0>> executeAction)
            {
                executeAction(_arg);

                return
                    default(T);
            }
        }
    }

}
