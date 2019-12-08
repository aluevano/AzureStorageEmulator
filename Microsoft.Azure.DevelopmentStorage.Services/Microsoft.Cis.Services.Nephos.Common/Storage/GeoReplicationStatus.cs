using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public enum GeoReplicationStatus
	{
		Invalid = 0,
		Unavailable = 2,
		Bootstrap = 3,
		Live = 4,
		Repair = 5
	}
}