using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PerformanceTelemetry.Timer;

namespace PerformanceTelemetry.Record
{
    public class PerformanceRecordFactory : IPerformanceRecordFactory
    {
        private readonly IPerformanceTimerFactory _timerFactory;

        public PerformanceRecordFactory(IPerformanceTimerFactory timerFactory)
        {
            if (timerFactory == null)
            {
                throw new ArgumentNullException("timerFactory");
            }

            _timerFactory = timerFactory;
        }

        /// <summary>
        /// Создать корневую запись
        /// </summary>
        /// <param name="className">Имя класса в котором зафиксировано событие</param>
        /// <param name="methodName">Имя метода в котором зафиксировано событие</param>
        public IPerformanceRecord CreateRecord(
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

            return
                this.CreateChildRecord(
                    className,
                    methodName,
                    null);
        }

        /// <summary>
        /// Создать запись
        /// </summary>
        /// <param name="className">Имя класса в котором зафиксировано событие</param>
        /// <param name="methodName">Имя метода в котором зафиксировано событие</param>
        /// <param name="parent">Родительская запись, если null - создается КОРНЕВАЯ запись</param>
        public IPerformanceRecord CreateChildRecord(
            string className,
            string methodName,
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

            return
                new PerformanceRecord(
                    className,
                    methodName,
                    this,
                    _timerFactory,
                    parent);
        }
    }
}
