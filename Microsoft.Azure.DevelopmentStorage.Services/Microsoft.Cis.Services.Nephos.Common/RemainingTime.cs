using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public struct RemainingTime
	{
		private static TimeSpan defaultTimeout;

		private TimeSpan timeout;

		private uint startMillisecond;

		static RemainingTime()
		{
			RemainingTime.defaultTimeout = TimeSpan.FromSeconds(30);
		}

		public RemainingTime(TimeSpan? ts)
		{
			TimeSpan? nullable = ts;
			this((nullable.HasValue ? nullable.GetValueOrDefault() : RemainingTime.defaultTimeout));
		}

		public RemainingTime(TimeSpan ts)
		{
			this.timeout = ts;
			this.startMillisecond = NativeMethods.timeGetTime();
		}

		public static implicit operator TimeSpan(RemainingTime rt)
		{
			uint num = NativeMethods.timeGetTime() - rt.startMillisecond;
			TimeSpan timeSpan = rt.timeout - TimeSpan.FromMilliseconds((double)((float)num));
			if (timeSpan < TimeSpan.Zero)
			{
				return TimeSpan.Zero;
			}
			return timeSpan;
		}
	}
}