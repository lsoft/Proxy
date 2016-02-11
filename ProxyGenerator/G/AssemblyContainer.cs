using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace ProxyGenerator.G
{
    internal class AssemblyContainer : IDisposable
    {
        private readonly ReaderWriterLockSlim _locker;
        private readonly List<Assembly> _assembliesList;

        private bool _disposed = false;

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

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _locker.Dispose();
            }
        }

        ~AssemblyContainer()
        {
            this.Dispose();
        }

    }
}
