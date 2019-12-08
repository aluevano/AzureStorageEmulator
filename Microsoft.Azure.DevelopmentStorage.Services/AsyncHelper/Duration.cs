using System;
using System.Diagnostics;

namespace AsyncHelper
{
	[DebuggerStepThrough]
	public struct Duration
	{
		private readonly DateTime start;

		private readonly TimeSpan elapsedAtStopwatchStart;

		private readonly Stopwatch stopwatch;

		public TimeSpan Elapsed
		{
			get
			{
				return this.stopwatch.Elapsed + this.elapsedAtStopwatchStart;
			}
		}

		public static Duration StartingNow
		{
			get
			{
				return new Duration(0);
			}
		}

		public DateTime StartTime
		{
			get
			{
				return this.start;
			}
		}

		private Duration(int unused)
		{
			this.start = DateTime.UtcNow;
			this.elapsedAtStopwatchStart = TimeSpan.Zero;
			this.stopwatch = Stopwatch.StartNew();
		}

		private Duration(TimeSpan elapsed)
		{
			this.start = DateTime.UtcNow - elapsed;
			this.elapsedAtStopwatchStart = elapsed;
			this.stopwatch = Stopwatch.StartNew();
		}

		public override bool Equals(object obj)
		{
			bool flag;
			try
			{
				flag = this == (Duration)obj;
			}
			catch (InvalidCastException invalidCastException)
			{
				flag = false;
			}
			return flag;
		}

		public override int GetHashCode()
		{
			return this.start.GetHashCode();
		}

		public static bool operator ==(Duration left, Duration right)
		{
			return left.start == right.start;
		}

		public static bool operator !=(Duration left, Duration right)
		{
			return !(left == right);
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

		public static Duration StartingFrom(TimeSpan elapsedTime)
		{
			return new Duration(elapsedTime);
		}
	}
}