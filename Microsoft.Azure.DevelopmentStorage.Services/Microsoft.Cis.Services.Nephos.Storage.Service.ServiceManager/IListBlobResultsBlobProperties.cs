using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IListBlobResultsBlobProperties
	{
		string BlobName
		{
			get;
		}

		string BlobType
		{
			get;
		}

		string CacheControl
		{
			get;
		}

		string ContainerName
		{
			get;
		}

		string ContentCrc64
		{
			get;
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

		long? ContentLength
		{
			get;
		}

		string ContentMD5
		{
			get;
		}

		string ContentType
		{
			get;
		}

		DateTime? CopyCompletionTime
		{
			get;
		}

		string CopyId
		{
			get;
		}

		string CopyProgress
		{
			get;
		}

		string CopySource
		{
			get;
		}

		string CopyStatus
		{
			get;
		}

		string CopyStatusDescription
		{
			get;
		}

		bool IsActualBlob
		{
			get;
		}

		bool? IsBlobEncrypted
		{
			get;
		}

		bool IsIncrementalCopy
		{
			get;
		}

		DateTime? LastCopySnapshot
		{
			get;
		}

		DateTime? LastModifiedTime
		{
			get;
		}

		string LeaseDuration
		{
			get;
		}

		string LeaseState
		{
			get;
		}

		string LeaseStatus
		{
			get;
		}

		NameValueCollection Metadata
		{
			get;
		}

		long? SequenceNumber
		{
			get;
		}

		DateTime Snapshot
		{
			get;
		}
	}
}