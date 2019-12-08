using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public static class BlobContainerExtensions
	{
		public static IListBlobObject CreateAppendBlobInstance(this IBlobContainer container, string blobName)
		{
			return container.CreateAppendBlobInstance(blobName, StorageStampHelpers.RootBlobSnapshotVersion, BlobServiceVersion.Original);
		}

		public static IListBlobObject CreateAppendBlobInstance(this IBlobContainer container, string blobName, BlobServiceVersion blobServiceVersion)
		{
			return container.CreateAppendBlobInstance(blobName, StorageStampHelpers.RootBlobSnapshotVersion, blobServiceVersion);
		}

		public static IBlobObject CreateBaseBlobObjectInstance(this IBlobContainer container, string blobName)
		{
			return container.CreateBaseBlobObjectInstance(blobName, StorageStampHelpers.RootBlobSnapshotVersion, BlobServiceVersion.Original);
		}

		public static IBlobObject CreateBaseBlobObjectInstance(this IBlobContainer container, string blobName, DateTime snapshot)
		{
			return container.CreateBaseBlobObjectInstance(blobName, snapshot, BlobServiceVersion.Original);
		}

		public static IBlobObject CreateBlobObjectInstance(this IBlobContainer container, BlobType blobType, string blobName)
		{
			return container.CreateBlobObjectInstance(blobType, blobName, BlobServiceVersion.Original);
		}

		public static IBlobObject CreateBlobObjectInstance(this IBlobContainer container, BlobType blobType, string blobName, BlobServiceVersion blobServiceVersion)
		{
			if (blobType == BlobType.ListBlob)
			{
				return container.CreateBlockBlobInstance(blobName, StorageStampHelpers.RootBlobSnapshotVersion, blobServiceVersion);
			}
			if (blobType == BlobType.AppendBlob)
			{
				return container.CreateAppendBlobInstance(blobName, StorageStampHelpers.RootBlobSnapshotVersion, blobServiceVersion);
			}
			if (blobType != BlobType.IndexBlob)
			{
				throw new ArgumentException("blobType");
			}
			return container.CreateIndexBlobObjectInstance(blobName, StorageStampHelpers.RootBlobSnapshotVersion, blobServiceVersion);
		}

		public static IListBlobObject CreateBlockBlobInstance(this IBlobContainer container, string blobName)
		{
			return container.CreateBlockBlobInstance(blobName, StorageStampHelpers.RootBlobSnapshotVersion, BlobServiceVersion.Original);
		}

		public static IListBlobObject CreateBlockBlobInstance(this IBlobContainer container, string blobName, BlobServiceVersion blobServiceVersion)
		{
			return container.CreateBlockBlobInstance(blobName, StorageStampHelpers.RootBlobSnapshotVersion, blobServiceVersion);
		}

		public static IIndexBlobObject CreateIndexBlobObjectInstance(this IBlobContainer container, string blobName)
		{
			return container.CreateIndexBlobObjectInstance(blobName, StorageStampHelpers.RootBlobSnapshotVersion, BlobServiceVersion.Original);
		}

		public static IIndexBlobObject CreateIndexBlobObjectInstance(this IBlobContainer container, string blobName, DateTime snapshot)
		{
			return container.CreateIndexBlobObjectInstance(blobName, snapshot, BlobServiceVersion.Original);
		}
	}
}