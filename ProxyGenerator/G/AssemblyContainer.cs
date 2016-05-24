using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace ProxyGenerator.G
{
    internal class AssemblyContainer
    {
        private readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();

        private readonly object _locker = new object();

        public AssemblyContainer()
        {
        }

        public void Add(Assembly a)
        {
            lock (_locker)
            {
                this._assemblies.Add(a);
            }
        }

        public bool IsExists(Assembly a)
        {
            lock (_locker)
            {
                return
                    this._assemblies.Contains(a);
            }
        }

    }
}
