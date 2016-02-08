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
        private const int DiagnosticWriteTimeIntervalInSeconds = 30;

        //������������ ���������� ������, ������� ����� ���� �������� ����� item writer'��
        //(������ ��� �������� - � ������ ����� ����������, ������� ���������� ����� �� ������� ���������)
        private const int MaximumItemCountCanBeWritedByOneItemSaver = 250;

        //������� ���� ��������
        private readonly IItemSaverFactory _itemSaverFactory;

        //������
        private readonly ITelemetryLogger _logger;

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
            ITelemetryLogger logger,
            bool suppressExceptions = true
            )
        {
            if (itemSaverFactory == null)
            {
                throw new ArgumentNullException("itemSaverFactory");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }


            _itemSaverFactory = itemSaverFactory;
            _logger = logger;
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
            //��� ����� �������, �� ����� �� �������� ����������
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

                    var now = DateTime.Now;
                    if ((now - _lastDiagnosticMessageTime).TotalSeconds >= DiagnosticWriteTimeIntervalInSeconds)
                    {
                        _lastDiagnosticMessageTime = now;

                        var rc = _recordQueue.Count;

                        var message = string.Format(
                            "Telemetry stat:{0}Error count: {1}{0}Queue size: {2}",
                            Environment.NewLine,
                            _errorCounter,
                            rc
                            );

                        _logger.LogMessage(this.GetType(), message);
                    }

                    #endregion

                    #region ���������� �������

                    //����� �� ��������� ��� ����������, ���� ��� ��������� ������� ������ - ������� ���������
                    //���� �� ���� �������, �, ���� �� ����, �� ��� �������� full blown ����������

                    IPerformanceRecordData item;
                    if (_recordQueue.TryDequeue(out item))
                    {
                        using (var itemSaver = _itemSaverFactory.CreateItemSaver())
                        {
                            itemSaver.SaveItem(item);

                            //��������� ����� ������, ������� ����� ���� �������� � ���� item writer
                            //�� ���� ���� ��� ����� ������, ���� ���� ���� ��� ������ ����������
                            var cnt = MaximumItemCountCanBeWritedByOneItemSaver;
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

                            _logger.LogHandledException(this.GetType(), "��� ���������� ������� �������� ������", excp);
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