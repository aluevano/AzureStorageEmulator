using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class FileErrorCodeStrings
	{
		public const string SharingViolation = "SharingViolation";

		public const string FileLockConflict = "FileLockConflict";

		public const string ClientCacheFlushDelay = "ClientCacheFlushDelay";

		public const string DirectoryNotEmpty = "DirectoryNotEmpty";

		public const string DeletePending = "DeletePending";

		public const string ParentNotFound = "ParentNotFound";

		public const string ReadOnlyAttribute = "ReadOnlyAttribute";

		public const string CannotDeleteFileOrDirectory = "CannotDeleteFileOrDirectory";

		public const string InvalidFileOrDirectoryPathName = "InvalidFileOrDirectoryPathName";

		public const string SmbShareFull = "ShareSizeLimitReached";

		public const string ShareSnapshotInProgress = "ShareSnapshotInProgress";

		public const string ShareSnapshotCountExceeded = "ShareSnapshotCountExceeded";

		public const string ShareHasSnapshots = "ShareHasSnapshots";
	}
}