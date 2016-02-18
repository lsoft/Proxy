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

        //подключение к базе данных
        private readonly SqlConnection _connection;


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

            _connection = new SqlConnection(_connectionString);
            _connection.Open();

            long lastRowId;
            SqlHelper.DoPreparation(
                _connection,
                null,
                _databaseName,
                _tableName,
                _logger,
                out lastRowId
                );

            SqlHelper.DoCleanup(
                _connection,
                null,
                _databaseName,
                _tableName,
                _aliveRowsCount,
                _logger
                );

            _stackIdContainer = SqlHelper.ReadStackTable(
                _connection,
                null,
                _databaseName,
                _tableName,
                _logger
                );

            _lastRowIdContainer = new LastRowIdContainer(lastRowId);
        }

        public IItemSaver CreateItemSaver()
        {
            var transaction = _connection.BeginTransaction(IsolationLevel.Snapshot);

            try
            {
                var r = new SqlCommandItemSaver(
                    _logger,
                    _stackIdContainer,
                    transaction,
                    _connection,
                    _databaseName,
                    _tableName,
                    _aliveRowsCount,
                    _lastRowIdContainer
                    );

                return
                    r;
            }
            catch
            {
                transaction.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (this._connection != null)
            {
                try
                {
                    this._connection.Dispose();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(this.GetType(), "Ошибка утилизации конекшена", excp);

                }
            }

            try
            {
                _stackIdContainer.Dispose();
            }
            catch (Exception excp)
            {
                _logger.LogHandledException(this.GetType(), "Ошибка утилизации хэш алгоритма", excp);

            }
        }
    }
}