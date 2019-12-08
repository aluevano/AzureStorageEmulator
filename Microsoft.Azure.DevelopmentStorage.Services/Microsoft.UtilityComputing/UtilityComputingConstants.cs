using System;

namespace Microsoft.UtilityComputing
{
	public static class UtilityComputingConstants
	{
		public const string UtilityTablesName = "XTableContainers";

		public const int UtilityTablesVersion = 2;

		public const int UtilityTablesVersionV1 = 1;

		public const string UtilityTablesEntityName = "Tables";

		public const string TablesTable = "Tables";

		public const string PartitionsTable = "Partitions";

		public const int TablesTableVersion = 2;

		public const int TablesTableAccountNameColumnIndex = 0;

		public const string UtilityTableName = "XUtilityRows";

		public const string XioUtilityTableName = "XioUtilityRows";

		public const int MaxAllowedPartitionKeyOccurencesInQueryFilterForPremiumTable = 1;

		public const string ProvisionedIOPS = "ProvisionedIOPS";

		public const string RequestedIOPS = "RequestedIOPS";

		public const string HashAlgorithm = "HashAlgorithm";

		public const string TableStatus = "TableStatus";

		public const int UtilityTableVersion = 1;

		public const int XioUtilityTableVersion = 1;

		public const string BlobContainersTableName = "XBlobContainers";

		public const int BlobContainersTableVersion = 2;

		public const string BlobTableName = "XBlobObjects";

		public const int BlobTableVersion = 2;

		public const string FileContainersTableName = "XFileContainers";

		public const int FileContainersTableVersion = 1;

		public const string FileTableName = "XFiles";

		public const int FileTableVersion = 1;

		public const string MessageTableName = "XMessageObjects2";

		public const int MessageTableVersion = 1;

		public const string MessageContainersTableName = "XQueueContainers";

		public const int MessageContainersTableVersion = 3;

		public const string AccountsTableName = "XStoreAccounts";

		public const int AccountsTableVersion = 3;

		public const string AccountColumnName = "AccountName";

		public const string TableColumnName = "TableName";

		public const string PartitionKeyColumnName = "PartitionKey";

		public const string HashKeyColumnName = "PartitionKeyHash";

		public const string RowKeyColumnName = "RowKey";

		public const int UtilityTablePrivateKeyCount = 2;

		public const int UtilityTablesPrivateKeyCount = 1;

		public const int UtilityTableAccountNameColumnIndex = 0;

		public const int UtilityTableTableNameColumnIndex = 1;

		public const int UtilityTableUserPartitionKeyColumnIndex = 2;

		public const int XioUtilityTablePartitionKeyHashColumnIndex = 2;

		public const int XioUtilityTableUserPartitionKeyColumnIndex = 3;

		public const int UtilityTableUserRowKeyColumnIndex = 3;

		public const int XioUtilityTableUserRowKeyColumnIndex = 4;

		public const string TimestampColumnName = "Timestamp";

		public const string ColumnValuesColumnName = "ColumnValues";

		public const int XBlobObjectColumnAccountNameIndex = 0;

		public const int XBlobObjectColumnContainerNameIndex = 1;

		public const int XBlobObjectColumnBlobNameIndex = 2;

		public const int XBlobObjectColumnBlobIndex = 3;

		public const int XBlobObjectColumnVersionTimeStampIndex = 4;

		public const int XBlobObjectColumnVersionNumberIndex = 5;

		public const int XBlobObjectColumnFlagsIndex = 6;

		public const int XBlobObjectColumnTypeIndex = 7;

		public const int XBlobObjectColumnSizeIndex = 8;

		public const int XBlobObjectColumnDeltaSizeIndex = 9;

		public const int XBlobObjectColumnAnchorIndex = 10;

		public const int XBlobObjectColumnAnchorSizeIndex = 11;

		public const int XBlobObjectColumnLeaseIndex = 12;

		public const string XBlobObjectColumnAccountName = "AccountName";

