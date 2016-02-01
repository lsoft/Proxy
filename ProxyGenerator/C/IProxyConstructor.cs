using System;

namespace ProxyGenerator.C
{
    public interface IProxyConstructor
    {
        /// <summary>
        /// Создание прокси-объекта
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс, выдаваемый наружу</typeparam>
        /// <typeparam name="TClass">Тип оборачиваемого объекта</typeparam>
        /// <param name="attributeType">Тип атрибута, которым помечены мемберы, годные к записи телеметрии</param>
        /// <param name="args">Аргументы для конструктора объекта</param>
        /// <returns>Сформированный прокси, которые с помощью интерфейса прикидывается оборачиваемым объектом</returns>
        TInterface CreateProxy<TInterface, TClass>(
            Type attributeType,
            params object[] args)
            where TInterface : class
            where TClass : class;
    }
}