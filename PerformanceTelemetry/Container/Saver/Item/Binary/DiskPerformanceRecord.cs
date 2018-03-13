using System;
using System.Collections.Generic;

namespace PerformanceTelemetry.Container.Saver.Item.Binary
{
    public class DiskPerformanceRecord
    {
        public string ClassName { get; }
        public string MethodName { get; }
        public DateTime StartTime { get; }
        public double TimeInterval { get; }

        public bool ExceptionExists { get; }
        public string ExceptionMessage { get; }
        public string ExceptionStackTrace { get; }
        public string FullException { get; }

        public string CreationStack { get; }

        public IReadOnlyList<DiskPerformanceRecord> Children { get; }

        public DiskPerformanceRecord(
            string className,
            string methodName,
            DateTime startTime,
            double timeInterval,
            bool exceptionExists,
            string exceptionMessage,
            string exceptionStackTrace,
            string fullException,
            string creationStack,
            List<DiskPerformanceRecord> children
            )
        {
            if (className == null)
            {
                throw new ArgumentNullException(nameof(className));
            }
            if (methodName == null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }
            if (creationStack == null)
            {
                throw new ArgumentNullException(nameof(creationStack));
            }
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }
            //exception related arguments allowed to be null

            ClassName = className;
            MethodName = methodName;
            StartTime = startTime;
            TimeInterval = timeInterval;
            ExceptionExists = exceptionExists;
            ExceptionMessage = exceptionMessage;
            ExceptionStackTrace = exceptionStackTrace;
            FullException = fullException;
            CreationStack = creationStack;
            Children = children;
        }

    }

}
