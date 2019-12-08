using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class BlobErrorCodeStrings
	{
		public const string InvalidBlockId = "InvalidBlockId";

		public const string BlobNotFound = "BlobNotFound";

		public const string BlobWriteProtected = "BlobWriteProtected";

		public const string OperationNotAllowedOnIncrementalCopyBlob = "OperationNotAllowedOnIncrementalCopyBlob";

		public const string CopySourceCannotBeIncrementalCopyBlob = "CopySourceCannotBeIncrementalCopyBlob";

		public const string IncrementalCopyOfEarlierSnapshotNotAllowed = "IncrementalCopyOfEarlierSnapshotNotAllowed";

		public const string IncrementalCopyBlobMismatch = "IncrementalCopyBlobMismatch";

		public const string IncrementalCopyBlobPreviousSnapshotDoesNotExist = "IncrementalCopyBlobPreviousSnapshotDoesNotExist";

		public const string BlobAlreadyExists = "BlobAlreadyExists";

		public const string InvalidBlobOrBlock = "InvalidBlobOrBlock";

		public const string InvalidBlockList = "InvalidBlockList";

		public const string InvalidArchiveOperationData = "InvalidArchiveOperationData";

		public const string BlobModifiedWhileReading = "BlobModifiedWhileReading";

		public const string CopyAcrossAccountsNotSupported = "CopyAcrossAccountsNotSupported";

		public const string CannotVerifyCopySource = "CannotVerifyCopySource";

		public const string PendingCopyOperation = "PendingCopyOperation";

		public const string NoPendingCopyOperation = "NoPendingCopyOperation";

		public const string CopyIdMismatch = "CopyIdMismatch";

		public const string IncrementalCopySourceMustBeSnapshot = "IncrementalCopySourceMustBeSnapshot";

		public const string InfiniteLeaseDurationRequired = "InfiniteLeaseDurationRequired";

		public const string InvalidSourceBlobType = "InvalidSourceBlobType";

		public const string InvalidSourceBlobUrl = "InvalidSourceBlobUrl";

		public const string LeaseNotPresentWithBlobOperation = "LeaseNotPresentWithBlobOperation";

		public const string LeaseNotPresentWithContainerOperation = "LeaseNotPresentWithContainerOperation";

		public const string LeaseNotPresentWithLeaseOperation = "LeaseNotPresentWithLeaseOperation";

		public const string LeaseLost = "LeaseLost";

		public const string LeaseIdMismatchWithBlobOperation = "LeaseIdMismatchWithBlobOperation";

		public const string LeaseIdMismatchWithContainerOperation = "LeaseIdMismatchWithContainerOperation";

		public const string LeaseIdMismatchWithLeaseOperation = "LeaseIdMismatchWithLeaseOperation";

		public const string LeaseIdMissing = "LeaseIdMissing";

		public const string LeaseAlreadyPresent = "LeaseAlreadyPresent";

		public const string LeaseAlreadyBroken = "LeaseAlreadyBroken";

		public const string LeaseIsBreakingAndCannotBeAcquired = "LeaseIsBreakingAndCannotBeAcquired";

		public const string LeaseIsBrokenAndCannotBeRenewed = "LeaseIsBrokenAndCannotBeRenewed";

		public const string LeaseIsBreakingAndCannotBeChanged = "LeaseIsBreakingAndCannotBeChanged";

		public const string SnapshotsPresent = "SnapshotsPresent";

		public const string InvalidBlobType = "InvalidBlobType";

		public const string InvalidVersionForPageBlobOperation = "InvalidVersionForPageBlobOperation";

		public const string FeatureVersionMismatch = "FeatureVersionMismatch";

		public const string InvalidPageRange = "InvalidPageRange";

		public const string SequenceNumberIncrementTooLarge = "SequenceNumberIncrementTooLarge";

		public const string MaxBlobSizeConditionNotMet = "MaxBlobSizeConditionNotMet";

		public const string AppendPositionConditionNotMet = "AppendPositionConditionNotMet";

		public const string BlockCountExceedsLimit = "BlockCountExceedsLimit";

		public const string SystemInUse = "SystemInUse";

		public const string PreviousSnapshotNotFound = "PreviousSnapshotNotFound";

		public const string BlobOperationNotSupported = "BlobOperationNotSupported";

		public const string PreviousSnapshotOperationNotSupported = "PreviousSnapshotOperationNotSupported";

		public const string BlobGenerationMismatch = "BlobGenerationMismatch";

		public const string BlobOverwritten = "BlobOverwritten";

		public const string PreviousSnapshotCannotBeNewer = "PreviousSnapshotCannotBeNewer";

		public const string LinkBlobCannotBeWriteProtected = "LinkBlobCannotBeWriteProtected";

		public const string LinkBlobCannotBeOverwritten = "LinkBlobCannotBeOverwritten";

		public const string BlobWithSnapshotsCannotBeWriteProtected = "BlobWithSnapshotsCannotBeWriteProtected";

		public const string BlockListTooLong = "BlockListTooLong";

		public const string UnauthorizedBlobOverwrite = "UnauthorizedBlobOverwrite";
	}
}