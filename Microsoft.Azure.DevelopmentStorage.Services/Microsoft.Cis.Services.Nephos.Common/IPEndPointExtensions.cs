using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class IPEndPointExtensions
	{
		public static string UnmaskedToString(this IPEndPoint endPoint)
		{
			MaskedIPAddress address = endPoint.Address as MaskedIPAddress;
			if (address == null)
			{
				return endPoint.ToString();
			}
			return string.Format("{0}:{1}", address.UnmaskedToString(), endPoint.Port);
		}
	}
}