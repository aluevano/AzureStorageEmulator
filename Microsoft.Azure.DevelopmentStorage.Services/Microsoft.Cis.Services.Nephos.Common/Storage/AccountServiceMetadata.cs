using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class AccountServiceMetadata : ICloneable
	{
		public AnalyticsSettings BlobAnalyticsSettings
		{
			get;
			set;
		}

		public GeoReplicationStats BlobGeoReplicationStats
		{
			get;
			set;
		}

		public AnalyticsSettings FileAnalyticsSettings
		{
			get;
			set;
		}

		public bool? IsAnalyticsDisabled
		{
			get;
			set;
		}

		public AnalyticsSettings QueueAnalyticsSettings
		{
			get;
			set;
		}

		public GeoReplicationStats QueueGeoReplicationStats
		{
			get;
			set;
		}

		public bool? SecondaryReadEnabled
		{
			get;
			set;
		}

		public AnalyticsSettings TableAnalyticsSettings
		{
			get;
			set;
		}

		public GeoReplicationStats TableGeoReplicationStats
		{
			get;
			set;
		}

		public AccountServiceMetadata()
		{
		}

		public object Clone()
		{
			AccountServiceMetadata accountServiceMetadatum = new AccountServiceMetadata()
			{
				IsAnalyticsDisabled = this.IsAnalyticsDisabled
			};
			if (this.QueueAnalyticsSettings != null)
			{
				accountServiceMetadatum.QueueAnalyticsSettings = (AnalyticsSettings)this.QueueAnalyticsSettings.Clone();
			}
			if (this.BlobAnalyticsSettings != null)
			{
				accountServiceMetadatum.BlobAnalyticsSettings = (AnalyticsSettings)this.BlobAnalyticsSettings.Clone();
			}
			if (this.TableAnalyticsSettings != null)
			{
				accountServiceMetadatum.TableAnalyticsSettings = (AnalyticsSettings)this.TableAnalyticsSettings.Clone();
			}
			if (this.FileAnalyticsSettings != null)
			{
				accountServiceMetadatum.FileAnalyticsSettings = (AnalyticsSettings)this.FileAnalyticsSettings.Clone();
			}
			accountServiceMetadatum.SecondaryReadEnabled = this.SecondaryReadEnabled;
			return accountServiceMetadatum;
		}
	}
}