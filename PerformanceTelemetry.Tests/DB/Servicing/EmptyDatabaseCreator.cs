using System;
using System.Data.SqlClient;

namespace PerformanceTelemetry.Tests.DB.Servicing
{
    public class EmptyDatabaseCreator
    {
        public void CreateDatabase(
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
                        string.Format(CreateDatabaseQueryText, databaseName),
                        connection
                        ))
                    {
                        q.ExecuteNonQuery();
                    }

                    using (var q = new SqlCommand(
                        string.Format(ModifyDatabaseQueryText, databaseName),
                        connection))
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


        private const string CreateDatabaseQueryText = @"
CREATE DATABASE [{0}]
 CONTAINMENT = NONE
";

        private const string ModifyDatabaseQueryText = @"
ALTER DATABASE [{0}] SET COMPATIBILITY_LEVEL = 110
ALTER DATABASE [{0}] SET ANSI_NULL_DEFAULT OFF 
ALTER DATABASE [{0}] SET ANSI_NULLS OFF 
ALTER DATABASE [{0}] SET ANSI_PADDING OFF 
ALTER DATABASE [{0}] SET ANSI_WARNINGS OFF 
ALTER DATABASE [{0}] SET ARITHABORT OFF 
ALTER DATABASE [{0}] SET AUTO_CLOSE OFF 
ALTER DATABASE [{0}] SET AUTO_CREATE_STATISTICS ON 
ALTER DATABASE [{0}] SET AUTO_SHRINK OFF 
ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS ON 
ALTER DATABASE [{0}] SET CURSOR_CLOSE_ON_COMMIT OFF 
ALTER DATABASE [{0}] SET CURSOR_DEFAULT  GLOBAL 
ALTER DATABASE [{0}] SET CONCAT_NULL_YIELDS_NULL OFF 
ALTER DATABASE [{0}] SET NUMERIC_ROUNDABORT OFF 
ALTER DATABASE [{0}] SET QUOTED_IDENTIFIER OFF 
ALTER DATABASE [{0}] SET RECURSIVE_TRIGGERS OFF 
ALTER DATABASE [{0}] SET DISABLE_BROKER 
ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
ALTER DATABASE [{0}] SET DATE_CORRELATION_OPTIMIZATION OFF 
ALTER DATABASE [{0}] SET PARAMETERIZATION SIMPLE 
ALTER DATABASE [{0}] SET READ_COMMITTED_SNAPSHOT OFF 
ALTER DATABASE [{0}] SET READ_WRITE 
ALTER DATABASE [{0}] SET RECOVERY FULL 
ALTER DATABASE [{0}] SET MULTI_USER 
ALTER DATABASE [{0}] SET PAGE_VERIFY CHECKSUM  
ALTER DATABASE [{0}] SET TARGET_RECOVERY_TIME = 0 SECONDS 
IF NOT EXISTS (SELECT name FROM sys.filegroups WHERE is_default=1 AND name = N'PRIMARY') ALTER DATABASE [{0}] MODIFY FILEGROUP [PRIMARY] DEFAULT
";


    }
}