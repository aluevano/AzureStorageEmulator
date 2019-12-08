using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class IndexBlobObject : BaseBlobObject, IIndexBlobObject, IBlobObject, IDisposable
	{
		public IndexBlobObject(IBlobObject blob) : base(blob)
		{
		}

		public IAsyncResult BeginClearPage(IBlobRegion blobRegion, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("IndexBlobObject.ClearPage", callback, state);
			asyncIteratorContext.Begin(this.ClearPageImpl(blobRegion, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetPageRangeList(IBlobRegion blobRegion, BlobPropertyNames additionalPropertyNames, IBlobObjectCondition condition, int maxPageRanges, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IPageRangeCollection> asyncIteratorContext = new AsyncIteratorContext<IPageRangeCollection>("IndexBlobObject.GetPageRangeList", callback, state);
			DateTime? nullable = null;
			asyncIteratorContext.Begin(this.GetPageRangeListImpl(blobRegion, additionalPropertyNames, condition, maxPageRanges, nullable, true, true, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetPageRangeList(IBlobRegion blobRegion, BlobPropertyNames additionalPropertyNames, IBlobObjectCondition condition, int maxPageRanges, DateTime? prevSnapshotTimestamp, bool isRangeCompressed, bool skipClearPages, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IPageRangeCollection> asyncIteratorContext = new AsyncIteratorContext<IPageRangeCollection>("IndexBlobObject.GetPageRangeList", callback, state);
			asyncIteratorContext.Begin(this.GetPageRangeListImpl(blobRegion, additionalPropertyNames, condition, maxPageRanges, prevSnapshotTimestamp, isRangeCompressed, skipClearPages, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginPutPage(IBlobRegion blobRegion, Stream inputStream, CrcReaderStream crcReaderStream, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("IndexBlobObject.PutPage", callback, state);
			asyncIteratorContext.Begin(this.PutPageImpl(blobRegion, inputStream, crcReaderStream, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IEnumerator<IAsyncResult> ClearPageImpl(IBlobRegion blobRegion, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = ((IIndexBlobObject)this.blob).BeginClearPage(blobRegion, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("IndexBlobObject.ClearPageImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				((IIndexBlobObject)this.blob).EndClearPage(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		public void EndClearPage(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public IPageRangeCollection EndGetPageRangeList(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<IPageRangeCollection> asyncIteratorContext = (AsyncIteratorContext<IPageRangeCollection>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public void EndPutPage(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private IEnumerator<IAsyncResult> GetPageRangeListImpl(IBlobRegion blobRegion, BlobPropertyNames additionalPropertyNames, IBlobObjectCondition condition, int maxPageRanges, DateTime? prevSnapshotTimestamp, bool isRangeCompressed, bool skipClearPages, AsyncIteratorContext<IPageRangeCollection> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = ((IIndexBlobObject)this.blob).BeginGetPageRangeList(blobRegion, additionalPropertyNames, Helpers.Convert(condition), maxPageRanges, prevSnapshotTimestamp, isRangeCompressed, skipClearPages, context.GetResumeCallback(), context.GetResumeState("IndexBlobObject.GetPageRangeListImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				context.ResultData = ((IIndexBlobObject)this.blob).EndGetPageRangeList(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> PutPageImpl(IBlobRegion blobRegion, Stream inputStream, CrcReaderStream crcReaderStream, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = ((IIndexBlobObject)this.blob).BeginPutPage(blobRegion, inputStream, crcReaderStream, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("IndexBlobObject.PutPageImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				((IIndexBlobObject)this.blob).EndPutPage(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}
	}
}