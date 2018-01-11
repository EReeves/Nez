using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Nez.Graphics.Batcher;
using Nez.Utils.DebugConsole;
using Nez.Utils.Fonts;

namespace Nez.Utils.Analysis
{
#if DEBUG

	/// <summary>
	///     Realtime CPU measuring tool
	/// </summary>
	/// <remarks>
	///     You can visually find bottle neck, and know how much you can put more CPU jobs
	///     by using this tool.
	///     Because of this is real time profile, you can find glitches in the game too.
	///     TimeRuler provide the following features:
	///     * Up to 8 bars (Configurable)
	///     * Change colors for each markers
	///     * Marker logging.
	///     * It won't even generate BeginMark/EndMark method calls when you got rid of the DEBUG constant.
	///     * It supports up to 32 (Configurable) nested BeginMark method calls.
	///     * Multithreaded safe
	///     * Automatically changes display frames based on frame duration.
	///     How to use:
	///     call timerRuler.StartFrame in top of the Game.Update method.
	///     Then, surround the code that you want measure by BeginMark and EndMark.
	///     timeRuler.BeginMark( "Update", Color.Blue );
	///     // process that you want to measure.
	///     timerRuler.EndMark( "Update" );
	///     Also, you can specify bar index of marker (default value is 0)
	///     timeRuler.BeginMark( 1, "Update", Color.Blue );
	///     All profiling methods has CondionalAttribute with "DEBUG".
	///     If you not specified "DEBUG" constant, it doesn't even generate
	///     method calls for BeginMark/EndMark.
	///     So, don't forget remove "DEBUG" constant when you release your game.
	/// </remarks>
	public class TimeRuler
    {
        #region Constants

	    /// <summary>
	    ///     Max bar count.
	    /// </summary>
	    private const int MaxBars = 8;

	    /// <summary>
	    ///     Maximum sample number for each bar.
	    /// </summary>
	    private const int MaxSamples = 256;

	    /// <summary>
	    ///     Maximum nest calls for each bar.
	    /// </summary>
	    private const int MaxNestCall = 32;

	    /// <summary>
	    ///     Maximum display frames.
	    /// </summary>
	    private const int MaxSampleFrames = 4;

	    /// <summary>
	    ///     Duration (in frame count) for take snap shot of log.
	    /// </summary>
	    private const int LogSnapDuration = 120;

	    /// <summary>
	    ///     Height(in pixels) of bar.
	    /// </summary>
	    private const int BarHeight = 8;

	    /// <summary>
	    ///     Padding(in pixels) of bar.
	    /// </summary>
	    private const int BarPadding = 2;

	    /// <summary>
	    ///     Delay frame count for auto display frame adjustment.
	    /// </summary>
	    private const int AutoAdjustDelay = 30;

        #endregion


        #region Properties and Fields

	    /// <summary>
	    ///     Gets/Set log display or no.
	    /// </summary>
	    public bool ShowLog;

	    /// <summary>
	    ///     Gets/Sets target sample frames.
	    /// </summary>
	    public int TargetSampleFrames;

	    /// <summary>
	    ///     Gets/Sets timer ruler width.
	    /// </summary>
	    public int Width;

        public bool Enabled = true;

        public static TimeRuler Instance;

	    /// <summary>
	    ///     Marker structure.
	    /// </summary>
	    private struct Marker
        {
            public int MarkerId;
            public float BeginTime;
            public float EndTime;
            public Color Color;
        }


	    /// <summary>
	    ///     Collection of markers.
	    /// </summary>
	    private class MarkerCollection
        {
            public int MarkCount;

            // Marker nest information.
            public readonly int[] MarkerNests = new int[MaxNestCall];

            // Marker collection.
            public readonly Marker[] Markers = new Marker[MaxSamples];

            public int NestCount;
        }


	    /// <summary>
	    ///     Frame logging information.
	    /// </summary>
	    private class FrameLog
        {
            public readonly MarkerCollection[] Bars;


            public FrameLog()
            {
                // Initialize markers.
                Bars = new MarkerCollection[MaxBars];
                for (var i = 0; i < MaxBars; ++i)
                    Bars[i] = new MarkerCollection();
            }
        }


