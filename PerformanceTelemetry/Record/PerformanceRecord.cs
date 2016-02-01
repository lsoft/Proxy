﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using PerformanceTelemetry.Timer;

namespace PerformanceTelemetry.Record
{
    public class PerformanceRecord : IPerformanceRecord
    {
        private readonly string _className;
        private readonly string _methodName;
        private readonly IPerformanceRecordFactory _recordFactory;
        private readonly IPerformanceRecord _parent;

        private readonly IPerformanceTimer _timer;
        private readonly DateTime _startTime;

        private double _timeInterval = 0.0;
        private Exception _exception = null;

        private readonly List<IPerformanceRecord> _diedChildren;
        private readonly string _creationStack;

        public bool Active
        {
            get;
            private set;
        }

        public IPerformanceRecord ActiveChild
        {
            get;
            private set;
        }

        public double TimeInterval
        {
            get
            {
                if (Active)
                {
                    //счет еще тикает
                    return
                        _timer.TimeInterval;
                }
                else
                {
                    //оттикались уже, берем посчитанное
                    return
                        _timeInterval;
                }
            }
        }

        public PerformanceRecord(
            string className,
            string methodName,
            IPerformanceRecordFactory recordFactory,
            IPerformanceTimerFactory timerFactory,
            IPerformanceRecord parent)
        {
            if (className == null)
            {
                throw new ArgumentNullException("className");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }
            if (recordFactory == null)
            {
                throw new ArgumentNullException("recordFactory");
            }
            if (timerFactory == null)
            {
                throw new ArgumentNullException("timerFactory");
            }

            //parent allowed to be null


            _className = className;
            _methodName = methodName;
            _recordFactory = recordFactory;
            _parent = parent;

            Active = true;
            _diedChildren = new List<IPerformanceRecord>();
            _creationStack = new StackTrace(1, true).ToString(); //единица подобрана так, чтобы "служебные" фреймы перформанса не попадали в стек

            //Запоминаем время
            _startTime = timerFactory.GetCurrentTime();
            _timer = timerFactory.CreatePerformanceTimer();
        }

        public IPerformanceRecord GetDeepestActiveRecord()
        {
            if (this.ActiveChild == null)
            {
                if (this.Active)
                {
                    return this;
                }

                return null;
            }

            return
                this.ActiveChild.GetDeepestActiveRecord();
        }

        public IPerformanceRecord CreateChild(
            string className,
            string methodName
            )
        {
            if (className == null)
            {
                throw new ArgumentNullException("className");
            }
            if (methodName == null)
            {
                throw new ArgumentNullException("methodName");
            }

            if (!this.Active)
            {
                throw new InvalidOperationException("Неактивный предок");
            }

            if (this.ActiveChild != null)
            {
                throw new InvalidOperationException("Уже есть активный чайлд");
            }

            this.ActiveChild = _recordFactory.CreateChildRecord(
                className,
                methodName,
                this);

            return
                this.ActiveChild;
        }

        public void Close()
        {
            if (!this.Active)
            {
                throw new InvalidOperationException("Запись уже закрыта");
            }

            if (this.ActiveChild != null)
            {
                throw new InvalidOperationException("Нельзя закрыть запись, которая имеет потомков");
            }

            //запоминаем время и вычисляем интервал
            _timeInterval = _timer.TimeInterval;

            //после фиксации времени делаем вспомогательную хрень
            this.Active = false;

            if (_parent != null)
            {
                _parent.ChildDied();
            }
        }

        public void ChildDied()
        {
            if (this.ActiveChild == null)
            {
                throw new InvalidOperationException("Нет активного чайлда");
            }

            this._diedChildren.Add(this.ActiveChild);
            this.ActiveChild = null;
        }

        public IPerformanceRecordData GetPerformanceData()
        {
            var children = new List<IPerformanceRecord>(this._diedChildren);

            if(this.ActiveChild != null)
            {
                children.Add(this.ActiveChild);
            }

            return
                new PerformanceRecordData(
                    this._className,
                    this._methodName,
                    this._startTime,
                    this.TimeInterval,
                    this._exception,
                    this._creationStack,
                    children.ConvertAll(j => j.GetPerformanceData()));
        }

        public void SetExceptionFlag(Exception excp)
        {
            if (excp == null)
            {
                throw new ArgumentNullException("excp");
            }

            if (_exception != null)
            {
                throw new InvalidOperationException("уже взведен флаг исключения");
            }

            _exception = excp;
        }
    }
}