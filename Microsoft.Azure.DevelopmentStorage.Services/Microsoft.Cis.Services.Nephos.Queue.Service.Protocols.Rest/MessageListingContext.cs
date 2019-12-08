using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class MessageListingContext
	{
		public bool IncludeInvisibleMessages
		{
			get;
			set;
		}

		public string Marker
		{
			get;
			set;
		}

		public int? MaxResults
		{
			get;
			set;
		}

		public string RequestVersion
		{
			get;
			set;
		}

		public MessageListingContext(string version)
		{
			this.RequestVersion = version;
		}
	}
}