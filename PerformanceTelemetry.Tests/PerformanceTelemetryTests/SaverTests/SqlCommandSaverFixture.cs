using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Management.Instrumentation;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceTelemetry.Container.Saver;
using PerformanceTelemetry.Container.Saver.Item.Sql.SqlCommand;
using PerformanceTelemetry.Record;
using PerformanceTelemetry.Tests.DB.DB;
using PerformanceTelemetry.Tests.DB.Servicing;

namespace PerformanceTelemetry.Tests.PerformanceTelemetryTests.SaverTests
{
    [TestClass]
    public class SqlCommandSaverFixture : DatabaseFixture
    {
        private const string TableName = "PerformanceArtifacts";

        //[ClassInitialize]
        //public static void GlobalInit(
        //    TestContext context
        //    )
        //{
        //}

        public SqlCommandSaverFixture()
        {
            try
            {
                //дропаем старую базу, если она осталась с пред. раза
                DropDatabase(Instance.DatabasePath);
            }
            catch (Exception excp)
            {
                Debug.WriteLine(excp.Message);
                Debug.WriteLine(excp.StackTrace);
            }
        }

        [TestInitialize]
        public void InitBeforeEachTest()
        {
            ////создаем базу
            //CreateEmptyDatabase(Instance.DatabasePath);

            //поднимаем бекап
            using (var tempTrn = TrnHelper.ExtractTrnToTempFile())
            {
                RestoreDatabase(
                    Instance.DatabasePath,
                    tempTrn.FilePath,
                    TrnHelper.MainDatabaseName
                    );
            }
        }

        [TestCleanup]
        public void CleanupAfterEachTest()
        {
            //дропаем ненужную базу
            DropDatabase(Instance.DatabasePath);
        }

        [TestMethod]
        public void TestMultipleDispose()
        {
            var connectionString = new SqlServerDatabaseConnectionString(
                Instance.DatabasePath,
                TestDatabaseName,
                true
                );

            using (var itemSaver = new SqlCommandItemSaverFactory(
                new EmptyLoggerAdapter(), 
                connectionString.ConnectionString,
                TrnHelper.MainDatabaseName,
                TableName,
                1000000L
                ))
            {
                using (var ebSaver = new EventBasedSaver(
                    itemSaver,
                    new EmptyLoggerAdapter(),
                    false
                    ))
                {
                    ebSaver.Dispose();
                }
            }

        }

        [TestMethod]
        public void TestPreparationLogic()
        {
            var connectionString = new SqlServerDatabaseConnectionString(
                Instance.DatabasePath,
                TestDatabaseName,
                true
                );

            using (var itemSaver = new SqlCommandItemSaverFactory(
                new EmptyLoggerAdapter(), 
                connectionString.ConnectionString,
                TrnHelper.MainDatabaseName,
                TableName,
                1000000L
                ))
            {
                using (var ebSaver = new EventBasedSaver(
                    itemSaver,
                    new EmptyLoggerAdapter(),
                    false
                    ))
                {

                }

                using (var ebSaver = new EventBasedSaver(
                    itemSaver,
                    new EmptyLoggerAdapter(),
                    false
                    ))
                {

                }
            }
        }

        [TestMethod]
        public void TwoTestWithSameDatabase()
        {
            var connectionString = new SqlServerDatabaseConnectionString(
                Instance.DatabasePath,
                TestDatabaseName,
                true
                );

            var record0 = new TestPerformanceRecordData(
                StringGenerator.GetString("ClassName0"),
                StringGenerator.GetString("MethodName0"),
                DateTime.Now,
                100,
                StringGenerator.GetString("CreationStack0")
                );

            var record1 = new TestPerformanceRecordData(
                StringGenerator.GetString("ClassName1"),
                StringGenerator.GetString("MethodName1"),
                DateTime.Now,
                200,
                StringGenerator.GetString("CreationStack1")
                );

            using (var itemSaver = new SqlCommandItemSaverFactory(
                new EmptyLoggerAdapter(),
                connectionString.ConnectionString,
                TrnHelper.MainDatabaseName,
                TableName,
                1000000L
                ))
            {
                using (var ebSaver = new EventBasedSaver(
                    itemSaver,
                    new EmptyLoggerAdapter(),
                    false
                    ))
                {
                }

            }

            using (var itemSaver = new SqlCommandItemSaverFactory(
                new EmptyLoggerAdapter(),
                connectionString.ConnectionString,
                TrnHelper.MainDatabaseName,
                TableName,
                1000000L
                ))
            {
                using (var ebSaver = new EventBasedSaver(
                    itemSaver,
                    new EmptyLoggerAdapter(),
                    false
                    ))
                {
                    ebSaver.Save(record1);
                }

            }
        }

