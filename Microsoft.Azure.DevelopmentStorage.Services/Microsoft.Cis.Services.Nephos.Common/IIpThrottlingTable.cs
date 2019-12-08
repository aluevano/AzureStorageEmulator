using System;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public interface IIpThrottlingTable
	{
		bool ShouldDropNewRequest(IPAddress ipAddress, out bool shouldAlert, out double additionalInfo);
	}
}