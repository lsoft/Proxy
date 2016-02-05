using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading;
using PerformanceTelemetry.Container.Saver.Item;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver
{
    public sealed class EventBasedSaver : IPerformanceSaver
    {
        //������������ ������ �������, ����� �������� ������ �� ������������ � ���, � ������������
        private const int MaximumQueueLength = 5000;

        //�������� ����� ������� ���������� ����������� ������� �������
        private const int DiagnosticWriteTimeIntervalInSeconds = 60;

        //����������� ������ ������� ��� ������� ����� ������������ �����������
        private const int WarningQueueSize = 1000;

        //������������ ���������� ������, ������� ����� ���� �������� ����� item writer'��
        //(������ ��� �������� - � ������ ����� ����������, ������� ���������� ����� �� ������� ���������)
        private const int MaximumItemCountCanBeWriterByOneItemSaver = 250;

        //������
        private readonly Action<string> _output;

        //������� ���� ��������
        private readonly IItemSaverFactory _itemSaverFactory;

        //������� ���������� ������ ����������
        private readonly bool _suppressExceptions;

        //������� ��� ������ ������ ���� ����������������, ��� ��� ��� ����������� �� ������ �����, � ������������ - �� �������
        private readonly ConcurrentQueue<IPerformanceRecordData> _recordQueue = new ConcurrentQueue<IPerformanceRecordData>();

        //�������, ��������������� � ���, ��� ������� ���� �����������
        private readonly ManualResetEvent _shouldStop = new ManualResetEvent(false);

        //�������, ��������������� � ���, ��� ��������� ����� ������ �� ����������
        private readonly AutoResetEvent _newRecord = new AutoResetEvent(false);

        //���������� ������ ��� ����������
        private int _errorCounter = 0;

        //�������, ��� ������ ��� �� ���������
        private int _started = 0;

        //����� ��������� ������ � ��� ������� �������
        private static DateTime _lastDiagnosticMessageTime = DateTime.Now;

        //�������, ��� ������ �������� ������
        private bool _disposed = false;

        //������� ����� ����������
        private Thread _workerThread;

        public EventBasedSaver(
            IItemSaverFactory itemSaverFactory,
            Action<string> logger,
            bool suppressExceptions = true
            )
        {
            if (itemSaverFactory == null)
            {
                throw new ArgumentNullException("itemSaverFactory");
            }

            _output = logger ?? Console.WriteLine;

            _itemSaverFactory = itemSaverFactory;
            _suppressExceptions = suppressExceptions;
        }

        public void Save(
            IPerformanceRecordData record
            )
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            //���������, �� ������������� �� �������
            //��� ������ ������ �������, �� ����� �� �������� ����������
            if (_recordQueue.Count < MaximumQueueLength)
            {
                //������� �� �������������

                //���� ��� �� ���������� - ��������
                if (Interlocked.CompareExchange(ref _started, 1, 0) == 0)
                {
                    this.WorkStart();
                }

                _recordQueue.Enqueue(record);

                _newRecord.Set();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_workerThread != null)
                {
                    _shouldStop.Set();

                    _workerThread.Join();
                }

                _shouldStop.Dispose();
                _newRecord.Dispose();

                _disposed = true;
            }
        }

        #region private code

        private void WorkStart()
        {
            var t = new Thread(WorkThread);
            _workerThread = t;

            _workerThread.Start();
        }

        private void WorkThread()
        {
            while (true)
            {
                var waitIndex = WaitHandle.WaitAny(new WaitHandle[] { _shouldStop, _newRecord }, -1); //��� ����� ������� ������� ���� ������ ���, ������� ����� waitIndex

                //����� ������������ �������, ������� �������� ��������� ��, � ��� ����� ��������� ������� ������
                //��� �����, ��� ��� ���� ����������� ��� ������� ����� ������:
                //using (var saver = new SomeSaver(...))
                //{
                //  saver.Save(...); <-- ������ _newRecord �����
                //} <-- ����� ��������� ������ _shouldStop �����
                //�� ���� �� �������� ���������� ����� ���������� _shouldStop � _newRecord
                //� ����������� ��� ����� �������� ����� �������, � ����� �������, �� �������� �������
                //��� � �������� �� ����� �������� �������� � ����������, ��� ��� ������ ������ �������, �������
                //��������� � ��������� ��������� ����� ��������� ����������, �� �������.
                //�� ����� ��������� ����������� �����, � ��� ��� ���� � ����������

                //������� ��� ����������� �� ����, ����� ������� ��������� ���, ������� �������� ���������,
                //����� - ������� (���� ����)

                try
                {
                    #region ������ � ��� ������� �������

                    var rc = _recordQueue.Count;
                    if (rc >= WarningQueueSize)
                    {
                        var now = DateTime.Now;
                        if ((now - _lastDiagnosticMessageTime).TotalSeconds >= DiagnosticWriteTimeIntervalInSeconds)
                        {
                            _lastDiagnosticMessageTime = now;

                            _output("_qsize: " + rc);
                        }
                    }

                    #endregion

                    #region ���������� �������

                    //����� �� ��������� ��� ����������, ���� ��� ��������� ������� ������ - ������� ���������
                    //���� �� ���� �������, �, ���� �� ����, �� ��� �������� full blown ����������

                    IPerformanceRecordData item;
                    if (_recordQueue.TryDequeue(out item))
                    {
                        using(var itemSaver = _itemSaverFactory.CreateItemSaver())
                        {
                            itemSaver.SaveItem(item);

                            //��������� ����� ������, ������� ����� ���� �������� � ���� item writer
                            //�� ���� ���� ��� ����� ������, ���� ���� ���� ��� ������ ����������
                            var cnt = MaximumItemCountCanBeWriterByOneItemSaver;
                            while (_recordQueue.TryDequeue(out item) && --cnt > 0)
                            {
                                itemSaver.SaveItem(item);
                            }

                            itemSaver.Commit();
                        }
                    }

                    #endregion
                }
                catch (Exception excp)
                {
                    //���� ��� ���������� ������� �������� ������, ����� ���� ���� "��������"
                    //��� ��� ����� ������ ���������� ����� ���� ����������, � ��� ��������
                    //� ���������� ������� � OutOfMemory

                    if (_suppressExceptions)
                    {
                        //��� ������ ������� ���������� ������ �� ������ ����� ������ � ���
                        //�� ���� ������ � ������, ��� ��� ��� �������������� �������

                        _errorCounter++;

                        if (_errorCounter < 100 || (_errorCounter % 100) == 0)
                        {
                            //����� ���� ����� ��������, ��� �� ���� ��������� ���������� �� ������ ���� ���������� � ����
                            //�������� ���-�� � ����� ��� ��������
                            //� ���� ������ ���������� ��� ������� �����������, � ������� ���������� �Ѩ � ��� ������ ����������
                            //������� ������ ��� ��������� ������������ � ���, � ����� ������������ ������ �����

                            _output(Exception2StringHelper.ToFullString(excp));
                        }
                    }
                    else
                    {
                        //������������� ��������, ���
                        //1) ��������� ��� ������ (���������� ����� � ������),
                        //2) ��� ������� ��������� ����� ��������� ������
                        //3) �� ������� ����� ��������� ����������
                        //�� � ���� ������ ������ ��� ����� ����� � �������
                        if (waitIndex == 0 || waitIndex == WaitHandle.WaitTimeout)
                        {
                            return;
                        }

                        throw;
                    }
                }

                //������� ����� ������� ��������� ��, ���� ������� ���� ���������
                if (waitIndex == 0 || waitIndex == WaitHandle.WaitTimeout)
                {
                    return;
                }

            }
        }

        #endregion

    }
}