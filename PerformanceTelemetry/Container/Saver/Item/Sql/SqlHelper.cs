using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PerformanceTelemetry.Container.Saver.Item.Sql
{
    internal class SqlHelper
    {
        public const int ClassNameMaxLength = 300;
        public const int MethodNameMaxLength = 300;
        public const int ExceptionMessageMaxLength = 300;

        public static bool DoPreparation(
            SqlConnection connection,
            SqlTransaction transaction,
            string databaseName,
            string tableName,
            ITelemetryLogger logger,
            out long lastRowId
            )
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            //transaction allowed to be null
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            var exist = false;
            lastRowId = 0L;

            logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before preparation");

            var clause0 = PreparationClause0.Replace("{_DatabaseName_}", databaseName);
            using (var cmd = new System.Data.SqlClient.SqlCommand(clause0, connection, transaction))
            {
                cmd.ExecuteNonQuery();
            }

            logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before clause 1");


            var clause1 = PreparationClause1.Replace("{_DatabaseName_}", databaseName);
            clause1 = clause1.Replace("{_TableName_}", tableName);
            using (var cmd = new System.Data.SqlClient.SqlCommand(clause1, connection, transaction))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lastRowId = (long)reader[0];
                        exist = (bool)reader[1];
                        break;
                    }
                }
            }

            logger.LogMessage(typeof(SqlHelper), string.Format("PerformanceTelemetry exist = {0}", exist));
            logger.LogMessage(typeof(SqlHelper), string.Format("PerformanceTelemetry last row Id = {0}", lastRowId));

            if (!exist)
            {
                logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before clause 2");

                var clause2 = PreparationClause2.Replace("{_DatabaseName_}", databaseName);
                clause2 = clause2.Replace("{_ClassNameMaxLength_}", ClassNameMaxLength.ToString(CultureInfo.InvariantCulture));
                clause2 = clause2.Replace("{_MethodNameMaxLength_}", MethodNameMaxLength.ToString(CultureInfo.InvariantCulture));
                clause2 = clause2.Replace("{_TableName_}", tableName);
                using (var cmd = new System.Data.SqlClient.SqlCommand(clause2, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before clause 3");

                var clause3 = PreparationClause3.Replace("{_DatabaseName_}", databaseName);
                clause3 = clause3.Replace("{_TableName_}", tableName);
                using (var cmd = new System.Data.SqlClient.SqlCommand(clause3, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before clause 4");

                var clause4 = PreparationClause4.Replace("{_DatabaseName_}", databaseName);
                clause4 = clause4.Replace("{_TableName_}", tableName);
                clause4 = clause4.Replace("{_ExceptionMessageMaxLength_}", ExceptionMessageMaxLength.ToString(CultureInfo.InvariantCulture));
                using (var cmd = new System.Data.SqlClient.SqlCommand(clause4, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before clause 5");

                var clause5 = PreparationClause5.Replace("{_DatabaseName_}", databaseName);
                clause5 = clause5.Replace("{_TableName_}", tableName);
                using (var cmd = new System.Data.SqlClient.SqlCommand(clause5, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before clause 6");

                var clause6 = PreparationClause6.Replace("{_DatabaseName_}", databaseName);
                clause6 = clause6.Replace("{_TableName_}", tableName);
                using (var cmd = new System.Data.SqlClient.SqlCommand(clause6, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before clause 7");

                var clause7 = PreparationClause7.Replace("{_DatabaseName_}", databaseName);
                clause7 = clause7.Replace("{_TableName_}", tableName);
                using (var cmd = new System.Data.SqlClient.SqlCommand(clause7, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before clause 8");

                var clause8 = PreparationClause8.Replace("{_DatabaseName_}", databaseName);
                clause8 = clause8.Replace("{_TableName_}", tableName);
                using (var cmd = new System.Data.SqlClient.SqlCommand(clause8, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }

                logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before clause 9");

                var clause9 = PreparationClause9.Replace("{_DatabaseName_}", databaseName);
                clause9 = clause9.Replace("{_TableName_}", tableName);
                using (var cmd = new System.Data.SqlClient.SqlCommand(clause9, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Preparation Finished!");

            return
                exist;
        }


        public static void DoCleanup(
            SqlConnection connection,
            SqlTransaction transaction,
            string databaseName,
            string tableName,
            long aliveRowsCount,
            ITelemetryLogger logger
            )
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            //transaction allowed to be null
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (aliveRowsCount <= 0)
            {
                throw new ArgumentException("aliveRowsCount <= 0");
            }

            logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before Cleanup");

            var deleteOldClause = DeleteOldClause.Replace("{_DatabaseName_}", databaseName);
            deleteOldClause = deleteOldClause.Replace("{_TableName_}", tableName);
            deleteOldClause = deleteOldClause.Replace("{_Barrier_}", aliveRowsCount.ToString(CultureInfo.InvariantCulture));

            using (var cmd = new System.Data.SqlClient.SqlCommand(deleteOldClause, connection, transaction))
            {
                cmd.ExecuteNonQuery();
            }

            logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry After Cleanup");

        }

        public static StackIdContainer ReadStackTable(
            SqlConnection connection,
            SqlTransaction transaction,
            string databaseName,
            string tableName,
            ITelemetryLogger logger
            )
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            //transaction allowed to be null
            if (databaseName == null)
            {
                throw new ArgumentNullException("databaseName");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            var result = new StackIdContainer();

            logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry Before ReadStackTable");

            var readStackClause = ReadStackTableClause.Replace("{_DatabaseName_}", databaseName);
            readStackClause = readStackClause.Replace("{_TableName_}", tableName);

            using (var cmd = new System.Data.SqlClient.SqlCommand(readStackClause, connection, transaction))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = (int)reader["id"];
                        var key = (string)reader["key"];

                        result.ForceAdd(key, id);
                    }
                }
            }

            logger.LogMessage(typeof(SqlHelper), "PerformanceTelemetry After ReadStackTable");

            return
                result;
        }
        
        private const string PreparationClause0 = @"
ALTER DATABASE [{_DatabaseName_}]
SET ALLOW_SNAPSHOT_ISOLATION ON
";

        private const string PreparationClause1 = @"
use [{_DatabaseName_}]

declare @result bigint;
set @result = 0

declare @exist bit;
set @exist  = 0

IF EXISTS (select 1 from sys.tables where name = '{_TableName_}')
BEGIN

    IF EXISTS (select 1 from sys.tables where name = '{_TableName_}Stack')
    BEGIN

        select @result = isnull(max(id), 0) from [dbo].[{_TableName_}]
        set @exist = 1
    END

END

select @result, @exist 
";

        private const string PreparationClause2 = @"
use [{_DatabaseName_}]


CREATE TABLE [dbo].[{_TableName_}Stack]
(
    [id] int IDENTITY(1,1) NOT NULL,
    [class_name] varchar({_ClassNameMaxLength_}) NOT NULL,
    [method_name] varchar({_MethodNameMaxLength_}) NOT NULL,
    [creation_stack] varchar(MAX) NOT NULL,
    [key] varchar(MAX) NOT NULL,
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
    [id] [bigint] NOT NULL,
    [date_commit] [datetime] NOT NULL,
    [id_parent] [bigint] NULL,
    [start_time] [datetime] NOT NULL,
    [exception_message] varchar({_ExceptionMessageMaxLength_}) NULL,
    [exception_stack] varchar(max) NULL,
    [time_interval] [real] NOT NULL,
    [id_stack] int NOT NULL,
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

CREATE NONCLUSTERED INDEX [IX_{_TableName_}_id_parent] ON [dbo].[{_TableName_}]
(
    [id_parent] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [IX_{_TableName_}_time_interval] ON [dbo].[{_TableName_}]
(
    [time_interval] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

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

        private const string PreparationClause7 = @"
CREATE FUNCTION [dbo].[GetTelemetryTree]
(
	@root_id bigint = 0
)
RETURNS @ResultTable TABLE 
(
    [id] [bigint]  NOT NULL,
	[date_commit] [datetime] NOT NULL,
	[id_parent] [bigint] NULL,
	[start_time] [datetime] NOT NULL,
	[time_interval] [real] NOT NULL,
	[class_name] [varchar](300) NOT NULL,
	[method_name] [varchar](300) NOT NULL,
	[creation_stack] [varchar](max) NOT NULL,
	[exception_message] [varchar](300)  NULL,
	[exception_stack] [varchar](max)  NULL,
	[exception_full_text] [varchar](max)  NULL
)
--WITH
--	SCHEMABINDING
AS 
BEGIN

	declare @ids table (id bigint)

	insert into @ResultTable
	output inserted.id into @ids
	select
		tl.[id], 
		tl.[date_commit], 
		tl.[id_parent], 
		tl.[start_time], 
		tl.[time_interval],
		tls.[class_name],
		tls.[method_name],
		tls.[creation_stack],
		tl.[exception_message], 
		tl.[exception_stack],  
		tl.[exception_full_text]
	from [dbo].[{_TableName_}] tl
	JOIN [dbo].[{_TableName_}Stack] tls ON tl.id_stack = tls.id
	where
		tl.[id] = @root_id

	declare acursor cursor for select id from [dbo].[{_TableName_}] where id_parent = @root_id;

	open acursor;
	declare @current_id bigint;
	fetch acursor into @current_id;

	while @@FETCH_STATUS = 0
	begin

	insert into @ResultTable
	select
		[id], 
		[date_commit], 
		[id_parent], 
		[start_time], 
		[time_interval],
		[class_name],
		[method_name],
		[creation_stack],
		[exception_message], 
		[exception_stack],  
		[exception_full_text]
	from [dbo].[GetTelemetryTree](@current_id) 

	fetch acursor into @current_id;
	end

	close acursor;
	deallocate acursor;

	RETURN;
END

";

        private const string PreparationClause8 = @"
CREATE FUNCTION [dbo].[GetSlowestItems]
(
	@cnt int = 100
)
RETURNS TABLE 
WITH
	SCHEMABINDING
AS RETURN
	select top(@cnt)
		tl.id,
		tl.id_parent,
		tl.start_time,
		tl.time_interval,
		tls.class_name,
		tls.method_name,
		tl.exception_message
	from [dbo].[{_TableName_}] tl
	join [dbo].[{_TableName_}Stack] tls on tls.id = tl.id_stack
	order by
		tl.time_interval desc
";

        private const string PreparationClause9 = @"
CREATE FUNCTION [dbo].[GetTelemetryCountAndAvg]
(
	@cnt int = 100
)
RETURNS TABLE 
WITH
	SCHEMABINDING
AS RETURN
	select top(@cnt)
		tls.class_name,
		tls.method_name,
		i.cnt,
		i.avg_time,
		i.min_time,
		i.max_time
	from (
		select
			tl.id_stack,
			count(*) cnt,
			avg(tl.time_interval) avg_time,
			min(tl.time_interval) min_time,
			max(tl.time_interval) max_time
		from [dbo].[{_TableName_}] tl with(nolock)
		group by
			tl.id_stack
		) i
	join [dbo].[{_TableName_}Stack] tls on tls.id = i.id_stack
	order by
		cnt desc
";

        private const string DeleteOldClause = @"
delete from
	[dbo].[{_TableName_}]
where
	id < (
		select
			min(id) min_id
		from [dbo].[{_TableName_}] ) - {_Barrier_}
";

        private const string ReadStackTableClause = @"
select
    id,
    [key]
from [dbo].[{_TableName_}Stack]
";
    }
}
