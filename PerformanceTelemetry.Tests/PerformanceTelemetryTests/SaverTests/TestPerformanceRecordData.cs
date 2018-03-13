using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using PerformanceTelemetry.Container.Saver.Item.Binary;
using PerformanceTelemetry.IterateHelper;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Tests.PerformanceTelemetryTests.SaverTests
{
    internal class TestPerformanceRecordData : IPerformanceRecordData
    {
        private readonly List<TestPerformanceRecordData> _children;

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

        public List<IPerformanceRecordData> GetChildren()
        {
            return
                new List<IPerformanceRecordData>(_children); //clone the collection
        }

        public TestPerformanceRecordData(string className, string methodName, DateTime startTime, double timeInterval, string creationStack)
        {
            ClassName = className;
            MethodName = methodName;
            StartTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second); //внутри СБД время округлится, если будут миллисекунды; и тест провалится!
            TimeInterval = timeInterval;
            CreationStack = creationStack;

            _children = new List<TestPerformanceRecordData>();
        }

        public TestPerformanceRecordData(string className, string methodName, DateTime startTime, double timeInterval, string creationStack, Exception exception)
        {
            ClassName = className;
            MethodName = methodName;
            StartTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second); //внутри СБД время округлится, если будут миллисекунды; и тест провалится!
            TimeInterval = timeInterval;
            CreationStack = creationStack;
            Exception = exception;

            _children = new List<TestPerformanceRecordData>();
        }

        public TestPerformanceRecordData(string className, string methodName, DateTime startTime, double timeInterval, string creationStack, params TestPerformanceRecordData[] children)
        {
            ClassName = className;
            MethodName = methodName;
            StartTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second); //внутри СБД время округлится, если будут миллисекунды; и тест провалится!
            TimeInterval = timeInterval;
            CreationStack = creationStack;
            _children = children.ToList();
        }

        public TestPerformanceRecordData(string className, string methodName, DateTime startTime, double timeInterval, string creationStack,  Exception exception, params TestPerformanceRecordData[] children)
        {
            ClassName = className;
            MethodName = methodName;
            StartTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, startTime.Second); //внутри СБД время округлится, если будут миллисекунды; и тест провалится!
            TimeInterval = timeInterval;
            CreationStack = creationStack;
            Exception = exception;
            _children = children.ToList();
        }

        internal bool CheckEqualityFor(
            DiskPerformanceRecord record
            )
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if(string.Compare(this.ClassName, record.ClassName, StringComparison.InvariantCulture) != 0)
            {
                return false;
            }

            if (string.Compare(this.MethodName, record.MethodName, StringComparison.InvariantCulture) != 0)
            {
                return false;
            }

            if (this.StartTime.Ticks != record.StartTime.Ticks)
            {
                return false;
            }

            if (Math.Abs(this.TimeInterval - record.TimeInterval) >= double.Epsilon)
            {
                return false;
            }

            if (string.Compare(this.CreationStack, record.CreationStack, StringComparison.InvariantCulture) != 0)
            {
                return false;
            }

            if (this.Exception == null && record.ExceptionExists)
            {
                return false;
            }

            if (this.Exception != null && !record.ExceptionExists)
            {
                return false;
            }

            if (this.Exception == null && !record.ExceptionExists)
            {
                //all fields are equals
                //no more fields to compare because exception does not present

                return true; 
            }

            var message = this.Exception.Message ?? string.Empty;

            if (string.Compare(message, record.ExceptionMessage, StringComparison.InvariantCulture) != 0)
            {
                return false;
            }

            var stackTrace = this.Exception.StackTrace ?? string.Empty;

            if (string.Compare(stackTrace, record.ExceptionStackTrace, StringComparison.InvariantCulture) != 0)
            {
                return false;
            }

            var fullException = Exception2StringHelper.ToFullString(this.Exception) ?? string.Empty;

            if (string.Compare(fullException, record.FullException, StringComparison.InvariantCulture) != 0)
            {
                return false;
            }

            if (this._children.Count != record.Children.Count)
            {
                return false;
            }
            
            foreach (var pair in this._children.ZipEqualLength(record.Children))
            {
                var eq = pair.Value1.CheckEqualityFor(pair.Value2);

                if (!eq)
                {
                    return false;
                }
            }


            //all fields are equals

            return
                true;
        }

#if SQL_SERVER_EXISTS
        internal bool CheckEqualityFor(
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
            var timeInterval = (float)reader["time_interval"];
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
#endif
    }
}