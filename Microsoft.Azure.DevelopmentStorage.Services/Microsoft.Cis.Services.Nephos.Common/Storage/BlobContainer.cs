using AsyncHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class BlobContainer : BaseBlobContainer, IBlobContainer, IBaseBlobContainer, IContainer, IDisposable
	{
		public readonly static DateTime DefaultSnapshotTimestamp;

		public override Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType ContainerType
		{
			get
			{
				return Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.BlobContainer;
			}
		}

		protected new IBlobContainer InternalContainer
		{
			get
			{
				return (IBlobContainer)base.InternalContainer;
			}
		}

		static BlobContainer()
		{
			BlobContainer.DefaultSnapshotTimestamp = DateTime.MaxValue;
		}

		internal BlobContainer(IBlobContainer container) : base(container)
		{
		}

		private IEnumerator<IAsyncResult> ListBlobsImpl(string blobNamePrefix, BlobPropertyNames propertyNames, string separator, string blobNameStart, DateTime? snapshotStart, IBlobObjectCondition condition, int maxBlobNames, BlobServiceVersion version, AsyncIteratorContext<IBlobObjectCollection> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.InternalContainer.BeginListBlobs(blobNamePrefix, propertyNames, separator, blobNameStart, snapshotStart, Helpers.Convert(condition), maxBlobNames, version, context.GetResumeCallback(), context.GetResumeState("BlobContainer.ListBlobsImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				IBlobObjectCollection blobObjectCollections = this.InternalContainer.EndListBlobs(asyncResult);
				context.ResultData = new BlobObjectCollection(blobObjectCollections);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.BeginListBlobs(string blobNamePrefix, BlobPropertyNames propertyNames, string separator, string blobNameStart, DateTime? snapshotStart, IBlobObjectCondition condition, int maxBlobNames, BlobServiceVersion version, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IBlobObjectCollection> asyncIteratorContext = new AsyncIteratorContext<IBlobObjectCollection>("RealBlobContainer.ListBlobs", callback, state);
			asyncIteratorContext.Begin(this.ListBlobsImpl(blobNamePrefix, propertyNames, separator, blobNameStart, snapshotStart, condition, maxBlobNames, version, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IListBlobObject Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.CreateAppendBlobInstance(string blobName, DateTime snapshot, BlobServiceVersion version)
		{
			IListBlobObject listBlobObject;
			try
			{
				IListBlobObject listBlobObject1 = this.InternalContainer.CreateAppendBlobInstance(blobName, snapshot, version);
				listBlobObject = new ListBlobObject(listBlobObject1);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			return listBlobObject;
		}

		IBlobObject Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.CreateBaseBlobObjectInstance(string blobName, DateTime snapshot, BlobServiceVersion version)
		{
			IBlobObject baseBlobObject;
			try
			{
				IBlobObject blobObject = this.InternalContainer.CreateBaseBlobObjectInstance(blobName, snapshot, version);
				baseBlobObject = new BaseBlobObject(blobObject);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			return baseBlobObject;
		}

		IListBlobObject Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.CreateBlockBlobInstance(string blobName, DateTime snapshot, BlobServiceVersion version)
		{
			IListBlobObject listBlobObject;
			try
			{
				IListBlobObject listBlobObject1 = this.InternalContainer.CreateBlockBlobInstance(blobName, snapshot, version);
				listBlobObject = new ListBlobObject(listBlobObject1);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			return listBlobObject;
		}

		IIndexBlobObject Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.CreateIndexBlobObjectInstance(string blobName, DateTime snapshot, BlobServiceVersion version)
		{
			IIndexBlobObject indexBlobObject;
			try
			{
				IIndexBlobObject indexBlobObject1 = this.InternalContainer.CreateIndexBlobObjectInstance(blobName, snapshot, version);
				indexBlobObject = new IndexBlobObject(indexBlobObject1);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			return indexBlobObject;
		}

		IBlobObjectCollection Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.EndListBlobs(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<IBlobObjectCollection> asyncIteratorContext = (AsyncIteratorContext<IBlobObjectCollection>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}
	}
}