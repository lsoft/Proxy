namespace PerformanceTelemetry.Record
{
    public interface IPerformanceRecordFactory
    {
        /// <summary>
        /// Создать корневую запись
        /// </summary>
        /// <param name="className">Имя класса в котором зафиксировано событие</param>
        /// <param name="methodName">Имя метода в котором зафиксировано событие</param>
        IPerformanceRecord CreateRecord(
            string className,
            string methodName
            );

        /// <summary>
        /// Создать запись
        /// </summary>
        /// <param name="className">Имя класса в котором зафиксировано событие</param>
        /// <param name="methodName">Имя метода в котором зафиксировано событие</param>
        /// <param name="parent">Родительская запись, если null - создается КОРНЕВАЯ запись</param>
        IPerformanceRecord CreateChildRecord(
            string className,
            string methodName,
            IPerformanceRecord parent);
    }
}