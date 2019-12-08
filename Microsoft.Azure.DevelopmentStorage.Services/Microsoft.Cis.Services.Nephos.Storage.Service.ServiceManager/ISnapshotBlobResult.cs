using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface ISnapshotBlobResult
	{
		DateTime LastModifiedTime
		{
			get;
		}

		DateTime SnapshotTimestamp
		{
			get;
		}
	}
}