		public const string XBlobObjectColumnContainerName = "ContainerName";

		public const string XBlobObjectColumnBlobName = "BlobName";

		public const string XBlobObjectColumnBlob = "Blob";

		public const string XBlobObjectColumnVersionTimeStamp = "VersionTimestamp";

		public const string XBlobObjectColumnVersionNumber = "VersionNumber";

		public const string XBlobObjectColumnFlags = "Flags";

		public const string XBlobObjectColumnType = "Type";

		public const string XBlobObjectColumnSize = "Size";

		public const string XBlobObjectColumnDeltaSize = "DeltaSize";

		public const string XBlobObjectColumnAnchor = "Anchor";

		public const string XBlobObjectColumnAnchorSize = "AnchorSize";

		public const string XBlobObjectColumnLease = "Lease";

		public const int XBlobFieldContentTypeIndex = 0;

		public const int XBlobFieldContentLengthIndex = 1;

		public const int XBlobFieldCreationTimeIndex = 2;

		public const int XBlobFieldLastModificationTimeIndex = 3;

		public const int XBlobFieldExpiryTimeIndex = 4;

		public const int XBlobFieldServiceMetadataIndex = 5;

		public const int XBlobFieldApplicationMetadataIndex = 6;

		public const int XBlobFieldBlobHeaderPointerIndex = 7;

		public const int XBlobFieldTblPointerIndex = 8;

		public const int XBlobFieldTblLastModificationTimeIndex = 9;

		public const int XBlobFieldBlockListUsedIndex = 10;

		public const int XLeaseFieldLeaseTypeIndex = 0;

		public const int XLeaseFieldLeaseSessionIdIndex = 1;

		public const int XLeaseFieldLeaseDurationIndex = 2;

		public const int XLeaseFieldLastLeaseEndTimeIndex = 3;

		public const int XLeaseFieldLeaseOverrideFlagIndex = 4;

		public const string XBLOB_METHOD_GET_BLOB = "GetBlob";

		public const string XBLOB_METHOD_GET_BLOCK_LIST = "GetBlockList";

		public const string XBLOB_METHOD_GET_COMMITTED_BLOCK_LIST = "GetCommittedBlockList";

		public const string XBLOB_METHOD_GET_TEMPORARY_BLOCK_LIST = "GetTemporaryBlockList";

		public const string XBLOB_METHOD_GET_BLOCK_RECORD_LIST = "GetBlockRecordList";

		public const string XBLOB_METHOD_GET_BLOCK_METADATA_LIST = "GetBlockMetadataList";

		public const string XBLOB_METHOD_GET_MULTIPLE_BLOCK_METADATA_LIST = "GetMultipleBlockMetadataList";

		public const string XBLOB_METHOD_PUT_BLOB = "PutBlob";

		public const string XBLOB_METHOD_PUT_BLOCK = "PutBlock";

		public const string XBLOB_METHOD_PUT_BLOCK_LIST = "PutBlockList";

		public const string XBLOB_METHOD_PUT_BLOCK_LIST_WITH_UPDATE = "PutBlockListWithUpdate";

		public const string XBLOB_METHOD_PUT_BLOCK_LIST_WITH_BLOCK_VERSIONS = "PutBlockListWithBlockVersions";

		public const string XBLOB_METHOD_COPY_BLOB = "CopyBlob";

		public const string XBLOB_METHOD_PROMOTE_SNAPSHOT = "PromoteSnapshot";

		public const string XBLOB_METHOD_DEFRAGMENT_TBL = "DefragmentTB";

		public const string XBLOB_METHOD_PUT_PAGE = "PutPage";

		public const string XBLOB_METHOD_PUT_PAGE_WITH_VERSION = "PutPageWithVersion";

		public const string XBLOB_METHOD_CLEAR_PAGE = "ClearPage";

		public const string XBLOB_METHOD_GET_PAGE_RANGES = "GetPageRanges";

		public const string XBLOB_METHOD_GET_MULTI_PAGE_RANGES = "GetMultiplePageRanges";

