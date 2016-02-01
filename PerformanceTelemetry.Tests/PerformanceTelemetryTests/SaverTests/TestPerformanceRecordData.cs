using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Tests.PerformanceTelemetryTests.SaverTests
{
    internal class TestPerformanceRecordData : IPerformanceRecordData
    {
        public string ClassName
        {
            get;
            private set;
        }

        public string MethodName
        {
            get;
            private set;
        }

        public DateTime StartTime
        {
            get;
            private set;
        }

        public double TimeInterval
        {
            get;
            private set;
        }

        public Exception Exception
        {
            get;
            private set;
        }

        public string CreationStack
        {
            get;
            private set;
        }

        public ReadOnlyCollection<IPerformanceRecordData> Children
        {
            get;
            private set;
        }

        public TestPerformanceRecordData(string className, string methodName, DateTime startTime, double timeInterval, string creationStack)
        {
            ClassName = className;
            MethodName = methodName;
            StartTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second); //внутри СБД время округлится, если будут миллисекунды; и тест провалится!
            TimeInterval = timeInterval;
            CreationStack = creationStack;

            Children = new ReadOnlyCollection<IPerformanceRecordData>(new List<IPerformanceRecordData>());
        }

        public TestPerformanceRecordData(string className, string methodName, DateTime startTime, double timeInterval, string creationStack, Exception exception)
        {
            ClassName = className;
            MethodName = methodName;
            StartTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second); //внутри СБД время округлится, если будут миллисекунды; и тест провалится!
            TimeInterval = timeInterval;
            CreationStack = creationStack;
            Exception = exception;

            Children = new ReadOnlyCollection<IPerformanceRecordData>(new List<IPerformanceRecordData>());
        }

        public TestPerformanceRecordData(string className, string methodName, DateTime startTime, double timeInterval, string creationStack, List<IPerformanceRecordData> children)
        {
            ClassName = className;
            MethodName = methodName;
            StartTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second); //внутри СБД время округлится, если будут миллисекунды; и тест провалится!
            TimeInterval = timeInterval;
            CreationStack = creationStack;
            Children = children.AsReadOnly();
        }

        public TestPerformanceRecordData(string className, string methodName, DateTime startTime, double timeInterval, string creationStack,  Exception exception, List<IPerformanceRecordData> children)
        {
            ClassName = className;
            MethodName = methodName;
            StartTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second); //внутри СБД время округлится, если будут миллисекунды; и тест провалится!
            TimeInterval = timeInterval;
            CreationStack = creationStack;
            Exception = exception;
            Children = children.AsReadOnly();
        }

        public bool CheckEqualityFor(
            long correctParentId,
            SqlDataReader reader
            )
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            var result = true;

            var id = (long) reader["id"];
            var parentId = reader["id_parent"] is DBNull ? 0L : (long)reader["id_parent"];
            var className = (string)reader["class_name"];
            var methodName = (string)reader["method_name"];
            var startTime = (DateTime)reader["start_time"];
            var exceptionMessage = reader["exception_message"] is DBNull ? null : (string)reader["exception_message"];
            var exceptionStack = reader["exception_stack"] is DBNull ? null : (string)reader["exception_stack"];
            var timeInterval = (double)reader["time_interval"];
            var creationStack = (string)reader["creation_stack"];
            var exceptionFullText = reader["exception_full_text"] is DBNull ? null : (string)reader["exception_full_text"];

            result &= correctParentId == parentId;
            result &= className == this.ClassName;
            result &= methodName == this.MethodName;
            result &= startTime == this.StartTime;
            result &= (exceptionMessage == null && this.Exception == null) || (exceptionMessage != null && this.Exception != null && exceptionMessage == this.Exception.Message);
            result &= (exceptionStack == null && (this.Exception == null || this.Exception.StackTrace == null)) || (exceptionStack != null && this.Exception != null && exceptionStack == this.Exception.StackTrace);
            result &= Math.Abs(timeInterval - this.TimeInterval) < double.Epsilon;
            result &= creationStack == this.CreationStack;
            result &= (exceptionFullText == null && this.Exception == null) || (exceptionFullText != null && this.Exception != null && exceptionFullText == Exception2StringHelper.ToFullString(this.Exception));

            return
                result;
        }
    }
}