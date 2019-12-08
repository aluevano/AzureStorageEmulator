using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlobContainer : IBaseBlobContainer, IContainer, IDisposable
	{
		IAsyncResult BeginListBlobs(string blobNamePrefix, BlobPropertyNames propertyNames, string separator, string blobNameStart, DateTime? snapshotStart, IBlobObjectCondition condition, int maxBlobNames, BlobServiceVersion version, AsyncCallback callback, object state);

		IListBlobObject CreateAppendBlobInstance(string blobName, DateTime snapshot, BlobServiceVersion blobServiceVersion);

		IBlobObject CreateBaseBlobObjectInstance(string blobName, DateTime snapshot, BlobServiceVersion blobServiceVersion);

		IListBlobObject CreateBlockBlobInstance(string blobName, DateTime snapshot, BlobServiceVersion blobServiceVersion);

		IIndexBlobObject CreateIndexBlobObjectInstance(string blobName, DateTime snapshot, BlobServiceVersion blobServiceVersion);

		IBlobObjectCollection EndListBlobs(IAsyncResult ar);
	}
}