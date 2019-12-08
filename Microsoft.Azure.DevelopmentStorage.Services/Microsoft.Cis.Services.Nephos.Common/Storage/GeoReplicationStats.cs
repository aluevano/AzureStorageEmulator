using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class GeoReplicationStats : ICloneable
	{
		public DateTime? LastSyncTime
		{
			get;
			set;
		}

		public GeoReplicationStatus? Status
		{
			get;
			set;
		}

		public GeoReplicationStats()
		{
		}

		public object Clone()
		{
			GeoReplicationStats geoReplicationStat = new GeoReplicationStats()
			{
				LastSyncTime = this.LastSyncTime,
				Status = this.Status
			};
			return geoReplicationStat;
		}

		public override string ToString()
		{
			return string.Format("(LastSyncTime:{0};Status:{1})", (this.LastSyncTime.HasValue ? this.LastSyncTime.Value.ToUniversalTime().ToString() : "<null>"), (this.Status.HasValue ? this.Status.Value.ToString() : "<null>"));
		}
	}
}