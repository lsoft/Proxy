using System;
using ProxyGenerator.PL;

namespace ProxyGenerator.Tests.ProxyGeneratorTests
{
    public class MockPayloadFactory : IProxyPayloadFactory
    {
        private readonly Action<Exception> _setExceptionFlag;
        private readonly Action _dispose;

        public MockPayloadFactory()
            : this(null, null)
        {
        }

        public MockPayloadFactory(
            Action<Exception> setExceptionFlag,
            Action dispose)
        {
            _setExceptionFlag = setExceptionFlag ?? new Action<Exception>(a => { });
            _dispose = dispose ?? new Action(() => { });
        }

        #region Implementation of IProxyPayloadFactory

        public IProxyPayload GetProxyPayload(string className, string methodName)
        {
            return
                new MockPayload(_setExceptionFlag, _dispose);
        }

        #endregion
    }
}
