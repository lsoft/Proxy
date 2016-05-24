using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProxyGenerator.G;
using ProxyGenerator.PL;
using ProxyGenerator.WrapMethodResolver;

namespace ProxyGenerator.C
{
    /// <summary>
    /// Standalone Конструктор прокси-объектов
    /// </summary>
    public class StandaloneProxyConstructor : IProxyConstructor
    {
        private readonly IProxyTypeGenerator _proxyTypeGenerator;

        /// <summary>
        /// Создание прокси-конструктора
        /// </summary>
        /// <param name="proxyTypeGenerator">Генератор прокси-типа</param>
        public StandaloneProxyConstructor(
            IProxyTypeGenerator proxyTypeGenerator
            )
        {
            if (proxyTypeGenerator == null)
            {
                throw new ArgumentNullException("proxyTypeGenerator");
            }

            _proxyTypeGenerator = proxyTypeGenerator;
        }

        /// <summary>
        /// Создание прокси-объекта
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс, выдаваемый наружу</typeparam>
        /// <typeparam name="TClass">Тип оборачиваемого объекта</typeparam>
        /// <param name="proxyPayloadFactory">Фабрика объектов полезной нагрузки</param>
        /// <param name="wrapResolver">Делегат-определитель, надо ли проксить метод</param>
        /// <param name="args">Аргументы для конструктора объекта</param>
        /// <returns>Сформированный прокси, которые с помощью интерфейса прикидывается оборачиваемым объектом</returns>
        public TInterface CreateProxy<TInterface, TClass>(
            IProxyPayloadFactory proxyPayloadFactory,
            WrapResolverDelegate wrapResolver,
            params object[] args
            )
            where TInterface : class
            where TClass : class
        {
            if (proxyPayloadFactory == null)
            {
                throw new ArgumentNullException("proxyPayloadFactory");
            }

            var p = new Parameters(
                proxyPayloadFactory,
                wrapResolver
                );

            var proxyType = _proxyTypeGenerator.CreateProxyType<TInterface, TClass>(
                p
                );

            var constructorArgs = new List<object>
                                  {
                                      proxyPayloadFactory
                                  };
            constructorArgs.AddRange(args);

            //создаем объект (занимает относительно много времени, так как вызов конструктора)
            var proxy = (TInterface)Activator.CreateInstance(proxyType, constructorArgs.ToArray());

            return proxy;

        }

        /// <summary>
        /// Создание прокси-объекта
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс, выдаваемый наружу</typeparam>
        /// <typeparam name="TClass">Тип оборачиваемого объекта</typeparam>
        /// <param name="proxyPayloadFactory">Фабрика объектов полезной нагрузки</param>
        /// <param name="attributeType">Тип атрибута, которым помечены мемберы, годные к записи телеметрии</param>
        /// <param name="args">Аргументы для конструктора объекта</param>
        /// <returns>Сформированный прокси, которые с помощью интерфейса прикидывается оборачиваемым объектом</returns>
        public TInterface CreateProxy<TInterface, TClass>(
            IProxyPayloadFactory proxyPayloadFactory,
            Type attributeType,
            params object[] args)
                where TInterface : class
                where TClass : class
        {
            return
                CreateProxy<TInterface, TClass>(
                    proxyPayloadFactory,
                    (mi) => AttributeWrapMethodResolver.NeedToWrap(attributeType, mi),
                    args
                    );
        }

    }
}
