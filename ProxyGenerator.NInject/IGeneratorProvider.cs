using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProxyGenerator.C;
using ProxyGenerator.G;

namespace ProxyGenerator.NInject
{
    public interface IGeneratorProvider
    {
        IProxyTypeGenerator ProvideGenerator();
    }
}
