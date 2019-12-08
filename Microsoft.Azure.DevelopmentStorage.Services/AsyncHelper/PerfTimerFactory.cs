using System;
using System.Diagnostics;

namespace AsyncHelper
{
	public static class PerfTimerFactory
	{
		public static IPerfTimer CreateTimer(bool isHighResolution, TimeSpan elapsed)
		{
			if (isHighResolution && Stopwatch.IsHighResolution)
			{
				return new HighResolutionDateTimeTimer(elapsed);
			}
			return new DateTimeTimer(elapsed);
		}
	}
}