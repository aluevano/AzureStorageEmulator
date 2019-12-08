using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IPutBlobProperties
	{
		string BlobLinkSource
		{
			get;
		}

		NameValueCollection BlobMetadata
		{
			get;
		}

		Microsoft.Cis.Services.Nephos.Common.Storage.BlobType BlobType
		{
			get;
		}

		string CacheControl
		{
			get;
		}

		long? ContentCrc64
		{
			get;
			set;
		}

		string ContentDisposition
		{
			get;
		}

		string ContentEncoding
		{
			get;
		}

		string ContentLanguage
		{
			get;
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		byte[] ContentMD5
		{
			get;
			set;
		}

		string ContentType
		{
			get;
		}

		DateTime? CreationTime
		{
			get;
		}

		Guid GenerationId
		{
			get;
			set;
		}

		DateTime? LastModifiedTime
		{
			get;
		}

		ILeaseInfo LeaseInfo
		{
			get;
		}

		long? MaxBlobSize
		{
			get;
		}

		Microsoft.Cis.Services.Nephos.Common.Storage.SequenceNumberUpdate SequenceNumberUpdate
		{
			get;
		}
	}
}