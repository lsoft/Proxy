using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading;
using PerformanceTelemetry.Container.Saver.Item;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container.Saver
{
    public class EventBasedSaver : IPerformanceSaver
    {
        protected readonly Action<string> _output;

        private readonly IItemSaverFactory _itemSaverFactory;
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


            //���� ��� �� ���������� - ��������
            if (Interlocked.CompareExchange(ref _started, 1, 0) == 0)
            {
                this.WorkStart();
            }

            _recordQueue.Enqueue(record);

            _newRecord.Set();
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
                    //����� �� ��������� ��� ����������, ���� ��� ��������� ������� ������ - ������� ���������
                    //���� �� ���� �������, �, ���� �� ����, �� ��� �������� full blown ����������

                    IPerformanceRecordData item;
                    if (_recordQueue.TryDequeue(out item))
                    {
                        using(var itemSaver = _itemSaverFactory.CreateItemSaver())
                        {
                            itemSaver.SaveItem(item);

                            while (_recordQueue.TryDequeue(out item))
                            {
                                itemSaver.SaveItem(item);
                            }

                            itemSaver.Commit();
                        }
                    }
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