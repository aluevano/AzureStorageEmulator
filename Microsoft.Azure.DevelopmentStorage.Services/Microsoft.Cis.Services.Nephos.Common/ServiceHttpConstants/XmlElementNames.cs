using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class XmlElementNames
	{
		public const string PrefixForNephosMetadata = "Meta-";

		public const string BlockList = "BlockList";

		public const string CommittedBlocks = "CommittedBlocks";

		public const string UncommittedBlocks = "UncommittedBlocks";

		public const string Committed = "Committed";

		public const string Uncommitted = "Uncommitted";

		public const string Latest = "Latest";

		public const string Block = "Block";

		public const string PageList = "PageList";

		public const string PageRange = "PageRange";

		public const string ClearRange = "ClearRange";

		public const string Start = "Start";

		public const string End = "End";

		public const string EnumerationResults = "EnumerationResults";

		public const string Prefix = "Prefix";

		public const string Marker = "Marker";

		public const string MaxResults = "MaxResults";

		public const string Delimiter = "Delimiter";

		public const string NextMarker = "NextMarker";

		public const string Containers = "Containers";

		public const string Container = "Container";

		public const string Shares = "Shares";

		public const string Share = "Share";

		public const string ContainerName = "Name";

		public const string ServiceEndpointAttribute = "ServiceEndpoint";

		public const string ContainerNameAttribute = "ContainerName";

		public const string AccountNameAttribute = "AccountName";

		public const string QueueNameAttribute = "QueueName";

		public const string LastModified = "LastModified";

		public const string Etag = "Etag";

		public const string Url = "Url";

		public const string CommonPrefixes = "CommonPrefixes";

		public const string ContentType = "ContentType";

		public const string ContentEncoding = "ContentEncoding";

		public const string ContentLanguage = "ContentLanguage";

		public const string ContentCrc64 = "Content-CRC64";

		public const string ContentMD5 = "ContentMD5";

		public const string CacheControl = "CacheControl";

		public const string StandardCacheControl = "Cache-Control";

		public const string StandardContentLanguage = "Content-Language";

		public const string StandardContentLength = "Content-Length";

		public const string StandardContentType = "Content-Type";

		public const string StandardContentEncoding = "Content-Encoding";

		public const string StandardContentMD5 = "Content-MD5";

		public const string StandardContentDisposition = "Content-Disposition";

		public const string StandardLastModifiedTime = "Last-Modified";

		public const string Size = "Size";

		public const string Blobs = "Blobs";

		public const string Blob = "Blob";

		public const string BlobName = "Name";

		public const string SourceAccountName = "SourceAccountName";

		public const string DestinationAccountName = "DestinationAccountName";

		public const string InvalidMetadataName = "x-ms-invalid-name";

		public const string InvalidMetadataValue = "x-ms-invalid-value";

		public const string Quota = "Quota";

		public const string PublicAccess = "PublicAccess";

		public const string BlobContainerName = "ContainerName";

		public const string BlobPrefix = "BlobPrefix";

		public const string BlobPrefixName = "Name";

		public const string Name = "Name";

		public const string Queues = "Queues";

		public const string Queue = "Queue";

		public const string QueueName = "QueueName";

		public const string QueueMessagesList = "QueueMessagesList";

		public const string QueueMessage = "QueueMessage";

		public const string MessageId = "MessageId";

		public const string InsertionTime = "InsertionTime";

		public const string PopReceipt = "PopReceipt";

		public const string TimeNextVisible = "TimeNextVisible";

		public const string MessageText = "MessageText";

		public const string VisibilityTimeOut = "VisibilityTimeOut";

		public const string MessageTTL = "MessageTTL";

		public const string QueueProperties = "QueueProperties";

		public const string ApproximateMessagesCount = "ApproximateMessagesCount";

		public const string DequeueCount = "DequeueCount";

		public const string ExpirationTime = "ExpirationTime";

		public const string Ranges = "Ranges";

		public const string Range = "Range";

		public const string Entries = "Entries";

		public const string File = "File";

		public const string Directory = "Directory";

		public const string DirectoryPath = "DirectoryPath";

		public const string ShareName = "ShareName";

		public const string ShareSnapshot = "ShareSnapshot";

		public const string TableClientContinuationTokenPrefix = "Next";

		public const string Atom = "atom";

		public const string ErrorRootElement = "Error";

		public const string ErrorCode = "Code";

		public const string ErrorMessage = "Message";

		public const string ErrorException = "ExceptionDetails";

		public const string ErrorExceptionMessage = "ExceptionMessage";

		public const string ErrorExceptionStackTrace = "StackTrace";

		public const string AuthenticationErrorDetail = "AuthenticationErrorDetail";

		public const string ListContainerSummary = "ListContainerSummary";

		public const string ListObjectSummary = "ListObjectSummary";

		public const string ListBlobSummary = "ListBlobSummary";

		public const string Count = "Count";

		public const string TotalMetadataSize = "TotalMetadataSize";

		public const string TotalContentSize = "TotalContentSize";

		public const string NextContainerName = "NextContainerName";

		public const string NextBlobName = "NextBlobName";

		public const string NextSnapshot = "NextSnapshot";

		public const string BlobType = "BlobType";

		public const string LeaseStatus = "LeaseStatus";

		public const string LeaseState = "LeaseState";

		public const string LeaseDuration = "LeaseDuration";

		public const string Snapshot = "Snapshot";

		public const string MetadataRoot = "Metadata";

		public const string Properties = "Properties";

		public const string BlobSequenceNumber = "x-ms-blob-sequence-number";

		public const string CopyId = "CopyId";

		public const string CopySource = "CopySource";

		public const string CopyStatus = "CopyStatus";

		public const string CopyStatusDescription = "CopyStatusDescription";

		public const string CopyProgress = "CopyProgress";

		public const string CopyCompletionTime = "CopyCompletionTime";

		public const string ServerEncrypted = "ServerEncrypted";

		public const string IncrementalCopy = "IncrementalCopy";

		public const string CopyDestinationSnapshot = "CopyDestinationSnapshot";

		public const string AccessTier = "AccessTier";

		public const string ArchiveStatus = "ArchiveStatus";

		public const string SignedIdentifiers = "SignedIdentifiers";

		public const string SignedIdentifier = "SignedIdentifier";

		public const string SignedIdentifierId = "Id";

		public const string SignedAccessPolicy = "AccessPolicy";

		public const string SignedStart = "Start";

		public const string SignedExpiry = "Expiry";

		public const string SignedPermission = "Permission";
	}
}