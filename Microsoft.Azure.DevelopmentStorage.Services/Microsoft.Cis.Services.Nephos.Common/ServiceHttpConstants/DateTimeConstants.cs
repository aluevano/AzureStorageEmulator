using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class DateTimeConstants
	{
		public readonly static DateTime MinimumIncrementalCopySnapshotTime;

		public readonly static DateTime MinimumBlobLastModificationTime;

		public readonly static DateTime MinimumCommittedBlobLastModificationTime;

		static DateTimeConstants()
		{
			DateTimeConstants.MinimumIncrementalCopySnapshotTime = DateTime.FromFileTimeUtc((long)0);
			DateTimeConstants.MinimumBlobLastModificationTime = DateTime.FromFileTimeUtc((long)0);
			DateTimeConstants.MinimumCommittedBlobLastModificationTime = DateTime.FromFileTimeUtc((long)1);
		}
	}
}