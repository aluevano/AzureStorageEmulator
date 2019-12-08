using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlobObjectCondition : IBaseObjectCondition
	{
		bool EnsureBlobIsAlreadySoftDeleted
		{
			get;
			set;
		}

		bool ExcludeSoftDeletedBlobs
		{
			get;
			set;
		}

		bool IncludeDisabledContainers
		{
			get;
		}

		bool IncludeExpiredBlobs
		{
			get;
			set;
		}

		bool IncludeSoftDeletedBlobsOnly
		{
			get;
			set;
		}

		string InternalArchiveBlobGenerationId
		{
			get;
		}

		DateTime? InternalArchiveBlobLMT
		{
			get;
		}

		Guid? InternalArchiveRequestId
		{
			get;
		}

		bool IsCopyOperationOnAppendBlobsAllowed
		{
			get;
		}

		bool IsDeletingOnlySnapshots
		{
			get;
		}

		bool IsDiskMountStateConditionRequired
		{
			get;
			set;
		}

		bool IsFetchingMetadata
		{
			get;
		}

		bool IsIncludingAppendBlobs
		{
			get;
		}

		bool IsIncludingPageBlobs
		{
			get;
		}

		bool IsIncludingSnapshots
		{
			get;
		}

		bool IsIncludingUncommittedBlobs
		{
			get;
		}

		bool IsOperationAllowedOnArchivedBlobs
		{
			get;
			set;
		}

		DateTime? IsRequiringLeaseIfNotModifiedSince
		{
			get;
		}

		bool IsRequiringNoSnapshots
		{
			get;
		}

		bool IsSkippingLastModificationTimeUpdate
		{
			get;
		}

		Guid? LeaseId
		{
			get;
		}

		BlobType RequiredBlobType
		{
			get;
		}

		long? SequenceNumber
		{
			get;
		}

		ComparisonOperator? SequenceNumberOperator
		{
			get;
		}

		bool SkipLeaseCheck
		{
			get;
			set;
		}

		IUpdateOptions UpdateOptions
		{
			get;
		}
	}
}