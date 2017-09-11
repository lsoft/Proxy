namespace ProxyGenerator.Payload
{
    /// <summary>
    /// Фабрика объектов полезной нагрузки прокси-объекта, которую прокси-генератор вставляет в прокси-объект
    /// </summary>
    public interface IProxyPayloadFactory
    {
        /// <summary>
        /// Получение пейлоада (этот метод вызывается из прокси-объекта)
        /// </summary>
        /// <param name="className">Название класса объекта, которым прикидывается прокси</param>
        /// <param name="methodName">Название метода объекта, которым прикидывается прокси</param>
        /// <returns>Полезная нагрузка враппера</returns>
        IProxyPayload GetProxyPayload(string className, string methodName);
    }
}
