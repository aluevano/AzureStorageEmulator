using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class BlobSummary : IBlobSummary, IDisposable
	{
		private IBlobSummary blobSummary;

		public long? ExpiredBlobCount
		{
			get
			{
				long? expiredBlobCount;
				try
				{
					expiredBlobCount = this.blobSummary.ExpiredBlobCount;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return expiredBlobCount;
			}
		}

		public string Name
		{
			get
			{
				string name;
				try
				{
					name = this.blobSummary.Name;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return name;
			}
		}

		public long? TotalExpiredBlobContentLength
		{
			get
			{
				long? totalExpiredBlobContentLength;
				try
				{
					totalExpiredBlobContentLength = this.blobSummary.TotalExpiredBlobContentLength;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return totalExpiredBlobContentLength;
			}
		}

		public long? TotalExpiredBlobMetadataSize
		{
			get
			{
				long? totalExpiredBlobMetadataSize;
				try
				{
					totalExpiredBlobMetadataSize = this.blobSummary.TotalExpiredBlobMetadataSize;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return totalExpiredBlobMetadataSize;
			}
		}

		public long? TotalUnexpiredBlobContentLength
		{
			get
			{
				long? totalUnexpiredBlobContentLength;
				try
				{
					totalUnexpiredBlobContentLength = this.blobSummary.TotalUnexpiredBlobContentLength;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return totalUnexpiredBlobContentLength;
			}
		}

		public long? TotalUnexpiredBlobMetadataSize
		{
			get
			{
				long? totalUnexpiredBlobMetadataSize;
				try
				{
					totalUnexpiredBlobMetadataSize = this.blobSummary.TotalUnexpiredBlobMetadataSize;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return totalUnexpiredBlobMetadataSize;
			}
		}

		public long? UnexpiredBlobCount
		{
			get
			{
				long? unexpiredBlobCount;
				try
				{
					unexpiredBlobCount = this.blobSummary.UnexpiredBlobCount;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return unexpiredBlobCount;
			}
		}

		public BlobSummary(IBlobSummary blobSummary)
		{
			this.blobSummary = blobSummary;
		}

		public void Dispose()
		{
			this.blobSummary.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}