using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PerformanceTelemetry.Record;
using PerformanceTelemetry.Timer;

namespace PerformanceTelemetry.Tests.PerformanceTelemetryTests.PerformanceRecord
{
    /// <summary>
    /// Summary description for UnitTest4
    /// </summary>
    [TestClass]
    public class UnitTest4
    {
        public UnitTest4()
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
        public void TestClose0()
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

            var pi = new PerformanceTelemetry.Record.PerformanceRecord(string.Empty, string.Empty, rf.Object, tf.Object, null);

            pi.Close();
        }

        [TestMethod]
        public void TestClose1()
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

            var pi = new PerformanceTelemetry.Record.PerformanceRecord(string.Empty, string.Empty, rf.Object, tf.Object, null);

            pi.Close();

            Assert.IsFalse(pi.Active);
        }

        [TestMethod]
        public void TestClose2()
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

            var pi = new PerformanceTelemetry.Record.PerformanceRecord(string.Empty, string.Empty, rf.Object, tf.Object, pr.Object);

            pi.Close();
            
            pr.Verify(a => a.ChildDied());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestClose3()
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

            var pi = new PerformanceTelemetry.Record.PerformanceRecord(string.Empty, string.Empty, rf.Object, tf.Object, null);

            var child = pi.CreateChild(string.Empty, string.Empty);

            pi.Close();
        }

    }

}
