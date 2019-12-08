using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;
using System.IO;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IIndexBlobObject : IBlobObject, IDisposable
	{
		IAsyncResult BeginClearPage(IBlobRegion blobRegion, IBlobObjectCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginGetPageRangeList(IBlobRegion blobRegion, BlobPropertyNames additionalPropertyNames, IBlobObjectCondition condition, int maxPageRanges, AsyncCallback callback, object state);

		IAsyncResult BeginGetPageRangeList(IBlobRegion blobRegion, BlobPropertyNames additionalPropertyNames, IBlobObjectCondition condition, int maxPageRanges, DateTime? prevSnapshotTimestamp, bool isRangeCompressed, bool skipClearPages, AsyncCallback callback, object state);

		IAsyncResult BeginPutPage(IBlobRegion blobRegion, Stream inputStream, CrcReaderStream crcReaderStream, IBlobObjectCondition condition, AsyncCallback callback, object state);

		void EndClearPage(IAsyncResult asyncResult);

		IPageRangeCollection EndGetPageRangeList(IAsyncResult asyncResult);

		void EndPutPage(IAsyncResult asyncResult);
	}
}