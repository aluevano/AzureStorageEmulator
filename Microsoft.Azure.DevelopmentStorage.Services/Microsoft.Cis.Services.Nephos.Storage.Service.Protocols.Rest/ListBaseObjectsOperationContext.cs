using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListBaseObjectsOperationContext : ListOperationContext
	{
		public bool IncludeDisabledContainers
		{
			get;
			set;
		}

		public DateTime? IncludeIfModifiedSince
		{
			get;
			set;
		}

		public DateTime? IncludeIfNotModifiedSince
		{
			get;
			set;
		}

		public bool IsFetchingMetadata
		{
			get;
			set;
		}

		public bool IsIncludingLeaseStateAndDurationInResponse
		{
			get;
			set;
		}

		public bool IsIncludingLeaseStatusInResponse
		{
			get;
			set;
		}

		public bool IsIncludingPublicAccessInResponse
		{
			get;
			set;
		}

		public bool IsUsingPropertiesElement
		{
			get;
			set;
		}

		public ListBaseObjectsOperationContext(string version) : base(version)
		{
		}
	}
}