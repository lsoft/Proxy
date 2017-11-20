using ProxyNinjectDemostration.ProxyRelated;

namespace ProxyNinjectDemostration.ApplicationThings.Class1
{
    public interface IInterface1ThatNeedToBeProxied
    {
        [Proxy]
        int SumWithWait500Msec(int a, int b);

        [Proxy]
        void GenerateExceptionAfter250Msec();

        string GetCurrentDateTime_NotProxied();
    }
}