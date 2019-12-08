using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Flags]
	public enum AccountServiceMetadataPropertyNames : long
	{
		None = 0,
		IsAnalyticsDisabled = 16384,
		BlobAnalyticsSettings = 32768,
		QueueAnalyticsSettings = 65536,
		TableAnalyticsSettings = 131072,
		FileAnalyticsSettings = 262144,
		AnalyticsSettings = 507904,
		SecondaryReadEnabled = 134217728,
		BlobGeoReplicationStats = 268435456,
		TableGeoReplicationStats = 536870912,
		QueueGeoReplicationStats = 1073741824,
		All = 2013773824
	}
}