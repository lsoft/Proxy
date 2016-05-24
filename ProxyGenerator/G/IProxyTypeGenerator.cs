using System;
using ProxyGenerator.PL;
using ProxyGenerator.WrapMethodResolver;

namespace ProxyGenerator.G
{
    /// <summary>
    /// Генератор ТИПОВ прокси-объектов
    /// Один должен быть создан в одном экземпляре на  каждый ТИП фабрики полезной нагрузки (IProxyPayloadFactory)
    /// </summary>
    public interface IProxyTypeGenerator
    {
        /// <summary>
        /// Создание прокси-объекта
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс, выдаваемый наружу</typeparam>
        /// <typeparam name="TClass">Тип оборачиваемого объекта</typeparam>
        /// <param name="p">Настройки генерации</param>
        /// <returns>Сформированный ТИП прокси, который после создания ЭКЗЕМПЛЯРА с помощью интерфейса прикидывается оборачиваемым объектом</returns>
        Type CreateProxyType<TInterface, TClass>(
            Parameters p
            )
                where TInterface : class
                where TClass : class;
    }
}