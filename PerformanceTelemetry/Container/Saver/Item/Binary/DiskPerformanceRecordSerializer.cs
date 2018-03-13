using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item.Binary
{
    public class DiskPerformanceRecordSerializer
    {
        public static Encoding ItemEncoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        public static ExtendedBinaryWriter CreateWriter(
            Stream target
            )
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if(!target.CanWrite)
            {
                throw new ArgumentException("!target.CanWrite", nameof(target));
            }

            var result = new ExtendedBinaryWriter(target, ItemEncoding, true);

            return
                result;
        }

        public static BinaryReader CreateReader(
            Stream source
            )
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!source.CanRead)
            {
                throw new ArgumentException("!source.CanRead", nameof(source));
            }

            var result = new BinaryReader(source, ItemEncoding, true);

            return
                result;
        }

        public static long WriteOne(
            IPerformanceRecordData item,
            ExtendedBinaryWriter target
            )
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var size = 0L;

            size += target.Write(item.ClassName);
            size += target.Write(item.MethodName);
            size += target.Write(item.StartTime.Ticks);
            size += target.Write(item.TimeInterval);

            size += target.Write(item.Exception != null ? (int)1 : (int)0);
            if (item.Exception != null)
            {
                size += target.Write(item.Exception.Message ?? string.Empty);
                size += target.Write(item.Exception.StackTrace ?? string.Empty);
                size += target.Write(Exception2StringHelper.ToFullString(item.Exception) ?? string.Empty);
            }

            size += target.Write(item.CreationStack);

            var children = item.GetChildren();

            size += target.Write(children?.Count ?? 0);
            foreach (var child in children)
            {
                size += WriteOne(child, target);
            }

            return size;
        }

        public static DiskPerformanceRecord ReadOne(
            BinaryReader source
            )
        {
            var className = source.ReadString();
            var methodName = source.ReadString();
            var startTime = new DateTime(source.ReadInt64());
            var timeInterval = source.ReadDouble();

            var exceptionExists = source.ReadInt32() > 0;
            var exceptionMessage = string.Empty;
            var exceptionStackTrace = string.Empty;
            var exceptionFullException = string.Empty;
            if (exceptionExists)
            {
                exceptionMessage = source.ReadString();
                exceptionStackTrace = source.ReadString();
                exceptionFullException = source.ReadString();
            }

            var creationStack = source.ReadString();

            var childrenCount = source.ReadInt32();

            var children = new List<DiskPerformanceRecord>();
            for (var cc = 0; cc < childrenCount; cc++)
            {
                var child = ReadOne(source);
                children.Add(child);
            }

            var result = new DiskPerformanceRecord(
                className,
                methodName,
                startTime,
                timeInterval,
                exceptionExists,
                exceptionMessage,
                exceptionStackTrace,
                exceptionFullException,
                creationStack,
                children
                );

            return
                result;
        }

    }

}
