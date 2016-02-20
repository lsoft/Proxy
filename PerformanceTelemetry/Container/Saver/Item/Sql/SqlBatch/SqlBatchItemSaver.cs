using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item.Sql.SqlBatch
{
    public class SqlBatchItemSaver : IItemSaver
    {
        //после сохранения такого количества батчей пытаться удалять старье
        private const long ItemCountBetweenCleanups = 25000L;

        private readonly ITelemetryLogger _logger;

        //контейнер хешей стека не обязан быть потоко-защищенным, так как обращения к нему идут только из потока сохранения
        private readonly StackIdContainer _stackIdContainer;

        //идентификатор последней существующей строки в СБД
        private readonly LastRowIdContainer _lastRowIdContainer;

        //транзакция к базе данных
        private readonly SqlTransaction _transaction;
        
        //подключение к базе данных
        private readonly SqlConnection _connection;

        private readonly string _databaseName;

        //корень имени таблиц для сохранения
        private readonly string _tableName;

        //максимальное количество строк, которое может сохраняться в таблице
        private readonly long _aliveRowsCount;

        //сколько итемов обработано после последней очистки
        private static long _processedItemCountSinceCleanup;

        //целевая таблица
        private readonly DataTable _targetTable;

        //копировщик
        private readonly SqlBulkCopy _copier;

        //заранее скомпилированные команды
        private readonly System.Data.SqlClient.SqlCommand _insertStackCommand;

        //признак, что класс утилизирован
        private bool _disposed = false;

        internal SqlBatchItemSaver(
            ITelemetryLogger logger,
            StackIdContainer stackIdContainer,
            string connectionString,
            string databaseName,
            string tableName,
            long aliveRowsCount,
            LastRowIdContainer lastRowIdContainer
            )
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            if (stackIdContainer == null)
            {
                throw new ArgumentNullException("stackIdContainer");
            }
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (lastRowIdContainer == null)
            {
                throw new ArgumentNullException("lastRowIdContainer");
            }

            if (aliveRowsCount <= 0)
            {
                throw new ArgumentException("aliveRowsCount <= 0");
            }

            _logger = logger;
            _stackIdContainer = stackIdContainer;
            _databaseName = databaseName;
            _tableName = tableName;
            _aliveRowsCount = aliveRowsCount;
            _lastRowIdContainer = lastRowIdContainer;

            var connection = new SqlConnection(connectionString);
            connection.Open();
            try
            {
                var transaction = connection.BeginTransaction(IsolationLevel.Snapshot);

                _connection = connection;
                _transaction = transaction;
            }
            catch
            {
                #region close connection

                if (this._connection != null)
                {
                    try
                    {
                        this._connection.Close();
                    }
                    catch (Exception excp)
                    {
                        _logger.LogHandledException(this.GetType(), "Ошибка закрытия конекшена в конструкторе", excp);

                    }
                }

                if (this._connection != null)
                {
                    try
                    {
                        this._connection.Dispose();
                    }
                    catch (Exception excp)
                    {
                        _logger.LogHandledException(this.GetType(), "Ошибка утилизации конекшена в конструкторе", excp);

                    }
                }

                #endregion

                throw;
            }

            var insertStackClause = InsertStackClause.Replace(
                "{_TableName_}",
                _tableName
                );

            _insertStackCommand = new System.Data.SqlClient.SqlCommand(insertStackClause, _connection, _transaction);
            _insertStackCommand.Parameters.Add("class_name", SqlDbType.VarChar, SqlHelper.ClassNameMaxLength);
            _insertStackCommand.Parameters.Add("method_name", SqlDbType.VarChar, SqlHelper.MethodNameMaxLength);
            _insertStackCommand.Parameters.Add("creation_stack", SqlDbType.VarChar, -1);
            _insertStackCommand.Parameters.Add("key", SqlDbType.VarChar, -1);
            _insertStackCommand.Prepare();

            _targetTable = new DataTable();
            _targetTable.Columns.Add("id", typeof(long));
            _targetTable.Columns.Add("date_commit", typeof(DateTime));
            _targetTable.Columns.Add("id_parent", typeof(long));
            _targetTable.Columns.Add("start_time", typeof(DateTime));
            _targetTable.Columns.Add("exception_message", typeof(string));
            _targetTable.Columns.Add("exception_stack", typeof(string));
            _targetTable.Columns.Add("time_interval", typeof(float));
            _targetTable.Columns.Add("id_stack", typeof(int));
            _targetTable.Columns.Add("exception_full_text", typeof(string));
            
            //создаем копировщик
            _copier = new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.TableLock
                //| SqlBulkCopyOptions.FireTriggers
                //| SqlBulkCopyOptions.UseInternalTransaction
                ,
                _transaction
                );
            _copier.DestinationTableName = tableName;
        }

        public void SaveItems(
            IPerformanceRecordData[] items,
            int itemCount
            )
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            //проверяем не настала ли пора удалять старье
            CheckAndPerformCleanupIfNecessary();

            //преобразуем итемы
            var rows = new List<DataRow>(itemCount);
            for (var cc = 0; cc < itemCount; cc++)
            {
                var item = items[cc];

                ConvertItem(
                    rows,
                    null,
                    item
                    );
            }

            //сохраняем итемы
            _copier.WriteToServer(rows.ToArray());

            //инкрементим сколько скопировали
            Interlocked.Add(ref _processedItemCountSinceCleanup, itemCount);
        }

        public void Commit()
        {
            if (!_disposed)
            {
                try
                {
                    _insertStackCommand.Dispose();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _insertStackCommand.Dispose()",
                        excp
                        );
                }

                try
                {
                    _targetTable.Dispose();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _targetTable.Dispose()",
                        excp
                        );
                }

                try
                {
                    _copier.Close();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _copier.Close()",
                        excp
                        );
                }

                try
                {
                    _transaction.Commit();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _transaction.Commit()",
                        excp
                        );
                }

                try
                {
                    _transaction.Dispose();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _transaction.Dispose()",
                        excp
                        );
                }

                if (this._connection != null)
                {
                    try
                    {
                        this._connection.Close();
                    }
                    catch (Exception excp)
                    {
                        _logger.LogHandledException(this.GetType(), "Ошибка закрытия конекшена", excp);

                    }
                }

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

                _disposed = true;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _insertStackCommand.Dispose();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _insertStackCommand.Dispose()",
                        excp
                        );
                }

                try
                {
                    _targetTable.Dispose();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _targetTable.Dispose()",
                        excp
                        );
                }

                try
                {
                    _copier.Close();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _copier.Close()",
                        excp
                        );
                }

                try
                {
                    _transaction.Rollback();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _transaction.Rollback()",
                        excp
                        );
                }

                try
                {
                    _transaction.Dispose();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _transaction.Dispose()",
                        excp
                        );
                }

                if (this._connection != null)
                {
                    try
                    {
                        this._connection.Close();
                    }
                    catch (Exception excp)
                    {
                        _logger.LogHandledException(this.GetType(), "Ошибка закрытия конекшена", excp);

                    }
                }

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

                _disposed = true;
            }
        }

        #region private code

        private void CheckAndPerformCleanupIfNecessary()
        {
            if (Interlocked.Read(ref _processedItemCountSinceCleanup) >= ItemCountBetweenCleanups)
            {
                //надо очищать
                SqlHelper.DoCleanup(
                    _connection,
                    _transaction,
                    _databaseName,
                    _tableName,
                    _aliveRowsCount,
                    _logger
                    );

                Interlocked.Exchange(ref _processedItemCountSinceCleanup, 0L);
            }
        }

        private void ConvertItem(
            List<DataRow> rows,
            long? parentId,
            IPerformanceRecordData item
            )
        {
            if (rows == null)
            {
                throw new ArgumentNullException("rows");
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            //вставляем стек, если необходимо
            var stackId = InsertNewStackIfNecessary(item);

            var exceptionMessage = item.Exception != null ? (object)CutOff(item.Exception.Message, SqlHelper.ExceptionMessageMaxLength) : null;
            var exceptionStack = item.Exception != null ? (object)item.Exception.StackTrace : null;
            var exceptionFullText = item.Exception != null ? (object)Exception2StringHelper.ToFullString(item.Exception) : null;

            var itemId = _lastRowIdContainer.GetIdForNewRow();

            //!!! сделать индексы интовые, а не строковые
            var row = _targetTable.NewRow();
            row["id"] = itemId;
            row["date_commit"] = DateTime.Now;
            row["id_parent"] = (object)parentId ?? DBNull.Value;
            row["start_time"] = item.StartTime;
            row["exception_message"] = exceptionMessage;
            row["exception_stack"] = exceptionStack;
            row["time_interval"] = item.TimeInterval;
            row["id_stack"] = stackId;
            row["exception_full_text"] = exceptionFullText;

            rows.Add(row);

            var children = item.GetChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    ConvertItem(
                        rows,
                        itemId,
                        child
                        );
                }
            }

        }

        private int InsertNewStackIfNecessary(
            IPerformanceRecordData item
            )
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            var stackId = _stackIdContainer.AddIfNecessaryAndReturnId(
                item,
                (itemKey, workItem) =>
                {
                    //такого стека нет, вставляем

                    _insertStackCommand.Parameters["class_name"].Value = CutOff(workItem.ClassName, SqlHelper.ClassNameMaxLength);
                    _insertStackCommand.Parameters["method_name"].Value = CutOff(workItem.MethodName, SqlHelper.MethodNameMaxLength);
                    _insertStackCommand.Parameters["creation_stack"].Value = workItem.CreationStack;
                    _insertStackCommand.Parameters["key"].Value = itemKey;

                    var workStackId = (int)_insertStackCommand.ExecuteScalar();

                    return
                        workStackId;
                }
                );

            return
                stackId;
        }

        private string CutOff(string message, int maxLength)
        {
            //message allowed to be null
            if (maxLength <= 3)
            {
                throw new ArgumentException("maxLength");
            }

            if (message == null)
            {
                return
                    null;
            }

            if (message.Length < maxLength)
            {
                return
                    message;
            }

            return
                string.Format(
                    "{0}{1}",
                    message.Substring(0, maxLength - 3),
                    "..."
                    );
        }

        private const string InsertStackClause = @"
insert into [dbo].[{_TableName_}Stack]
    (  class_name,  method_name,  creation_stack, [key] )
output inserted.$identity 
values
    ( @class_name, @method_name, @creation_stack, @key )
";

        #endregion

    }
}