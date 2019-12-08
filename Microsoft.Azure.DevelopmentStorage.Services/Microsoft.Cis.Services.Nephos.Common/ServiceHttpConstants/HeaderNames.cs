using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class HeaderNames
	{
		public const string AcceptLanguage = "Accept-Language";

		public const string CacheControl = "Cache-Control";

		public const string Connection = "Connection";

		public const string ContentLanguage = "Content-Language";

		public const string ContentLength = "Content-Length";

		public const string ContentType = "Content-Type";

		public const string ContentEncoding = "Content-Encoding";

		public const string ContentDisposition = "Content-Disposition";

		public const string ContentMD5 = "Content-MD5";

		public const string ContentRange = "Content-Range";

		public const string LastModifiedTime = "Last-Modified";

		public const string Server = "Server";

		public const string Allow = "Allow";

		public const string Accept = "Accept";

		public const string AcceptCharset = "Accept-Charset";

		public const string ETag = "ETag";

		public const string Expect = "Expect";

		public const string Range = "Range";

		public const string Date = "Date";

		public const string Authorization = "Authorization";

		public const string IfModifiedSince = "If-Modified-Since";

		public const string IfUnmodifiedSince = "If-Unmodified-Since";

		public const string IfMatch = "If-Match";

		public const string IfNoneMatch = "If-None-Match";

		public const string IfRange = "If-Range";

		public const string Host = "Host";

		public const string Via = "Via";

		public const string RetryAfter = "Retry-After";

		public const string Referer = "Referer";

		public const string UserAgent = "User-Agent";

		public const string AcceptRanges = "Accept-Ranges";

		public const string ProxyConnection = "Proxy-Connection";

		public const string TransferEncoding = "Transfer-Encoding";

		public const string Vary = "Vary";

		public const string Location = "Location";

		public const string Origin = "Origin";

		public const string AccessControlRequestMethod = "Access-Control-Request-Method";

		public const string AccessControlRequestHeaders = "Access-Control-Request-Headers";

		public const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";

		public const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

		public const string AccessControlAllowMethods = "Access-Control-Allow-Methods";

		public const string AccessControlMaxAge = "Access-Control-Max-Age";

		public const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";

		public const string AccessControlExposeHeaders = "Access-Control-Expose-Headers";

		public const string PrefixForNephosHeader = "x-ms-";

		public const string PrefixForNephosProperties = "x-ms-prop-";

		public const string PrefixForNephosMetadata = "x-ms-meta-";

		public const string PrefixForNephosTableContinuationToken = "x-ms-continuation-";

		public const string PrefixForNephosSource = "x-ms-source-";

		public const string PrefixForNephosTestHeader = "x-ms-test-";

		public const string NephosDateTime = "x-ms-date";

		public const string NephosRange = "x-ms-range";

		public const string NephosMultiRange = "x-ms-multi-range";

		public const string NephosCacheControl = "x-ms-blob-cache-control";

		public const string NephosContentEncoding = "x-ms-blob-content-encoding";

		public const string NephosContentDisposition = "x-ms-blob-content-disposition";

		public const string NephosContentLanguage = "x-ms-blob-content-language";

		public const string NephosContentLength = "x-ms-blob-content-length";

		public const string NephosContentMD5 = "x-ms-blob-content-md5";

		public const string NephosContentCrc64 = "x-ms-content-crc64";

		public const string NephosBlobContentCrc64 = "x-ms-blob-content-crc64";

		public const string NephosContentType = "x-ms-blob-content-type";

		public const string NephosAllowAccessDeletedResource = "x-ms-allow-access-deleted-account-container";

		public const string NephosAllowAccessToDeletingBlob = "x-ms-allow-access-to-deleting-blob";

		public const string NephosAccessTier = "x-ms-access-tier";

		public const string NephosAccessTierInferred = "x-ms-access-tier-inferred";

		public const string NephosReturnArchivePendingHeaderForTest = "x-ms-return-archive-pending-header-for-test";

		public const string ServerEncrypted = "x-ms-server-encrypted";

		public const string RequestServerEncrypted = "x-ms-request-server-encrypted";

		public const string PutBlobComputeMD5 = "x-ms-put-blob-compute-md5";

		public const string RangeGetContentCrc64 = "x-ms-range-get-content-crc64";

		public const string RangeGetContentMD5 = "x-ms-range-get-content-md5";

		public const string CreationTime = "x-ms-prop-creation-time";

		public const string NephosRequestId = "x-ms-request-id";

		public const string ClientRequestId = "x-ms-client-request-id";

		public const string NephosServerLatency = "x-ms-server-latency";

		public const string Version = "x-ms-version";

		public const string SkipToken = "x-ms-skiptoken";

		public const string MaxAccountsRecords = "x-ms-top";

		public const string ExpiryTime = "x-ms-prop-expiry-time";

		public const string NephosSystemProperties = "x-ms-system-properties";

		public const string NephosDiskTag = "x-ms-disk-tag";

		public const string NephosDiskResourceUri = "x-ms-disk-resource-uri";

		public const string NephosWriteProtection = "x-ms-write-protection";

		public const string NephosBackgroundFillCompletionTime = "x-ms-background-fill-completion-time";

		public const string NephosBackgroundFillProgress = "x-ms-background-fill-progress";

		public const string NephosBackgroundFillStatusDescription = "x-ms-background-fill-status-description";

		public const string NephosBackgroundFillStatus = "x-ms-background-fill-status";

		public const string NephosFileCacheControl = "x-ms-cache-control";

		public const string NephosFileContentEncoding = "x-ms-content-encoding";

		public const string NephosFileContentLanguage = "x-ms-content-language";

		public const string NephosFileContentLength = "x-ms-content-length";

		public const string NephosFileContentMD5 = "x-ms-content-md5";

		public const string NephosFileContentType = "x-ms-content-type";

		public const string NephosFileContentDisposition = "x-ms-content-disposition";

		public const string NephosCopyAction = "x-ms-copy-action";

		public const string NephosCopySource = "x-ms-copy-source";

		public const string NephosCopyId = "x-ms-copy-id";

		public const string NephosCopyStatus = "x-ms-copy-status";

		public const string NephosCopyStatusDescription = "x-ms-copy-status-description";

		public const string NephosCopyProgress = "x-ms-copy-progress";

		public const string NephosCopyCompletionTime = "x-ms-copy-completion-time";

		public const string NephosCopyType = "x-ms-copy-type";

		public const string NephosIncrementalCopy = "x-ms-incremental-copy";

		public const string NephosCopyDestinationSnapshot = "x-ms-copy-destination-snapshot";

		public const string SourceIfModifiedSince = "x-ms-source-if-modified-since";

		public const string SourceIfUnmodifiedSince = "x-ms-source-if-unmodified-since";

		public const string SourceIfMatch = "x-ms-source-if-match";

		public const string SourceIfNoneMatch = "x-ms-source-if-none-match";

		public const string SourceLeaseId = "x-ms-source-lease-id";

		public const string BlobConditionMaxSize = "x-ms-blob-condition-maxsize";

		public const string BlobConditionAppendPosition = "x-ms-blob-condition-appendpos";

		public const string BlobAppendOffset = "x-ms-blob-append-offset";

		public const string BlobCommittedBlockCount = "x-ms-blob-committed-block-count";

		public const string BlobExcludeNonSystemHeaders = "x-ms-exclude-non-system-headers";

		public const string DisabledStatus = "x-ms-prop-disabled";

		public const string ForceUpdate = "x-ms-force-update";

		public const string PublicAccessExpiredHeader = "x-ms-prop-publicaccess";

		public const string BlobPublicAccess = "x-ms-blob-public-access";

		public const string FilePublicAccess = "x-ms-file-public-access";

		public const string NextMarker = "x-ms-marker";

		public const string NephosBlobContentMD5ExpiredHeader = "x-ms-prop-blob-content-md5";

		public const string BlobType = "x-ms-blob-type";

		public const string NephosFileType = "x-ms-type";

		public const string PageWrite = "x-ms-page-write";

		public const string NephosFileWrite = "x-ms-write";

		public const string LeaseAction = "x-ms-lease-action";

		public const string LeaseStatus = "x-ms-lease-status";

		public const string LeaseState = "x-ms-lease-state";

		public const string LeaseId = "x-ms-lease-id";

		public const string LeaseDurationResponse = "x-ms-lease-time";

		public const string LeaseEndTime = "x-ms-lease-end-time";

		public const string LeaseDurationRequest = "x-ms-lease-duration";

		public const string ProposedLeaseId = "x-ms-proposed-lease-id";

		public const string LeaseBreakPeriod = "x-ms-lease-break-period";

		public const string DiskState = "x-ms-disk-state";

		public const string Snapshot = "x-ms-snapshot";

		public const string DeleteSnapshots = "x-ms-delete-snapshots";

		public const string SequenceNumberAction = "x-ms-sequence-number-action";

		public const string BlobSequenceNumber = "x-ms-blob-sequence-number";

		public const string SequenceNumberEqualCondition = "x-ms-if-sequence-number-eq";

		public const string SequenceNumberLessThanCondition = "x-ms-if-sequence-number-lt";

		public const string SequenceNumberLessThanOrEqualCondition = "x-ms-if-sequence-number-le";

		public const string ShareQuota = "x-ms-share-quota";

		public const string MessageId = "x-ms-prop-messageid";

		public const string ApproximateMessagesCount = "x-ms-approximate-messages-count";

		public const string PopReceiptId = "x-ms-popreceipt";

		public const string TimeNextVisible = "x-ms-time-next-visible";

		public const string GenerationId = "x-ms-generation-id";

		public const string KeyName = "x-ms-key-name";

		public const string MsiPrincipalId = "x-ms-identity-principal-id";

		public const string MsiTenantId = "x-ms-client-tenant-id";

		public const string MsiIdentityUrl = "x-ms-identity-url";

		public const string MsiClientId = "x-ms-msi-client-id";

		public const string MsiClientSecret = "x-ms-msi-client-secret";
	}
}