using System;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class MaskedIPAddress : IPAddress
	{
		private string maskedIpAddress;

		public MaskedIPAddress(byte[] address) : base(address)
		{
			this.maskedIpAddress = (new IPAddress(this.GenerateMaskedAddress())).ToString();
		}

		private byte[] GenerateMaskedAddress()
		{
			byte[] addressBytes = base.GetAddressBytes();
			if (base.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
			{
				addressBytes[3] = 0;
			}
			return addressBytes;
		}

		public override string ToString()
		{
			return this.maskedIpAddress;
		}

		public string UnmaskedToString()
		{
			return base.ToString();
		}
	}
}