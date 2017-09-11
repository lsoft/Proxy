using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyGenerator.Wrapper
{
    internal interface IWrapper
    {
        string ToSourceCode();
    }
}
