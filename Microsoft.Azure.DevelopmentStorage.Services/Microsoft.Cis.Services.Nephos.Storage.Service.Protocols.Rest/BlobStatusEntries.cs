using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public static class BlobStatusEntries
	{
		public readonly static NephosStatusEntry InvalidBlockId;

		public readonly static NephosStatusEntry BlobAlreadyExists;

		public readonly static NephosStatusEntry BlobWriteProtected;

		public readonly static NephosStatusEntry CopySourceCannotBeIncrementalCopyBlob;

		public readonly static NephosStatusEntry OperationNotAllowedOnIncrementalCopyBlob;

		public readonly static NephosStatusEntry IncrementalCopyOfEarlierSnapshotNotAllowed;

		public readonly static NephosStatusEntry IncrementalCopyBlobMismatch;

		public readonly static NephosStatusEntry IncrementalCopyBlobPreviousSnapshotDoesNotExist;

		public readonly static NephosStatusEntry LinkBlobCannotBeOverwritten;

		public readonly static NephosStatusEntry LinkBlobCannotBeWriteProtected;

		public readonly static NephosStatusEntry BlobWithSnapshotsCannotBeWriteProtected;

		public readonly static NephosStatusEntry InvalidBlobOrBlock;

		public readonly static NephosStatusEntry InvalidBlockList;

		public readonly static NephosStatusEntry InvalidArchiveOperationData;

		public readonly static NephosStatusEntry BlobModifiedWhileReading;

		public readonly static NephosStatusEntry LeaseNotPresentWithBlobOperation;

		public readonly static NephosStatusEntry LeaseNotPresentWithContainerOperation;

		public readonly static NephosStatusEntry LeaseNotPresentWithBlobLeaseOperation;

		public readonly static NephosStatusEntry LeaseNotPresentWithContainerLeaseOperation;

		public readonly static NephosStatusEntry LeaseLostWithBlobOperation;

		public readonly static NephosStatusEntry LeaseLostWithContainerOperation;

		public readonly static NephosStatusEntry LeaseIdMismatchWithBlobLeaseOperation;

		public readonly static NephosStatusEntry LeaseIdMismatchWithContainerLeaseOperation;

		public readonly static NephosStatusEntry LeaseIdMismatchWithBlobOperation;

		public readonly static NephosStatusEntry LeaseIdMismatchWithContainerOperation;

		public readonly static NephosStatusEntry LeaseIdMissingWithBlobOperation;

		public readonly static NephosStatusEntry LeaseIdMissingWithContainerOperation;

		public readonly static NephosStatusEntry LeaseAlreadyPresent;

		public readonly static NephosStatusEntry LeaseAlreadyBroken;

		public readonly static NephosStatusEntry LeaseIsBrokenAndCannotBeRenewed;

		public readonly static NephosStatusEntry LeaseIsBreakingAndCannotBeAcquired;

		public readonly static NephosStatusEntry LeaseIsBreakingAndCannotBeChanged;

		public readonly static NephosStatusEntry InfiniteLeaseDurationRequired;

		public readonly static NephosStatusEntry SnapshotsPresent;

		public readonly static NephosStatusEntry InvalidBlobType;

		public readonly static NephosStatusEntry InvalidVersionForPageBlobOperation;

		public readonly static NephosStatusEntry InvalidVersionForAppendBlobOperation;

		public readonly static NephosStatusEntry InvalidVersionForBlobTypeInBlobList;

		public readonly static NephosStatusEntry InvalidPageRange;

		public readonly static NephosStatusEntry SequenceNumberIncrementTooLarge;

		public readonly static NephosStatusEntry CopyAcrossAccountsNotSupported;

		public readonly static NephosStatusEntry AuthorizingCopySourceTimedOut;

		public readonly static NephosStatusEntry PendingCopyOperation;

		public readonly static NephosStatusEntry NoPendingCopyOperation;

		public readonly static NephosStatusEntry CopyIdMismatch;

		public readonly static NephosStatusEntry IncrementalCopySourceMustBeSnapshot;

		public readonly static NephosStatusEntry InvalidSourceBlobType;

		public readonly static NephosStatusEntry InvalidSourceBlobUrl;

		public readonly static NephosStatusEntry AppendPositionConditionNotMet;

		public readonly static NephosStatusEntry MaxBlobSizeConditionNotMet;

		public readonly static NephosStatusEntry BlockCountExceedsLimit;

		public readonly static NephosStatusEntry BlockListTooLong;

		public readonly static NephosStatusEntry UncommittedBlockCountExceedsLimit;

		public readonly static NephosStatusEntry May16BlockCountExceedsLimit;

		public readonly static NephosStatusEntry PreviousSnapshotNotFound;

		public readonly static NephosStatusEntry DifferentialGetPageRangesNotSupportedOnPreviousSnapshot;

		public readonly static NephosStatusEntry BlobGenerationMismatch;

		public readonly static NephosStatusEntry BlobOverwritten;

		public readonly static NephosStatusEntry PreviousSnapshotCannotBeNewer;

		public readonly static NephosStatusEntry UnauthorizedBlobOverwrite;

		static BlobStatusEntries()
		{
			BlobStatusEntries.InvalidBlockId = new NephosStatusEntry("InvalidBlockId", HttpStatusCode.BadRequest, "Block ID is invalid. Block ID must be base64 encoded.");
			BlobStatusEntries.BlobAlreadyExists = new NephosStatusEntry("BlobAlreadyExists", HttpStatusCode.Conflict, "The specified blob already exists.");
			BlobStatusEntries.BlobWriteProtected = new NephosStatusEntry("BlobWriteProtected", HttpStatusCode.Conflict, "The specified blob is write protected.");
			BlobStatusEntries.CopySourceCannotBeIncrementalCopyBlob = new NephosStatusEntry("CopySourceCannotBeIncrementalCopyBlob", HttpStatusCode.Conflict, "Source blob of a copy operation cannot be non-snapshot incremental copy blob.");
			BlobStatusEntries.OperationNotAllowedOnIncrementalCopyBlob = new NephosStatusEntry("OperationNotAllowedOnIncrementalCopyBlob", HttpStatusCode.Conflict, "The specified operation is not allowed on an incremental copy blob.");
			BlobStatusEntries.IncrementalCopyOfEarlierSnapshotNotAllowed = new NephosStatusEntry("IncrementalCopyOfEarlierSnapshotNotAllowed", HttpStatusCode.Conflict, "The specified snapshot is earlier than the last snapshot copied into the incremental copy blob.");
			BlobStatusEntries.IncrementalCopyBlobMismatch = new NephosStatusEntry("IncrementalCopyBlobMismatch", HttpStatusCode.Conflict, "The specified source blob is different than the copy source of the existing incremental copy blob.");
			BlobStatusEntries.IncrementalCopyBlobPreviousSnapshotDoesNotExist = new NephosStatusEntry("IncrementalCopyBlobPreviousSnapshotDoesNotExist", HttpStatusCode.Conflict, "The previously copied source snapshot does not exist.");
			BlobStatusEntries.LinkBlobCannotBeOverwritten = new NephosStatusEntry("LinkBlobCannotBeOverwritten", HttpStatusCode.Conflict, "Link blob cannot be overwritten.");
			BlobStatusEntries.LinkBlobCannotBeWriteProtected = new NephosStatusEntry("LinkBlobCannotBeWriteProtected", HttpStatusCode.Conflict, "Link blob cannot be write protected.");
			BlobStatusEntries.BlobWithSnapshotsCannotBeWriteProtected = new NephosStatusEntry("BlobWithSnapshotsCannotBeWriteProtected", HttpStatusCode.Conflict, "The specified blob has one or more snapshots and cannot be write protected.");
			BlobStatusEntries.InvalidBlobOrBlock = new NephosStatusEntry("InvalidBlobOrBlock", HttpStatusCode.BadRequest, "The specified blob or block content is invalid.");
			BlobStatusEntries.InvalidBlockList = new NephosStatusEntry("InvalidBlockList", HttpStatusCode.BadRequest, "The specified block list is invalid.");
			BlobStatusEntries.InvalidArchiveOperationData = new NephosStatusEntry("InvalidArchiveOperationData", HttpStatusCode.BadRequest, "The specified data either has extra elements, is missing one or more of the necessary elements or the archiveMetadata and/or encryptionKey is not base64 encoded");
			BlobStatusEntries.BlobModifiedWhileReading = new NephosStatusEntry("BlobModifiedWhileReading", HttpStatusCode.Conflict, "The blob has been modified while being read.");
			BlobStatusEntries.LeaseNotPresentWithBlobOperation = new NephosStatusEntry("LeaseNotPresentWithBlobOperation", HttpStatusCode.PreconditionFailed, "There is currently no lease on the blob.");
			BlobStatusEntries.LeaseNotPresentWithContainerOperation = new NephosStatusEntry("LeaseNotPresentWithContainerOperation", HttpStatusCode.PreconditionFailed, "There is currently no lease on the container.");
			BlobStatusEntries.LeaseNotPresentWithBlobLeaseOperation = new NephosStatusEntry("LeaseNotPresentWithLeaseOperation", HttpStatusCode.Conflict, "There is currently no lease on the blob.");
			BlobStatusEntries.LeaseNotPresentWithContainerLeaseOperation = new NephosStatusEntry("LeaseNotPresentWithLeaseOperation", HttpStatusCode.Conflict, "There is currently no lease on the container.");
			BlobStatusEntries.LeaseLostWithBlobOperation = new NephosStatusEntry("LeaseLost", HttpStatusCode.PreconditionFailed, "A lease ID was specified, but the lease for the blob has expired.");
			BlobStatusEntries.LeaseLostWithContainerOperation = new NephosStatusEntry("LeaseLost", HttpStatusCode.PreconditionFailed, "A lease ID was specified, but the lease for the container has expired.");
			BlobStatusEntries.LeaseIdMismatchWithBlobLeaseOperation = new NephosStatusEntry("LeaseIdMismatchWithLeaseOperation", HttpStatusCode.Conflict, "The lease ID specified did not match the lease ID for the blob.");
			BlobStatusEntries.LeaseIdMismatchWithContainerLeaseOperation = new NephosStatusEntry("LeaseIdMismatchWithLeaseOperation", HttpStatusCode.Conflict, "The lease ID specified did not match the lease ID for the container.");
			BlobStatusEntries.LeaseIdMismatchWithBlobOperation = new NephosStatusEntry("LeaseIdMismatchWithBlobOperation", HttpStatusCode.PreconditionFailed, "The lease ID specified did not match the lease ID for the blob.");
			BlobStatusEntries.LeaseIdMismatchWithContainerOperation = new NephosStatusEntry("LeaseIdMismatchWithContainerOperation", HttpStatusCode.PreconditionFailed, "The lease ID specified did not match the lease ID for the container.");
			BlobStatusEntries.LeaseIdMissingWithBlobOperation = new NephosStatusEntry("LeaseIdMissing", HttpStatusCode.PreconditionFailed, "There is currently a lease on the blob and no lease ID was specified in the request.");
			BlobStatusEntries.LeaseIdMissingWithContainerOperation = new NephosStatusEntry("LeaseIdMissing", HttpStatusCode.PreconditionFailed, "There is currently a lease on the container and no lease ID was specified in the request.");
			BlobStatusEntries.LeaseAlreadyPresent = new NephosStatusEntry("LeaseAlreadyPresent", HttpStatusCode.Conflict, "There is already a lease present.");
			BlobStatusEntries.LeaseAlreadyBroken = new NephosStatusEntry("LeaseAlreadyBroken", HttpStatusCode.Conflict, "The lease has already been broken and cannot be broken again.");
			BlobStatusEntries.LeaseIsBrokenAndCannotBeRenewed = new NephosStatusEntry("LeaseIsBrokenAndCannotBeRenewed", HttpStatusCode.Conflict, "The lease ID matched, but the lease has been broken explicitly and cannot be renewed.");
			BlobStatusEntries.LeaseIsBreakingAndCannotBeAcquired = new NephosStatusEntry("LeaseIsBreakingAndCannotBeAcquired", HttpStatusCode.Conflict, "The lease ID matched, but the lease is currently in breaking state and cannot be acquired until it is broken.");
			BlobStatusEntries.LeaseIsBreakingAndCannotBeChanged = new NephosStatusEntry("LeaseIsBreakingAndCannotBeChanged", HttpStatusCode.Conflict, "The lease ID matched, but the lease is currently in breaking state and cannot be changed.");
			BlobStatusEntries.InfiniteLeaseDurationRequired = new NephosStatusEntry("InfiniteLeaseDurationRequired", HttpStatusCode.PreconditionFailed, "The lease ID matched, but the specified lease must be an infinite-duration lease.");
			BlobStatusEntries.SnapshotsPresent = new NephosStatusEntry("SnapshotsPresent", HttpStatusCode.Conflict, "This operation is not permitted because the blob has snapshots.");
			BlobStatusEntries.InvalidBlobType = new NephosStatusEntry("InvalidBlobType", HttpStatusCode.Conflict, "The blob type is invalid for this operation.");
			BlobStatusEntries.InvalidVersionForPageBlobOperation = new NephosStatusEntry("InvalidVersionForPageBlobOperation", HttpStatusCode.BadRequest, string.Format("All operations for PageBlob require at least version {0}.", "2009-09-19"));
			BlobStatusEntries.InvalidVersionForAppendBlobOperation = new NephosStatusEntry("FeatureVersionMismatch", HttpStatusCode.Conflict, string.Format("The operation for AppendBlob requires at least version {0}.", "2015-02-21"));
			BlobStatusEntries.InvalidVersionForBlobTypeInBlobList = new NephosStatusEntry("FeatureVersionMismatch", HttpStatusCode.Conflict, "The type of a blob in the container is unrecognized by this version.");
			BlobStatusEntries.InvalidPageRange = new NephosStatusEntry("InvalidPageRange", HttpStatusCode.RequestedRangeNotSatisfiable, "The page range specified is invalid.");
			BlobStatusEntries.SequenceNumberIncrementTooLarge = new NephosStatusEntry("SequenceNumberIncrementTooLarge", HttpStatusCode.Conflict, "The sequence number increment cannot be performed because it would result in overflow of the sequence number.");
			BlobStatusEntries.CopyAcrossAccountsNotSupported = new NephosStatusEntry("CopyAcrossAccountsNotSupported", HttpStatusCode.BadRequest, "The copy source account and destination account must be the same.");
			BlobStatusEntries.AuthorizingCopySourceTimedOut = new NephosStatusEntry("CannotVerifyCopySource", HttpStatusCode.InternalServerError, "Could not verify the copy source within the specified time.");
			BlobStatusEntries.PendingCopyOperation = new NephosStatusEntry("PendingCopyOperation", HttpStatusCode.Conflict, "There is currently a pending copy operation.");
			BlobStatusEntries.NoPendingCopyOperation = new NephosStatusEntry("NoPendingCopyOperation", HttpStatusCode.Conflict, "There is currently no pending copy operation.");
			BlobStatusEntries.CopyIdMismatch = new NephosStatusEntry("CopyIdMismatch", HttpStatusCode.Conflict, "The specified copy ID did not match the copy ID for the pending copy operation.");
			BlobStatusEntries.IncrementalCopySourceMustBeSnapshot = new NephosStatusEntry("IncrementalCopySourceMustBeSnapshot", HttpStatusCode.Conflict, "The source for incremental copy request must be a snapshot.");
			BlobStatusEntries.InvalidSourceBlobType = new NephosStatusEntry("InvalidSourceBlobType", HttpStatusCode.Conflict, "The copy source blob type is invalid for this operation.");
			BlobStatusEntries.InvalidSourceBlobUrl = new NephosStatusEntry("InvalidSourceBlobUrl", HttpStatusCode.Conflict, "The source url for incremental copy request must be valid azure storage blob url.");
			BlobStatusEntries.AppendPositionConditionNotMet = new NephosStatusEntry("AppendPositionConditionNotMet", HttpStatusCode.PreconditionFailed, "The append position condition specified was not met.");
			BlobStatusEntries.MaxBlobSizeConditionNotMet = new NephosStatusEntry("MaxBlobSizeConditionNotMet", HttpStatusCode.PreconditionFailed, "The max blob size condition specified was not met.");
			BlobStatusEntries.BlockCountExceedsLimit = new NephosStatusEntry("BlockCountExceedsLimit", HttpStatusCode.Conflict, "The block count exceeds the maximum permissible limit.");
			BlobStatusEntries.BlockListTooLong = new NephosStatusEntry("BlockListTooLong", HttpStatusCode.BadRequest, "The block list may not contain more than 50,000 blocks.");
			BlobStatusEntries.UncommittedBlockCountExceedsLimit = new NephosStatusEntry("BlockCountExceedsLimit", HttpStatusCode.Conflict, "The uncommitted block count cannot exceed the maximum limit of 100,000 blocks.");
			BlobStatusEntries.May16BlockCountExceedsLimit = new NephosStatusEntry("BlockCountExceedsLimit", HttpStatusCode.Conflict, "The committed block count cannot exceed the maximum limit of 50,000 blocks.");
			BlobStatusEntries.PreviousSnapshotNotFound = new NephosStatusEntry("PreviousSnapshotNotFound", HttpStatusCode.Conflict, "The previous snapshot is not found.");
			BlobStatusEntries.DifferentialGetPageRangesNotSupportedOnPreviousSnapshot = new NephosStatusEntry("PreviousSnapshotOperationNotSupported", HttpStatusCode.Conflict, "Differential Get Page Ranges is not supported on the previous snapshot.");
			BlobStatusEntries.BlobGenerationMismatch = new NephosStatusEntry("BlobGenerationMismatch", HttpStatusCode.Conflict, "The previous snapshot is from a different blob generation.");
			BlobStatusEntries.BlobOverwritten = new NephosStatusEntry("BlobOverwritten", HttpStatusCode.Conflict, "The blob has been recreated since the previous snapshot was taken.");
			BlobStatusEntries.PreviousSnapshotCannotBeNewer = new NephosStatusEntry("PreviousSnapshotCannotBeNewer", HttpStatusCode.BadRequest, "The prevsnapshot query parameter value cannot be newer than snapshot query parameter value.");
			BlobStatusEntries.UnauthorizedBlobOverwrite = new NephosStatusEntry("UnauthorizedBlobOverwrite", HttpStatusCode.Forbidden, "This request is not authorized to perform blob overwrites.");
		}
	}
}