	    /// <summary>
	    ///     Marker information
	    /// </summary>
	    private class MarkerInfo
        {
            // Marker log.
            public readonly MarkerLog[] Logs = new MarkerLog[MaxBars];

            // Name of marker.
            public readonly string Name;


            public MarkerInfo(string name)
            {
                this.Name = name;
            }
        }


	    /// <summary>
	    ///     Marker log information.
	    /// </summary>
	    private struct MarkerLog
        {
            public float SnapMin;
            public float SnapMax;
            public float SnapAvg;
            public float Min;
            public float Max;
            public float Avg;
            public int Samples;
            public Color Color;
            public bool Initialized;
        }

        // Logs for each frames.
        private readonly FrameLog[] _logs;

        // Previous frame log.
        private FrameLog _prevLog;

        // Current log.
        private FrameLog _curLog;

        // Current frame count.
        private int _frameCount;

        // Stopwatch for measure the time.
        private readonly Stopwatch _stopwatch = new Stopwatch();

        // Marker information array.
        private readonly List<MarkerInfo> _markers = new List<MarkerInfo>();

        // Dictionary that maps from marker name to marker id.
        private readonly Dictionary<string, int> _markerNameToIdMap = new Dictionary<string, int>();

        // Display frame adjust counter.
        private int _frameAdjust;

        // Current display frame count.
        private int _sampleFrames;

        // Marker log string.
        private readonly StringBuilder _logString = new StringBuilder(512);

        // You want to call StartFrame at beginning of Game.Update method.
        // But Game.Update gets calls multiple time when game runs slow in fixed time step mode.
        // In this case, we should ignore StartFrame call.
        // To do this, we just keep tracking of number of StartFrame calls until Draw gets called.
        private int _updateCount;


        // TimerRuler draw position.
        private Vector2 _position;

        #endregion


        #region Initialization

        static TimeRuler()
        {
            Instance = new TimeRuler();
        }


        public TimeRuler()
        {
            // Initialize Parameters.
            _logs = new FrameLog[2];
            for (var i = 0; i < _logs.Length; ++i)
                _logs[i] = new FrameLog();

            _sampleFrames = TargetSampleFrames = 1;

            Width = (int) (Core.GraphicsDevice.Viewport.Width * 0.8f);

            Core.Emitter.AddObserver(CoreEvents.GraphicsDeviceReset, OnGraphicsDeviceReset);
            OnGraphicsDeviceReset();
        }


        private void OnGraphicsDeviceReset()
        {
            var layout = new Layout(Core.GraphicsDevice.Viewport);
            _position = layout.Place(new Vector2(Width, BarHeight), 0, 0.01f, Alignment.BottomCenter);
        }


        [Command("timeruler", "Toggles the display of the TimerRuler on/off")]
        private static void ToggleTimeRuler()
        {
            Instance.ShowLog = !Instance.ShowLog;
            DebugConsole.DebugConsole.Instance.Log("TimeRuler enabled: " + (Instance.ShowLog ? "yes" : "no"));
            DebugConsole.DebugConsole.Instance.IsOpen = false;
        }

        #endregion


        #region Measuring methods

