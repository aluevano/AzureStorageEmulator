using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class ListQueuesOperationContext
	{
		public bool IsFetchingMetadata
		{
			get;
			set;
		}

		public bool IsIncludingUrlInResponse
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

		public string Prefix
		{
			get;
			set;
		}

		public string RequestVersion
		{
			get;
			set;
		}

		public ListQueuesOperationContext(string version)
		{
			this.RequestVersion = version;
		}
	}
}