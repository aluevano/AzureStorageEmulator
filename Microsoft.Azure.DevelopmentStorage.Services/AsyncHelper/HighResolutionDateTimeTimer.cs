using System;
using System.Diagnostics;

namespace AsyncHelper
{
	internal class HighResolutionDateTimeTimer : IPerfTimer, IDisposable
	{
		private readonly Stopwatch stopWatchTimer;

		private TimeSpan elapsed;

		public TimeSpan Elapsed
		{
			get
			{
				return this.elapsed + this.stopWatchTimer.Elapsed;
			}
		}

		public bool IsRunning
		{
			get
			{
				return this.stopWatchTimer.IsRunning;
			}
		}

		public HighResolutionDateTimeTimer() : this(TimeSpan.Zero)
		{
		}

		public HighResolutionDateTimeTimer(TimeSpan elapsed)
		{
			this.stopWatchTimer = new Stopwatch();
			this.elapsed = elapsed;
		}

		public void Dispose()
		{
			if (this.stopWatchTimer.IsRunning)
			{
				this.stopWatchTimer.Stop();
			}
		}

		public TimeSpan Remaining(TimeSpan total)
		{
			TimeSpan elapsed = this.Elapsed;
			if (elapsed > total)
			{
				return TimeSpan.Zero;
			}
			return total - elapsed;
		}

		public void Reset()
		{
			this.elapsed = TimeSpan.Zero;
			this.stopWatchTimer.Reset();
		}

		public void Start()
		{
			this.stopWatchTimer.Start();
		}

		public void Stop()
		{
			this.stopWatchTimer.Stop();
		}
	}
}