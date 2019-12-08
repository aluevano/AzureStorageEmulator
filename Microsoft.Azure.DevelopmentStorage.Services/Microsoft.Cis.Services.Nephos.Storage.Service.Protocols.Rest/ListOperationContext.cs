using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListOperationContext
	{
		public string Delimiter
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

		public bool SupportSurrogatesInListingPrefixString
		{
			get;
			set;
		}

		public ListOperationContext(string version)
		{
			this.RequestVersion = version;
		}
	}
}