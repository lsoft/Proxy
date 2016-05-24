using System;
using Ninject;
using ProxyGenerator.PL;

namespace ProxyGenerator.NInject
{
    public class NamedProxyPayloadFactoryProvider : IProxyPayloadFactoryProvider
    {
        private readonly string _proxyPayloadFactoryName;

        public NamedProxyPayloadFactoryProvider(
            string proxyPayloadFactoryName = null
            )
        {
            _proxyPayloadFactoryName = proxyPayloadFactoryName;
        }

        public IProxyPayloadFactory GetProxyPayloadFactory(
            IKernel kernel
            )
        {
            if (kernel == null)
            {
                throw new ArgumentNullException("kernel");
            }

            IProxyPayloadFactory proxyPayloadFactory;
            if (string.IsNullOrEmpty(_proxyPayloadFactoryName))
            {
                proxyPayloadFactory = kernel.Get<IProxyPayloadFactory>();
            }
            else
            {
                proxyPayloadFactory = kernel.Get<IProxyPayloadFactory>(_proxyPayloadFactoryName);
            }

            return
                proxyPayloadFactory;
        }
    }
}