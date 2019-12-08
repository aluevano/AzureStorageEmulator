using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Service;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	[SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification="While it is sometimes undesirable to have a class with the same name as the current namespace, it makes logical sense in this situation..")]
	public abstract class ServiceManager : BaseServiceManager
	{
		public Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.AuthorizationCondition AuthorizationCondition
		{
			get;
			set;
		}

		public abstract Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		protected ServiceManager()
		{
		}

		public abstract IAsyncResult BeginAbortCopyBlob(IAccountIdentifier identifier, string account, string container, string blob, Guid copyId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginAcquireBlobLease(IAccountIdentifier identifier, string account, string container, string blob, LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginAcquireContainerLease(IAccountIdentifier identifier, string account, string container, LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginAppendBlock(IAccountIdentifier identifier, string account, string container, string blob, Stream inputStream, long contentLength, long? contentCRC64, byte[] contentMD5, bool returnCRC64, BlobObjectCondition condition, long? conditionalMaxBlobSize, long? conditionalAppendBlockPosition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginAsynchronousCopyBlob(IAccountIdentifier accessIdentifier, string destinationAccount, string destinationContainer, string destinationBlob, UriString copySource, bool isLargeBlockBlobAllowed, bool is8TBPageBlobAllowed, FECopyType copyType, DateTime? sourceSnapshot, ICopyBlobProperties copyBlobProperties, BlobObjectCondition destinationCondition, OverwriteOption destinationOverwriteOption, BlobObjectCondition sourceCondition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginBreakBlobLease(IAccountIdentifier identifier, string account, string container, string blob, TimeSpan? leaseBreakPeriod, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginBreakContainerLease(IAccountIdentifier identifier, string account, string container, TimeSpan? leaseBreakPeriod, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginChangeBlobLease(IAccountIdentifier identifier, string account, string container, string blob, Guid leaseId, Guid proposedLeaseId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginChangeContainerLease(IAccountIdentifier identifier, string account, string container, Guid leaseId, Guid proposedLeaseId, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginClearPage(IAccountIdentifier identifier, string account, string container, string blob, long offset, long length, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginCreateContainer(IAccountIdentifier identifier, string account, string container, NameValueCollection metadata, BaseAclSettings acl, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginDeleteBlob(IAccountIdentifier identifier, string account, string container, string blob, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginDeleteContainer(IAccountIdentifier identifier, string account, string container, Guid? leaseId, DateTime? ifModifiedSince, DateTime? ifNotModifiedSince, DateTime? snapshotTimestamp, bool? isRequiringNoSnapshots, bool? isDeletingOnlySnapshots, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetBlob(IAccountIdentifier identifier, string account, string container, string blob, Stream outputStream, BlobPropertyNames additionalPropertyNames, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, bool excludeNonSystemHeaders, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptBlobProperties interceptBlobPropertiesCallback, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetBlob(IAccountIdentifier identifier, string account, string container, string blob, Stream outputStream, long startOffset, long numberOfBytes, bool isCalculatingCrc64ForRange, bool isCalculatingMD5ForRange, BlobPropertyNames additionalPropertyNames, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, bool excludeNonSystemHeaders, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptBlobProperties interceptBlobPropertiesCallback, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptRangeCrc64 interceptRangeCrc64Callback, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptRangeMD5 interceptRangeMD5Callback, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetBlobMetadata(IAccountIdentifier identifier, string account, string container, string blob, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetBlobProperties(IAccountIdentifier identifier, string account, string container, string blob, bool supportCrc64, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, bool excludeNonSystemHeaders, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetBlobServiceProperties(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetBlobServiceStats(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetBlockList(IAccountIdentifier identifier, string account, string container, string blob, BlockListTypes blockListTypes, BlobServiceVersion blobServiceVersion, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetContainerAcl(IAccountIdentifier identifier, string account, string container, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetContainerMetadata(IAccountIdentifier identifier, string account, string container, DateTime? snapshot, DateTime? ifModifiedSince, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetContainerProperties(IAccountIdentifier identifier, string account, string container, DateTime? snapshot, DateTime? ifModifiedSinceTime, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginGetPageRangeList(IAccountIdentifier identifier, string account, string container, string blob, long offset, long length, BlobPropertyNames additionalPropertyNames, BlobObjectCondition condition, DateTime? snapshot, int maxPageRanges, DateTime? prevSnapshotTimestamp, bool isRangeCompressed, bool skipClearPages, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginListBlobs(IAccountIdentifier identifier, string account, string container, BlobServiceVersion version, string blobPrefix, string delimiter, string containerMarker, string blobMarker, DateTime? snapshotMarker, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, bool includeDisabledContainers, bool isFetchingMetadata, bool isIncludingSnapshots, bool isIncludingPageBlobs, bool isIncludingAppendBlobs, bool isIncludingUncommittedBlobs, bool isIncludingLeaseStatus, int? maxBlobNames, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginListContainers(IAccountIdentifier identifier, string account, string containerPrefix, string delimiter, string containerMarker, bool includeDisabledContainers, bool includeSnapshots, bool isFetchingLeaseInfo, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, int? maxContainerNames, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginPutBlob(IAccountIdentifier identifier, string account, string container, string blob, Stream inputStream, long contentLength, IPutBlobProperties putBlobProperties, bool supportCrc64, bool calculateCrc64, bool storeCrc64, bool verifyCrc64, bool calculateMd5, bool storeMd5, bool verifyMd5, bool generationIdEnabled, bool isLargeBlockBlobRequest, bool is8TBPageBlobAllowed, BlobObjectCondition condition, OverwriteOption overwriteOption, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginPutBlobFromBlocks(IAccountIdentifier identifier, string account, string container, string blob, BlobServiceVersion blobServiceVersion, byte[][] blockIdList, BlockSource[] blockSourceList, IPutBlobProperties putBlobProperties, BlobObjectCondition condition, OverwriteOption overwriteOption, IUpdateOptions updateOptions, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginPutBlock(IAccountIdentifier identifier, string account, string container, string blob, byte[] blockIdentifier, Stream inputStream, long contentLength, long? contentCRC64, byte[] contentMD5, bool isLargeBlockBlobRequest, bool supportCRC64, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginPutPage(IAccountIdentifier identifier, string account, string container, string blob, Stream inputStream, long offset, long length, long? contentCRC64, byte[] contentMD5, bool supportCRC64, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginReleaseBlobLease(IAccountIdentifier identifier, string account, string container, string blob, Guid leaseId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginReleaseContainerLease(IAccountIdentifier identifier, string account, string container, Guid leaseId, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginRenewBlobLease(IAccountIdentifier identifier, string account, string container, string blob, LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginRenewContainerLease(IAccountIdentifier identifier, string account, string container, LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginSetBlobMetadata(IAccountIdentifier identifier, string account, string container, string blob, NameValueCollection metadata, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginSetBlobProperties(IAccountIdentifier identifier, string account, string container, string blob, IPutBlobProperties properties, bool supportCrc64, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginSetBlobServiceProperties(IAccountIdentifier identifier, string ownerAccountName, AnalyticsSettings settings, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginSetContainerAcl(IAccountIdentifier identifier, string account, string container, ContainerAclSettings acl, DateTime? ifModifiedSince, DateTime? ifNotModifiedSince, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginSetContainerMetadata(IAccountIdentifier identifier, string account, string container, NameValueCollection metadata, DateTime? ifModifiedSince, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginSnapshotBlob(IAccountIdentifier identifier, string account, string container, string blob, NameValueCollection metadata, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginSynchronousCopyBlob(IAccountIdentifier accessIdentifier, string destinationAccount, string destinationContainer, string destinationBlob, string sourceAccount, string sourceContainer, string sourceBlob, UriString copySource, bool isSourceSignedAccess, bool isDestinationSignedAccess, ICopyBlobProperties copyBlobProperties, BlobObjectCondition destinationCondition, OverwriteOption destinationOverwriteOption, BlobObjectCondition sourceCondition, DateTime? sourceSnapshot, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		public abstract void EndAbortCopyBlob(IAsyncResult asyncResult);

		public abstract ILeaseInfoResult EndAcquireBlobLease(IAsyncResult asyncResult);

		public abstract ILeaseInfoResult EndAcquireContainerLease(IAsyncResult asyncResult);

		public abstract IAppendBlockResult EndAppendBlock(IAsyncResult asyncResult);

		public abstract ICopyBlobResult EndAsynchronousCopyBlob(IAsyncResult asyncResult, out TimeSpan copySourceHeadRequestRoundTripLatency);

		public abstract ILeaseInfoResult EndBreakBlobLease(IAsyncResult asyncResult);

		public abstract ILeaseInfoResult EndBreakContainerLease(IAsyncResult asyncResult);

		public abstract ILeaseInfoResult EndChangeBlobLease(IAsyncResult asyncResult);

		public abstract ILeaseInfoResult EndChangeContainerLease(IAsyncResult asyncResult);

		public abstract IClearPageResult EndClearPage(IAsyncResult asyncResult);

		public abstract DateTime EndCreateContainer(IAsyncResult ar);

		public abstract void EndDeleteBlob(IAsyncResult ar);

		public abstract void EndDeleteContainer(IAsyncResult ar);

		public abstract void EndGetBlob(IAsyncResult result);

		public abstract DateTime EndGetBlobMetadata(IAsyncResult ar, out NameValueCollection metadata);

		public abstract IBlobProperties EndGetBlobProperties(IAsyncResult ar);

		public abstract AnalyticsSettings EndGetBlobServiceProperties(IAsyncResult ar);

		public abstract GeoReplicationStats EndGetBlobServiceStats(IAsyncResult ar);

		public abstract IBlockLists EndGetBlockList(IAsyncResult result);

		public abstract DateTime EndGetContainerAcl(IAsyncResult ar, out ContainerAclSettings acl);

		public abstract IContainerProperties EndGetContainerMetadata(IAsyncResult ar);

		public abstract IContainerProperties EndGetContainerProperties(IAsyncResult ar);

		public abstract IGetPageRangeListResult EndGetPageRangeList(IAsyncResult result);

		public abstract IListBlobsResultCollection EndListBlobs(IAsyncResult ar);

		public abstract IListContainersResultCollection EndListContainers(IAsyncResult ar);

		public abstract IPutBlobResult EndPutBlob(IAsyncResult asyncResult);

		public abstract IPutBlobFromBlocksResult EndPutBlobFromBlocks(IAsyncResult asyncResult);

		public abstract IPutBlockResult EndPutBlock(IAsyncResult asyncResult);

		public abstract IPutPageResult EndPutPage(IAsyncResult asyncResult);

		public abstract ILeaseInfoResult EndReleaseBlobLease(IAsyncResult asyncResult);

		public abstract ILeaseInfoResult EndReleaseContainerLease(IAsyncResult asyncResult);

		public abstract ILeaseInfoResult EndRenewBlobLease(IAsyncResult asyncResult);

		public abstract ILeaseInfoResult EndRenewContainerLease(IAsyncResult asyncResult);

		public abstract ISetBlobMetadataResult EndSetBlobMetadata(IAsyncResult ar);

		public abstract ISetBlobPropertiesResult EndSetBlobProperties(IAsyncResult asyncResult);

		public abstract void EndSetBlobServiceProperties(IAsyncResult ar);

		public abstract DateTime EndSetContainerAcl(IAsyncResult ar);

		public abstract DateTime EndSetContainerMetadata(IAsyncResult ar);

		public abstract ISnapshotBlobResult EndSnapshotBlob(IAsyncResult ar);

		public abstract ICopyBlobResult EndSynchronousCopyBlob(IAsyncResult asyncResult);

		public delegate bool InterceptBlobProperties(IBlobProperties blobProperties);

		public delegate void InterceptRangeCrc64(long? crc64);

		public delegate void InterceptRangeMD5(byte[] md5);
	}
}