	    /// <summary>
	    ///     Start new frame.
	    /// </summary>
	    [Conditional("DEBUG")]
        public void StartFrame()
        {
            lock (this)
            {
                // We skip reset frame when this method gets called multiple times.
                var count = Interlocked.Increment(ref _updateCount);
                if (Enabled && 1 < count && count < MaxSampleFrames)
                    return;

                // Update current frame log.
                _prevLog = _logs[_frameCount++ & 0x1];
                _curLog = _logs[_frameCount & 0x1];

                var endFrameTime = (float) _stopwatch.Elapsed.TotalMilliseconds;

                // Update marker and create a log.
                for (var barIdx = 0; barIdx < _prevLog.Bars.Length; ++barIdx)
                {
                    var prevBar = _prevLog.Bars[barIdx];
                    var nextBar = _curLog.Bars[barIdx];

                    // Re-open marker that didn't get called EndMark in previous frame.
                    for (var nest = 0; nest < prevBar.NestCount; ++nest)
                    {
                        var markerIdx = prevBar.MarkerNests[nest];

                        prevBar.Markers[markerIdx].EndTime = endFrameTime;

                        nextBar.MarkerNests[nest] = nest;
                        nextBar.Markers[nest].MarkerId =
                            prevBar.Markers[markerIdx].MarkerId;
                        nextBar.Markers[nest].BeginTime = 0;
                        nextBar.Markers[nest].EndTime = -1;
                        nextBar.Markers[nest].Color = prevBar.Markers[markerIdx].Color;
                    }

                    // Update marker log.
                    for (var markerIdx = 0; markerIdx < prevBar.MarkCount; ++markerIdx)
                    {
                        var duration = prevBar.Markers[markerIdx].EndTime -
                                       prevBar.Markers[markerIdx].BeginTime;

                        var markerId = prevBar.Markers[markerIdx].MarkerId;
                        var m = _markers[markerId];

                        m.Logs[barIdx].Color = prevBar.Markers[markerIdx].Color;

                        if (!m.Logs[barIdx].Initialized)
                        {
                            // First frame process.
                            m.Logs[barIdx].Min = duration;
                            m.Logs[barIdx].Max = duration;
                            m.Logs[barIdx].Avg = duration;

                            m.Logs[barIdx].Initialized = true;
                        }
                        else
                        {
                            // Process after first frame.
                            m.Logs[barIdx].Min = System.Math.Min(m.Logs[barIdx].Min, duration);
                            m.Logs[barIdx].Max = System.Math.Min(m.Logs[barIdx].Max, duration);
                            m.Logs[barIdx].Avg += duration;
                            m.Logs[barIdx].Avg *= 0.5f;

                            if (m.Logs[barIdx].Samples++ >= LogSnapDuration)
                            {
                                m.Logs[barIdx].SnapMin = m.Logs[barIdx].Min;
                                m.Logs[barIdx].SnapMax = m.Logs[barIdx].Max;
                                m.Logs[barIdx].SnapAvg = m.Logs[barIdx].Avg;
                                m.Logs[barIdx].Samples = 0;
                            }
                        }
                    }

                    nextBar.MarkCount = prevBar.NestCount;
                    nextBar.NestCount = prevBar.NestCount;
                }

                // Start measuring.
                _stopwatch.Reset();
                _stopwatch.Start();
            }
        }


	    /// <summary>
	    ///     Start measure time.
	    /// </summary>
	    /// <param name="markerName">name of marker.</param>
	    /// <param name="color">color/param>
	    [Conditional("DEBUG")]
        public void BeginMark(string markerName, Color color)
        {
            BeginMark(0, markerName, color);
        }


	    /// <summary>
	    ///     Start measure time.
	    /// </summary>
	    /// <param name="barIndex">index of bar</param>
	    /// <param name="markerName">name of marker.</param>
	    /// <param name="color">color/param>
	    [Conditional("DEBUG")]
        public void BeginMark(int barIndex, string markerName, Color color)
        {
            lock (this)
            {
                if (barIndex < 0 || barIndex >= MaxBars)
                    throw new ArgumentOutOfRangeException("barIndex");

                var bar = _curLog.Bars[barIndex];

                if (bar.MarkCount >= MaxSamples)
                    throw new OverflowException(
                        "Exceeded sample count.\n" +
                        "Either set larger number to TimeRuler.MaxSmpale or" +
                        "lower sample count.");

                if (bar.NestCount >= MaxNestCall)
                    throw new OverflowException(
                        "Exceeded nest count.\n" +
                        "Either set larget number to TimeRuler.MaxNestCall or" +
                        "lower nest calls.");

                // Gets registered marker.
                int markerId;
                if (!_markerNameToIdMap.TryGetValue(markerName, out markerId))
                {
                    // Register this if this marker is not registered.
                    markerId = _markers.Count;
                    _markerNameToIdMap.Add(markerName, markerId);
                    _markers.Add(new MarkerInfo(markerName));
                }

                // Start measuring.
                bar.MarkerNests[bar.NestCount++] = bar.MarkCount;

                // Fill marker parameters.
                bar.Markers[bar.MarkCount].MarkerId = markerId;
                bar.Markers[bar.MarkCount].Color = color;
                bar.Markers[bar.MarkCount].BeginTime = (float) _stopwatch.Elapsed.TotalMilliseconds;

                bar.Markers[bar.MarkCount].EndTime = -1;

                bar.MarkCount++;
            }
        }


