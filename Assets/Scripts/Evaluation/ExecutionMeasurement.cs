using System.Collections.Generic;
using System.Diagnostics;

namespace Evaluation {
    
    /// <summary>
    /// Class for measuring the execution time of a specific method. Can be started and ended with a provided method and
    /// stores the elapsed milliseconds to a public accessible List.
    /// </summary>
    public class ExecutionMeasurement {
        
        /// <summary>
        /// If the measurement should be disabled.
        /// </summary>
        public bool Disable { get; set; }
        
        /// <summary>
        /// List containing all elapsed times, its size corresponds to the number of execution times
        /// </summary>
        public List<double> ElapsedTimeMs { get; } = new List<double>();

        // the stopwatch to be used
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// Starts or resumes the time measurement, regardless of calling class.
        /// </summary>
        public void StartMeasurement() {
            _stopwatch.Start();
        }

        /// <summary>
        /// Stops the time measurement, does not save or reset it.
        /// </summary>
        public void PauseMeasurement() {
            _stopwatch.Stop();
        }

        /// <summary>
        /// Ends the time measurement, saves the elapsed milliseconds since 'StartMeasurement()' was called into the
        /// provided list. Resets the internal Stopwatch afterwards.
        /// </summary>
        public void EndMeasurement() {
            _stopwatch.Stop();
            if (!Disable) {
                ElapsedTimeMs.Add(_stopwatch.Elapsed.TotalMilliseconds);
            }
                
            _stopwatch.Reset();
        }
    }
}