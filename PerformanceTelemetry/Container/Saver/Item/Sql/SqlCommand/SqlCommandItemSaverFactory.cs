using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Cryptography;

namespace PerformanceTelemetry.Container.Saver.Item.Sql.SqlCommand
{
    public class SqlCommandItemSaverFactory : IItemSaverFactory, IDisposable
    {
        //контейнер хешей стека не обязан быть потоко-защищенным, так как обращения к нему идут только из потока сохранения
        private readonly StackIdContainer _stackIdContainer;

        //идентификатор последней существующей строки в СБД
        private readonly LastRowIdContainer _lastRowIdContainer;

        private readonly ITelemetryLogger _logger;
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly string _tableName;
        private readonly long _aliveRowsCount;

        private volatile bool _safeMode = false;

        public SqlCommandItemSaverFactory(
            ITelemetryLogger logger,
            string connectionString,
            string databaseName,
            string tableName,
            long aliveRowsCount
            )

        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }

            if (aliveRowsCount <= 0)
            {
                throw new ArgumentException("aliveRowsCount <= 0");
            }

            
            _stackIdContainer = new StackIdContainer();
            _logger = logger;
            _connectionString = connectionString;
            _databaseName = databaseName;
            _tableName = tableName;
            _aliveRowsCount = aliveRowsCount;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    long lastRowId;
                    SqlHelper.DoPreparation(
                        connection,
                        null,
                        _databaseName,
                        _tableName,
                        _logger,
                        out lastRowId
                        );

                    _stackIdContainer = SqlHelper.ReadStackTable(
                        connection,
                        null,
                        _databaseName,
                        _tableName,
                        _logger
                        );

                    _lastRowIdContainer = new LastRowIdContainer(lastRowId);
                }
            }
            catch (Exception excp)
            {
                _safeMode = true;

                logger.LogHandledException(
                    this.GetType(),
                    "Error database patching. Telemetry is going offline.",
                    excp
                    );
            }
        }

        public IItemSaver CreateItemSaver()
        {
            IItemSaver r = null;

            if (_safeMode)
            {
                //при ошибке патчинга, логгинг отключается

                r = FakeItemSaver.Instance;
            }
            else
            {
                r = new SqlCommandItemSaver(
                    _logger,
                    _stackIdContainer,
                    _connectionString,
                    _databaseName,
                    _tableName,
                    _aliveRowsCount,
                    _lastRowIdContainer
                    );
            }

            return
                r;
        }

        public void Dispose()
        {
            try
            {
                if (_stackIdContainer != null)
                {
                    _stackIdContainer.Dispose();
                }
            }
            catch (Exception excp)
            {
                _logger.LogHandledException(this.GetType(), "Ошибка утилизации контейнера стеков", excp);

            }
        }
    }
}