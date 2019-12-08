using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class IPAddressExtensions
	{
		public static int CompareTo(this IPAddress ip, IPAddress otherIp)
		{
			if (ip.AddressFamily != otherIp.AddressFamily)
			{
				throw new ArgumentException("Cannot compare two addresses of different address families");
			}
			byte[] addressBytes = ip.GetAddressBytes();
			byte[] numArray = otherIp.GetAddressBytes();
			for (int i = 0; i < (int)addressBytes.Length; i++)
			{
				if (addressBytes[i] < numArray[i])
				{
					return -1;
				}
				if (addressBytes[i] > numArray[i])
				{
					return 1;
				}
			}
			return 0;
		}
	}
}