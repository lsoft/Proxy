using System;
using ProxyGenerator.PL;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    public class MockPayload : IProxyPayload
    {
        private readonly Action<Exception> _setExceptionFlag;
        private readonly Action _dispose;

        public MockPayload(
            Action<Exception> setExceptionFlag,
            Action dispose)
        {
            _setExceptionFlag = setExceptionFlag ?? new Action<Exception>(a => { });
            _dispose = dispose ?? new Action(() => { });
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _dispose();
        }

        public void SetExceptionFlag(Exception excp)
        {
            _setExceptionFlag(excp);
        }

        #endregion
    }
}
