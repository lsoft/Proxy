using System;
using System.Threading;

namespace ProxyDemonstration.ApplicationThings.Class1
{
    public class Class1ThatNeedToBeProxied : IInterface1ThatNeedToBeProxied
    {
        public int SumWithWait500Msec(int a, int b)
        {
            Thread.Sleep(500);

            return
                a + b;
        }

        public void GenerateExceptionAfter250Msec()
        {
            Thread.Sleep(250);

            throw new InvalidOperationException("most wanted exception!");
        }

        public string GetCurrentDateTime_NotProxied()
        {
            return
                DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff");
        }
    }
}