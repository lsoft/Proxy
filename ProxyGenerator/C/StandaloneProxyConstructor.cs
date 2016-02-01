using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProxyGenerator.G;
using ProxyGenerator.PL;

namespace ProxyGenerator.C
{
    /// <summary>
    /// Standalone Конструктор прокси-объектов
    /// </summary>
    public class StandaloneProxyConstructor : IProxyConstructor
    {
        private readonly IProxyPayloadFactory _payloadFactory;
        private readonly IProxyTypeGenerator _proxyTypeGenerator;

        /// <summary>
        /// Создание прокси-конструктора
        /// </summary>
        /// <param name="payloadFactory">Фабрика полезной нагрузки прокси-объекта</param>
        /// <param name="proxyTypeGenerator">Генератор прокси-типа</param>
        public StandaloneProxyConstructor(
            IProxyPayloadFactory payloadFactory,
            IProxyTypeGenerator proxyTypeGenerator)
        {
            if (payloadFactory == null)
            {
                throw new ArgumentNullException("payloadFactory");
            }
            if (proxyTypeGenerator == null)
            {
                throw new ArgumentNullException("proxyTypeGenerator");
            }

            _payloadFactory = payloadFactory;
            _proxyTypeGenerator = proxyTypeGenerator;
        }

        /// <summary>
        /// Создание прокси-объекта
        /// </summary>
        /// <typeparam name="TInterface">Интерфейс, выдаваемый наружу</typeparam>
        /// <typeparam name="TClass">Тип оборачиваемого объекта</typeparam>
        /// <param name="attributeType">Тип атрибута, которым помечены мемберы, годные к записи телеметрии</param>
        /// <param name="args">Аргументы для конструктора объекта</param>
        /// <returns>Сформированный прокси, которые с помощью интерфейса прикидывается оборачиваемым объектом</returns>
        public TInterface CreateProxy<TInterface, TClass>(
            Type attributeType,
            params object[] args)
                where TInterface : class
                where TClass : class
        {
            var proxyType = _proxyTypeGenerator.CreateProxyType<TInterface, TClass>(
                attributeType);

            var constructorArgs = new List<object>
                                  {
                                      _payloadFactory
                                  };
            constructorArgs.AddRange(args);

            //создаем объект (занимает относительно много времени, так как вызов конструктора)
            var proxy = (TInterface)Activator.CreateInstance(proxyType, constructorArgs.ToArray());

            return proxy;

        }

    }
}
