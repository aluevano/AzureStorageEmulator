using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;
using System.IO;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IListBlobObject : IBlobObject, IDisposable
	{
		long? BlobAppendOffset
		{
			get;
		}

		IAsyncResult BeginAppendBlock(long contentLength, Stream inputStream, CrcReaderStream crcReaderStream, IBlobObjectCondition condition, long? conditionalMaxBlobSize, long? conditionalAppendBlockPosition, AsyncCallback callback, object state);

		IAsyncResult BeginGetBlockList(IBlobObjectCondition condition, BlockListTypes blockListTypes, BlobServiceVersion blobServiceVersion, AsyncCallback callback, object state);

		IAsyncResult BeginPutBlob(string contentType, long contentLength, byte[] serviceMetadata, byte[] applicationMetadata, byte[][] blockIdList, BlockSource[] blockSourceList, byte[] contentMD5, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginPutBlock(byte[] blockIdentifier, long contentLength, Stream inputStream, CrcReaderStream crcReaderStream, byte[] contentMD5, bool isLargeBlockBlobRequest, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginPutBlock(byte[] blockIdentifier, long contentLength, Stream inputStream, byte[] contentMD5, AsyncCallback callback, object state);

		void EndAppendBlock(IAsyncResult asyncResult);

		IBlockLists EndGetBlockList(IAsyncResult asyncResult);

		void EndPutBlock(IAsyncResult asyncResult);
	}
}