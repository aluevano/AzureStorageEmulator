using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class VnetSourceDetails
	{
		public IPAddress CustomerAddress
		{
			get;
			set;
		}

		public uint IPv6Prefix
		{
			get;
			set;
		}

		public ushort RegionId
		{
			get;
			set;
		}

		public ushort SubnetTrafficTag
		{
			get;
			set;
		}

		public uint VnetTrafficTag
		{
			get;
			set;
		}

		public VnetSourceDetails()
		{
		}
	}
}