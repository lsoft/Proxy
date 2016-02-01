using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceTelemetry.Tests.DB.Servicing
{
    /// <summary>
    /// Строка подключения к SQL SERVER
    /// </summary>
    public class SqlServerDatabaseConnectionString : IDatabaseConnectionString
    {
        private readonly string _databasePath;
        private readonly string _databaseName;
        private readonly bool _marsEnabled;

        public string ConnectionString
        {
            get
            {
                return
                    string.Format(
                        "Data Source={0}; Initial Catalog={1}; Integrated Security=true; Connection Timeout=10; Max Pool Size=50; {2}",
                        _databasePath,
                        _databaseName,
                        _marsEnabled ? "MultipleActiveResultSets=True" : string.Empty);
            }
        }

        public bool AllowMultiplyConnectionsToDatabase
        {
            get
            {
                return true;
            }
        }

        public bool AllowMultipleActiveResultSets
        {
            get
            {
                return
                    _marsEnabled;
            }
        }

        public SqlServerDatabaseConnectionString(
            string databasePath,
            string databaseName,
            bool marsEnabled = false
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

            _databasePath = databasePath;
            _databaseName = databaseName;
            _marsEnabled = marsEnabled;
        }

        public IDatabaseConnectionString RetargetWithDifferentPath(
            string folderPath
            )
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException("folderPath");
            }

            return
                new SqlServerDatabaseConnectionString(
                    folderPath,
                    _databaseName
                    );
        }
    }
}
