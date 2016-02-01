using System;
using System.Data.SqlClient;
using System.IO;

namespace PerformanceTelemetry.Tests.DB.Servicing
{
    public class DatabaseRestorer
    {

        public void RestoreDatabase(
            string databasePath,
            string databaseName,
            string pathToTrnFile,
            string defaultDatabaseName
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
            if (pathToTrnFile == null)
            {
                throw new ArgumentNullException("pathToTrnFile");
            }
            if (defaultDatabaseName == null)
            {
                throw new ArgumentNullException("defaultDatabaseName");
            }

            var systemTemp = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);

            var pathToMdfFile =
                Path.Combine(
                    systemTemp,
                    Path.GetRandomFileName() + ".mdf"
                    );
            var pathToLdfFile =
                Path.Combine(
                    systemTemp,
                    Path.GetRandomFileName() + ".ldf"
                    );

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
                        string.Format(
                            RestoreDatabaseQueryText,
                            databaseName,
                            pathToTrnFile,
                            defaultDatabaseName,
                            pathToMdfFile,
                            pathToLdfFile
                            ),
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

        private const string RestoreDatabaseQueryText = @"
RESTORE DATABASE [{0}] FROM  DISK = N'{1}' 
with
   MOVE N'{2}' TO N'{3}',
   MOVE N'{2}_log' TO N'{4}'
";


    }
}