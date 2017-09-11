using System;

namespace ProxyGenerator.Payload
{
    /// <summary>
    /// Полезная нагрузка, которую прокси генератор включает в прокси
    /// </summary>
    public interface IProxyPayload : IDisposable
    {
        /// <summary>
        /// Метод передачи в пейлоад признака, что вызов метода завершился исключением
        /// </summary>
        /// <param name="excp">Тип исключения</param>
        void SetExceptionFlag(Exception excp);
    }
}
