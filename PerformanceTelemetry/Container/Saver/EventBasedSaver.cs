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

        //признак, что сейвер завершил работу
        private bool _disposed = false;

        //рабочий поток сохранения
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


            //если еще не стартовали - стартуем
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
                var waitIndex = WaitHandle.WaitAny(new WaitHandle[] { _shouldStop, _newRecord }, -1); //при смене порядка эвентов надо менять код, которые юзает waitIndex

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
                    //чтобы не создавать зря транзакцию, если нас разбудило событие выхода - сначала извлекаем
                    //хотя бы один элемент, и, если он есть, то уже начинаем full blown сохранение

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
                    //если при сохранении рекорда возникла ошибка, лучше этот итем "потерять"
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

                            _output(Exception2StringHelper.ToFullString(excp));
                        }
                    }
                    else
                    {
                        //маловероятная ситуация, что
                        //1) сработали два евента (сохранения итема и выхода),
                        //2) при попытке сохранить итемы произошла ошибка
                        //3) не включен режим суппресса исключений
                        //то в этом случае ошибку все равно давим и выходим
                        if (waitIndex == 0 || waitIndex == WaitHandle.WaitTimeout)
                        {
                            return;
                        }

                        throw;
                    }
                }

                //выходим после попытки сохранить всё, если конечно было приказано
                if (waitIndex == 0 || waitIndex == WaitHandle.WaitTimeout)
                {
                    return;
                }

            }
        }

        #endregion

    }
}