	    /// <summary>
	    ///     End measuring.
	    /// </summary>
	    /// <param name="markerName">Name of marker.</param>
	    [Conditional("DEBUG")]
        public void EndMark(string markerName)
        {
            EndMark(0, markerName);
        }


	    /// <summary>
	    ///     End measuring.
	    /// </summary>
	    /// <param name="barIndex">Index of bar.</param>
	    /// <param name="markerName">Name of marker.</param>
	    [Conditional("DEBUG")]
        public void EndMark(int barIndex, string markerName)
        {
            lock (this)
            {
                if (barIndex < 0 || barIndex >= MaxBars)
                    throw new ArgumentOutOfRangeException("barIndex");

                var bar = _curLog.Bars[barIndex];

                if (bar.NestCount <= 0)
                    throw new InvalidOperationException("Call BeingMark method before calling EndMark method.");

                int markerId;
                if (!_markerNameToIdMap.TryGetValue(markerName, out markerId))
                    throw new InvalidOperationException(
                        string.Format("Maker '{0}' is not registered." +
                                      "Make sure you specifed same name as you used for BeginMark method",
                            markerName));

                var markerIdx = bar.MarkerNests[--bar.NestCount];
                if (bar.Markers[markerIdx].MarkerId != markerId)
                    throw new InvalidOperationException(
                        "Incorrect call order of BeginMark/EndMark method." +
                        "You call it like BeginMark(A), BeginMark(B), EndMark(B), EndMark(A)" +
                        " But you can't call it like " +
                        "BeginMark(A), BeginMark(B), EndMark(A), EndMark(B).");

                bar.Markers[markerIdx].EndTime = (float) _stopwatch.Elapsed.TotalMilliseconds;
            }
        }


	    /// <summary>
	    ///     Get average time of given bar index and marker name.
	    /// </summary>
	    /// <param name="barIndex">Index of bar</param>
	    /// <param name="markerName">name of marker</param>
	    /// <returns>average spending time in ms.</returns>
	    public float GetAverageTime(int barIndex, string markerName)
        {
            if (barIndex < 0 || barIndex >= MaxBars)
                throw new ArgumentOutOfRangeException("barIndex");

            var result = 0f;
            int markerId;
            if (_markerNameToIdMap.TryGetValue(markerName, out markerId))
                result = _markers[markerId].Logs[barIndex].Avg;

            return result;
        }


	    /// <summary>
	    ///     Reset marker log.
	    /// </summary>
	    [Conditional("DEBUG")]
        public void ResetLog()
        {
            lock (this)
            {
                foreach (var markerInfo in _markers)
                    for (var i = 0; i < markerInfo.Logs.Length; ++i)
                    {
                        markerInfo.Logs[i].Initialized = false;
                        markerInfo.Logs[i].SnapMin = 0;
                        markerInfo.Logs[i].SnapMax = 0;
                        markerInfo.Logs[i].SnapAvg = 0;

                        markerInfo.Logs[i].Min = 0;
                        markerInfo.Logs[i].Max = 0;
                        markerInfo.Logs[i].Avg = 0;

                        markerInfo.Logs[i].Samples = 0;
                    }
            }
        }

        #endregion


        #region Draw

        [Conditional("DEBUG")]
        public void Render()
        {
            Render(_position, Width);
        }


