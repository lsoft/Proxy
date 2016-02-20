using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item.Sql.SqlCommand
{
    public class SqlCommandItemSaver : IItemSaver
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

        //����� ��� ������ �����. �����
        private readonly string _databaseName;

        //������ ����� ������ ��� ����������
        private readonly string _tableName;

        //������������ ���������� �����, ������� ����� ����������� � �������
        private readonly long _aliveRowsCount;

        //������� ������ ���������� ����� ��������� �������
        private static long _processedItemCountSinceCleanup;

        //������� ���������������� �������
        private readonly System.Data.SqlClient.SqlCommand _insertItemCommand;
        private readonly System.Data.SqlClient.SqlCommand _insertStackCommand;

        //�������, ��� ����� ������������
        private bool _disposed = false;

        internal SqlCommandItemSaver(
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

            var insertItemClause = InsertItemClause.Replace(
                "{_TableName_}",
                _tableName
                );

            _insertItemCommand = new System.Data.SqlClient.SqlCommand(insertItemClause, _connection, _transaction);
            _insertItemCommand.Parameters.Add("id", SqlDbType.BigInt);
            _insertItemCommand.Parameters.Add("id_parent", SqlDbType.BigInt);
            _insertItemCommand.Parameters.Add("start_time", SqlDbType.DateTime);
            _insertItemCommand.Parameters.Add("exception_message", SqlDbType.VarChar, SqlHelper.ExceptionMessageMaxLength);
            _insertItemCommand.Parameters.Add("exception_stack", SqlDbType.VarChar, -1);
            _insertItemCommand.Parameters.Add("time_interval", SqlDbType.Float);
            _insertItemCommand.Parameters.Add("id_stack", SqlDbType.Int);
            _insertItemCommand.Parameters.Add("exception_full_text", SqlDbType.VarChar, -1);
            _insertItemCommand.Prepare();
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

            //��������� �����
            for (var cc = 0; cc < itemCount; cc++)
            {
                var item = items[cc];

                if (item == null)
                {
                    throw new InvalidOperationException("item");
                }

                this.SaveItem(
                    null,
                    item
                    );

                Interlocked.Increment(ref _processedItemCountSinceCleanup);
            }
        }

        public void Commit()
        {
            if (!_disposed)
            {
                try
                {
                    _insertItemCommand.Dispose();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "������ _insertItemCommand.Dispose()",
                        excp
                        );
                }

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
                    _insertItemCommand.Dispose();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "������ _insertItemCommand.Dispose()",
                        excp
                        );
                }

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

        private long SaveItem(
            long? parentId,
            IPerformanceRecordData item
            )
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            //��������� � ��������� ����, ���� ����������

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

            var itemId = _lastRowIdContainer.GetIdForNewRow();

            //��������� ��������� ������
            var exceptionMessage = item.Exception != null ? (object)CutOff(item.Exception.Message, SqlHelper.ExceptionMessageMaxLength) : null;
            var exceptionStack = item.Exception != null ? (object) item.Exception.StackTrace : null;
            var exceptionFullText = item.Exception != null ? (object) Exception2StringHelper.ToFullString(item.Exception) : null;

            _insertItemCommand.Parameters["id"].Value = itemId;
            _insertItemCommand.Parameters["id_parent"].Value = (object)parentId ?? DBNull.Value;
            _insertItemCommand.Parameters["start_time"].Value = item.StartTime;
            _insertItemCommand.Parameters["exception_message"].Value = exceptionMessage ?? DBNull.Value;
            _insertItemCommand.Parameters["exception_stack"].Value = exceptionStack ?? DBNull.Value;
            _insertItemCommand.Parameters["time_interval"].Value = item.TimeInterval;
            _insertItemCommand.Parameters["id_stack"].Value = stackId;
            _insertItemCommand.Parameters["exception_full_text"].Value = exceptionFullText ?? DBNull.Value;

            _insertItemCommand.ExecuteNonQuery();

            var children = item.GetChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    SaveItem(itemId, child);
                }
            }

            return
                itemId;
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

        private const string InsertItemClause = @"
insert into [dbo].[{_TableName_}]
    ( date_commit,  id, id_parent,  start_time,  exception_message,  exception_stack,  time_interval,  id_stack,  exception_full_text)
values
    (   GETDATE(), @id, @id_parent, @start_time, @exception_message, @exception_stack, @time_interval, @id_stack, @exception_full_text);

--select SCOPE_IDENTITY();
";

        #endregion

    }

}