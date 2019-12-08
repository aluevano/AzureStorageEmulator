using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Specialized;
using System.Globalization;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ListContainersResultContainerProperties : IListContainersResultContainerProperties
	{
		private string accountNameForLogging;

		private string containerName;

		private DateTime? lastModifiedTime;

		private NameValueCollection applicationMetadata;

		private string leaseStatus;

		private string leaseState;

		private string leaseDuration;

		private long containerQuotaInGB;

		private string publicAccessLevel;

		public string ContainerName
		{
			get
			{
				return this.containerName;
			}
		}

		public long ContainerQuotaInGB
		{
			get
			{
				return this.containerQuotaInGB;
			}
		}

		public DateTime? LastModifiedTime
		{
			get
			{
				return this.lastModifiedTime;
			}
		}

		public string LeaseDuration
		{
			get
			{
				return this.leaseDuration;
			}
		}

		public string LeaseState
		{
			get
			{
				return this.leaseState;
			}
		}

		public string LeaseStatus
		{
			get
			{
				return this.leaseStatus;
			}
		}

		public NameValueCollection Metadata
		{
			get
			{
				return this.applicationMetadata;
			}
		}

		public string PublicAccessLevel
		{
			get
			{
				return this.publicAccessLevel;
			}
		}

		public ListContainersResultContainerProperties(IBaseBlobContainer container)
		{
			long num;
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}
			this.containerName = container.ContainerName;
			NephosAssertionException.Assert(container.LastModificationTime.HasValue);
			this.lastModifiedTime = container.LastModificationTime;
			if (container.ApplicationMetadata != null)
			{
				this.applicationMetadata = new NameValueCollection();
				try
				{
					MetadataEncoding.Decode(container.ApplicationMetadata, this.applicationMetadata);
				}
				catch (MetadataFormatException metadataFormatException1)
				{
					MetadataFormatException metadataFormatException = metadataFormatException1;
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { this.containerName };
					throw new NephosStorageDataCorruptionException(string.Format(invariantCulture, "Error decoding application metadata for container {0}", objArray), metadataFormatException);
				}
			}
			if (container.LeaseInfo != null)
			{
				if (container.LeaseInfo.Type == LeaseType.ReadWrite && container.LeaseInfo.Duration.HasValue)
				{
					TimeSpan? duration = container.LeaseInfo.Duration;
					TimeSpan zero = TimeSpan.Zero;
					if ((duration.HasValue ? duration.GetValueOrDefault() <= zero : true))
					{
						goto Label1;
					}
					this.leaseStatus = "locked";
					goto Label0;
				}
			Label1:
				this.leaseStatus = "unlocked";
			Label0:
				if (container.LeaseInfo.State.HasValue)
				{
					this.leaseState = LeaseStateStrings.LeaseStates[(int)container.LeaseInfo.State.Value];
					if (container.LeaseInfo.State.Equals(Microsoft.Cis.Services.Nephos.Common.Storage.LeaseState.Leased))
					{
						TimeSpan? nullable = container.LeaseInfo.Duration;
						TimeSpan timeSpan = TimeSpan.FromSeconds(4294967295);
						if ((!nullable.HasValue ? true : nullable.GetValueOrDefault() != timeSpan))
						{
							this.leaseDuration = "fixed";
						}
						else
						{
							this.leaseDuration = "infinite";
						}
					}
				}
			}
			if (container.ServiceMetadata != null)
			{
				NameValueCollection nameValueCollection = new NameValueCollection();
				try
				{
					MetadataEncoding.Decode(container.ServiceMetadata, nameValueCollection);
				}
				catch (MetadataFormatException metadataFormatException3)
				{
					MetadataFormatException metadataFormatException2 = metadataFormatException3;
					CultureInfo cultureInfo = CultureInfo.InvariantCulture;
					object[] objArray1 = new object[] { this.containerName };
					throw new NephosStorageDataCorruptionException(string.Format(cultureInfo, "Error decoding service metadata for container {0}", objArray1), metadataFormatException2);
				}
				this.containerQuotaInGB = (long)RealServiceManager.MaxShareQuotaInGBPriorToLargeFileShareFeature;
				string str = nameValueCollection.Get(RealServiceManager.XSmbContainerQuotaMetadataName);
				if (str != null)
				{
					if (!long.TryParse(str, out num) || num < (long)RealServiceManager.MinShareQuotaInGB || num > this.containerQuotaInGB)
					{
						TimeSpan? nullable1 = null;
						AlertsManager.AlertOrLogException(string.Format("Invalid XsmbContainerQuota retrieved from servicemetadata for account {0}: {1}", this.accountNameForLogging, str), "InvalidXSMBContainerQuota", nullable1);
					}
					else
					{
						this.containerQuotaInGB = num;
					}
				}
				string str1 = nameValueCollection.Get("PublicAccess");
				string str2 = nameValueCollection.Get("PublicAccess1");
				if (str1 != null)
				{
					this.publicAccessLevel = str1;
					return;
				}
				if (str2 != null)
				{
					this.publicAccessLevel = str2;
				}
			}
		}
	}
}