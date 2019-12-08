using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class BaseBlobObject : IBlobObject, IDisposable
	{
		public IBlobObject blob;

		public byte[] ApplicationMetadata
		{
			get
			{
				byte[] applicationMetadata;
				try
				{
					applicationMetadata = this.blob.ApplicationMetadata;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return applicationMetadata;
			}
		}

		public int? CommittedBlockCount
		{
			get
			{
				return this.blob.CommittedBlockCount;
			}
		}

		public string ContainerName
		{
			get
			{
				string containerName;
				try
				{
					containerName = this.blob.ContainerName;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return containerName;
			}
		}

		public long? ContentCrc64
		{
			get
			{
				long? contentCrc64;
				try
				{
					contentCrc64 = this.blob.ContentCrc64;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return contentCrc64;
			}
		}

		public long? ContentLength
		{
			get
			{
				long? contentLength;
				try
				{
					contentLength = this.blob.ContentLength;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return contentLength;
			}
		}

		public byte[] ContentMD5
		{
			get
			{
				byte[] contentMD5;
				try
				{
					contentMD5 = this.blob.ContentMD5;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return contentMD5;
			}
		}

		public string ContentType
		{
			get
			{
				string contentType;
				try
				{
					contentType = this.blob.ContentType;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return contentType;
			}
		}

		public DateTime? CreationTime
		{
			get
			{
				DateTime? creationTime;
				try
				{
					creationTime = this.blob.CreationTime;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return creationTime;
			}
		}

		public bool? IsBlobEncrypted
		{
			get
			{
				bool? isBlobEncrypted;
				try
				{
					isBlobEncrypted = this.blob.IsBlobEncrypted;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return isBlobEncrypted;
			}
		}

		public bool IsIncrementalCopy
		{
			get
			{
				return this.blob.IsIncrementalCopy;
			}
		}

		public bool? IsWriteEncrypted
		{
			get
			{
				bool? isWriteEncrypted;
				try
				{
					isWriteEncrypted = this.blob.IsWriteEncrypted;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return isWriteEncrypted;
			}
		}

		public DateTime? LastModificationTime
		{
			get
			{
				DateTime? lastModificationTime;
				try
				{
					lastModificationTime = this.blob.LastModificationTime;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return lastModificationTime;
			}
		}

		public ILeaseInfo LeaseInfo
		{
			get
			{
				ILeaseInfo leaseInfo;
				try
				{
					leaseInfo = this.blob.LeaseInfo;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return leaseInfo;
			}
		}

		public string Name
		{
			get
			{
				string name;
				try
				{
					name = this.blob.Name;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return name;
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get
			{
				return this.blob.OperationStatus;
			}
			set
			{
				this.blob.OperationStatus = value;
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get
			{
				return this.blob.ProviderInjection;
			}
			set
			{
				this.blob.ProviderInjection = value;
			}
		}

		public long? SequenceNumber
		{
			get
			{
				long? sequenceNumber;
				try
				{
					sequenceNumber = this.blob.SequenceNumber;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return sequenceNumber;
			}
		}

		public byte[] ServiceMetadata
		{
			get
			{
				byte[] serviceMetadata;
				try
				{
					serviceMetadata = this.blob.ServiceMetadata;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return serviceMetadata;
			}
		}

		public DateTime Snapshot
		{
			get
			{
				DateTime snapshot;
				try
				{
					snapshot = this.blob.Snapshot;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return snapshot;
			}
		}

		public TimeSpan Timeout
		{
			get
			{
				return this.blob.Timeout;
			}
			set
			{
				try
				{
					this.blob.Timeout = StorageStampHelpers.AdjustTimeoutRange(value);
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		public BlobType Type
		{
			get
			{
				BlobType type;
				try
				{
					type = this.blob.Type;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return type;
			}
		}

		public BaseBlobObject(IBlobObject blob)
		{
			this.blob = blob;
		}

		private IEnumerator<IAsyncResult> AbortCopyBlobImpl(Guid copyId, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginAbortCopyBlob(copyId, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.AbortCopyBlobImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndAbortCopyBlob(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> AcquireLeaseImpl(LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginAcquireLease(leaseType, leaseDuration, proposedLeaseId, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.AcquireLeaseImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndAcquireLease(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> AsynchronousCopyBlobImpl(UriString copySource, bool isSourceAzureBlob, FECopyType copyType, long contentLength, string contentType, NameValueCollection serviceMetadataCollection, byte[] applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, string sourceETag, IBlobObjectCondition destinationCondition, AsyncIteratorContext<CopyBlobOperationInfo> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginAsynchronousCopyBlob(copySource, isSourceAzureBlob, copyType, contentLength, contentType, serviceMetadataCollection, applicationMetadata, sequenceNumberUpdate, overwriteOption, sourceETag, Helpers.Convert(destinationCondition), context.GetResumeCallback(), context.GetResumeState("IBlobObject.BeginAsynchronousCopyBlob"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			CopyBlobOperationInfo copyBlobOperationInfo = null;
			try
			{
				copyBlobOperationInfo = this.blob.EndAsynchronousCopyBlob(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			context.ResultData = copyBlobOperationInfo;
		}

		public IAsyncResult BeginAbortCopyBlob(Guid copyId, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.AbortCopyBlob", callback, state);
			asyncIteratorContext.Begin(this.AbortCopyBlobImpl(copyId, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginAcquireLease(LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.AcquireLease", callback, state);
			asyncIteratorContext.Begin(this.AcquireLeaseImpl(leaseType, leaseDuration, proposedLeaseId, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginAsynchronousCopyBlob(UriString copySource, bool isSourceAzureBlob, FECopyType copyType, long contentLength, string contentType, NameValueCollection serviceMetadataCollection, byte[] applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, string sourceETag, IBlobObjectCondition destinationCondition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<CopyBlobOperationInfo> asyncIteratorContext = new AsyncIteratorContext<CopyBlobOperationInfo>("RealBlobObject.AsynchronousCopyBlob", callback, state);
			asyncIteratorContext.Begin(this.AsynchronousCopyBlobImpl(copySource, isSourceAzureBlob, copyType, contentLength, contentType, serviceMetadataCollection, applicationMetadata, sequenceNumberUpdate, overwriteOption, sourceETag, destinationCondition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginBreakLease(TimeSpan? leaseBreakPeriod, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.BreakLease", callback, state);
			asyncIteratorContext.Begin(this.BreakLeaseImpl(leaseBreakPeriod, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginChangeLease(Guid leaseId, Guid proposedLeaseId, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.ChangeLease", callback, state);
			asyncIteratorContext.Begin(this.ChangeLeaseImpl(leaseId, proposedLeaseId, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetBlob(IBlobRegion blobRegion, BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<CrcStream> asyncIteratorContext = new AsyncIteratorContext<CrcStream>("RealBlobObject.GetBlob", callback, state);
			asyncIteratorContext.Begin(this.GetBlobImpl(blobRegion, propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetProperties(BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginPutBlob(string contentType, long contentLength, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, Stream inputStream, CrcReaderStream crcReaderStream, byte[] contentMD5, bool invokeGeneratePutBlobServiceMetadata, GeneratePutBlobServiceMetadata generatePutBlobServiceMetadata, bool isLargeBlockBlobRequest, bool is8TBPageBlobAllowed, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.PutBlob", callback, state);
			asyncIteratorContext.Begin(this.PutBlobImpl(contentType, contentLength, maxBlobSize, serviceMetadata, applicationMetadata, inputStream, crcReaderStream, contentMD5, invokeGeneratePutBlobServiceMetadata, generatePutBlobServiceMetadata, isLargeBlockBlobRequest, is8TBPageBlobAllowed, sequenceNumberUpdate, overwriteOption, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginReleaseLease(Guid leaseId, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.ReleaseLease", callback, state);
			asyncIteratorContext.Begin(this.ReleaseLeaseImpl(leaseId, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginRenewLease(LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.RenewLease", callback, state);
			asyncIteratorContext.Begin(this.RenewLeaseImpl(leaseType, leaseId, leaseDuration, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetApplicationMetadata(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.SetApplicationMetadata", callback, state);
			asyncIteratorContext.Begin(this.SetApplicationMetadataImpl(metadata, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetExpiryTime(DateTime expiryTime, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.SetExpiryTime", callback, state);
			asyncIteratorContext.Begin(this.SetExpiryTimeImpl(expiryTime, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetProperties(string contentType, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.SetProperties", callback, state);
			asyncIteratorContext.Begin(this.SetPropertiesImpl(contentType, maxBlobSize, serviceMetadata, applicationMetadata, sequenceNumberUpdate, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetProperties(string contentType, long? maxBlobSize, NameValueCollection serviceMetadata, ServiceMetadataUpdatePolicy serviceMetadataPolicy, string[] serviceMetadataPolicyArguments, NameValueCollection applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.SetProperties", callback, state);
			asyncIteratorContext.Begin(this.SetPropertiesImpl(contentType, maxBlobSize, serviceMetadata, serviceMetadataPolicy, serviceMetadataPolicyArguments, applicationMetadata, sequenceNumberUpdate, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetServiceMetadata(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobObject.SetServiceMetadata", callback, state);
			asyncIteratorContext.Begin(this.SetServiceMetadataImpl(metadata, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSnapshotBlob(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<DateTime> asyncIteratorContext = new AsyncIteratorContext<DateTime>("RealBlobObject.SnapshotBlob", callback, state);
			asyncIteratorContext.Begin(this.SnapshotBlobImpl(metadata, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSynchronousCopyBlob(string sourceAccount, IBlobObject sourceBlob, DateTime? expiryTime, byte[] applicationMetadata, OverwriteOption overwriteOption, IBlobObjectCondition sourceCondition, IBlobObjectCondition destinationCondition, UriString copySource, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<CopyBlobOperationInfo> asyncIteratorContext = new AsyncIteratorContext<CopyBlobOperationInfo>("RealBlobObject.SynchronousCopyBlob", callback, state);
			asyncIteratorContext.Begin(this.SynchronousCopyBlobImpl(sourceAccount, sourceBlob, expiryTime, applicationMetadata, overwriteOption, sourceCondition, destinationCondition, copySource, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IEnumerator<IAsyncResult> BreakLeaseImpl(TimeSpan? leaseBreakPeriod, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginBreakLease(leaseBreakPeriod, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.BreakLeaseImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndBreakLease(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> ChangeLeaseImpl(Guid leaseId, Guid proposedLeaseId, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginChangeLease(leaseId, proposedLeaseId, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.ChangeLeaseImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndChangeLease(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.blob.Dispose();
			}
		}

		public void EndAbortCopyBlob(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndAcquireLease(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public CopyBlobOperationInfo EndAsynchronousCopyBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<CopyBlobOperationInfo> asyncIteratorContext = (AsyncIteratorContext<CopyBlobOperationInfo>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public void EndBreakLease(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndChangeLease(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public CrcStream EndGetBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<CrcStream> asyncIteratorContext = (AsyncIteratorContext<CrcStream>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public void EndGetProperties(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndPutBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndReleaseLease(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndRenewLease(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndSetApplicationMetadata(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndSetExpiryTime(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndSetProperties(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndSetServiceMetadata(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public DateTime EndSnapshotBlob(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<DateTime> asyncIteratorContext = (AsyncIteratorContext<DateTime>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public CopyBlobOperationInfo EndSynchronousCopyBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<CopyBlobOperationInfo> asyncIteratorContext = (AsyncIteratorContext<CopyBlobOperationInfo>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		private IEnumerator<IAsyncResult> GetBlobImpl(IBlobRegion blobRegion, BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncIteratorContext<CrcStream> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginGetBlob(blobRegion, propertyNames, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("RealBlobObject.GetBlobImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				context.ResultData = this.blob.EndGetBlob(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> GetPropertiesImpl(BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginGetProperties(propertyNames, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.GetPropertiesImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndGetProperties(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> PutBlobImpl(string contentType, long contentLength, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, Stream inputStream, CrcReaderStream crcReaderStream, byte[] contentMD5, bool invokeGeneratePutBlobServiceMetadata, GeneratePutBlobServiceMetadata generatePutBlobServiceMetadata, bool isLargeBlockBlobRequest, bool is8TBPageBlobAllowed, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			if (contentLength < (long)0)
			{
				throw new ArgumentOutOfRangeException("contentLength", "contentLength must be >= 0");
			}
			try
			{
				asyncResult = this.blob.BeginPutBlob(contentType, contentLength, maxBlobSize, serviceMetadata, applicationMetadata, inputStream, crcReaderStream, contentMD5, invokeGeneratePutBlobServiceMetadata, generatePutBlobServiceMetadata, isLargeBlockBlobRequest, is8TBPageBlobAllowed, sequenceNumberUpdate, overwriteOption, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("RealBlobObject.PutBlobImpl"));
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

		private IEnumerator<IAsyncResult> ReleaseLeaseImpl(Guid leaseId, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginReleaseLease(leaseId, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.ReleaseLeaseImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndReleaseLease(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> RenewLeaseImpl(LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginRenewLease(leaseType, leaseId, leaseDuration, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.RenewLeaseImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndRenewLease(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> SetApplicationMetadataImpl(byte[] metadata, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginSetApplicationMetadata(metadata, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.SetApplicationMetadataImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndSetApplicationMetadata(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> SetExpiryTimeImpl(DateTime expiryTime, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginSetExpiryTime(StorageStampHelpers.AdjustDateTimeRange(expiryTime), Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.SetExpiryTimeImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndSetExpiryTime(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> SetPropertiesImpl(string contentType, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginSetProperties(contentType, maxBlobSize, serviceMetadata, applicationMetadata, sequenceNumberUpdate, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.SetPropertiesImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndSetProperties(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> SetPropertiesImpl(string contentType, long? maxBlobSize, NameValueCollection serviceMetadata, ServiceMetadataUpdatePolicy serviceMetadataPolicy, string[] serviceMetadataPolicyArguments, NameValueCollection applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginSetProperties(contentType, maxBlobSize, serviceMetadata, serviceMetadataPolicy, serviceMetadataPolicyArguments, applicationMetadata, sequenceNumberUpdate, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.SetPropertiesImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndSetProperties(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> SetServiceMetadataImpl(byte[] metadata, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginSetServiceMetadata(metadata, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.SetServiceMetadataImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.blob.EndSetServiceMetadata(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> SnapshotBlobImpl(byte[] metadata, IBlobObjectCondition condition, AsyncIteratorContext<DateTime> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginSnapshotBlob(metadata, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BaseBlobObject.SnapshotBlobImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			DateTime minValue = DateTime.MinValue;
			try
			{
				minValue = this.blob.EndSnapshotBlob(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			NephosAssertionException.Assert(minValue != DateTime.MinValue, "The snapshot timestamp must be set!");
			context.ResultData = minValue;
		}

		private IEnumerator<IAsyncResult> SynchronousCopyBlobImpl(string sourceAccount, IBlobObject sourceBlob, DateTime? expiryTime, byte[] applicationMetadata, OverwriteOption overwriteOption, IBlobObjectCondition sourceCondition, IBlobObjectCondition destinationCondition, UriString copySource, AsyncIteratorContext<CopyBlobOperationInfo> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.blob.BeginSynchronousCopyBlob(sourceAccount, ((BaseBlobObject)sourceBlob).blob, StorageStampHelpers.AdjustNullableDatetimeRange(expiryTime), applicationMetadata, overwriteOption, Helpers.Convert(sourceCondition), Helpers.Convert(destinationCondition), copySource, context.GetResumeCallback(), context.GetResumeState("IBlobObject.BeginSynchronousCopyBlob"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			CopyBlobOperationInfo copyBlobOperationInfo = null;
			try
			{
				copyBlobOperationInfo = this.blob.EndSynchronousCopyBlob(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			context.ResultData = copyBlobOperationInfo;
		}
	}
}