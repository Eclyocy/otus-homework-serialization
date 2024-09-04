namespace CustomSerialization.Models
{
    public class TimedResult<T>(T? result, double time)
    {
        /// <summary>
        /// Method result.
        /// </summary>
        public T? Result { get; set; } = result;

        /// <summary>
        /// Elapsed time in milliseconds.
        /// </summary>
        public double Time { get; set; } = time;
    }
}
