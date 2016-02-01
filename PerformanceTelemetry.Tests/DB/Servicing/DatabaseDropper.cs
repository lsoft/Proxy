using System;
using System.Data.SqlClient;

namespace PerformanceTelemetry.Tests.DB.Servicing
{
    public class DatabaseDropper
    {

        public DatabaseDropper(
            )
        {
        }

        public void DropDatabase(
            string databasePath,
            string databaseName
            )
        {
            if (databasePath == null)
            {
                throw new ArgumentNullException("databasePath");
            }
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }

            //формируем строку подключения
            var defaultConnectionString = new SqlServerDatabaseConnectionString(
                databasePath,
                string.Empty
                );

            //создаем базу
            using (var connection = new SqlConnection(defaultConnectionString.ConnectionString))
            {
                connection.Open();
                try
                {
                    using (var q = new SqlCommand(
                        string.Format(DropDatabaseQueryText, databaseName),
                        connection
                        ))
                    {
                        q.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }


        private const string DropDatabaseQueryText = @"
USE [master]
ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [{0}];
";


    }
}