using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceTelemetry
{
    /// <summary>
    /// Логгер для библиотеки телеметрии
    /// </summary>
    public interface ITelemetryLogger
    {
        /// <summary>
        /// Залоггировать ошибку
        /// </summary>
        /// <param name="sourceType">Тип, из которого был вызов</param>
        /// <param name="message">Сообщенеи</param>
        /// <param name="excp">Ексцепшен</param>
        void LogHandledException(
            Type sourceType,
            string message,
            Exception excp);
    }
}
