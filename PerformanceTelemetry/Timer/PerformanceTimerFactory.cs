using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerformanceTelemetry.Timer
{
    public class PerformanceTimerFactory : IPerformanceTimerFactory
    {
        public IPerformanceTimer CreatePerformanceTimer()
        {
            return 
                new PerformanceTimer();
        }

        public DateTime GetCurrentTime()
        {
            return
                DateTime.Now;
        }
    }
}
