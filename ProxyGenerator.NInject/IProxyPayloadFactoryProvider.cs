using Ninject;
using ProxyGenerator.Payload;

namespace ProxyGenerator.NInject
{
    public interface IProxyPayloadFactoryProvider
    {
        IProxyPayloadFactory GetProxyPayloadFactory(IKernel kernel);
    }
}