using System;
using ProxyGenerator.PL;
using ProxyGenerator.WrapMethodResolver;

namespace ProxyGenerator.G
{
    /// <summary>
    /// Класс настроек для генерации прокси типа
    /// </summary>
    public class Parameters
    {
        public IProxyPayloadFactory ProxyPayloadFactory
        {
            get;
            private set;
        }

        public WrapResolverDelegate WrapResolver
        {
            get;
            private set;
        }

        public string GeneratedAssemblyName
        {
            get;
            private set;
        }

        public string[] AdditionalReferencedAssembliesLocation
        {
            get;
            private set;
        }

        /// <summary>
        /// Конструктор класса настроек для генерации прокси типа
        /// </summary>
        /// <param name="proxyPayloadFactory">Фабрика объектов полезной нагрузки</param>
        /// <param name="wrapResolver">Делегат-определитель, надо ли проксить метод</param>
        /// <param name="generatedAssemblyName">Необязательный параметр имени генерируемой сборки</param>
        /// <param name="additionalReferencedAssembliesLocation">Сборки, на которые надо дополнительно сделать референсы при компиляции прокси</param>
        public Parameters(
            IProxyPayloadFactory proxyPayloadFactory,
            WrapResolverDelegate wrapResolver,
            string generatedAssemblyName = null,
            string[] additionalReferencedAssembliesLocation = null
            )
        {
            if (proxyPayloadFactory == null)
            {
                throw new ArgumentNullException("proxyPayloadFactory");
            }
            if (wrapResolver == null)
            {
                throw new ArgumentNullException("wrapResolver");
            }
            //generatedAssemblyName allowed to be null
            //additionalReferencedAssembliesLocation allowed to be null

            ProxyPayloadFactory = proxyPayloadFactory;
            WrapResolver = wrapResolver;
            GeneratedAssemblyName = generatedAssemblyName;
            AdditionalReferencedAssembliesLocation = additionalReferencedAssembliesLocation;
        }
    }
}