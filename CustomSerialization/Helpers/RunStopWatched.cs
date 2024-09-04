using System.Diagnostics;
using CustomSerialization.Models;

namespace CustomSerialization.Helpers
{
    /// <summary>
    /// Helper for running a method while measuring elapsed time.
    /// </summary>
    internal static class RunStopWatched
    {
        #region public methods

        /// <summary>
        /// Run specified function several times and measure the elapsed time.
        /// </summary>
        /// <typeparam name="T">Type of object returned by <paramref name="function"/>.</typeparam>
        /// <param name="function">Function to be tested.</param>
        /// <param name="iterations">The number of attempts to run the function.</param>
        /// <returns>Function result and its average work time.</returns>
        public static TimedResult<T> RunFunction<T>(
            Func<T> function,
            int iterations)
        {
            ArgumentNullException.ThrowIfNull(function);

            if (iterations <= 0)
            {
                throw new ArgumentException("Iterations must be greater than zero.", nameof(iterations));
            }

            T? result = default;
            List<long> executionTimesInTicks = new();

            for (int i = 0; i < iterations; i++)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                result = function();
                stopwatch.Stop();

                executionTimesInTicks.Add(stopwatch.Elapsed.Ticks);
            }

            return new(
                result: result,
                time: executionTimesInTicks.Average() / TimeSpan.TicksPerMillisecond);
        }

        #endregion
    }
}
