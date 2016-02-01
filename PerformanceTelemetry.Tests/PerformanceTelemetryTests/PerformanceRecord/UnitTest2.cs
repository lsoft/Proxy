using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PerformanceTelemetry.Record;
using PerformanceTelemetry.Timer;

namespace PerformanceTelemetry.Tests.PerformanceTelemetryTests.PerformanceRecord
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
        public void TestDeepestActiveChild0()
        {
            var rf = new Mock<IPerformanceRecordFactory>();
            rf.Setup(a => a.CreateChildRecord(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IPerformanceRecord>()))
                .Returns(new Mock<IPerformanceRecord>().Object);

            var tf = new Mock<IPerformanceTimerFactory>();
            tf.Setup(a => a.CreatePerformanceTimer())
                .Returns(new Mock<IPerformanceTimer>().Object);

            var pi = new PerformanceTelemetry.Record.PerformanceRecord(string.Empty, string.Empty, rf.Object, tf.Object, null);

            var child = pi.GetDeepestActiveRecord();

            Assert.AreSame(pi, child);
        }

        [TestMethod]
        public void TestDeepestActiveChild1()
        {
            var rf = new Mock<IPerformanceRecordFactory>();
            rf.Setup(a => a.CreateChildRecord(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IPerformanceRecord>()))
                .Returns(new Mock<IPerformanceRecord>().Object);

            var tf = new Mock<IPerformanceTimerFactory>();
            tf.Setup(a => a.CreatePerformanceTimer())
                .Returns(new Mock<IPerformanceTimer>().Object);

            var pi = new PerformanceTelemetry.Record.PerformanceRecord(string.Empty, string.Empty, rf.Object, tf.Object, null);
            pi.Close();

            var child = pi.GetDeepestActiveRecord();

            Assert.IsNull(child);
        }

        [TestMethod]
        public void TestDeepestActiveChild2()
        {
            var pr = new Mock<IPerformanceRecord>();
            pr.Setup(a => a.GetDeepestActiveRecord())
                .Returns(pr.Object);

            var rf = new Mock<IPerformanceRecordFactory>();
            rf.Setup(a => a.CreateChildRecord(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IPerformanceRecord>()))
                .Returns(pr.Object);

            var tf = new Mock<IPerformanceTimerFactory>();
            tf.Setup(a => a.CreatePerformanceTimer())
                .Returns(new Mock<IPerformanceTimer>().Object);

            var pi0 = new PerformanceTelemetry.Record.PerformanceRecord(string.Empty, string.Empty, rf.Object, tf.Object, null);
            var pi1 = pi0.CreateChild(string.Empty, string.Empty);

            var child = pi0.GetDeepestActiveRecord();

            Assert.AreSame(pi1, child);
        }

    }

}
