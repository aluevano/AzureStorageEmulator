using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class ListBlobObject : BaseBlobObject, IListBlobObject, IBlobObject, IDisposable
	{
		public long? BlobAppendOffset
		{
			get
			{
				return ((IListBlobObject)this.blob).BlobAppendOffset;
			}
		}

		public ListBlobObject(IListBlobObject blob) : base(blob)
		{
		}

		private IEnumerator<IAsyncResult> AppendBlockImpl(long contentLength, Stream inputStream, CrcReaderStream crcReaderStream, IBlobObjectCondition condition, long? conditionalMaxBlobSize, long? conditionalAppendBlockPosition, AsyncIteratorContext<IAppendBlockResult> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = ((IListBlobObject)this.blob).BeginAppendBlock(contentLength, inputStream, crcReaderStream, condition, conditionalMaxBlobSize, conditionalAppendBlockPosition, context.GetResumeCallback(), context.GetResumeState("RealBlobObject.AppendBlockImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				((IListBlobObject)this.blob).EndAppendBlock(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		public IAsyncResult BeginAppendBlock(long contentLength, Stream inputStream, CrcReaderStream crcReaderStream, IBlobObjectCondition condition, long? maxBlobSizeCondition, long? blockAppendPositionCondition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IAppendBlockResult> asyncIteratorContext = new AsyncIteratorContext<IAppendBlockResult>("RealBlobObject.AppendBlock", callback, state);
			asyncIteratorContext.Begin(this.AppendBlockImpl(contentLength, inputStream, crcReaderStream, condition, maxBlobSizeCondition, blockAppendPositionCondition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetBlockList(IBlobObjectCondition condition, BlockListTypes blockListTypes, BlobServiceVersion blobServiceVersion, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IBlockLists> asyncIteratorContext = new AsyncIteratorContext<IBlockLists>("RealBlobObject.GetBlockList", callback, state);
			asyncIteratorContext.Begin(this.GetBlockListImpl(condition, blockListTypes, blobServiceVersion, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginPutBlob(string contentType, long contentLength, byte[] serviceMetadata, byte[] applicationMetadata, byte[][] blockList, BlockSource[] blockSourceList, byte[] contentMD5, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.PutBlob", callback, state);
			asyncIteratorContext.Begin(this.PutBlobImpl(contentType, contentLength, serviceMetadata, applicationMetadata, blockList, blockSourceList, contentMD5, overwriteOption, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginPutBlock(byte[] blockIdentifier, long contentLength, Stream inputStream, byte[] contentMD5, AsyncCallback callback, object state)
		{
			return this.BeginPutBlock(blockIdentifier, contentLength, inputStream, null, contentMD5, false, null, callback, state);
		}

		public IAsyncResult BeginPutBlock(byte[] blockIdentifier, long contentLength, Stream inputStream, CrcReaderStream crcReaderStream, byte[] contentMD5, bool isLargeBlockBlobRequest, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.PutBlock", callback, state);
			asyncIteratorContext.Begin(this.PutBlockImpl(blockIdentifier, contentLength, inputStream, crcReaderStream, contentMD5, isLargeBlockBlobRequest, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public void EndAppendBlock(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<IAppendBlockResult>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public IBlockLists EndGetBlockList(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<IBlockLists> asyncIteratorContext = (AsyncIteratorContext<IBlockLists>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public void EndPutBlock(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private IEnumerator<IAsyncResult> GetBlockListImpl(IBlobObjectCondition condition, BlockListTypes blockListTypes, BlobServiceVersion blobServiceVersion, AsyncIteratorContext<IBlockLists> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = ((IListBlobObject)this.blob).BeginGetBlockList(Helpers.Convert(condition), blockListTypes, blobServiceVersion, context.GetResumeCallback(), context.GetResumeState("RealBlobObject.GetBlockListImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				context.ResultData = ((IListBlobObject)this.blob).EndGetBlockList(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> PutBlobImpl(string contentType, long contentLength, byte[] serviceMetadata, byte[] applicationMetadata, byte[][] blockIdList, BlockSource[] blockSourceList, byte[] contentMD5, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			bool flag = contentLength == (long)-1;
			object[] objArray = new object[] { contentLength };
			NephosAssertionException.Assert(flag, "The contentLength we are going to pass into XStore for commiting blob is invalid: {0}", objArray);
			try
			{
				asyncResult = ((IListBlobObject)this.blob).BeginPutBlob(contentType, contentLength, serviceMetadata, applicationMetadata, blockIdList, blockSourceList, contentMD5, overwriteOption, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("RealBlobObject.PutBlobImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndPutBlob(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> PutBlockImpl(byte[] blockIdentifier, long contentLength, Stream inputStream, CrcReaderStream crcReaderStream, byte[] contentMD5, bool isLargeBlockBlobRequest, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = ((IListBlobObject)this.blob).BeginPutBlock(blockIdentifier, contentLength, inputStream, crcReaderStream, contentMD5, isLargeBlockBlobRequest, condition, context.GetResumeCallback(), context.GetResumeState("RealBlobObject.PutBlockImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				((IListBlobObject)this.blob).EndPutBlock(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}
	}
}