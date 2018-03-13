using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceTelemetry.Container.Saver;
using PerformanceTelemetry.Container.Saver.Item.Binary;
using PerformanceTelemetry.Container.Saver.Item.Sql.SqlBatch;
using PerformanceTelemetry.Record;
using PerformanceTelemetry.Tests.DB.DB;
using PerformanceTelemetry.Tests.DB.Servicing;

namespace PerformanceTelemetry.Tests.PerformanceTelemetryTests.SaverTests
{
    [TestClass]
    public class BinaryItemSaverFixture
    {
        private const string TableName = "PerformanceArtifacts";

        //[ClassInitialize]
        //public static void GlobalInit(
        //    TestContext context
        //    )
        //{
        //}

        //public BinaryItemSaverFixture()
        //{
        //    try
        //    {
        //        //дропаем старую базу, если она осталась с пред. раза
        //        DropDatabase(Instance.DatabasePath);
        //    }
        //    catch (Exception excp)
        //    {
        //        Debug.WriteLine(excp.Message);
        //        Debug.WriteLine(excp.StackTrace);
        //    }
        //}

        //[TestInitialize]
        //public void InitBeforeEachTest()
        //{
        //    ////создаем базу
        //    //CreateEmptyDatabase(Instance.DatabasePath);

        //    //поднимаем бекап
        //    using (var tempTrn = TrnHelper.ExtractTrnToTempFile())
        //    {
        //        RestoreDatabase(
        //            Instance.DatabasePath,
        //            tempTrn.FilePath,
        //            TrnHelper.MainDatabaseName
        //            );
        //    }
        //}

        //[TestCleanup]
        //public void CleanupAfterEachTest()
        //{
        //    //дропаем ненужную базу
        //    DropDatabase(Instance.DatabasePath);
        //}

        [TestMethod]
        public void TestSaveItem0()
        {
            var record = new TestPerformanceRecordData(
                StringGenerator.GetString("ClassName"),
                StringGenerator.GetString("MethodName"),
                DateTime.Now,
                100,
                StringGenerator.GetString("CreationStack"),
                new FakeException("fake exception for test purposes"),

                new TestPerformanceRecordData(
                    StringGenerator.GetString("Child 1:ClassName"),
                    StringGenerator.GetString("Child 1:MethodName"),
                    DateTime.Now.AddSeconds(-1),
                    99,
                    StringGenerator.GetString("Child 1:CreationStack")
                    ),
                new TestPerformanceRecordData(
                    StringGenerator.GetString("Child 2:ClassName"),
                    StringGenerator.GetString("Child 2:MethodName"),
                    DateTime.Now.AddSeconds(-2),
                    98,
                    StringGenerator.GetString("Child 2:CreationStack")
                    )
                );

            var folderPath = Path.Combine(
                Path.GetTempPath(),
                "_D-" + Guid.NewGuid()
                );

            var areEqual = false;

            try
            {

                const string DataFileMask = "perf.data";
                const string KeyFileMask = "perf.key";

                var itemSaver = new BinaryItemSaverFactory(
                    folderPath,
                    DataFileMask,
                    KeyFileMask,
                    TimeSpan.FromHours(-1),
                    new EmptyLoggerAdapter()
                    );

                using (var ebSaver = new BatchBasedSaver(
                    itemSaver,
                    new EmptyLoggerAdapter(),
                    false
                    ))
                {
                    ebSaver.Save(record);
                }

                var files = Directory.GetFiles(folderPath, "*.data", SearchOption.AllDirectories);

                if (files == null || files.Length != 1)
                {
                    throw new InternalTestFailureException("files == null || files.Length != 1");
                }

                var file = files[0];

                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (var reader = DiskPerformanceRecordSerializer.CreateReader(stream))
                {
                    var readRecord = DiskPerformanceRecordSerializer.ReadOne(reader);

                    if (readRecord == null)
                    {
                        throw new InternalTestFailureException("readItem == null");
                    }

                    areEqual = record.CheckEqualityFor(readRecord);
                }
            }
            finally
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
               
            }

            Assert.IsTrue(areEqual);

        }

    }

}
