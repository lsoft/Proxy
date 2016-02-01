using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PerformanceTelemetry.Container.Saver;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Tests.PerformanceTelemetryTests.PerformanceContainer
{
    /// <summary>
    /// Summary description for UnitTest2
    /// </summary>
    [TestClass]
    public class UnitTest2
    {
        public UnitTest2()
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
        public void TestOpenPerformanceSession0()
        {
            var rf = new Mock<IPerformanceRecordFactory>();
            rf.Setup(a => a.CreateRecord(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Mock<IPerformanceRecord>().Object);
            rf.Setup(a => a.CreateChildRecord(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IPerformanceRecord>()))
                .Returns(new Mock<IPerformanceRecord>().Object);

            var sf = new Mock<IPerformanceSaver>();

            var pc = new PerformanceTelemetry.Container.PerformanceContainer(
                new EmptyLoggerAdapter(),
                rf.Object,
                sf.Object);

            var r = pc.OpenPerformanceSession(
                1,
                string.Empty,
                string.Empty);

            Assert.IsNotNull(r);

        }

        [TestMethod]
        public void TestOpenPerformanceSession1()
        {
            var r1 = new Mock<IPerformanceRecord>();

            var r0 = new Mock<IPerformanceRecord>();
            r0.Setup(a => a.GetDeepestActiveRecord())
                .Returns(r0.Object);
            r0.Setup(a => a.CreateChild(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(r1.Object);

            var rf = new Mock<IPerformanceRecordFactory>();
            rf.Setup(a => a.CreateRecord(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(r0.Object);
            rf.Setup(a => a.CreateChildRecord(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IPerformanceRecord>()))
                .Throws(new InternalTestFailureException("этого вызова быть не должно"));

            var sf = new Mock<IPerformanceSaver>();

            var pc = new PerformanceTelemetry.Container.PerformanceContainer(
                new EmptyLoggerAdapter(),
                rf.Object,
                sf.Object);

            var r_0 = pc.OpenPerformanceSession(
                1,
                string.Empty,
                string.Empty);

            var r_1 = pc.OpenPerformanceSession(
                1,
                string.Empty,
                string.Empty);

            Assert.IsNotNull(r_0);
            Assert.IsNotNull(r_1);

            Assert.AreSame(r0.Object, r_0);
            Assert.AreSame(r1.Object, r_1);

        }






    }
}
