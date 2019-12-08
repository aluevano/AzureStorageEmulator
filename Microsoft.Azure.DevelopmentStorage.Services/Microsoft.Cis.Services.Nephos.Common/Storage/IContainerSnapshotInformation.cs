using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IContainerSnapshotInformation
	{
		DateTime LastModificationTime
		{
			get;
		}

		DateTime SnapshotTimestamp
		{
			get;
		}

		ulong SnapshotVersion
		{
			get;
		}
	}
}