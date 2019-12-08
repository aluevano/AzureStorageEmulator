using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlobObject : IDisposable
	{
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Each property provides a direct 1-to-1 mapping with the XStore API, so seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		byte[] ApplicationMetadata
		{
			get;
		}

		int? CommittedBlockCount
		{
			get;
		}

		string ContainerName
		{
			get;
		}

		long? ContentCrc64
		{
			get;
		}

		long? ContentLength
		{
			get;
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Each property provides a direct 1-to-1 mapping with the XStore API, so seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		byte[] ContentMD5
		{
			get;
		}

		string ContentType
		{
			get;
		}

		DateTime? CreationTime
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

		bool? IsWriteEncrypted
		{
			get;
		}

		DateTime? LastModificationTime
		{
			get;
		}

		ILeaseInfo LeaseInfo
		{
			get;
		}

		string Name
		{
			get;
		}

		Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get;
			set;
		}

		long? SequenceNumber
		{
			get;
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Each property provides a direct 1-to-1 mapping with the XStore API, so seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		byte[] ServiceMetadata
		{
			get;
		}

		DateTime Snapshot
		{
			get;
		}

		TimeSpan Timeout
		{
			get;
			set;
		}

		BlobType Type
		{
			get;
		}

		IAsyncResult BeginAbortCopyBlob(Guid copyId, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginAcquireLease(LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginAsynchronousCopyBlob(UriString copySource, bool isSourceAzureBlob, FECopyType copyType, long contentLength, string contentType, NameValueCollection serviceMetadataCollection, byte[] applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, string sourceETag, IBlobObjectCondition destinationCondition, AsyncCallback callback, object state);

		IAsyncResult BeginBreakLease(TimeSpan? leaseBreakPeriod, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginChangeLease(Guid leaseId, Guid proposedLeaseId, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginGetBlob(IBlobRegion blobRegion, BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginGetProperties(BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginPutBlob(string contentType, long contentLength, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, Stream inputStream, CrcReaderStream crcReaderStream, byte[] contentMD5, bool invokeGeneratePutBlobServiceMetadata, GeneratePutBlobServiceMetadata generatePutBlobServiceMetadata, bool isLargeBlockBlobRequest, bool is8TBPageBlobAllowed, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginReleaseLease(Guid leaseId, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginRenewLease(LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginSetApplicationMetadata(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginSetExpiryTime(DateTime expiryTime, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginSetProperties(string contentType, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginSetProperties(string contentType, long? maxBlobSize, NameValueCollection serviceMetadata, ServiceMetadataUpdatePolicy serviceMetadataPolicy, string[] serviceMetadataPolicyArguments, NameValueCollection applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginSetServiceMetadata(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginSnapshotBlob(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginSynchronousCopyBlob(string sourceAccount, IBlobObject sourceBlob, DateTime? expiryTime, byte[] applicationMetadata, OverwriteOption overwriteOption, IBlobObjectCondition sourceCondition, IBlobObjectCondition destinationCondition, UriString copySource, AsyncCallback callback, object state);

		void EndAbortCopyBlob(IAsyncResult asyncResult);

		void EndAcquireLease(IAsyncResult asyncResult);

		CopyBlobOperationInfo EndAsynchronousCopyBlob(IAsyncResult asyncResult);

		void EndBreakLease(IAsyncResult asyncResult);

		void EndChangeLease(IAsyncResult asyncResult);

		CrcStream EndGetBlob(IAsyncResult asyncResult);

		void EndGetProperties(IAsyncResult ar);

		void EndPutBlob(IAsyncResult asyncResult);

		void EndReleaseLease(IAsyncResult asyncResult);

		void EndRenewLease(IAsyncResult asyncResult);

		void EndSetApplicationMetadata(IAsyncResult ar);

		void EndSetExpiryTime(IAsyncResult ar);

		void EndSetProperties(IAsyncResult asyncResult);

		void EndSetServiceMetadata(IAsyncResult ar);

		DateTime EndSnapshotBlob(IAsyncResult ar);

		CopyBlobOperationInfo EndSynchronousCopyBlob(IAsyncResult asyncResult);
	}
}