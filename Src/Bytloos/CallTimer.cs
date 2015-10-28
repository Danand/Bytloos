using System;
using System.Diagnostics;

namespace Bytloos
{
    /// <summary>
    /// Mini tool for measuring calls.
    /// </summary>
    public class CallTimer
    {
        /// <summary>
        /// Creates new timer.
        /// </summary>
        /// <param name="action">Action to measure.</param>
        public CallTimer(Action action)
        {
            var memoryUsageBefore = GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();

            action.Invoke();

            MemoryUsage = GC.GetTotalMemory(false) - memoryUsageBefore;

            stopwatch.Stop();

            Elapsed = stopwatch.Elapsed;
        }

        /// <summary>
        /// Time wasted to call.
        /// </summary>
        public TimeSpan Elapsed { get; private set; }

        /// <summary>
        /// Memory used to call.
        /// </summary>
        public long MemoryUsage { get; private set; }
    }
}