		public const string XBLOB_METHOD_GET_PAGE_STREAM_NAMES = "GetPageStreamNames";

		public const string XBLOB_METHOD_GET_INDEX_BLOB_COPY_INFORMATION = "GetIndexBlobCopyInformation";

		public const string XBLOB_METHOD_COPY_INDEX_BLOB = "CopyIndexBlob";

		public const string XBLOB_METHOD_SET_INDEX_BLOB_SEQUENCE_NUMBER = "SetIndexBlobSequenceNumber";

		public const string XBLOB_METHOD_ADJUST_INDEX_BLOB_SIZE = "AdjustIndexBlobSize";

		public const string XBLOB_METHOD_MARK_BLOB_FOR_DELETION = "MarkBlobForDeletion";

		public const string XBLOB_METHOD_GET_COMMITTED_BLOCK_COUNT = "GetCommittedBlockCount";

		public const string XBLOB_METHOD_GET_BLOB_LMT_SEQUENCE_NUMBER = "GetBlobLmtAndSequenceNumber";

		public const string XBLOB_METHOD_ADD_COPY_BLOB = "AddCopyBlob";

		public const string XBLOB_METHOD_CREATE_BLOB_SNAPSHOT = "CreateBlobSnapshot";

		public const string XBLOB_METHOD_DELETE_BLOB_SNAPSHOT = "DeleteBlobSnapshot";

		public const string XBLOB_METHOD_GET_SNAPSHOT_TIMESTAMP = "GetSnapshotTimestamp";

		public const string XBLOB_METHOD_COPY_START = "CopyStart";

		public const string XBLOB_METHOD_COPY_END = "CopyEnd";

		public const string XBLOB_METHOD_BLOB_TIER_CHANGE = "BlobTierChange";

		public const string XBLOB_METHOD_ARCHIVE_COMPLETION = "BlobArchiveCompletion";

		public const string GET_IS_IN_STOP_SEND = "GET_IS_IN_STOP_SEND";

		public const string GET_GEO_SENDER_INFO = "GET_GEO_SENDER_INFO";

		public const uint XBLOB_TYPE_LIST_BLOB = 1;

		public const uint XBLOB_TYPE_INDEX_BLOB = 2;

		public const int XStoreAccountsColumnXStoreAccountNameIndex = 0;

		public const int XStoreAccountsColumnRDAccountNameIndex = 1;

		public const int XStoreAccountsColumnAccessPermissionsIndex = 2;

		public const int XStoreAccountsColumnStateIndex = 3;

		public const int XStoreAccountsColumnIsAdminIndex = 4;

		public const int XStoreAccountsColumnLastModificationTimeIndex = 5;

		public const int XStoreAccountsColumnExpiryTimeIndex = 6;

		public const int XStoreAccountsColumnSecretKeysIndex = 7;

		public const int XStoreAccountsColumnServiceMetadataIndex = 8;

		public const int XStoreAccountsColumnGeoRegionIndex = 9;

		public const int XStoreAccountsColumnLeaseIndex = 10;

		public const string XStoreAccountsColumnXStoreAccountName = "XStoreAccountName";

		public const string XStoreAccountsColumnRDAccountName = "RDAccountName";

		public const string XStoreAccountsColumnAccessPermissions = "AccessPermissions";

		public const string XStoreAccountsColumnState = "State";

		public const string XStoreAccountsColumnIsAdmin = "IsAdmin";

		public const string XStoreAccountsColumnLastModificationTime = "LastModificationTime";

		public const string XStoreAccountsColumnExpiryTime = "ExpiryTime";

		public const string XStoreAccountsColumnSecretKeys = "SecretKeys";

		public const string XStoreAccountsColumnServiceMetadata = "ServiceMetadata";

		public const string XStoreAccountsColumnGeoRegion = "GeoRegion";

		public const string XStoreAccountsColumnLease = "Lease";

		public const string XFilesHandlesAccountNameColumnName = "AccountName";

