using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver.Item.PlainText
{
    public class PlainTextItemSaver : IItemSaver
    {
        private readonly object _locker;
        private readonly Action<string, object[]> _output;

        public PlainTextItemSaver(
            object locker,
            Action<string, object[]> output
            )
        {
            if (locker == null)
            {
                throw new ArgumentNullException("locker");
            }
            //output allowed to be null

            _locker = locker;
            _output = output ?? Console.WriteLine;
        }

        public void SaveItem(IPerformanceRecordData item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            lock (_locker)
            {
                WriteItem(0, item);
            }
        }

        public void Commit()
        {
            //nothing to do
        }

        public void Dispose()
        {
            //nothing to do
        }

        #region private code

        private void WriteItem(int level, IPerformanceRecordData item)
        {
            var prior = new string(' ', level * 2);

            _output(
                "{0}[{1} - {2}] takes {3} msec, created at {4} {5}",
                new object[]
                {
                    prior,
                    item.StartTime.ToString("dd.MM.yyyy HH:mm:ss.fff"),
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"),
                    item.TimeInterval,
                    item.CreationStack,
                    item.Exception != null
                        ? "[" + item.Exception.Message + "]"
                        : string.Empty
                });

            if (item.Children != null && item.Children.Count > 0)
            {
                foreach (var citem in item.Children)
                {
                    WriteItem(level + 1, citem);
                }
            }
        }

        #endregion
    }
}
