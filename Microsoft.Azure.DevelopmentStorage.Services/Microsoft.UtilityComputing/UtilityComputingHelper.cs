using System;
using System.Text;

namespace Microsoft.UtilityComputing
{
	public sealed class UtilityComputingHelper
	{
		private readonly static TimeSpan TimeoutAlertThresholdInSeconds;

		private readonly static TimeSpan AlertThresholdForTimeoutAlert;

		private static DateTime s_lastTimoutAlertOccurrence;

		private static object s_alertSyncObject;

		private static int s_timoutCountSinceLastAlert;

		public readonly static TimeSpan InfiniteTimeoutTimeSpan;

		static UtilityComputingHelper()
		{
			UtilityComputingHelper.TimeoutAlertThresholdInSeconds = TimeSpan.FromSeconds(150);
			UtilityComputingHelper.AlertThresholdForTimeoutAlert = TimeSpan.FromHours(1);
			UtilityComputingHelper.s_lastTimoutAlertOccurrence = DateTime.MinValue;
			UtilityComputingHelper.s_alertSyncObject = new object();
			UtilityComputingHelper.s_timoutCountSinceLastAlert = 0;
			UtilityComputingHelper.InfiniteTimeoutTimeSpan = TimeSpan.MaxValue;
		}

		private UtilityComputingHelper()
		{
		}

		public static DateTime GetEndTime(TimeSpan timeout)
		{
			if (timeout == TimeSpan.MaxValue)
			{
				return DateTime.MaxValue;
			}
			DateTime utcNow = DateTime.UtcNow;
			if ((DateTime.MaxValue - utcNow) < timeout)
			{
				return DateTime.MaxValue;
			}
			return utcNow + timeout;
		}

		public static DateTime GetEndTime(int timeout)
		{
			if (timeout == -1)
			{
				return DateTime.MaxValue;
			}
			return UtilityComputingHelper.GetEndTime(TimeSpan.FromMilliseconds((double)timeout));
		}

		public static TimeSpan GetRemainingTime(DateTime endTime)
		{
			if (endTime == DateTime.MaxValue)
			{
				return UtilityComputingHelper.InfiniteTimeoutTimeSpan;
			}
			TimeSpan timeSpan = endTime - DateTime.UtcNow;
			if (timeSpan < TimeSpan.Zero)
			{
				return TimeSpan.Zero;
			}
			return timeSpan;
		}

		public static int GetRemainingTimeMilliseconds(DateTime endTime)
		{
			TimeSpan remainingTime = UtilityComputingHelper.GetRemainingTime(endTime);
			if (remainingTime == UtilityComputingHelper.InfiniteTimeoutTimeSpan)
			{
				return -1;
			}
			return (int)remainingTime.TotalMilliseconds;
		}

		public static int GetRemainingTimeOrThrow(DateTime endTime)
		{
			int remainingTimeMilliseconds = UtilityComputingHelper.GetRemainingTimeMilliseconds(endTime);
			if (remainingTimeMilliseconds == 0)
			{
				if ((DateTime.UtcNow - endTime).TotalSeconds > UtilityComputingHelper.TimeoutAlertThresholdInSeconds.TotalSeconds)
				{
					lock (UtilityComputingHelper.s_alertSyncObject)
					{
						UtilityComputingHelper.s_timoutCountSinceLastAlert++;
					}
				}
				throw new TimeoutException();
			}
			return remainingTimeMilliseconds;
		}

		public static string MakeMaxVersionedName(string name, out bool isVersioned)
		{
			isVersioned = false;
			StringBuilder stringBuilder = new StringBuilder(name);
			if (stringBuilder.ToString().IndexOf('\u0001') == -1)
			{
				stringBuilder.Append("\u0001￿￿￿￿￿￿￿￿￿￿￿￿￿￿￿￿");
			}
			else
			{
				isVersioned = true;
			}
			return stringBuilder.ToString();
		}
	}
}