		public const string XFilesHandlesContainerNameColumnName = "ContainerName";

		public const string XFilesHandlesTableTypeColumnName = "TableType";

		public const string XFilesHandlesKeyFileIdColumnName = "KeyFileId";

		public const string XFilesHandlesKeyParentIdColumnName = "KeyParentId";

		public const string XFilesHandlesBlobNameColumnName = "BlobName";

		public const string XFilesHandlesHandleIdColumnName = "HandleId";

		public const string XFilesHandlesFileIdColumnName = "FileId";

		public const string XFilesHandlesParentIdColumnName = "ParentId";

		public const string XFilesHandlesContainerVersionColumnName = "ContainerVersion";

		public const string XFilesHandlesSessionIdColumnName = "SessionId";

		public const string XFilesHandlesAccountCreationTimeColumnName = "AccountCreationTime";

		public const string XFilesHandlesFrontEndIdColumnName = "FrontEndId";

		public const string XFilesHandlesRestartCountColumnName = "RestartCount";

		public const string XFilesHandlesCreateIdColumnName = "CreateId";

		public const string XFilesHandlesHandleFlagsColumnName = "HandleFlags";

		public const string XFilesHandlesClientGuidColumnName = "ClientGuid";

		public const string XFilesHandlesLeaseKeyColumnName = "LeaseKey";

		public const string XFilesHandlesTimeoutColumnName = "Timeout";

		public const string XFilesHandlesValidUntilColumnName = "ValidUntil";

		public const string XFilesHandlesRenameLinkColumnName = "RenameLink";

		public const string XFilesHandlesLockSequenceColumnName = "LockSequence";

		public const string XFilesHandlesDictionaryColumnName = "Dictionary";

		public const ulong XFILE_DEFAULT_FILE_ID = 0L;

		public const string TimeStampFormat = "yyyyMMdd hh:mm:ss.fffffff";

		internal const string AccessPermissionsColumnName = "AccessPermissions";

		internal const string IsValidColumnName = "IsValid";

		internal const string ExpiryTimeColumnName = "ExpiryTime";

		internal const string MaxVersionedNameSuffix = "\u0001￿￿￿￿￿￿￿￿￿￿￿￿￿￿￿￿";

		internal const char MaxVersionedNameDelimiter = '\u0001';

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_VERSIONTIMESTAMP = "VersionTimestamp";

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_TYPE = "ArchiveType";

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_PRIORITY = "Priority";

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_REQUEST_ID = "RequestId";

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_GENERATION_ID = "GenerationId";

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_BLOB_ETAG = "BlobEtag";

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_BLOB_SAS = "BlobSAS";

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_STATUS = "Status";

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_START_TIME = "StartTime";

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_NEXTRETRY_TIME = "NextRetryTime";

		internal const string XBLOB_ARCHIVE_REQUEST_COLUMN_SERVICEMETADATA = "ServiceMetadata";

		internal const string XBLOB_ARCHIVE_COLUMN_VERSIONTIMESTAMP = "VersionTimestamp";

		internal const string XBLOB_ARCHIVE_COLUMN_GENERATION_ID = "GenerationId";

		internal const string XBLOB_ARCHIVE_COLUMN_BLOB_ETAG = "BlobETag";

		internal const string XBLOB_ARCHIVE_COLUMN_METADATA = "ArchiveMetadata";

		internal const string XBLOB_ARCHIVE_COLUMN_ENCRYPTION_KEY = "ArchiveEncryptionKey";

		internal const string XBLOB_ARCHIVE_COLUMN_REQUEST_ID = "ArchiveRequestId";

		internal const string XBLOB_ARCHIVE_COLUMN_BLOB_EXPIRY_TIME = "ExpiryTime";

		internal const string XBLOB_ARCHIVE_COLUMN_BLOB_ARCHIVED_TIME = "ArchivedTime";

		internal const string XBLOB_ARCHIVE_COLUMN_SERVICEMETADATA = "ServiceMetadata";
	}
}