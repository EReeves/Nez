using System;

namespace Nez.Analysis
{
	/// <summary>
	///     Stopwatch is used to measure the general performance of Silverlight functionality. Silverlight
	///     does not currently provide a high resolution timer as is available in many operating systems,
	///     so the resolution of this timer is limited to milliseconds. This class is best used to measure
	///     the relative performance of functions over many iterations.
	/// </summary>
	public sealed class Stopwatch
    {
        private long _elapsed;
        private long _startTick;


	    /// <summary>
	    ///     Gets a value indicating whether the instance is currently recording.
	    /// </summary>
	    public bool IsRunning { get; private set; }


	    /// <summary>
	    ///     Gets the Elapsed time as a Timespan.
	    /// </summary>
	    public TimeSpan Elapsed => TimeSpan.FromMilliseconds(ElapsedMilliseconds);


	    /// <summary>
	    ///     Gets the Elapsed time as the total number of milliseconds.
	    /// </summary>
	    public long ElapsedMilliseconds => GetCurrentElapsedTicks() / TimeSpan.TicksPerMillisecond;


	    /// <summary>
	    ///     Gets the Elapsed time as the total number of ticks (which is faked
	    ///     as Silverlight doesn't have a way to get at the actual "Ticks")
	    /// </summary>
	    public long ElapsedTicks => GetCurrentElapsedTicks();


	    /// <summary>
	    ///     Creates a new instance of the class and starts the watch immediately.
	    /// </summary>
	    /// <returns>An instance of Stopwatch, running.</returns>
	    public static Stopwatch StartNew()
        {
            var sw = new Stopwatch();
            sw.Start();
            return sw;
        }


	    /// <summary>
	    ///     Completely resets and deactivates the timer.
	    /// </summary>
	    public void Reset()
        {
            _elapsed = 0;
            IsRunning = false;
            _startTick = 0;
        }


	    /// <summary>
	    ///     Begins the timer.
	    /// </summary>
	    public void Start()
        {
            if (!IsRunning)
            {
                _startTick = GetCurrentTicks();
                IsRunning = true;
            }
        }


	    /// <summary>
	    ///     Stops the current timer.
	    /// </summary>
	    public void Stop()
        {
            if (IsRunning)
            {
                _elapsed += GetCurrentTicks() - _startTick;
                IsRunning = false;
            }
        }


        private long GetCurrentElapsedTicks()
        {
            return _elapsed + (IsRunning ? GetCurrentTicks() - _startTick : 0);
        }


        private long GetCurrentTicks()
        {
            // TickCount: Gets the number of milliseconds elapsed since the system started.
            return Environment.TickCount * TimeSpan.TicksPerMillisecond;
        }
    }
}