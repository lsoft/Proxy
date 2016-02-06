using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item.SqlText
{
    public class SqlTextItemSaver : IItemSaver
    {
        //после сохранения такого количества итемов пытаться удалять старье
        private const long BatchBetweenCleanups = 250000L;

        private readonly HashContainer _hashContainer;

        //транзакция к базе данных
        private readonly SqlTransaction _transaction;
        
        //подключение к базе данных
        private readonly SqlConnection _connection;

        //хешер для поиска соотв. стека
        private readonly MD5 _md5;
        private readonly string _databaseName;

        //корень имени таблиц для сохранения
        private readonly string _tableName;

        //индекс итема для очистки
        private static long _cleanupIndex;

        //заранее скомпилированные команды
        private readonly SqlCommand _insertItemCommand;
        private readonly SqlCommand _insertStackCommand;

        //признак, что класс утилизирован
        private bool _disposed = false;

        public SqlTextItemSaver(
            HashContainer hashContainer,
            SqlTransaction transaction,
            SqlConnection connection,
            MD5 md5,
            string databaseName,
            string tableName
            )
        {
            if (hashContainer == null)
            {
                throw new ArgumentNullException("hashContainer");
            }
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (md5 == null)
            {
                throw new ArgumentNullException("md5");
            }
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            _hashContainer = hashContainer;
            _transaction = transaction;
            _connection = connection;
            _md5 = md5;
            _databaseName = databaseName;
            _tableName = tableName;

            var insertStackClause = InsertStackClause.Replace(
                "{_TableName_}",
                _tableName
                );

            _insertStackCommand = new SqlCommand(insertStackClause, _connection, _transaction);
            _insertStackCommand.Parameters.Add("id", SqlDbType.UniqueIdentifier);
            _insertStackCommand.Parameters.Add("class_name", SqlDbType.VarChar, SqlTextItemSaverFactory.ClassNameMaxLength);
            _insertStackCommand.Parameters.Add("method_name", SqlDbType.VarChar, SqlTextItemSaverFactory.MethodNameMaxLength);
            _insertStackCommand.Parameters.Add("creation_stack", SqlDbType.VarChar, -1);
            _insertStackCommand.Prepare();

            var insertItemClause = InsertItemClause.Replace(
                "{_TableName_}",
                _tableName
                );

            _insertItemCommand = new SqlCommand(insertItemClause, _connection, _transaction);
            _insertItemCommand.Parameters.Add("id_parent", SqlDbType.BigInt);
            _insertItemCommand.Parameters.Add("start_time", SqlDbType.DateTime);
            _insertItemCommand.Parameters.Add("exception_message", SqlDbType.VarChar, SqlTextItemSaverFactory.ExceptionMessageMaxLength);
            _insertItemCommand.Parameters.Add("exception_stack", SqlDbType.VarChar, -1);
            _insertItemCommand.Parameters.Add("time_interval", SqlDbType.Float);
            _insertItemCommand.Parameters.Add("id_stack", SqlDbType.UniqueIdentifier);
            _insertItemCommand.Parameters.Add("exception_full_text", SqlDbType.VarChar, -1);
            _insertItemCommand.Prepare();
        }

        public void SaveItem(IPerformanceRecordData item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            //проверяем не настала ли пора удалять старье
            if (Interlocked.Increment(ref _cleanupIndex) == BatchBetweenCleanups)
            {
                //надо очищать
                SqlTextItemSaverFactory.DoCleanup(
                    _connection,
                    _transaction,
                    _databaseName,
                    _tableName
                    );

                Interlocked.Exchange(ref _cleanupIndex, 0L);
            }


            this.SaveItem(
                null,
                item
                );
        }

        public void Commit()
        {
            if (!_disposed)
            {
                _insertItemCommand.Dispose();
                _insertStackCommand.Dispose();

                _transaction.Commit();
                _transaction.Dispose();

                _disposed = true;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _insertItemCommand.Dispose();
                _insertStackCommand.Dispose();

                _transaction.Rollback();
                _transaction.Dispose();

                _disposed = true;
            }
        }

        #region private code

        private long SaveItem(
            long? parentId,
            IPerformanceRecordData item
            )
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            var result = 0L;

            //проверяем и вставляем стек, если необходимо

            var combined = string.Format("{0}.{1}", item.ClassName, item.MethodName);
            var combinedHash = _md5.ComputeHash(Encoding.UTF8.GetBytes(combined));
            var combinedGuid = new Guid(combinedHash);

            if (!_hashContainer.Contains(combinedGuid))
            {
                //такого стека нет, вставляем

                _insertStackCommand.Parameters["id"].Value = combinedGuid;
                _insertStackCommand.Parameters["class_name"].Value = CutOff(item.ClassName, SqlTextItemSaverFactory.ClassNameMaxLength);
                _insertStackCommand.Parameters["method_name"].Value = CutOff(item.MethodName, SqlTextItemSaverFactory.MethodNameMaxLength);
                _insertStackCommand.Parameters["creation_stack"].Value = item.CreationStack;

                _insertStackCommand.ExecuteNonQuery();

                _hashContainer.Add(combinedGuid);
            }


            //вставляем остальные данные
            var exceptionMessage = item.Exception != null ? (object) CutOff(item.Exception.Message, SqlTextItemSaverFactory.ExceptionMessageMaxLength) : null;
            var exceptionStack = item.Exception != null ? (object) item.Exception.StackTrace : null;
            var exceptionFullText = item.Exception != null ? (object) Exception2StringHelper.ToFullString(item.Exception) : null;

            _insertItemCommand.Parameters["id_parent"].Value = (object) parentId ?? DBNull.Value;
            _insertItemCommand.Parameters["start_time"].Value = item.StartTime;
            _insertItemCommand.Parameters["exception_message"].Value = exceptionMessage ?? DBNull.Value;
            _insertItemCommand.Parameters["exception_stack"].Value = exceptionStack ?? DBNull.Value;
            _insertItemCommand.Parameters["time_interval"].Value = item.TimeInterval;
            _insertItemCommand.Parameters["id_stack"].Value = combinedGuid;
            _insertItemCommand.Parameters["exception_full_text"].Value = exceptionFullText ?? DBNull.Value;

            result = (long) (decimal) _insertItemCommand.ExecuteScalar();

            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    SaveItem(result, child);
                }
            }

            return
                result;
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
    (  id,  class_name,  method_name,  creation_stack )
values
    ( @id, @class_name, @method_name, @creation_stack )
";

        private const string InsertItemClause = @"
insert into [dbo].[{_TableName_}]
    ( date_commit,  id_parent,  start_time,  exception_message,  exception_stack,  time_interval,  id_stack,  exception_full_text)
values
    (   GETDATE(), @id_parent, @start_time, @exception_message, @exception_stack, @time_interval, @id_stack, @exception_full_text);

select SCOPE_IDENTITY();
";

        #endregion

    }

}