        [TestMethod]
        public void TestSaveItem0()
        {
            var connectionString = new SqlServerDatabaseConnectionString(
                Instance.DatabasePath,
                TestDatabaseName,
                true
                );

            var record = new TestPerformanceRecordData(
                StringGenerator.GetString("ClassName"),
                StringGenerator.GetString("MethodName"),
                DateTime.Now,
                100,
                StringGenerator.GetString("CreationStack")
                );

            using (var itemSaver = new SqlCommandItemSaverFactory(
                new EmptyLoggerAdapter(), 
                connectionString.ConnectionString,
                TrnHelper.MainDatabaseName,
                TableName,
                1000000L
                ))
            {
                using (var ebSaver = new EventBasedSaver(
                    itemSaver,
                    new EmptyLoggerAdapter(),
                    false
                    ))
                {
                    ebSaver.Save(record);
                }
            }

            var dataExists = false;

            using (var connection = new SqlConnection(connectionString.ConnectionString))
            {
                connection.Open();

                var commandText = string.Format(@"
select top(1)
    *
from [{0}].dbo.[{1}] l
join [{0}].dbo.[{1}Stack] s on l.id_stack = s.id
order
    by l.id desc", TrnHelper.MainDatabaseName, TableName);

                using (var command = new SqlCommand(commandText, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Assert.IsTrue(record.CheckEqualityFor(0, reader));

                            dataExists = true;
                        }
                    }
                }
            }

            Assert.IsTrue(dataExists);
        }

        [TestMethod]
        public void TestSaveItem1()
        {
            var connectionString = new SqlServerDatabaseConnectionString(
                Instance.DatabasePath,
                TestDatabaseName,
                true
                );

            var record = new TestPerformanceRecordData(
                StringGenerator.GetString("ClassName"),
                StringGenerator.GetString("MethodName"),
                DateTime.Now,
                100,
                StringGenerator.GetString("CreationStack"),
                new Exception(StringGenerator.GetString("ExceptionMessage"))
                );

            using (var itemSaver = new SqlCommandItemSaverFactory(
                new EmptyLoggerAdapter(), 
                connectionString.ConnectionString,
                TrnHelper.MainDatabaseName,
                TableName,
                1000000L
                ))
            {
                using (var ebSaver = new EventBasedSaver(
                    itemSaver,
                    new EmptyLoggerAdapter(),
                    false
                    ))
                {
                    ebSaver.Save(record);
                }
            }

            var dataExists = false;

            using (var connection = new SqlConnection(connectionString.ConnectionString))
            {
                connection.Open();

                var commandText = string.Format(@"
select top(1)
    *
from [{0}].dbo.[{1}] l
join [{0}].dbo.[{1}Stack] s on l.id_stack = s.id
order by
    l.id desc", TrnHelper.MainDatabaseName, TableName);

                using (var command = new SqlCommand(commandText, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Assert.IsTrue(record.CheckEqualityFor(0, reader));

                            dataExists = true;
                        }
                    }
                }
            }

            Assert.IsTrue(dataExists);
        }

        [TestMethod]
        public void TestSaveItem2()
        {
            var connectionString = new SqlServerDatabaseConnectionString(
                Instance.DatabasePath,
                TestDatabaseName,
                true
                );


            var child0 = new TestPerformanceRecordData(
                StringGenerator.GetString("ClassName:Inner0"),
                StringGenerator.GetString("MethodName:Inner0"),
                DateTime.Now.AddSeconds(1),
                10,
                StringGenerator.GetString("CreationStack:Inner0")
                );

            var child1 = new TestPerformanceRecordData(
                StringGenerator.GetString("ClassName:Inner1"),
                StringGenerator.GetString("MethodName:Inner1"),
                DateTime.Now.AddSeconds(2),
                50,
                StringGenerator.GetString("CreationStack:Inner1")
                );

            var record = new TestPerformanceRecordData(
                StringGenerator.GetString("ClassName"),
                StringGenerator.GetString("MethodName"),
                DateTime.Now,
                100,
                StringGenerator.GetString("CreationStack"),
                new List<IPerformanceRecordData>
                {
                    child0,
                    child1
                }
                );

            var dataExists0 = false;
            var dataExists1 = false;
            var dataExists2 = false;

            using (var itemSaver = new SqlCommandItemSaverFactory(
                new EmptyLoggerAdapter(), 
                connectionString.ConnectionString,
                TrnHelper.MainDatabaseName,
                TableName,
                1000000L
                ))
            {
                using (var ebSaver = new EventBasedSaver(
                    itemSaver,
                    new EmptyLoggerAdapter(),
                    false
                    ))
                {
                    ebSaver.Save(record);
                }
            }

            using (var connection = new SqlConnection(connectionString.ConnectionString))
            {
                connection.Open();

                var commandText = string.Format(@"
select
    *
from [{0}].dbo.[{1}] l
join [{0}].dbo.[{1}Stack] s on l.id_stack = s.id
order by
    l.id asc", TrnHelper.MainDatabaseName, TableName);

                using (var command = new SqlCommand(commandText, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var index = 0;

                        while (reader.Read())
                        {
                            switch (index)
                            {
                                case 0:
                                    Assert.IsTrue(record.CheckEqualityFor(0, reader));
                                    dataExists0 = true;
                                    break;
                                case 1:
                                    Assert.IsTrue(child0.CheckEqualityFor(1, reader));
                                    dataExists1 = true;
                                    break;
                                case 2:
                                    Assert.IsTrue(child1.CheckEqualityFor(1, reader));
                                    dataExists2 = true;
                                    break;
                                default:
                                    Assert.Fail();
                                    break;
                            }

                            index++;
                        }
                    }
                }
            }

            Assert.IsTrue(dataExists0);
            Assert.IsTrue(dataExists1);
            Assert.IsTrue(dataExists2);
        }
    }
}
