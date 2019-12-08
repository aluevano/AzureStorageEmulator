using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListBlobsOperationContext : ListBaseObjectsOperationContext
	{
		public string ContainerMarkerListingBlobs
		{
			get;
			set;
		}

		public bool IsIncludingAccessTierInResponse
		{
			get;
			set;
		}

		public bool IsIncludingAppendBlobs
		{
			get;
			set;
		}

		public bool IsIncludingBlobTypeInResponse
		{
			get;
			set;
		}

		public bool IsIncludingCacheControlInResponse
		{
			get;
			set;
		}

		public bool IsIncludingContentDispositionInResponse
		{
			get;
			set;
		}

		public bool IsIncludingCopyPropertiesInResponse
		{
			get;
			set;
		}

		public bool IsIncludingCrc64InResponse
		{
			get;
			set;
		}

		public bool IsIncludingEncryption
		{
			get;
			set;
		}

		public bool IsIncludingIncrementalCopy
		{
			get;
			set;
		}

		public bool IsIncludingPageBlobs
		{
			get;
			set;
		}

		public bool IsIncludingSnapshots
		{
			get;
			set;
		}

		public bool IsIncludingUncommittedBlobs
		{
			get;
			set;
		}

		public bool ListingAcrossContainers
		{
			get;
			set;
		}

		public DateTime? SnapshotMarker
		{
			get;
			set;
		}

		public ListBlobsOperationContext(string version) : base(version)
		{
			base.SupportSurrogatesInListingPrefixString = true;
		}
	}
}