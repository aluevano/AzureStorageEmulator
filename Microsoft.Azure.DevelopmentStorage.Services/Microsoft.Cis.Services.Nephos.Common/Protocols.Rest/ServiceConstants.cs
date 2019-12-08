using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public static class ServiceConstants
	{
		public readonly static long DefaultPutDataRateForTimeout;

		public readonly static long DefaultGetDataRateForTimeout;

		static ServiceConstants()
		{
			ServiceConstants.DefaultPutDataRateForTimeout = (long)104857;
			ServiceConstants.DefaultGetDataRateForTimeout = (long)524288;
		}
	}
}