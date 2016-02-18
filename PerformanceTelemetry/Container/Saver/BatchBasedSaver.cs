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
    /// Сохранитель итемов батчами; это уменьшает затраты ЦП, но дает задержку в сохранении итемов.
    /// Рекомендуется использовать в случае большого потока событий (ливня событий).
    /// </summary>
    public sealed class BatchBasedSaver : IPerformanceSaver
    {
        //максимальный размер очереди, после которого эвенты не добавляеются в нее, а игнорируются
        private const int MaximumQueueLength = 5000;

        //интервал через который записывать диагностику размера очереди
        private const int DiagnosticWriteTimeIntervalInSeconds = 30;

        //максимальное количество итемов, которое может быть записано одним item writer'ом
        //(иногда это означает - в рамках одной транзакции, которые желательно время от времени закрывать)
        private const int BatchSize = 1000;

        //время ожидания, пока наберется количество итемов в размере как минимум 1 батч
        private const int BatchWaitTimeoutMsec = 10000;

        //фабрика итем сейверов
        private readonly IItemSaverFactory _itemSaverFactory;

        //логгер
        private readonly ITelemetryLogger _logger;

        //признак подавления ошибок телеметрии
        private readonly bool _suppressExceptions;

        //очередь для записи должна быть потокозащищенной, так как она наполняется из одного трида, а опустошается - из другого
        private readonly ConcurrentQueue<IPerformanceRecordData> _recordQueue = new ConcurrentQueue<IPerformanceRecordData>();

        //событие, сигнализирующее о том, что сейверу пора завершаться
        private readonly ManualResetEvent _shouldStop = new ManualResetEvent(false);

        //событие, сигнализирующее о том, что появилась новая запись на сохранение
        private readonly AutoResetEvent _newRecord = new AutoResetEvent(false);

        //количество ошибок при сохранении
        private int _errorCounter = 0;

        //признак, что сейвер еще не стартовал
        private int _started = 0;

        //время последней записи в лог размера очереди
        private static DateTime _lastDiagnosticMessageTime = DateTime.Now;

        //признак, что сейвер завершил работу
        private bool _disposed = false;

        //рабочий поток сохранения
        private Thread _workerThread;

        //контейнер итемов для сохранения, чтобы не пересоздавать его постоянно
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

            //проверяем, не переполнилась ли очередь
            //при ливне событий, мы можем не успевать записывать
            var rc = _recordQueue.Count;
            if (rc < MaximumQueueLength)
            {
                //очередь не переполнилась

                //если еще не стартовали - стартуем
                if (Interlocked.CompareExchange(ref _started, 1, 0) == 0)
                {
                    this.WorkStart();
                }

                _recordQueue.Enqueue(record);

                if (rc + 1 >= BatchSize)
                {
                    //итемов набралось на батч

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
                ); //при смене порядка эвентов надо менять код, которые юзает waitIndex

                //после срабатывания события, сначала пытаемся сохранить всё, а уже потом проверяем условие выхода
                //это нужно, так как если срабатывают оба события очень быстро:
                //using (var saver = new SomeSaver(...))
                //{
                //  saver.Save(...); <-- рейзим _newRecord евент
                //} <-- почти мгновенно рейзим _shouldStop евент
                //то трид не успевает проснуться между активацией _shouldStop и _newRecord
                //а просыпается уже после сработки обоих ивентов, и сразу выходит, не сохранив события
                //это в принципе не очень страшная ситуация в продакшене, так как потеря одного события, которое
                //произошло в последние мгновения перед закрытием телеметрии, не страшна.
                //но такое поведение проваливает тесты, а это уже хуже и раздражает

                //поэтому вне зависимости от того, какое событие пробудило нас, сначала пытаемся сохранить,
                //потом - выходим (если надо)

                try
                {
                    ProcessQueue();
                }
                catch (Exception excp)
                {
                    //если при сохранении рекорда возникла ошибка, лучше этот батч "потерять"
                    //так как иначе ошибка сохранения может быть постоянной, а это приведет
                    //к разбуханию очереди и OutOfMemory

                    if (_suppressExceptions)
                    {
                        //при ошибке инсерта телеметрии ничего не делаем кроме сранья в лог
                        //не надо падать и прочее, так как это второстепенная функция

                        _errorCounter++;

                        if (_errorCounter < 100 || (_errorCounter % 100) == 0)
                        {
                            //может быть такая ситуация, что НИ ОДНО сообщение телеметрии не сможет быть закомичено в базу
                            //например что-то с базой или таблицей
                            //в этом случае телеметрия лог завалит сообщениями, в которых потеряется ВСЁ и лог станет нечитаемым
                            //поэтому первые сто сообщений записываются в лог, а потом записываемся КАЖДОЕ СОТОЕ

                            _logger.LogHandledException(this.GetType(), "При сохранении рекорда возникла ошибка", excp);
                        }
                    }
                    else
                    {
                        //маловероятная ситуация, что
                        //1) сработали два евента (сохранения итема и выхода),
                        //2) при попытке сохранить итемы произошла ошибка
                        //3) не включен режим суппресса исключений
                        //то в этом случае ошибку все равно давим и выходим
                        if (waitIndex == 0)
                        {
                            return;
                        }

                        throw;
                    }

                    //в случае ошибки, необходимо сделать таймаут, чтобы не сжирать весь процессор
                    Thread.Yield();
                }

                //выходим после попытки сохранить всё, если конечно было приказано
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
                #region запись в лог размера очереди

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

                #region опустошаем очередь

                //набраем батч на сохранение
                var cnt = 0;

                IPerformanceRecordData item;
                while (_recordQueue.TryDequeue(out item) && cnt < BatchSize)
                {
                    _batch[cnt] = item;

                    cnt++;
                }

                if (cnt > 0)
                {
                    //есть что-то для сохранения

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