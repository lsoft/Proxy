using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace PerformanceTelemetry.Container.Saver.Item.SqlText
{
    public class SqlTextItemSaverFactory : IItemSaverFactory, IDisposable
    {
        public const int ClassNameMaxLength = 300;
        public const int MethodNameMaxLength = 300;
        public const int ExceptionMessageMaxLength = 300;

        //контейнер хешей стека не обязан быть потоко-защищенным, так как обращения к нему идут только из потока сохранения
        private readonly HashContainer _hashContainer;

        private readonly Action<string> _output;
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly string _tableName;

        //хешер для поиска соотв. стека
        private readonly MD5 _md5 = MD5.Create();

        //подключение к базе данных
        private readonly SqlConnection _connection;


        public SqlTextItemSaverFactory(
            Action<string> logger,
            string connectionString,
            string databaseName,
            string tableName
            )

        {
            //logger allowed to be null
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }

            _output = logger ?? Console.WriteLine;
            _hashContainer = new HashContainer();
            _connectionString = connectionString;
            _databaseName = databaseName;
            _tableName = tableName;

            _connection = new SqlConnection(_connectionString);
            _connection.Open();

            DoPreparation();
        }

        public IItemSaver CreateItemSaver()
        {
            var transaction = _connection.BeginTransaction(IsolationLevel.Snapshot);

            try
            {
                var r = new SqlTextItemSaver(
                    _hashContainer,
                    transaction,
                    _connection,
                    _md5,
                    _tableName
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
                    _output(excp.Message);
                    _output(excp.StackTrace);

                }
            }

            _md5.Dispose();
        }

        #region private code

        private void DoPreparation()
        {

            _output("PerformanceTelemetry Before preparation");

            var clause0 = PreparationClause0.Replace("{_DatabaseName_}", _databaseName);
            using (var cmd = new SqlCommand(clause0, _connection))
            {
                cmd.ExecuteNonQuery();
            }

            _output("PerformanceTelemetry Before clause 1");


            var prepared = false;

            var clause1 = PreparationClause1.Replace("{_DatabaseName_}", _databaseName);
            clause1 = clause1.Replace("{_TableName_}", _tableName);
            using (var cmd = new SqlCommand(clause1, _connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        prepared = (bool) reader[0];
                        break;
                    }
                }
            }

            _output(string.Format("PerformanceTelemetry Prepared {0}", prepared));

            if (!prepared)
            {
                _output("PerformanceTelemetry Before clause 2");

                var clause2 = PreparationClause2.Replace("{_DatabaseName_}", _databaseName);
                clause2 = clause2.Replace("{_ClassNameMaxLength_}", ClassNameMaxLength.ToString());
                clause2 = clause2.Replace("{_MethodNameMaxLength_}", MethodNameMaxLength.ToString());
                clause2 = clause2.Replace("{_TableName_}", _tableName);
                using (var cmd = new SqlCommand(clause2, _connection))
                {
                    cmd.ExecuteNonQuery();
                }

                _output("PerformanceTelemetry Before clause 3");

                var clause3 = PreparationClause3.Replace("{_DatabaseName_}", _databaseName);
                clause3 = clause3.Replace("{_TableName_}", _tableName);
                using (var cmd = new SqlCommand(clause3, _connection))
                {
                    cmd.ExecuteNonQuery();
                }

                _output("PerformanceTelemetry Before clause 4");

                var clause4 = PreparationClause4.Replace("{_DatabaseName_}", _databaseName);
                clause4 = clause4.Replace("{_TableName_}", _tableName);
                clause4 = clause4.Replace("{_ExceptionMessageMaxLength_}", ExceptionMessageMaxLength.ToString());
                using (var cmd = new SqlCommand(clause4, _connection))
                {
                    cmd.ExecuteNonQuery();
                }

                _output("PerformanceTelemetry Before clause 5");

                var clause5 = PreparationClause5.Replace("{_DatabaseName_}", _databaseName);
                clause5 = clause5.Replace("{_TableName_}", _tableName);
                using (var cmd = new SqlCommand(clause5, _connection))
                {
                    cmd.ExecuteNonQuery();
                }

                _output("PerformanceTelemetry Before clause 6");

                var clause6 = PreparationClause6.Replace("{_DatabaseName_}", _databaseName);
                clause6 = clause6.Replace("{_TableName_}", _tableName);
                using (var cmd = new SqlCommand(clause6, _connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            _output("PerformanceTelemetry Before DeleteOldClause");

            var deleteOldClause = DeleteOldClause.Replace("{_DatabaseName_}", _databaseName);
            deleteOldClause = deleteOldClause.Replace("{_TableName_}", _tableName);
            deleteOldClause = deleteOldClause.Replace("{_Barrier_}", DateTime.Now.AddMonths(-1).ToString("yyyyMMdd"));
            using (var cmd = new SqlCommand(deleteOldClause, _connection))
            {
                cmd.ExecuteNonQuery();
            }

            _output("PerformanceTelemetry Before ReadStackTableClause");

            var readStackClause = ReadStackTableClause.Replace("{_DatabaseName_}", _databaseName);
            readStackClause = readStackClause.Replace("{_TableName_}", _tableName);

            using (var cmd = new SqlCommand(readStackClause, _connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var guid = (Guid) reader["id"];

                        _hashContainer.Add(guid);
                    }
                }
            }

            _output("PerformanceTelemetry After preparation");
        }

        private const string PreparationClause0 = @"
ALTER DATABASE [{_DatabaseName_}]
SET ALLOW_SNAPSHOT_ISOLATION ON
";

        private const string PreparationClause1 = @"
use [{_DatabaseName_}]

declare @result bit;

set @result = 0

IF EXISTS (select 1 from sys.tables where name = '{_TableName_}')
BEGIN

    IF EXISTS (select 1 from sys.tables where name = '{_TableName_}Stack')
    BEGIN

        IF EXISTS (select 1 from sys.views where name = 'TelemetryView')
        BEGIN
            set @result = 1
        END

    END

END

select @result
";

        private const string PreparationClause2 = @"
use [{_DatabaseName_}]


CREATE TABLE [dbo].[{_TableName_}Stack]
(
    id uniqueidentifier NOT NULL,
    [class_name] varchar({_ClassNameMaxLength_}) NOT NULL,
    [method_name] varchar({_MethodNameMaxLength_}) NOT NULL,
    creation_stack varchar(MAX) NOT NULL
)  ON [PRIMARY]

";

        private const string PreparationClause3 = @"
ALTER TABLE [dbo].[{_TableName_}Stack] ADD CONSTRAINT
    PK_{_TableName_}Stack PRIMARY KEY CLUSTERED 
    (
    id
    ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
";

        private const string PreparationClause4 = @"
CREATE TABLE [dbo].[{_TableName_}]
(
    [id] [bigint] IDENTITY(1,1) NOT NULL,
    [date_commit] [datetime] NOT NULL,
    [id_parent] [bigint] NULL,
    [start_time] [datetime] NOT NULL,
    [exception_message] varchar({_ExceptionMessageMaxLength_}) NULL,
    [exception_stack] varchar(max) NULL,
    [time_interval] [float] NOT NULL,
    [id_stack] uniqueidentifier NOT NULL,
    [exception_full_text] varchar(max) NULL,
    CONSTRAINT [PK_{_TableName_}] PRIMARY KEY CLUSTERED 
    (
        [id] ASC,
        [date_commit] ASC
    )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
";

        private const string PreparationClause5 = @"
ALTER TABLE [dbo].[{_TableName_}] ALTER COLUMN [exception_message]
    ADD SPARSE

ALTER TABLE [dbo].[{_TableName_}] ALTER COLUMN [exception_stack]
    ADD SPARSE

ALTER TABLE [dbo].[{_TableName_}] ALTER COLUMN [exception_full_text]
    ADD SPARSE

ALTER TABLE [dbo].[{_TableName_}] ADD CONSTRAINT
    FK_{_TableName_}_{_TableName_}Stack FOREIGN KEY
    (
        id_stack
    ) REFERENCES [dbo].[{_TableName_}Stack]
    (
        id
    )   ON UPDATE  NO ACTION 
	    ON DELETE  NO ACTION 
";

        private const string PreparationClause6 = @"
CREATE VIEW [dbo].[TelemetryView]
WITH SCHEMABINDING 
AS
    SELECT
        tl.id,
        tl.id_parent,
        tl.start_time,
        tls.class_name,
        tls.method_name,
        tls.creation_stack, 
        tl.time_interval,
        tl.exception_message,
        tl.exception_stack,
        tl.exception_full_text,
        tl.date_commit
FROM [dbo].[{_TableName_}] tl
JOIN [dbo].[{_TableName_}Stack] tls ON tl.id_stack = tls.id

";

        private const string DeleteOldClause = @"
delete from [dbo].[{_TableName_}]
where [date_commit] < '{_Barrier_}'
";

        private const string ReadStackTableClause = @"
select
    id
    --,creation_stack
from [dbo].[{_TableName_}Stack]
";

        #endregion

    }
}