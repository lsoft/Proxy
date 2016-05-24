using Ninject;
using ProxyGenerator.PL;

namespace ProxyGenerator.NInject
{
    public interface IProxyPayloadFactoryProvider
    {
        IProxyPayloadFactory GetProxyPayloadFactory(IKernel kernel);
    }
}