using System;
using System.Runtime.InteropServices;

namespace PerformanceTelemetry.Timer
{
    public class PerformanceTimer : IPerformanceTimer
    {
        private static readonly bool _isPerfCounterSupported;
        private static readonly Int64 _currentFrequency;

        private readonly Int64 _startValue;

        [DllImport("kernel32.dll")]
        private static extern int QueryPerformanceCounter(ref Int64 count);
        [DllImport("kernel32.dll")]
        private static extern int QueryPerformanceFrequency(ref Int64 frequency);

        static PerformanceTimer()
        {
            // Query the high-resolution timer only if it is supported.
            // A returned frequency of 1000 typically indicates that it is not
            // supported and is emulated by the OS using the same value that is
            // returned by Environment.TickCount.
            // A return value of 0 indicates that the performance counter is
            // not supported.
            int returnVal = QueryPerformanceFrequency(ref _currentFrequency);

            if (returnVal != 0 && _currentFrequency != 1000)
            {
                // The performance counter is supported.
                _isPerfCounterSupported = true;
            }
            else
            {
                // The performance counter is not supported. Use
                // Environment.TickCount instead.
                _currentFrequency = 1000;
            }
        }

        public PerformanceTimer()
        {
            _startValue = Value;
        }

        public Int64 Frequency
        {
            get
            {
                return _currentFrequency;
            }
        }

        public double TimeInterval
        {
            get
            {
                return
                    (Value - _startValue) / (double) Frequency;
            }
        }

        public Int64 Value
        {
            get
            {
                Int64 tickCount = 0;

                if (_isPerfCounterSupported)
                {
                    // Get the value here if the counter is supported.
                    QueryPerformanceCounter(ref tickCount);
                    
                    return
                        tickCount;
                }
                else
                {
                    // Otherwise, use Environment.TickCount.
                    return
                        Environment.TickCount;
                }
            }
        }
    }
}
