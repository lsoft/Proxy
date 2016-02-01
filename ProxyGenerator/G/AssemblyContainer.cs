using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace ProxyGenerator.G
{
    internal class AssemblyContainer
    {
        private readonly ReaderWriterLockSlim _locker;
        private readonly List<Assembly> _assembliesList;

        public AssemblyContainer()
        {
            _locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _assembliesList = new List<Assembly>();
        }

        public void Add(Assembly a)
        {
            _locker.EnterWriteLock();
            try
            {
                this._assembliesList.Add(a);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public bool Remove(Assembly a)
        {
            _locker.EnterWriteLock();
            try
            {
                return this._assembliesList.Remove(a);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public bool IsExists(Assembly a)
        {
            _locker.EnterReadLock();
            try
            {
                return
                    this._assembliesList.Contains(a);
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }
    }
}
