using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class VersionedRequestSettings
	{
		public bool AbortCopyBlobApiEnabled
		{
			get;
			protected set;
		}

		public bool AbortCopyFileApiEnabled
		{
			get;
			protected set;
		}

		public bool AcquireBlobLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool AcquireContainerLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool AcquireFileLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool AcquireShareLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool AppendBlockApiEnabled
		{
			get;
			protected set;
		}

		public bool BlobPreflightApiEnabled
		{
			get;
			protected set;
		}

		public bool BreakBlobLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool BreakContainerLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool BreakFileLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool BreakShareLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool ChangeBlobLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool ChangeContainerLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool ChangeFileLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool ChangeShareLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool ClearMessagesApiEnabled
		{
			get;
			protected set;
		}

		public bool ClearPageApiEnabled
		{
			get;
			protected set;
		}

		public bool ClearRangeApiEnabled
		{
			get;
			protected set;
		}

		public bool CopyBlobApiEnabled
		{
			get;
			protected set;
		}

		public bool CopyFileApiEnabled
		{
			get;
			protected set;
		}

		public bool CreateContainerApiEnabled
		{
			get;
			protected set;
		}

		public bool CreateDirectoryApiEnabled
		{
			get;
			protected set;
		}

		public bool CreateQueueApiEnabled
		{
			get;
			protected set;
		}

		public bool CreateShareApiEnabled
		{
			get;
			protected set;
		}

		public bool DeleteBlobApiEnabled
		{
			get;
			protected set;
		}

		public bool DeleteContainerApiEnabled
		{
			get;
			protected set;
		}

		public bool DeleteDirectoryApiEnabled
		{
			get;
			protected set;
		}

		public bool DeleteFileApiEnabled
		{
			get;
			protected set;
		}

		public bool DeleteMessageApiEnabled
		{
			get;
			protected set;
		}

		public bool DeleteQueueApiEnabled
		{
			get;
			protected set;
		}

		public bool DeleteShareApiEnabled
		{
			get;
			protected set;
		}

		public bool FilePreflightApiEnabled
		{
			get;
			protected set;
		}

		public bool GetBlobApiEnabled
		{
			get;
			protected set;
		}

		public bool GetBlobMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool GetBlobPropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetBlobServicePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetBlobServiceStatsApiEnabled
		{
			get;
			protected set;
		}

		public bool GetBlockListApiEnabled
		{
			get;
			protected set;
		}

		public bool GetContainerAclApiEnabled
		{
			get;
			protected set;
		}

		public bool GetContainerMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool GetContainerPropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetDirectoryMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool GetDirectoryPropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetFileApiEnabled
		{
			get;
			protected set;
		}

		public bool GetFileMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool GetFilePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetFileServicePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetMessagesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetPageRangesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetPostMigrationFileInfoApiEnabled
		{
			get;
			protected set;
		}

		public bool GetQueueAclApiEnabled
		{
			get;
			protected set;
		}

		public bool GetQueueMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool GetQueueServicePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetQueueServiceStatsApiEnabled
		{
			get;
			protected set;
		}

		public bool GetShareAclApiEnabled
		{
			get;
			protected set;
		}

		public bool GetShareMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool GetSharePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetShareStatsApiEnabled
		{
			get;
			protected set;
		}

		public bool GetTableAclApiEnabled
		{
			get;
			protected set;
		}

		public bool GetTableServicePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool GetTableServiceStatsApiEnabled
		{
			get;
			protected set;
		}

		public bool IncrementalCopyBlobApiEnabled
		{
			get;
			protected set;
		}

		public bool ListBlobsApiEnabled
		{
			get;
			protected set;
		}

		public bool ListContainersApiEnabled
		{
			get;
			protected set;
		}

		public bool ListFilesApiEnabled
		{
			get;
			protected set;
		}

		public bool ListQueuesApiEnabled
		{
			get;
			protected set;
		}

		public bool ListRangesApiEnabled
		{
			get;
			protected set;
		}

		public bool ListSharesApiEnabled
		{
			get;
			protected set;
		}

		public bool PutBlobApiEnabled
		{
			get;
			protected set;
		}

		public bool PutBlockApiEnabled
		{
			get;
			protected set;
		}

		public bool PutBlockListApiEnabled
		{
			get;
			protected set;
		}

		public bool PutFileApiEnabled
		{
			get;
			protected set;
		}

		public bool PutMessageApiEnabled
		{
			get;
			protected set;
		}

		public bool PutPageApiEnabled
		{
			get;
			protected set;
		}

		public bool PutRangeApiEnabled
		{
			get;
			protected set;
		}

		public bool QueuePreflightApiEnabled
		{
			get;
			protected set;
		}

		public bool ReleaseBlobLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool ReleaseContainerLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool ReleaseFileLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool ReleaseShareLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool RenewBlobLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool RenewContainerLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool RenewFileLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool RenewShareLeaseApiEnabled
		{
			get;
			protected set;
		}

		public bool SetBlobInternalPropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool SetBlobMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool SetBlobPropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool SetBlobServicePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool SetContainerAclApiEnabled
		{
			get;
			protected set;
		}

		public bool SetContainerMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool SetDirectoryMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool SetFileMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool SetFilePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool SetFileServicePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool SetPostMigrationFileInfoApiEnabled
		{
			get;
			protected set;
		}

		public bool SetQueueAclApiEnabled
		{
			get;
			protected set;
		}

		public bool SetQueueMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool SetQueueServicePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool SetShareAclApiEnabled
		{
			get;
			protected set;
		}

		public bool SetShareMetadataApiEnabled
		{
			get;
			protected set;
		}

		public bool SetSharePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool SetTableAclApiEnabled
		{
			get;
			protected set;
		}

		public bool SetTableServicePropertiesApiEnabled
		{
			get;
			protected set;
		}

		public bool SnapshotBlobApiEnabled
		{
			get;
			protected set;
		}

		public bool SnapshotShareApiEnabled
		{
			get;
			protected set;
		}

		public bool TableAndRowOperationApisEnabled
		{
			get;
			protected set;
		}

		public bool TableBatchApiEnabled
		{
			get;
			protected set;
		}

		public bool TablePreflightApiEnabled
		{
			get;
			protected set;
		}

		public bool UndeleteBlobApiEnabled
		{
			get;
			protected set;
		}

		public bool UpdateMessageApiEnabled
		{
			get;
			protected set;
		}

		public bool UseNewGetBlockListImplementation
		{
			get;
			protected set;
		}

		public bool UseNewPutBlockListImplementation
		{
			get;
			protected set;
		}

		public string VersionString
		{
			get;
			protected set;
		}

		protected VersionedRequestSettings()
		{
		}
	}
}