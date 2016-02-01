using System;
using ProxyGenerator.PL;

namespace ProxyGenerator.G
{
    /// <summary>
    /// Генератор ТИПОВ прокси-объектов
    /// Один должен быть создан в одном экземпляре на  каждый ТИП фабрики полезной нагрузки (IProxyPayloadFactory)
    /// </summary>
    public interface IProxyTypeGenerator
    {
        /// <summary>
        /// Фабрика полезной нагрузки, которая зарегистрирована в этом генераторе
        /// </summary>
        IProxyPayloadFactory PayloadFactory
        {
            get;
        }

        /// <summary>
        /// Создание прокси-объекта
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс, выдаваемый наружу</typeparam>
        /// <typeparam name="TClass">Тип оборачиваемого объекта</typeparam>
        /// <param name="attributeType">Тип атрибута, которым помечены мемберы, годные к записи телеметрии</param>
        /// <param name="generatedAssemblyName">Необязательный параметр имени генерируемой сборки</param>
        /// <param name="additionalReferencedAssembliesLocation">Сборки, на которые надо дополнительно сделать референсы при компиляции прокси</param>
        /// <returns>Сформированный ТИП прокси, который после создания ЭКЗЕМПЛЯРА с помощью интерфейса прикидывается оборачиваемым объектом</returns>
        Type CreateProxyType<TInterface, TClass>(
            Type attributeType,
            string generatedAssemblyName = null,
            string[] additionalReferencedAssembliesLocation = null)
                where TInterface : class
                where TClass : class;
    }
}