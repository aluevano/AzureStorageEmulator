using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public static class Helpers
	{
		public static BlobObjectCondition Convert(IBlobObjectCondition condition)
		{
			if (condition == null)
			{
				return null;
			}
			return new BlobObjectCondition(condition.IncludeDisabledContainers, condition.IncludeExpiredBlobs, StorageStampHelpers.AdjustNullableDatetimeRange(condition.IfModifiedSinceTime), StorageStampHelpers.AdjustNullableDatetimeRange(condition.IfNotModifiedSinceTime), StorageStampHelpers.AdjustDateTimeRange(condition.IfLastModificationTimeMatch), StorageStampHelpers.AdjustDateTimeRange(condition.IfLastModificationTimeMismatch), condition.UpdateOptions, condition.LeaseId, condition.IsFetchingMetadata, condition.IsIncludingSnapshots, condition.IsIncludingPageBlobs, condition.IsIncludingAppendBlobs, condition.IsIncludingUncommittedBlobs, condition.IsDeletingOnlySnapshots, condition.IsRequiringNoSnapshots, condition.IsSkippingLastModificationTimeUpdate, condition.SequenceNumberOperator, condition.SequenceNumber, condition.RequiredBlobType, condition.IsMultipleConditionalHeaderEnabled, condition.IsCopyOperationOnAppendBlobsAllowed, condition.IsRequiringLeaseIfNotModifiedSince, condition.IsDiskMountStateConditionRequired, condition.ExcludeSoftDeletedBlobs, condition.EnsureBlobIsAlreadySoftDeleted, condition.IsOperationAllowedOnArchivedBlobs, condition.SkipLeaseCheck, condition.InternalArchiveBlobLMT, condition.InternalArchiveBlobGenerationId, condition.InternalArchiveRequestId, condition.IncludeSoftDeletedBlobsOnly);
		}

		public static ContainerCondition Convert(IContainerCondition condition)
		{
			if (condition == null)
			{
				return null;
			}
			return new ContainerCondition(condition.IncludeDisabledContainers, condition.IncludeExpiredContainers, condition.IncludeSnapshots, StorageStampHelpers.AdjustNullableDatetimeRange(condition.IfModifiedSinceTime), StorageStampHelpers.AdjustNullableDatetimeRange(condition.IfNotModifiedSinceTime), condition.SnapshotTimestamp, condition.IsRequiringNoSnapshots, condition.IsDeletingOnlySnapshots, condition.LeaseId);
		}
	}
}