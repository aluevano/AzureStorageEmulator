using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class ParameterLimits
	{
		public readonly static TimeSpan DateHeaderLag;

		static ParameterLimits()
		{
			ParameterLimits.DateHeaderLag = TimeSpan.FromMinutes(15);
		}
	}
}