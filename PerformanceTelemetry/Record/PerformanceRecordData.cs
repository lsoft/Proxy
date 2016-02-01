using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PerformanceTelemetry.Record
{
    /// <summary>
    /// Контейнер результатов работы PerformanceRecord
    /// </summary>
    public class PerformanceRecordData : IPerformanceRecordData
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

        public PerformanceRecordData(
            string className,
            string methodName,
            DateTime startTime, 
            double timeInterval, 
            Exception exception, 
            string creationStack,
            List<IPerformanceRecordData> children)
        {
            if (className == null)
            {
                throw new ArgumentNullException("className");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            //exception allowed to be null
            if (creationStack == null)
            {
                throw new ArgumentNullException("creationStack");
            }
            //children allowed to be null

            ClassName = className;
            MethodName = methodName;
            StartTime = startTime;
            TimeInterval = timeInterval;
            Exception = exception;
            CreationStack = creationStack;
            Children = children.AsReadOnly();
        }
    }
}
