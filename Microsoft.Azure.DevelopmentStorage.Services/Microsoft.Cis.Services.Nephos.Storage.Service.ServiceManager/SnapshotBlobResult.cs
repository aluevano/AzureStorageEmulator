using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class SnapshotBlobResult : ISnapshotBlobResult
	{
		private DateTime lastModifiedTime;

		private DateTime snapshotTimestamp;

		public DateTime LastModifiedTime
		{
			get
			{
				return JustDecompileGenerated_get_LastModifiedTime();
			}
			set
			{
				JustDecompileGenerated_set_LastModifiedTime(value);
			}
		}

		public DateTime JustDecompileGenerated_get_LastModifiedTime()
		{
			return this.lastModifiedTime;
		}

		public void JustDecompileGenerated_set_LastModifiedTime(DateTime value)
		{
			this.lastModifiedTime = value;
		}

		public DateTime SnapshotTimestamp
		{
			get
			{
				return JustDecompileGenerated_get_SnapshotTimestamp();
			}
			set
			{
				JustDecompileGenerated_set_SnapshotTimestamp(value);
			}
		}

		public DateTime JustDecompileGenerated_get_SnapshotTimestamp()
		{
			return this.snapshotTimestamp;
		}

		public void JustDecompileGenerated_set_SnapshotTimestamp(DateTime value)
		{
			this.snapshotTimestamp = value;
		}

		public SnapshotBlobResult()
		{
		}
	}
}