using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Specialized;
using System.Globalization;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ListBlobResultsBlobProperties : IListBlobResultsBlobProperties
	{
		private string blobName;

		private string containerName;

		private DateTime? lastModifiedTime;

		private long? size;

		private string contentDisposition;

		private string contentType;

		private string contentLanguage;

		private string contentEncoding;

		private string contentCrc64;

		private string contentMD5;

		private long? sequenceNumber;

		private string blobType;

		private string leaseStatus;

		private string leaseState;

		private string leaseDuration;

		private DateTime snapshot;

		private NameValueCollection metadata;

		private string cacheControl;

		private bool isActualBlob;

		private string copyId;

		private string copySource;

		private string copyStatus;

		private string copyStatusDescription;

		private string copyProgress;

		private DateTime? copyCompletionTime;

		private bool? isBlobEncrypted;

		private bool isIncrementalCopy;

		private DateTime? lastCopySnapshot;

		public string BlobName
		{
			get
			{
				return this.blobName;
			}
		}

		public string BlobType
		{
			get
			{
				this.CheckIsActualBlob();
				return this.blobType;
			}
		}

		public string CacheControl
		{
			get
			{
				this.CheckIsActualBlob();
				return this.cacheControl;
			}
		}

		public string ContainerName
		{
			get
			{
				return this.containerName;
			}
		}

		public string ContentCrc64
		{
			get
			{
				this.CheckIsActualBlob();
				return this.contentCrc64;
			}
		}

		public string ContentDisposition
		{
			get
			{
				this.CheckIsActualBlob();
				return this.contentDisposition;
			}
		}

		public string ContentEncoding
		{
			get
			{
				this.CheckIsActualBlob();
				return this.contentEncoding;
			}
		}

		public string ContentLanguage
		{
			get
			{
				this.CheckIsActualBlob();
				return this.contentLanguage;
			}
		}

		public long? ContentLength
		{
			get
			{
				this.CheckIsActualBlob();
				return this.size;
			}
		}

		public string ContentMD5
		{
			get
			{
				this.CheckIsActualBlob();
				return this.contentMD5;
			}
		}

		public string ContentType
		{
			get
			{
				this.CheckIsActualBlob();
				return this.contentType;
			}
		}

		public DateTime? CopyCompletionTime
		{
			get
			{
				this.CheckIsActualBlob();
				return this.copyCompletionTime;
			}
		}

		public string CopyId
		{
			get
			{
				this.CheckIsActualBlob();
				return this.copyId;
			}
		}

		public string CopyProgress
		{
			get
			{
				this.CheckIsActualBlob();
				return this.copyProgress;
			}
		}

		public string CopySource
		{
			get
			{
				this.CheckIsActualBlob();
				return this.copySource;
			}
		}

		public string CopyStatus
		{
			get
			{
				this.CheckIsActualBlob();
				return this.copyStatus;
			}
		}

		public string CopyStatusDescription
		{
			get
			{
				this.CheckIsActualBlob();
				return this.copyStatusDescription;
			}
		}

		public bool IsActualBlob
		{
			get
			{
				return this.isActualBlob;
			}
		}

		public bool? IsBlobEncrypted
		{
			get
			{
				this.CheckIsActualBlob();
				return this.isBlobEncrypted;
			}
		}

		public bool IsIncrementalCopy
		{
			get
			{
				this.CheckIsActualBlob();
				return this.isIncrementalCopy;
			}
		}

		public DateTime? LastCopySnapshot
		{
			get
			{
				this.CheckIsActualBlob();
				return this.lastCopySnapshot;
			}
		}

		public DateTime? LastModifiedTime
		{
			get
			{
				this.CheckIsActualBlob();
				return this.lastModifiedTime;
			}
		}

		public string LeaseDuration
		{
			get
			{
				this.CheckIsActualBlob();
				return this.leaseDuration;
			}
		}

		public string LeaseState
		{
			get
			{
				this.CheckIsActualBlob();
				return this.leaseState;
			}
		}

		public string LeaseStatus
		{
			get
			{
				this.CheckIsActualBlob();
				return this.leaseStatus;
			}
		}

		public NameValueCollection Metadata
		{
			get
			{
				this.CheckIsActualBlob();
				return this.metadata;
			}
		}

		public long? SequenceNumber
		{
			get
			{
				this.CheckIsActualBlob();
				return this.sequenceNumber;
			}
		}

		public DateTime Snapshot
		{
			get
			{
				this.CheckIsActualBlob();
				return this.snapshot;
			}
		}

		public ListBlobResultsBlobProperties(IBlobObject blobObject, string separator)
		{
			if (blobObject == null)
			{
				throw new ArgumentNullException("blobObject");
			}
			this.blobName = blobObject.Name;
			this.containerName = blobObject.ContainerName;
			if (separator == null || blobObject.Type != Microsoft.Cis.Services.Nephos.Common.Storage.BlobType.None)
			{
				this.isActualBlob = true;
				this.contentType = blobObject.ContentType;
				NephosAssertionException.Assert(blobObject.LastModificationTime.HasValue);
				NephosAssertionException.Assert(blobObject.ContentLength.HasValue);
				this.lastModifiedTime = blobObject.LastModificationTime;
				this.size = blobObject.ContentLength;
				NameValueCollection nameValueCollection = null;
				if (blobObject.ServiceMetadata != null)
				{
					nameValueCollection = new NameValueCollection();
					try
					{
						MetadataEncoding.Decode(blobObject.ServiceMetadata, nameValueCollection);
					}
					catch (MetadataFormatException metadataFormatException1)
					{
						MetadataFormatException metadataFormatException = metadataFormatException1;
						CultureInfo invariantCulture = CultureInfo.InvariantCulture;
						object[] objArray = new object[] { this.blobName };
						throw new NephosStorageDataCorruptionException(string.Format(invariantCulture, "Error decoding service metadata for blob {0}", objArray), metadataFormatException);
					}
					this.contentEncoding = nameValueCollection[RealServiceManager.ContentEncodingTag];
					this.contentLanguage = nameValueCollection[RealServiceManager.ContentLanguageTag];
					this.cacheControl = nameValueCollection[RealServiceManager.CacheControlTag];
					this.contentCrc64 = nameValueCollection[RealServiceManager.ContentCrc64Tag];
					this.contentMD5 = nameValueCollection[RealServiceManager.ContentMD5Tag];
					this.contentDisposition = nameValueCollection[RealServiceManager.ContentDispositionTag];
					if (this.contentMD5 != null && this.contentMD5.Equals(RealServiceManager.EmptyContentMD5Value, StringComparison.OrdinalIgnoreCase))
					{
						this.contentMD5 = null;
					}
					this.copyId = nameValueCollection[RealServiceManager.CopyIdTag];
					this.copySource = nameValueCollection[RealServiceManager.CopySourceTag];
					this.copyStatus = nameValueCollection[RealServiceManager.CopyStatusTag];
					this.copyStatusDescription = nameValueCollection[RealServiceManager.CopyStatusDescriptionTag];
					if (!string.IsNullOrEmpty(nameValueCollection[RealServiceManager.CopyProgressOffsetTag]) && !string.IsNullOrEmpty(nameValueCollection[RealServiceManager.CopyProgressTotalTag]))
					{
						this.copyProgress = string.Format("{0}/{1}", nameValueCollection[RealServiceManager.CopyProgressOffsetTag], nameValueCollection[RealServiceManager.CopyProgressTotalTag]);
					}
					this.copyCompletionTime = RealServiceManager.ParseDateTimeFromString(nameValueCollection[RealServiceManager.CopyCompletionTimeTag]);
				}
				if (blobObject.ApplicationMetadata != null)
				{
					this.metadata = new NameValueCollection();
					try
					{
						MetadataEncoding.Decode(blobObject.ApplicationMetadata, this.metadata);
					}
					catch (MetadataFormatException metadataFormatException3)
					{
						MetadataFormatException metadataFormatException2 = metadataFormatException3;
						CultureInfo cultureInfo = CultureInfo.InvariantCulture;
						object[] objArray1 = new object[] { this.blobName };
						throw new NephosStorageDataCorruptionException(string.Format(cultureInfo, "Error decoding application metadata for blob {0}", objArray1), metadataFormatException2);
					}
				}
				this.blobType = BlobTypeStrings.GetString(blobObject.Type);
				if (blobObject.LeaseInfo != null)
				{
					if (blobObject.LeaseInfo.Type == LeaseType.ReadWrite && blobObject.LeaseInfo.Duration.HasValue)
					{
						TimeSpan? duration = blobObject.LeaseInfo.Duration;
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
					if (blobObject.LeaseInfo.State.HasValue)
					{
						this.leaseState = LeaseStateStrings.LeaseStates[(int)blobObject.LeaseInfo.State.Value];
						if (blobObject.LeaseInfo.State.Equals(Microsoft.Cis.Services.Nephos.Common.Storage.LeaseState.Leased))
						{
							TimeSpan? nullable = blobObject.LeaseInfo.Duration;
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
				this.snapshot = blobObject.Snapshot;
				if (blobObject.Type != Microsoft.Cis.Services.Nephos.Common.Storage.BlobType.IndexBlob)
				{
					this.sequenceNumber = null;
				}
				else
				{
					NephosAssertionException.Assert(blobObject.SequenceNumber.HasValue, "SequenceNumber must be present for PageBlob's.");
					this.sequenceNumber = blobObject.SequenceNumber;
				}
				this.isBlobEncrypted = blobObject.IsBlobEncrypted;
				this.isIncrementalCopy = blobObject.IsIncrementalCopy;
				if (this.isIncrementalCopy && nameValueCollection != null)
				{
					this.lastCopySnapshot = RealServiceManager.ParseDateTimeFromString(nameValueCollection[RealServiceManager.LastCopySnapshotTag]);
				}
			}
		}

		private void CheckIsActualBlob()
		{
			if (!this.isActualBlob)
			{
				throw new InvalidOperationException(string.Concat(this.blobName, " is not an actual blob"));
			}
		}
	}
}