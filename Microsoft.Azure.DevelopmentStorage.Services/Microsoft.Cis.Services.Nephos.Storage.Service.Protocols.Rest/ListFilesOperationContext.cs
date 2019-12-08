using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListFilesOperationContext : ListOperationContext
	{
		public DateTime? SnapshotMarker
		{
			get;
			set;
		}

		public ListFilesOperationContext(string version) : base(version)
		{
			base.SupportSurrogatesInListingPrefixString = true;
		}
	}
}