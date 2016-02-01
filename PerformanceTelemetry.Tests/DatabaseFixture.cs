using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerformanceTelemetry.Tests.DB.DB;
using PerformanceTelemetry.Tests.DB.InstanceProvider;
using PerformanceTelemetry.Tests.DB.Servicing;

namespace PerformanceTelemetry.Tests
{
    public abstract class DatabaseFixture
    {
        protected const string TestDatabaseName = TrnHelper.MainDatabaseName;

        protected SqlServerInstanceInformation Instance
        {
            get;
            private set;
        }

        protected DatabaseFixture(
            )
        {
            var instance = GetSqlInstance();
            this.Instance = instance;
        }


        protected void DropDatabase(
            string databasePath
            )
        {
            if (databasePath == null)
            {
                throw new ArgumentNullException("databasePath");
            }

            var dropper = new DatabaseDropper();
            dropper.DropDatabase(
                databasePath,
                TestDatabaseName
                );
        }

        protected void RestoreDatabase(
            string databasePath,
            string pathToTrnFile,
            string defaultDatabaseName
            )
        {
            if (databasePath == null)
            {
                throw new ArgumentNullException("databasePath");
            }
            if (pathToTrnFile == null)
            {
                throw new ArgumentNullException("pathToTrnFile");
            }
            if (defaultDatabaseName == null)
            {
                throw new ArgumentNullException("defaultDatabaseName");
            }

            var restorer = new DatabaseRestorer();

            restorer.RestoreDatabase(
                databasePath,
                TestDatabaseName,
                pathToTrnFile,
                defaultDatabaseName
                );
        }

        protected void CreateEmptyDatabase(
            string databasePath
            )
        {
            if (databasePath == null)
            {
                throw new ArgumentNullException("databasePath");
            }

            var creator = new EmptyDatabaseCreator();

            creator.CreateDatabase(
                databasePath,
                TestDatabaseName
                );
        }

        private SqlServerInstanceInformation GetSqlInstance()
        {
            if (_cachedInformation == null)
            {
                var ip = new WmiSqlServerInstanceProvider();
                var instances = ip.EnumerateSqlInstances();
                instances.RemoveAll(j => j.SqlServerInstanceVersion == SqlServerInstanceInformation.SqlServerInstanceVersionEnum.Unknown);

                if (instances.Count == 0)
                {
                    throw new InternalTestFailureException("Нет доступных SQL Instances");
                }

                if (instances.Count > 1)
                {
                    Debug.WriteLine("Найдено {0} SQL инстансов", instances.Count);
                }

                _cachedInformation = instances[0];
            }

            Debug.WriteLine(string.Format("Выбран {0}", _cachedInformation.FullInformation));

            return
                _cachedInformation;
        }

        private SqlServerInstanceInformation _cachedInformation = null;

    }
}
