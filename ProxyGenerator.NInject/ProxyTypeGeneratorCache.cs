using System;
using System.Collections.Generic;
using System.Threading;
using Ninject;
using Ninject.Syntax;
using ProxyGenerator.G;

namespace ProxyGenerator.NInject
{
    internal static class ProxyTypeGeneratorCache
    {
        private static readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static readonly Dictionary<int, IProxyTypeGenerator> _generatorDict = new Dictionary<int, IProxyTypeGenerator>();

        public static IProxyTypeGenerator ProvideProxyTypeGenerator(
            IResolutionRoot root)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            var hash = root.GetHashCode().GetHashCode();

            //initially try to obtain ProxyConstructor under read-lock (frequent operation)
            _locker.EnterReadLock();
            try
            {
                if (_generatorDict.ContainsKey(hash))
                {
                    return
                        _generatorDict[hash];
                }
            }
            finally
            {
                _locker.ExitReadLock();
            }

            //if it fails, obtain write-lock, once again try to obtain, in case of failure construct ProxyConstructor (rare operation)
            _locker.EnterWriteLock();
            try
            {
                if (!_generatorDict.ContainsKey(hash))
                {
                    var proxyGeneratorProvider = root.Get<IGeneratorProvider>();

                    var generator = proxyGeneratorProvider.ProvideGenerator();

                    _generatorDict.Add(
                        hash,
                        generator);
                }

                return
                    _generatorDict[hash];
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }
    }
}