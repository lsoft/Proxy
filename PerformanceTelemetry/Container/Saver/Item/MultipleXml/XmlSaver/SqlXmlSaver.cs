using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace PerformanceTelemetry.Container.Saver.Item.MultipleXml.XmlSaver
{
    public class SqlXmlSaver : IXmlSaver
    {
        private readonly object _locker = new object();

        private readonly string _tableName;

        private SqlConnection _connection;

        public SqlXmlSaver(
            string connectionString,
            string tableName
            )
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            _connection = new SqlConnection(connectionString);
            _tableName = tableName;

            _connection.Open();

            this.CheckForTable();

            this.ClearOldEntries();
        }

        #region Implementation of IXmlSaver

        public void Save(StringBuilder xmlString)
        {
            var clause0 = InsertClause.Replace("{_TableName_}", _tableName);
            var clause1 = clause0.Replace("{_Record_}", xmlString.ToString());

            lock (_locker)
            {
                using (var cmd = new SqlCommand(clause1, _connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        public void Dispose()
        {
            var connection = Interlocked.Exchange(ref this._connection, null);
            if (connection != null)
            {
                connection.Dispose();
            }
        }

        private void ClearOldEntries()
        {
            var clause0 = ClearOldEntriesClause.Replace("{_TableName_}", _tableName);
            var clause1 = clause0.Replace("{_Barrier_}", DateTime.Now.AddMonths(-1).ToString("yyyyMMdd"));

            using (var cmd = new SqlCommand(clause1, _connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void CheckForTable()
        {
            var clause = CreateTableClause.Replace("{_TableName_}", _tableName);

            using(var cmd = new SqlCommand(clause, _connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private const string ClearOldEntriesClause = @"
delete from [dbo].[{_TableName_}]
where [date_commit] < '{_Barrier_}';
";

        private const string InsertClause = @"
insert into [dbo].[{_TableName_}]
    ([date_commit], [record])
values
    (GETDATE(), '{_Record_}');
";

        private const string CreateTableClause = @"
IF NOT EXISTS (select 1 from sys.tables where name = '{_TableName_}')
BEGIN
execute sp_executesql @statement = N'
	CREATE TABLE [dbo].[{_TableName_}](
		[date_commit] [datetime] NOT NULL,
		[id] [int] IDENTITY(1,1) NOT NULL,
		[record] [xml] NOT NULL,
	 CONSTRAINT [PK_{_TableName_}] PRIMARY KEY CLUSTERED 
	(
		[date_commit] ASC,
		[id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
'

execute sp_executesql @statement = N'
CREATE PRIMARY XML INDEX [XML_{_TableName_}_record] ON [dbo].[{_TableName_}] 
(
	[record]
)WITH (PAD_INDEX  = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
'

execute sp_executesql @statement = N'
CREATE FUNCTION [dbo].[UrlDecode](@url varchar(max)) 
RETURNS varchar(max) 
AS 
BEGIN 
    DECLARE @count int, @c char(1), @cenc char(2), @i int, @urlReturn varchar(3072) 
    SET @count = Len(@url) 
    SET @i = 1 
    SET @urlReturn = '''' 
    WHILE (@i <= @count) 
     BEGIN 
        SET @c = substring(@url, @i, 1) 
        IF @c LIKE ''[!%]'' ESCAPE ''!'' 
         BEGIN 
            SET @cenc = substring(@url, @i + 1, 2) 
            SET @c = CHAR(CASE WHEN SUBSTRING(@cenc, 1, 1) LIKE ''[0-9]'' 
                                THEN CAST(SUBSTRING(@cenc, 1, 1) as int) 
                                ELSE CAST(ASCII(UPPER(SUBSTRING(@cenc, 1, 1)))-55 as int) 
                            END * 16 + 
                            CASE WHEN SUBSTRING(@cenc, 2, 1) LIKE ''[0-9]'' 
                                THEN CAST(SUBSTRING(@cenc, 2, 1) as int) 
                                ELSE CAST(ASCII(UPPER(SUBSTRING(@cenc, 2, 1)))-55 as int) 
                            END) 
            SET @urlReturn = @urlReturn + @c 
            SET @i = @i + 2 
         END 
        ELSE 
        IF @c = ''+''
            BEGIN
                SET @urlReturn = @urlReturn + '' ''
            END
        ELSE
            BEGIN 
                SET @urlReturn = @urlReturn + @c 
            END 
        SET @i = @i +1 
     END 
    RETURN @urlReturn 
END 
'

END
";
    }
}


/*

declare @methodName varchar(1000)
set @methodName = 'Operation1500'

select
	@methodName,
	avg(record.value('(/PerformanceRecord/TimeInterval)[1]', 'real')) average,
	max(record.value('(/PerformanceRecord/TimeInterval)[1]', 'real')) maximum,
	sum(record.value('(/PerformanceRecord/TimeInterval)[1]', 'real')) summary
from dbo.PERFORMANCE_TELEMETRY pt
where
	dbo.UrlDecode(record.value('(/PerformanceRecord/CreationStackBase64)[1]', 'varchar(max)')) like ('%' + @methodName + '%')

 
 
 
 
 
drop function GetPlain
GO
CREATE function GetPlain()
returns @results TABLE (date_commit datetime, id int, PerformanceRecordReduced xml) as
begin
	insert @results
	select
		T2.Loc.value('(./DateCommit)[1]', 'datetime') date_commit,
		T2.Loc.value('(./Id)[1]', 'int') id,
		T2.Loc.query('.') prr
	from (
		select CONVERT(xml, prList) sqlColumn
		from (
			select (
				select 
					pt.date_commit as 'DateCommit',
					pt.id as 'Id',
					--T2.Loc.query('.'),
					T2.Loc.query('./StartDate'),
					T2.Loc.query('./LogDate'),
					T2.Loc.query('./TimeInterval'),
					dbo.UrlDecode(T2.Loc.value('(./CreationStackBase64)[1]', 'varchar(max)')) as 'CreationStack',
					T2.Loc.query('./IsException')
				from dbo.PERFORMANCE_TELEMETRY pt
				CROSS APPLY record.nodes('//PerformanceRecord') as T2(Loc)
				for xml path('PerformanceRecordReduced' )
				) prList
			) a 
		) b
	CROSS APPLY b.sqlColumn.nodes('/PerformanceRecordReduced') as T2(Loc)

	
	RETURN;
end
GO

select * from dbo.GetPlain()

 */