        [Conditional("DEBUG")]
        public void Render(Vector2 position, int width)
        {
            // Reset update count.
            Interlocked.Exchange(ref _updateCount, 0);

            if (!ShowLog)
                return;

            // Gets Batcher, SpriteFont, and WhiteTexture from Graphics.
            var batcher = Graphics.Graphics.Instance.Batcher;
            var font = Graphics.Graphics.Instance.BitmapFont;

            // Adjust size and position based of number of bars we should draw.
            var height = 0;
            var maxTime = 0f;
            foreach (var bar in _prevLog.Bars)
                if (bar.MarkCount > 0)
                {
                    height += BarHeight + BarPadding * 2;
                    maxTime = System.Math.Max(maxTime, bar.Markers[bar.MarkCount - 1].EndTime);
                }

            // Auto display frame adjustment.
            // For example, if the entire process of frame doesn't finish in less than 16.6ms
            // then it will adjust display frame duration as 33.3ms.
            const float frameSpan = 1.0f / 60.0f * 1000f;
            var sampleSpan = _sampleFrames * frameSpan;

            if (maxTime > sampleSpan)
                _frameAdjust = System.Math.Max(0, _frameAdjust) + 1;
            else
                _frameAdjust = System.Math.Min(0, _frameAdjust) - 1;

            if (System.Math.Abs(_frameAdjust) > AutoAdjustDelay)
            {
                _sampleFrames = System.Math.Min(MaxSampleFrames, _sampleFrames);
                _sampleFrames = System.Math.Max(TargetSampleFrames, (int) (maxTime / frameSpan) + 1);

                _frameAdjust = 0;
            }

            // compute factor that converts from ms to pixel.
            var msToPs = width / sampleSpan;

            // Draw start position.
            var startY = (int) position.Y - (height - BarHeight);

            // Current y position.
            var y = startY;

            batcher.Begin();

            // Draw transparency background.
            var rc = new Rectangle((int) position.X, y, width, height);
            batcher.DrawRect(rc, new Color(0, 0, 0, 128));

            // Draw markers for each bars.
            rc.Height = BarHeight;
            foreach (var bar in _prevLog.Bars)
            {
                rc.Y = y + BarPadding;
                if (bar.MarkCount > 0)
                    for (var j = 0; j < bar.MarkCount; ++j)
                    {
                        var bt = bar.Markers[j].BeginTime;
                        var et = bar.Markers[j].EndTime;
                        var sx = (int) (position.X + bt * msToPs);
                        var ex = (int) (position.X + et * msToPs);
                        rc.X = sx;
                        rc.Width = System.Math.Max(ex - sx, 1);

                        batcher.DrawRect(rc, bar.Markers[j].Color);
                    }

                y += BarHeight + BarPadding;
            }

            // Draw grid lines.
            // Each grid represents ms.
            rc = new Rectangle((int) position.X, startY, 1, height);
            for (var t = 1.0f; t < sampleSpan; t += 1.0f)
            {
                rc.X = (int) (position.X + t * msToPs);
                batcher.DrawRect(rc, Color.Gray);
            }

            // Draw frame grid.
            for (var i = 0; i <= _sampleFrames; ++i)
            {
                rc.X = (int) (position.X + frameSpan * i * msToPs);
                batcher.DrawRect(rc, Color.White);
            }

            // Generate log string.
            y = startY - font.LineHeight;
            _logString.Length = 0;
            foreach (var markerInfo in _markers)
                for (var i = 0; i < MaxBars; ++i)
                    if (markerInfo.Logs[i].Initialized)
                    {
                        if (_logString.Length > 0)
                            _logString.Append("\n");

                        _logString.Append(" Bar ");
                        _logString.Append(i);
                        _logString.Append("   [");
                        _logString.Append(markerInfo.Name);

                        _logString.Append("] Avg.:  ");
                        _logString.Append(markerInfo.Logs[i].SnapAvg.ToString("0.0000"));
                        _logString.Append(" ms");

                        y -= font.LineHeight;
                    }

            // Compute background size and draw it.
            var size = font.MeasureString(_logString);
            rc = new Rectangle((int) position.X, y, (int) size.X + 25, (int) size.Y + 5);
            batcher.DrawRect(rc, new Color(0, 0, 0, 128));

            // Draw log string.
            batcher.DrawString(font, _logString, new Vector2(position.X + 22, y + 3), Color.White);


            // Draw log color boxes.
            y += (int) (font.LineHeight * 0.3f);
            rc = new Rectangle((int) position.X + 4, y, 10, 10);
            var rc2 = new Rectangle((int) position.X + 5, y + 1, 8, 8);
            foreach (var markerInfo in _markers)
                for (var i = 0; i < MaxBars; ++i)
                    if (markerInfo.Logs[i].Initialized)
                    {
                        rc.Y = y;
                        rc2.Y = y + 1;
                        batcher.DrawRect(rc, Color.White);
                        batcher.DrawRect(rc2, markerInfo.Logs[i].Color);

                        y += font.LineHeight;
                    }

            batcher.End();
        }

        #endregion
    }

#endif
}