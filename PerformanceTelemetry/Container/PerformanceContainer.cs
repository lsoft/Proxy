using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PerformanceTelemetry.Container.Saver;
using PerformanceTelemetry.Record;

namespace PerformanceTelemetry.Container
{
    /// <summary>
    /// Контейнер активных рекордов
    /// </summary>
    public class PerformanceContainer : IPerformanceContainer
    {
        #region private mock classes

        /// <summary>
        /// Класс-пустышка, возвращается хост-приложению, если в PerformanceContainer.OpenPerformanceSession
        /// произошла какая-то ошибка.
        /// Чтобы не возвращать нулл и соотв. чтоы хост-приложение не падало на NullReferenceException
        /// </summary>
        private class PerformanceRecordMock : IPerformanceRecord
        {
            #region Implementation of IPerformanceRecordData

            public string ClassName
            {
                get
                {
                    return
                        "ClassName";
                }
            }

            public string MethodName
            {
                get
                {
                    return
                        "MethodName";
                }
            }

            public DateTime StartTime
            {
                get
                {
                    return DateTime.Now;
                }
            }

            public double TimeInterval
            {
                get
                {
                    return 1.0;
                }
            }

            public Exception Exception
            {
                get
                {
                    return null;
                }
            }

            public string CreationStack
            {
                get
                {
                    var result = new StackTrace(2, true).ToString(); //двойка подобрана так, чтобы "служебные" фреймы перформанса не попадали в стек

                    result = string.IsInterned(result) ?? result; //memory economy

                    return
                        result;
                }
            }

            public List<IPerformanceRecordData> GetChildren()
            {
                return
                    new List<IPerformanceRecordData>();
            }

            #endregion

            #region Implementation of IPerformanceRecord

            public bool Active
            {
                get
                {
                    return true;
                }
            }

            public IPerformanceRecord GetDeepestActiveRecord()
            {
                return this;
            }
            
            public IPerformanceRecord CreateChild(string className, string methodName)
            {
                return new PerformanceRecordMock();
            }

            public void Close()
            {
                //nothing to do
            }

            public void SetExceptionFlag(Exception excp)
            {
                //nothing to do
            }

            public void ChildDied()
            {
                //nothing to do
            }

            public IPerformanceRecordData GetPerformanceData()
            {
                return
                    this;
            }

            #endregion
        }

        #endregion

        private readonly ITelemetryLogger _logger;
        private readonly IPerformanceRecordFactory _factory;
        private IPerformanceSaver _saver;

        private readonly ConcurrentDictionary<int, IPerformanceRecord> _activeRecordDict;


        public PerformanceContainer(
            ITelemetryLogger logger,
            IPerformanceRecordFactory factory,
            IPerformanceSaver saver)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (saver == null)
            {
                throw new ArgumentNullException("saver");
            }

            _logger = logger;
            _factory = factory;
            _saver = saver;
            _activeRecordDict = new ConcurrentDictionary<int, IPerformanceRecord>();
        }


        public IPerformanceRecord OpenPerformanceSession(
            int managedThreadId,
            string className,
            string methodName
            )
        {
            try
            {
                if (className == null)
                {
                    throw new ArgumentNullException("className");
                }
                if (methodName == null)
                {
                    throw new ArgumentNullException("methodName");
                }

                IPerformanceRecord result;

                //определяем, есть ли перформасы в этом триде
                IPerformanceRecord root;
                if (_activeRecordDict.TryGetValue(managedThreadId, out root))
                {
                    //вложенный перформанс, берем предка
                    var newParent = root.GetDeepestActiveRecord();

                    //если нашелся открытый парент, значит от него рождаем
                    if (newParent == null)
                    {
                        //парент не нашелся, соотв. это уже умершие счетчики
                        //Это ошибка, так как умершие счетчики должны сохраняться и стираться
                        throw new InvalidOperationException("newParent == null");
                    }

                    //создаем запись
                    result = newParent.CreateChild(
                        className,
                        methodName);
                }
                else
                {
                    //нет родительского контейнера
                    result = _factory.CreateRecord(
                        className,
                        methodName);

                    //записываем в контейнер
                    _activeRecordDict[managedThreadId] = result;
                }

                return result;
            }
            catch (Exception excp)
            {
                _logger.LogHandledException(
                    this.GetType(),
                    "Ошибка открытия сессии перформанса",
                    excp);

                //пофигу, если статистика сломается, хост-приложение должно работать
                //для этого возвращаем пустышку, в которой падать нечему
                var mockResult = new PerformanceRecordMock();

                return
                    mockResult;
            }
        }

        public void ClosePerformanceSession(int threadId, IPerformanceRecord closingRecord)
        {
            try
            {
                if (closingRecord == null)
                {
                    throw new ArgumentNullException("closingRecord");
                }

                closingRecord.Close();

                //определяем, если это корень умер, то после помирания сохраняем его в файл и удаляем из хранилища
                IPerformanceRecord containerRecord;
                if (_activeRecordDict.TryGetValue(threadId, out containerRecord))
                {
                    if (ReferenceEquals(containerRecord, closingRecord)) //сравнение тупо по ссылке
                    {
                        //удаляем
                        IPerformanceRecord removedObjet;
                        _activeRecordDict.TryRemove(threadId, out removedObjet);

                        //сохраняем
                        _saver.Save(closingRecord.GetPerformanceData());
                    }
                }
            }
            catch (Exception excp)
            {
                _logger.LogHandledException(
                    this.GetType(),
                    "Ошибка закрытия сессии перформанса",
                    excp);

                //пофигу, если статистика сломается, хост-приложение должно работать
            }
        }

        public void Dispose()
        {
            if (this._saver != null)
            {
                try
                {
                    this._saver.Dispose();
                }
                catch (Exception excp)
                {
                    _logger.LogHandledException(
                        this.GetType(),
                        "Ошибка _saver.Dispose",
                        excp);

                    //пофигу, если статистика сломается, хост-приложение должно работать
                }

                this._saver = null;
            }
        }
    }
}
