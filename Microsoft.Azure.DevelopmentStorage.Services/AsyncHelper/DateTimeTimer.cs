using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AsyncHelper
{
	[DebuggerStepThrough]
	public class DateTimeTimer : IPerfTimer
	{
		private uint startMillisecond;

		private TimeSpan elapsed;

		public TimeSpan Elapsed
		{
			get
			{
				this.UpdateElapsedTime();
				return this.elapsed;
			}
		}

		public bool IsRunning
		{
			get
			{
				return JustDecompileGenerated_get_IsRunning();
			}
			set
			{
				JustDecompileGenerated_set_IsRunning(value);
			}
		}

		private bool JustDecompileGenerated_IsRunning_k__BackingField;

		public bool JustDecompileGenerated_get_IsRunning()
		{
			return this.JustDecompileGenerated_IsRunning_k__BackingField;
		}

		private void JustDecompileGenerated_set_IsRunning(bool value)
		{
			this.JustDecompileGenerated_IsRunning_k__BackingField = value;
		}

		public DateTimeTimer() : this(TimeSpan.Zero)
		{
		}

		public DateTimeTimer(TimeSpan elapsed)
		{
			this.elapsed = elapsed;
			this.IsRunning = false;
		}

		public TimeSpan Remaining(TimeSpan total)
		{
			if (TimeSpan.MaxValue == total)
			{
				return TimeSpan.MaxValue;
			}
			TimeSpan elapsed = this.Elapsed;
			if (elapsed > total)
			{
				return TimeSpan.Zero;
			}
			return total - elapsed;
		}

		public void Reset()
		{
			this.IsRunning = false;
			this.elapsed = TimeSpan.Zero;
		}

		public void Start()
		{
			if (!this.IsRunning)
			{
				this.startMillisecond = DateTimeTimer.NativeMethods.timeGetTime();
				this.IsRunning = true;
			}
		}

		public static DateTimeTimer StartNew()
		{
			return DateTimeTimer.StartNewFrom(TimeSpan.Zero);
		}

		public static DateTimeTimer StartNewFrom(TimeSpan elapsed)
		{
			DateTimeTimer dateTimeTimer = new DateTimeTimer(elapsed);
			dateTimeTimer.Start();
			return dateTimeTimer;
		}

		public void Stop()
		{
			this.UpdateElapsedTime();
			this.IsRunning = false;
		}

		private void UpdateElapsedTime()
		{
			if (this.IsRunning)
			{
				uint num = DateTimeTimer.NativeMethods.timeGetTime();
				this.elapsed += TimeSpan.FromMilliseconds((double)((float)(num - this.startMillisecond)));
				this.startMillisecond = num;
			}
		}

		private static class NativeMethods
		{
			[DllImport("winmm.dll", CharSet=CharSet.None, ExactSpelling=false)]
			public static extern uint timeGetTime();
		}
	}
}