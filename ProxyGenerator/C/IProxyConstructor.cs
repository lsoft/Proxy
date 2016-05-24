using System;
using ProxyGenerator.PL;
using ProxyGenerator.WrapMethodResolver;

namespace ProxyGenerator.C
{
    public interface IProxyConstructor
    {
        /// <summary>
        /// Создание прокси-объекта
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс, выдаваемый наружу</typeparam>
        /// <typeparam name="TClass">Тип оборачиваемого объекта</typeparam>
        /// <param name="proxyPayloadFactory">Фабрика объектов полезной нагрузки</param>
        /// <param name="wrapResolver">Делегат-определитель, надо ли проксить метод</param>
        /// <param name="args">Аргументы для конструктора объекта</param>
        /// <returns>Сформированный прокси, которые с помощью интерфейса прикидывается оборачиваемым объектом</returns>
        TInterface CreateProxy<TInterface, TClass>(
            IProxyPayloadFactory proxyPayloadFactory,
            WrapResolverDelegate wrapResolver,
            params object[] args
            )
            where TInterface : class
            where TClass : class;

        /// <summary>
        /// Создание прокси-объекта
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс, выдаваемый наружу</typeparam>
        /// <typeparam name="TClass">Тип оборачиваемого объекта</typeparam>
        /// <param name="proxyPayloadFactory">Фабрика объектов полезной нагрузки</param>
        /// <param name="attributeType">Тип атрибута, которым помечены мемберы, годные к записи телеметрии</param>
        /// <param name="args">Аргументы для конструктора объекта</param>
        /// <returns>Сформированный прокси, которые с помощью интерфейса прикидывается оборачиваемым объектом</returns>
        TInterface CreateProxy<TInterface, TClass>(
            IProxyPayloadFactory proxyPayloadFactory,
            Type attributeType,
            params object[] args
            )
            where TInterface : class
            where TClass : class;
    }
}