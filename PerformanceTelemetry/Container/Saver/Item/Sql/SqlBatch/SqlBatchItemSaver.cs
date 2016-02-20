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
        //����� ���������� ������ ���������� ������ �������� ������� ������
        private const long ItemCountBetweenCleanups = 25000L;

        private readonly ITelemetryLogger _logger;

        //��������� ����� ����� �� ������ ���� ������-����������, ��� ��� ��������� � ���� ���� ������ �� ������ ����������
        private readonly StackIdContainer _stackIdContainer;

        //������������� ��������� ������������ ������ � ���
        private readonly LastRowIdContainer _lastRowIdContainer;

        //���������� � ���� ������
        private readonly SqlTransaction _transaction;
        
        //����������� � ���� ������
        private readonly SqlConnection _connection;

        private readonly string _databaseName;

        //������ ����� ������ ��� ����������
        private readonly string _tableName;

        //������������ ���������� �����, ������� ����� ����������� � �������
        private readonly long _aliveRowsCount;

        //������� ������ ���������� ����� ��������� �������
        private static long _processedItemCountSinceCleanup;

        //������� �������
        private readonly DataTable _targetTable;

        //����������
        private readonly SqlBulkCopy _copier;

        //������� ���������������� �������
        private readonly System.Data.SqlClient.SqlCommand _insertStackCommand;

        //�������, ��� ����� ������������
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
                        _logger.LogHandledException(this.GetType(), "������ �������� ��������� � ������������", excp);

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
                        _logger.LogHandledException(this.GetType(), "������ ���������� ��������� � ������������", excp);

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
            
            //������� ����������
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

            //��������� �� ������� �� ���� ������� ������
            CheckAndPerformCleanupIfNecessary();

            //����������� �����
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

            //��������� �����
            _copier.WriteToServer(rows.ToArray());

            //����������� ������� �����������
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
                        "������ _insertStackCommand.Dispose()",
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
                        "������ _targetTable.Dispose()",
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
                        "������ _copier.Close()",
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
                        "������ _transaction.Commit()",
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
                        "������ _transaction.Dispose()",
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
                        _logger.LogHandledException(this.GetType(), "������ �������� ���������", excp);

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
                        _logger.LogHandledException(this.GetType(), "������ ���������� ���������", excp);

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
                        "������ _insertStackCommand.Dispose()",
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
                        "������ _targetTable.Dispose()",
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
                        "������ _copier.Close()",
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
                        "������ _transaction.Rollback()",
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
                        "������ _transaction.Dispose()",
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
                        _logger.LogHandledException(this.GetType(), "������ �������� ���������", excp);

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
                        _logger.LogHandledException(this.GetType(), "������ ���������� ���������", excp);

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
                //���� �������
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

            //��������� ����, ���� ����������
            var stackId = InsertNewStackIfNecessary(item);

            var exceptionMessage = item.Exception != null ? (object)CutOff(item.Exception.Message, SqlHelper.ExceptionMessageMaxLength) : null;
            var exceptionStack = item.Exception != null ? (object)item.Exception.StackTrace : null;
            var exceptionFullText = item.Exception != null ? (object)Exception2StringHelper.ToFullString(item.Exception) : null;

            var itemId = _lastRowIdContainer.GetIdForNewRow();

            //!!! ������� ������� �������, � �� ���������
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
                    //������ ����� ���, ���������

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