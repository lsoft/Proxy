using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using PerformanceTelemetry.Container.Saver.Item;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver
{
    /// <summary>
    /// ����������� ������ �������; ��� ��������� ������� ��, �� ���� �������� � ���������� ������.
    /// ������������� ������������ � ������ �������� ������ ������� (����� �������).
    /// </summary>
    public sealed class BatchBasedSaver : IPerformanceSaver
    {
        //������������ ������ �������, ����� �������� ������ �� ������������ � ���, � ������������
        private const int MaximumQueueLength = 5000;

        //�������� ����� ������� ���������� ����������� ������� �������
        private const int DiagnosticWriteTimeIntervalInSeconds = 30;

        //������������ ���������� ������, ������� ����� ���� �������� ����� item writer'��
        //(������ ��� �������� - � ������ ����� ����������, ������� ���������� ����� �� ������� ���������)
        private const int BatchSize = 1000;

        //����� ��������, ���� ��������� ���������� ������ � ������� ��� ������� 1 ����
        private const int BatchWaitTimeoutMsec = 10000;

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

        //��������� ������ ��� ����������, ����� �� ������������� ��� ���������
        private readonly IPerformanceRecordData[] _batch = new IPerformanceRecordData[BatchSize];

        public BatchBasedSaver(
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
            var rc = _recordQueue.Count;
            if (rc < MaximumQueueLength)
            {
                //������� �� �������������

                //���� ��� �� ���������� - ��������
                if (Interlocked.CompareExchange(ref _started, 1, 0) == 0)
                {
                    this.WorkStart();
                }

                _recordQueue.Enqueue(record);

                if (rc + 1 >= BatchSize)
                {
                    //������ ��������� �� ����

                    _newRecord.Set();
                }
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
                var waitIndex = WaitHandle.WaitAny(
                    new WaitHandle[]
                    {
                        _shouldStop,
                        _newRecord
                    },
                    BatchWaitTimeoutMsec
                ); //��� ����� ������� ������� ���� ������ ���, ������� ����� waitIndex

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
                    ProcessQueue();
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
                        if (waitIndex == 0)
                        {
                            return;
                        }

                        throw;
                    }

                    //� ������ ������, ���������� ������� �������, ����� �� ������� ���� ���������
                    Thread.Yield();
                }

                //������� ����� ������� ��������� ��, ���� ������� ���� ���������
                if (waitIndex == 0)
                {
                    return;
                }

            }
        }

        private void ProcessQueue()
        {
            do
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

                //������� ���� �� ����������
                var cnt = 0;

                IPerformanceRecordData item;
                while (_recordQueue.TryDequeue(out item) && cnt < BatchSize)
                {
                    _batch[cnt] = item;

                    cnt++;
                }

                if (cnt > 0)
                {
                    //���� ���-�� ��� ����������

                    using (var itemSaver = _itemSaverFactory.CreateItemSaver())
                    {
                        itemSaver.SaveItems(_batch, cnt);

                        itemSaver.Commit();
                    }
                }

                #endregion

            } while (_recordQueue.Count >= BatchSize && !_shouldStop.WaitOne(0));
        }

        #endregion

    }
}