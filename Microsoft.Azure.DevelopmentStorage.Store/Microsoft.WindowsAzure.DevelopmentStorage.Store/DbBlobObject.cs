using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbBlobObject : IBlobObject, IDisposable
	{
		protected Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob _blob;

		protected DbStorageManager _storageManager;

		private IBlobObject _realBlobObject;

		protected BlobServiceVersion _blobServiceVersion;

		protected BlobServiceMetaData _blobMetadata;

		public int? CommittedBlockCount
		{
			get
			{
				return JustDecompileGenerated_get_CommittedBlockCount();
			}
			set
			{
				JustDecompileGenerated_set_CommittedBlockCount(value);
			}
		}

		private int? JustDecompileGenerated_CommittedBlockCount_k__BackingField;

		public int? JustDecompileGenerated_get_CommittedBlockCount()
		{
			return this.JustDecompileGenerated_CommittedBlockCount_k__BackingField;
		}

		public void JustDecompileGenerated_set_CommittedBlockCount(int? value)
		{
			this.JustDecompileGenerated_CommittedBlockCount_k__BackingField = value;
		}

		public virtual long? ContentLength
		{
			get
			{
				if (this._realBlobObject != null)
				{
					return this._realBlobObject.ContentLength;
				}
				if (this._blob is PageBlob)
				{
					return ((PageBlob)this._blob).MaxSize;
				}
				return new long?(this._blob.ContentLength);
			}
		}

		public ILeaseInfo LeaseInfo
		{
			get
			{
				return JustDecompileGenerated_get_LeaseInfo();
			}
			set
			{
				JustDecompileGenerated_set_LeaseInfo(value);
			}
		}

		private ILeaseInfo JustDecompileGenerated_LeaseInfo_k__BackingField;

		public ILeaseInfo JustDecompileGenerated_get_LeaseInfo()
		{
			return this.JustDecompileGenerated_LeaseInfo_k__BackingField;
		}

		protected void JustDecompileGenerated_set_LeaseInfo(ILeaseInfo value)
		{
			this.JustDecompileGenerated_LeaseInfo_k__BackingField = value;
		}

		byte[] Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.ApplicationMetadata
		{
			get
			{
				return this._blob.Metadata;
			}
		}

		string Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.ContainerName
		{
			get
			{
				return this._blob.ContainerName;
			}
		}

		long? Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.ContentCrc64
		{
			get
			{
				return this._blob.ContentCrc64;
			}
		}

		byte[] Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.ContentMD5
		{
			get
			{
				return this._blob.ContentMD5;
			}
		}

		string Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.ContentType
		{
			get
			{
				return this._blob.ContentType;
			}
		}

		DateTime? Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.CreationTime
		{
			get
			{
				return new DateTime?(this._blob.CreationTime);
			}
		}

		bool? Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.IsBlobEncrypted
		{
			get
			{
				return new bool?(true);
			}
		}

		bool Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.IsIncrementalCopy
		{
			get
			{
				if (!(this._blob is PageBlob))
				{
					return false;
				}
				bool? isIncrementalCopy = ((PageBlob)this._blob).IsIncrementalCopy;
				if (!isIncrementalCopy.HasValue)
				{
					return false;
				}
				return isIncrementalCopy.GetValueOrDefault();
			}
		}

		bool? Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.IsWriteEncrypted
		{
			get
			{
				return new bool?(true);
			}
		}

		DateTime? Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.LastModificationTime
		{
			get
			{
				if (!this._blob.LastModificationTime.HasValue)
				{
					return null;
				}
				if (this._blob.LastModificationTime.Value <= SqlDateTime.MinValue.Value)
				{
					return new DateTime?(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc));
				}
				return new DateTime?(this._blob.LastModificationTime.Value);
			}
		}

		byte[] Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.ServiceMetadata
		{
			get
			{
				return this._blob.ServiceMetadata;
			}
		}

		DateTime Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.Snapshot
		{
			get
			{
				if (this._blob.VersionTimestamp >= SqlDateTime.MaxValue.Value)
				{
					return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
				}
				return this._blob.VersionTimestamp;
			}
		}

		BlobType Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.Type
		{
			get
			{
				return this._blob.BlobType;
			}
		}

		public string Name
		{
			get
			{
				return this._blob.BlobName;
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get;
			set;
		}

		public long? SequenceNumber
		{
			get
			{
				return this._blob.SequenceNumber;
			}
		}

		public TimeSpan Timeout
		{
			get;
			set;
		}

		internal DbBlobObject(DbBlobContainer container, string blobName, DateTime snapshot, BlobType blobType, BlobServiceVersion blobServiceVersion) : this(container.StorageManager, DbBlobObject.MakeBlob(blobType), blobServiceVersion)
		{
			this._blob.AccountName = container.Account.Name;
			this._blob.ContainerName = container.ContainerName;
			this._blob.BlobName = blobName;
			this._blob.VersionTimestamp = snapshot;
			this.OperationStatus = container.OperationStatus;
		}

		internal DbBlobObject(DbStorageManager storageManager, Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob, BlobServiceVersion blobServiceVersion)
		{
			this._blob = blob;
			this._storageManager = storageManager;
			this._blobServiceVersion = blobServiceVersion;
		}

		private IEnumerator<IAsyncResult> AbortCopyBlobImpl(Guid copyId, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				for (int i = 0; i < 3; i++)
				{
					try
					{
						using (TransactionScope transactionScope = new TransactionScope())
						{
							Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = null;
							using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
							{
								this.LoadContainer(dbContext);
								Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob1 = this.TryLoadBlob(dbContext);
								if (blob1 == null)
								{
									throw new BlobNotFoundException();
								}
								DbBlobObject.CheckConditionsAndReturnResetRequired(blob1, new BlobLeaseInfo(blob1, DateTime.UtcNow), condition, null, false);
								if (blob1.ServiceMetadata == null)
								{
									throw new NoPendingCopyException();
								}
								BlobServiceMetaData instance = BlobServiceMetaData.GetInstance(blob1.ServiceMetadata);
								if (!"pending".Equals(instance.CopyStatus))
								{
									throw new NoPendingCopyException();
								}
								if (!copyId.ToString().Equals(instance.CopyId, StringComparison.OrdinalIgnoreCase))
								{
									throw new CopyIdMismatchException();
								}
								instance.CopyStatus = "aborted";
								instance.CopyCompletionTime = new long?(DateTime.UtcNow.ToFileTimeUtc());
								blob = DbBlobObject.ClearBlob(blob1, instance.GetMetadata());
								dbContext.Blobs.DeleteOnSubmit(blob1);
								dbContext.SubmitChanges();
							}
							using (DevelopmentStorageDbDataContext developmentStorageDbDataContext = DevelopmentStorageDbDataContext.GetDbContext())
							{
								developmentStorageDbDataContext.Blobs.InsertOnSubmit(blob);
								developmentStorageDbDataContext.SubmitChanges();
								transactionScope.Complete();
								break;
							}
						}
					}
					catch (SqlException sqlException)
					{
						if (sqlException.Number != 1205)
						{
							throw;
						}
					}
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobObject.AbortCopyBlob"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> AcquireLeaseImpl(LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			// 
			// Current member / type: System.Collections.Generic.IEnumerator`1<System.IAsyncResult> Microsoft.WindowsAzure.DevelopmentStorage.Store.DbBlobObject::AcquireLeaseImpl(Microsoft.Cis.Services.Nephos.Common.Storage.LeaseType,System.TimeSpan,System.Nullable`1<System.Guid>,Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObjectCondition,AsyncHelper.AsyncIteratorContext`1<AsyncHelper.NoResults>)
			// File path: C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\Microsoft.Azure.DevelopmentStorage.Store.dll
			// 
			// Product version: 2017.3.1005.3
			// Exception in: System.Collections.Generic.IEnumerator<System.IAsyncResult> AcquireLeaseImpl(Microsoft.Cis.Services.Nephos.Common.Storage.LeaseType,System.TimeSpan,System.Nullable<System.Guid>,Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObjectCondition,AsyncHelper.AsyncIteratorContext<AsyncHelper.NoResults>)
			// 
			// The given key was not present in the dictionary.
			//    at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
			//    at ..() in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\GotoElimination\GotoCancelation.cs:line 61
			//    at ..() in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\GotoElimination\GotoCancelation.cs:line 35
			//    at ..(DecompilationContext ,  ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\GotoElimination\GotoCancelation.cs:line 26
			//    at ..(MethodBody ,  , ILanguage ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\DecompilationPipeline.cs:line 88
			//    at ..(MethodBody , ILanguage ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\DecompilationPipeline.cs:line 70
			//    at Telerik.JustDecompiler.Decompiler.Extensions.( , ILanguage , MethodBody , DecompilationContext& ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\Extensions.cs:line 95
			//    at Telerik.JustDecompiler.Decompiler.Extensions.(MethodBody , ILanguage , DecompilationContext& ,  ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\Extensions.cs:line 58
			//    at ..(ILanguage , MethodDefinition ,  ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\WriterContextServices\BaseWriterContextService.cs:line 117
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com

		}

		private IEnumerator<IAsyncResult> AsynchronousCopyBlobImpl(UriString copySource, bool isSourceAzureBlob, FECopyType copyType, long contentLength, string contentType, NameValueCollection serviceMetadataCollection, byte[] applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, string sourceETag, IBlobObjectCondition destinationCondition, AsyncIteratorContext<CopyBlobOperationInfo> context)
		{
			IAsyncResult asyncResult2 = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (TransactionScope transactionScope = new TransactionScope())
				{
					DateTime utcNow = DateTime.UtcNow;
					BlobLeaseInfo blobLeaseInfo = null;
					Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob nullable = null;
					Guid guid = Guid.NewGuid();
					string str = "pending";
					using (DevelopmentStorageDbDataContext developmentStorageDbDataContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						if (applicationMetadata != null && (int)applicationMetadata.Length > 8192)
						{
							throw new ArgumentOutOfRangeException("applicationMetadata", "Metadata length exceeds the specified limit");
						}
						StorageStampHelpers.CheckBlobName(this._blob.BlobName, this._blob.ContainerName);
						this.LoadContainer(developmentStorageDbDataContext);
						Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob1 = this.TryLoadBlob(developmentStorageDbDataContext);
						DbBlobObject.CheckCopyState(blob1);
						if (blob1 == null && overwriteOption == OverwriteOption.UpdateExistingOnly)
						{
							if (destinationCondition == null || !destinationCondition.LeaseId.HasValue)
							{
								throw new ConditionNotMetException("", new bool?(false), null);
							}
							throw new LeaseNotPresentException();
						}
						if (!isSourceAzureBlob || this is DbListBlobObject)
						{
							if (blob1 != null)
							{
								if (blob1.BlobType != BlobType.ListBlob)
								{
									throw new InvalidBlobTypeException();
								}
								if (((BlockBlob)blob1).IsCommitted.Value && overwriteOption == OverwriteOption.CreateNewOnly)
								{
									throw new BlobAlreadyExistsException();
								}
								blobLeaseInfo = new BlobLeaseInfo(blob1, utcNow);
								DbBlobObject.CheckConditionsAndReturnResetRequired(blob1, blobLeaseInfo, destinationCondition, new bool?(false), true);
								developmentStorageDbDataContext.Blobs.DeleteOnSubmit(blob1);
								developmentStorageDbDataContext.SubmitChanges();
							}
							nullable = DbBlobObject.CloneBlobInstanceForAsyncCopyBlob(this._blob.AccountName, this._blob.ContainerName, this._blob.BlobName, blob1 as BlockBlob);
						}
						else
						{
							if (blob1 != null)
							{
								if (blob1.BlobType != BlobType.IndexBlob)
								{
									throw new InvalidBlobTypeException();
								}
								if (overwriteOption == OverwriteOption.CreateNewOnly)
								{
									throw new BlobAlreadyExistsException();
								}
								blobLeaseInfo = new BlobLeaseInfo(blob1, utcNow);
								DbBlobObject.CheckConditionsAndReturnResetRequired(blob1, blobLeaseInfo, destinationCondition, new bool?(false), true);
								developmentStorageDbDataContext.Blobs.DeleteOnSubmit(blob1);
								developmentStorageDbDataContext.SubmitChanges();
							}
							nullable = DbBlobObject.CloneBlobInstanceForAsyncCopyBlob(this._blob.AccountName, this._blob.ContainerName, this._blob.BlobName, blob1 as PageBlob, copyType);
							if (sequenceNumberUpdate != null)
							{
								((PageBlob)nullable).SequenceNumber = new long?(sequenceNumberUpdate.SequenceNumber);
							}
							((PageBlob)nullable).MaxSize = new long?(contentLength);
						}
						this.CheckInfiniteLeaseOrFail(blobLeaseInfo, false);
					}
					using (DevelopmentStorageDbDataContext developmentStorageDbDataContext1 = DevelopmentStorageDbDataContext.GetDbContext())
					{
						nullable.ContentLength = (long)0;
						nullable.GenerationId = Guid.NewGuid().ToString();
						nullable.ContentType = contentType ?? "application/octet-stream";
						nullable.Metadata = applicationMetadata;
						BlobServiceMetaData instance = BlobServiceMetaData.GetInstance(serviceMetadataCollection);
						instance.CopyId = guid.ToString();
						instance.CopySource = copySource.RawString;
						instance.CopyStatus = str;
						instance.CopyProgressOffset = "0";
						instance.CopyProgressTotal = contentLength.ToString();
						nullable.ServiceMetadata = instance.GetMetadata();
						developmentStorageDbDataContext1.Blobs.InsertOnSubmit(nullable);
						developmentStorageDbDataContext1.SubmitChanges();
						developmentStorageDbDataContext1.Refresh(RefreshMode.OverwriteCurrentValues, nullable);
						transactionScope.Complete();
						CopyBlobManager.AcceptAsycCopyJob(copySource.RawString, isSourceAzureBlob, sourceETag, this is DbListBlobObject, contentLength, nullable.AccountName, nullable.ContainerName, nullable.BlobName, nullable.VersionTimestamp, guid.ToString(), nullable.LastModificationTime, () => {
							if (copyType == FECopyType.Incremental && this is DbPageBlobObject)
							{
								IAsyncResult asyncResult1 = ((DbPageBlobObject)this).BeginSnapshotBlob(nullable.Metadata, null, (IAsyncResult asyncResult) => {
									DateTime resultData = ((AsyncIteratorContext<DateTime>)asyncResult).ResultData;
									using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
									{
										PageBlob pageBlob = (PageBlob)this.LoadBlob(dbContext);
										NameValueCollection nameValueCollection = new NameValueCollection();
										MetadataEncoding.Decode(pageBlob.ServiceMetadata, nameValueCollection);
										nameValueCollection[RealServiceManager.LastCopySnapshotTag] = resultData.ToFileTimeUtc().ToString();
										pageBlob.ServiceMetadata = MetadataEncoding.Encode(nameValueCollection);
										Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = dbContext.Blobs.Single<Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob>((Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob b) => (b.AccountName == pageBlob.AccountName) && (b.ContainerName == pageBlob.ContainerName) && (b.BlobName == pageBlob.BlobName) && (b.VersionTimestamp == resultData));
										nameValueCollection = new NameValueCollection();
										MetadataEncoding.Decode(blob.ServiceMetadata, nameValueCollection);
										nameValueCollection[RealServiceManager.LastCopySnapshotTag] = resultData.ToFileTimeUtc().ToString();
										blob.ServiceMetadata = MetadataEncoding.Encode(nameValueCollection);
										dbContext.SubmitChanges();
									}
								}, null, false);
								this.EndSnapshotBlob(asyncResult1);
							}
						});
						blobLeaseInfo = new BlobLeaseInfo(nullable, utcNow);
						this._blob = nullable;
						this.LeaseInfo = blobLeaseInfo;
						this._blobMetadata = instance;
					}
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobObject.AsynchronousCopyBlobImpl"));
			yield return asyncResult2;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult2);
		}

		public IAsyncResult BeginAbortCopyBlob(Guid copyId, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.AbortCopyBlob", callback, state);
			asyncIteratorContext.Begin(this.AbortCopyBlobImpl(copyId, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginAsynchronousCopyBlob(UriString copySource, bool isSourceAzureBlob, FECopyType copyType, long contentLength, string contentType, NameValueCollection serviceMetadataCollection, byte[] applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, string sourceETag, IBlobObjectCondition destinationCondition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<CopyBlobOperationInfo> asyncIteratorContext = new AsyncIteratorContext<CopyBlobOperationInfo>("DbBlobObject.AsynchronousCopyBlobImpl", callback, state);
			asyncIteratorContext.Begin(this.AsynchronousCopyBlobImpl(copySource, isSourceAzureBlob, copyType, contentLength, contentType, serviceMetadataCollection, applicationMetadata, sequenceNumberUpdate, overwriteOption, sourceETag, destinationCondition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public virtual IAsyncResult BeginGetBlob(IBlobRegion blobRegion, BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			this.SetRealBlobObject();
			return this._realBlobObject.BeginGetBlob(blobRegion, propertyNames, condition, callback, state);
		}

		public virtual IAsyncResult BeginPutBlob(string contentType, long contentLength, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, Stream inputStream, CrcReaderStream crcReaderStream, byte[] contentMD5, bool invokeGeneratePutBlobServiceMetadata, GeneratePutBlobServiceMetadata generatePutBlobServiceMetadata, bool isLargeBlockBlobRequest, bool is8TBPageBlobAllowed, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			throw new InvalidOperationException();
		}

		public virtual IAsyncResult BeginSnapshotBlob(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			this.SetRealBlobObject();
			return this._realBlobObject.BeginSnapshotBlob(metadata, condition, callback, state);
		}

		private IEnumerator<IAsyncResult> BreakLeaseImpl(TimeSpan? leaseBreakPeriod, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			// 
			// Current member / type: System.Collections.Generic.IEnumerator`1<System.IAsyncResult> Microsoft.WindowsAzure.DevelopmentStorage.Store.DbBlobObject::BreakLeaseImpl(System.Nullable`1<System.TimeSpan>,Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObjectCondition,AsyncHelper.AsyncIteratorContext`1<AsyncHelper.NoResults>)
			// File path: C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\Microsoft.Azure.DevelopmentStorage.Store.dll
			// 
			// Product version: 2017.3.1005.3
			// Exception in: System.Collections.Generic.IEnumerator<System.IAsyncResult> BreakLeaseImpl(System.Nullable<System.TimeSpan>,Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObjectCondition,AsyncHelper.AsyncIteratorContext<AsyncHelper.NoResults>)
			// 
			// The given key was not present in the dictionary.
			//    at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
			//    at ..() in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\GotoElimination\GotoCancelation.cs:line 61
			//    at ..() in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\GotoElimination\GotoCancelation.cs:line 35
			//    at ..(DecompilationContext ,  ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\GotoElimination\GotoCancelation.cs:line 26
			//    at ..(MethodBody ,  , ILanguage ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\DecompilationPipeline.cs:line 88
			//    at ..(MethodBody , ILanguage ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\DecompilationPipeline.cs:line 70
			//    at Telerik.JustDecompiler.Decompiler.Extensions.( , ILanguage , MethodBody , DecompilationContext& ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\Extensions.cs:line 95
			//    at Telerik.JustDecompiler.Decompiler.Extensions.(MethodBody , ILanguage , DecompilationContext& ,  ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\Extensions.cs:line 58
			//    at ..(ILanguage , MethodDefinition ,  ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\WriterContextServices\BaseWriterContextService.cs:line 117
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com

		}

		private IEnumerator<IAsyncResult> ChangeLeaseImpl(Guid leaseId, Guid proposedLeaseId, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					this.LoadContainer(dbContext);
					Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob nullable = this.LoadBlob(dbContext, true);
					DbBlobObject.CheckCopyState(nullable);
					DateTime utcNow = DateTime.UtcNow;
					BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(nullable, utcNow);
					DbBlobObject.CheckConditionsExceptLease(nullable, condition, null);
					switch (blobLeaseInfo.State.Value)
					{
						case LeaseState.Available:
						{
							throw new LeaseNotPresentException();
						}
						case LeaseState.Leased:
						{
							if (!blobLeaseInfo.Id.HasValue || !(leaseId != blobLeaseInfo.Id.Value) || !(proposedLeaseId != blobLeaseInfo.Id.Value))
							{
								break;
							}
							throw new LeaseHeldException();
						}
						case LeaseState.Expired:
						{
							throw new LeaseNotPresentException();
						}
						case LeaseState.Breaking:
						{
							if (!blobLeaseInfo.Id.HasValue || !(leaseId != blobLeaseInfo.Id.Value) || !(proposedLeaseId != blobLeaseInfo.Id.Value))
							{
								throw new LeaseBrokenException();
							}
							throw new LeaseHeldException();
						}
						case LeaseState.Broken:
						{
							throw new LeaseNotPresentException();
						}
					}
					nullable.LeaseId = new Guid?(proposedLeaseId);
					nullable.IsLeaseOp = true;
					this.SubmitLeaseChanges(dbContext);
					blobLeaseInfo.SetBlob(nullable, utcNow);
					this._blob = nullable;
					this.LeaseInfo = blobLeaseInfo;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobObject.ChangeLease"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		protected static bool CheckConditionsAndReturnResetRequired(Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob, BlobLeaseInfo leaseInfo, IBlobObjectCondition condition, bool? isForSource, bool isWrite)
		{
			bool flag = false;
			if (condition != null)
			{
				if (!condition.IsMultipleConditionalHeaderEnabled)
				{
					DbBlobObject.CheckForSingleConditionalHeader(blob, condition, isForSource);
				}
				else
				{
					DbBlobObject.CheckForMultipleConditionalHeader(blob, condition, isForSource);
				}
			}
			if (leaseInfo != null)
			{
				if (isWrite)
				{
					LeaseState? state = leaseInfo.State;
					LeaseState valueOrDefault = state.GetValueOrDefault();
					if (state.HasValue)
					{
						switch (valueOrDefault)
						{
							case LeaseState.Available:
							{
								if (condition != null && condition.LeaseId.HasValue)
								{
									if (!leaseInfo.Id.HasValue)
									{
										throw new LeaseNotPresentException();
									}
									throw new LeaseLostException();
								}
								flag = true;
								break;
							}
							case LeaseState.Leased:
							{
								if (condition == null || !condition.LeaseId.HasValue)
								{
									throw new LeaseHeldException();
								}
								if (condition != null && !(leaseInfo.Id.Value != condition.LeaseId.Value))
								{
									break;
								}
								throw new LeaseHeldException();
							}
							case LeaseState.Expired:
							case LeaseState.Broken:
							{
								if (condition != null && condition.LeaseId.HasValue)
								{
									throw new LeaseLostException();
								}
								flag = true;
								break;
							}
							case LeaseState.Breaking:
							{
								if (condition != null && condition.LeaseId.HasValue && !(leaseInfo.Id.Value != condition.LeaseId.Value))
								{
									break;
								}
								throw new LeaseHeldException();
							}
						}
					}
				}
				else if (condition != null && condition.LeaseId.HasValue)
				{
					LeaseState? nullable = leaseInfo.State;
					LeaseState leaseState = nullable.GetValueOrDefault();
					if (nullable.HasValue)
					{
						switch (leaseState)
						{
							case LeaseState.Available:
							{
								if (!leaseInfo.Id.HasValue)
								{
									throw new LeaseNotPresentException();
								}
								throw new LeaseLostException();
							}
							case LeaseState.Leased:
							case LeaseState.Breaking:
							{
								if (leaseInfo.Id.Value == condition.LeaseId.Value)
								{
									break;
								}
								throw new LeaseHeldException();
							}
							case LeaseState.Expired:
							case LeaseState.Broken:
							{
								throw new LeaseLostException();
							}
						}
					}
				}
			}
			if (condition != null && condition.SequenceNumber.HasValue)
			{
				long value = condition.SequenceNumber.Value;
				long num = blob.SequenceNumber.Value;
				switch (condition.SequenceNumberOperator.Value)
				{
					case ComparisonOperator.LessThan:
					{
						if (num < value)
						{
							return flag;
						}
						throw new SequenceNumberConditionNotMetException();
					}
					case ComparisonOperator.LessThanOrEqual:
					{
						if (num <= value)
						{
							return flag;
						}
						throw new SequenceNumberConditionNotMetException();
					}
					case ComparisonOperator.Equal:
					{
						if (num == value)
						{
							return flag;
						}
						throw new SequenceNumberConditionNotMetException();
					}
					case ComparisonOperator.NotEqual:
					{
						if (num != value)
						{
							return flag;
						}
						throw new SequenceNumberConditionNotMetException();
					}
					case ComparisonOperator.GreaterThanOrEqual:
					{
						if (num >= value)
						{
							return flag;
						}
						throw new SequenceNumberConditionNotMetException();
					}
					case ComparisonOperator.GreaterThan:
					{
						if (num > value)
						{
							return flag;
						}
						throw new SequenceNumberConditionNotMetException();
					}
				}
				throw new InvalidOperationException();
			}
			return flag;
		}

		protected static void CheckConditionsExceptLease(Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob, IBlobObjectCondition condition, bool? isForSource)
		{
			DbBlobObject.CheckConditionsAndReturnResetRequired(blob, null, condition, isForSource, false);
		}

		internal static void CheckCopyState(Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blobObj)
		{
			if (blobObj != null && "pending".Equals(BlobServiceMetaData.GetInstance(blobObj.ServiceMetadata).CopyStatus, StringComparison.OrdinalIgnoreCase))
			{
				throw new PendingCopyException();
			}
		}

		private static void CheckForMultipleConditionalHeader(Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob, IBlobObjectCondition condition, bool? isForSource)
		{
			Dictionary<Condition, bool> conditions = new Dictionary<Condition, bool>();
			if (condition.RequiredBlobType != BlobType.None && condition.RequiredBlobType != blob.BlobType)
			{
				throw new InvalidBlobTypeException("", null, true);
			}
			if (condition.IfLastModificationTimeMatch != null && (int)condition.IfLastModificationTimeMatch.Length > 0)
			{
				DateTime[] ifLastModificationTimeMatch = condition.IfLastModificationTimeMatch;
				int num = 0;
				while (num < (int)ifLastModificationTimeMatch.Length)
				{
					if (!ifLastModificationTimeMatch[num].Equals(blob.LastModificationTime))
					{
						num++;
					}
					else
					{
						conditions.Add(Condition.IfLastModificationTimeMatch, true);
						break;
					}
				}
				if (!conditions.ContainsKey(Condition.IfLastModificationTimeMatch))
				{
					conditions.Add(Condition.IfLastModificationTimeMatch, false);
				}
			}
			if (condition.IfNotModifiedSinceTime.HasValue)
			{
				DateTime value = blob.LastModificationTime.Value;
				if (condition.IfNotModifiedSinceTime.Value.Second == 0)
				{
					value = new DateTime(value.Ticks / (long)10000000 * (long)10000000);
				}
				if (condition.IfNotModifiedSinceTime.Value >= value)
				{
					conditions.Add(Condition.IfNotModifiedSinceTime, true);
				}
				else
				{
					conditions.Add(Condition.IfNotModifiedSinceTime, false);
				}
			}
			if (condition.IfLastModificationTimeMismatch != null && (int)condition.IfLastModificationTimeMismatch.Length > 0)
			{
				DateTime[] ifLastModificationTimeMismatch = condition.IfLastModificationTimeMismatch;
				int num1 = 0;
				while (num1 < (int)ifLastModificationTimeMismatch.Length)
				{
					if (!ifLastModificationTimeMismatch[num1].Equals(blob.LastModificationTime))
					{
						num1++;
					}
					else
					{
						conditions.Add(Condition.IfLastModificationTimeMismatch, false);
						break;
					}
				}
				if (!conditions.ContainsKey(Condition.IfLastModificationTimeMismatch))
				{
					conditions.Add(Condition.IfLastModificationTimeMismatch, true);
				}
			}
			if (condition.IfModifiedSinceTime.HasValue)
			{
				DateTime dateTime = condition.IfModifiedSinceTime.Value;
				DateTime? lastModificationTime = blob.LastModificationTime;
				if ((lastModificationTime.HasValue ? dateTime < lastModificationTime.GetValueOrDefault() : true))
				{
					conditions.Add(Condition.IfModifiedSinceTime, true);
				}
				else
				{
					conditions.Add(Condition.IfModifiedSinceTime, false);
				}
			}
			DbBlobObject.EvaluateConditionsAndThrowException(conditions, isForSource);
		}

		private static void CheckForSingleConditionalHeader(Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob, IBlobObjectCondition condition, bool? isForSource)
		{
			if (condition.RequiredBlobType != BlobType.None && condition.RequiredBlobType != blob.BlobType)
			{
				throw new InvalidBlobTypeException("", null, true);
			}
			if (condition.IfLastModificationTimeMatch != null && (int)condition.IfLastModificationTimeMatch.Length > 0)
			{
				DateTime ifLastModificationTimeMatch = condition.IfLastModificationTimeMatch[0];
				DateTime? lastModificationTime = blob.LastModificationTime;
				if ((!lastModificationTime.HasValue ? true : ifLastModificationTimeMatch != lastModificationTime.GetValueOrDefault()))
				{
					throw new ConditionNotMetException(null, isForSource, null);
				}
			}
			if (condition.IfLastModificationTimeMismatch != null && (int)condition.IfLastModificationTimeMismatch.Length > 0)
			{
				DateTime ifLastModificationTimeMismatch = condition.IfLastModificationTimeMismatch[0];
				DateTime? nullable = blob.LastModificationTime;
				if ((!nullable.HasValue ? false : ifLastModificationTimeMismatch == nullable.GetValueOrDefault()))
				{
					throw new ConditionNotMetException(null, isForSource, null);
				}
			}
			if (condition.IfModifiedSinceTime.HasValue)
			{
				DateTime value = condition.IfModifiedSinceTime.Value;
				DateTime? lastModificationTime1 = blob.LastModificationTime;
				if ((lastModificationTime1.HasValue ? value >= lastModificationTime1.GetValueOrDefault() : false))
				{
					throw new ConditionNotMetException(null, isForSource, null);
				}
			}
			if (condition.IfNotModifiedSinceTime.HasValue)
			{
				DateTime dateTime = blob.LastModificationTime.Value;
				if (condition.IfNotModifiedSinceTime.Value.Second == 0)
				{
					dateTime = new DateTime(dateTime.Ticks / (long)10000000 * (long)10000000);
				}
				if (condition.IfNotModifiedSinceTime.Value < dateTime)
				{
					throw new ConditionNotMetException(null, isForSource, null);
				}
			}
		}

		private void CheckInfiniteLeaseOrFail(BlobLeaseInfo leaseInfo, bool shouldCheckFeb12Version)
		{
			if ((!shouldCheckFeb12Version || this.IsRequestAtleastFeb12()) && leaseInfo != null && leaseInfo.State.HasValue && (leaseInfo.State.Value == LeaseState.Leased || leaseInfo.State.Value == LeaseState.Breaking) && !DbBlobContainer.IsInfiniteLease(leaseInfo.Duration.Value, false))
			{
				throw new LeaseDurationNotInfiniteException();
			}
		}

		internal static Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob ClearBlob(Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob, byte[] metadata)
		{
			Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob1 = null;
			if (!(blob is BlockBlob))
			{
				PageBlob pageBlob = new PageBlob()
				{
					AccountName = blob.AccountName,
					ContainerName = blob.ContainerName,
					BlobName = blob.BlobName,
					VersionTimestamp = blob.VersionTimestamp,
					ContentLength = (long)0,
					Metadata = blob.Metadata,
					SequenceNumber = blob.SequenceNumber,
					ServiceMetadata = metadata,
					FileName = Guid.NewGuid().ToString(),
					MaxSize = (blob as PageBlob).MaxSize
				};
				blob1 = pageBlob;
			}
			else
			{
				BlockBlob blockBlob = new BlockBlob()
				{
					AccountName = blob.AccountName,
					ContainerName = blob.ContainerName,
					BlobName = blob.BlobName,
					VersionTimestamp = blob.VersionTimestamp,
					DirectoryPath = BlockBlobDataManager.CreateUniqueDirectoryLoadBalanced(),
					HasBlock = new bool?(false),
					ContentLength = (long)0,
					Metadata = blob.Metadata,
					ServiceMetadata = metadata,
					UncommittedBlockIdLength = null,
					IsCommitted = new bool?(true)
				};
				blob1 = blockBlob;
			}
			DbBlobObject.CopyCommonProperties(blob, blob1);
			return blob1;
		}

		internal static PageBlob CloneBlobInstanceForAsyncCopyBlob(string accountName, string containerName, string blobName, PageBlob blob, FECopyType copyType)
		{
			PageBlob pageBlob = new PageBlob()
			{
				AccountName = accountName,
				ContainerName = containerName,
				BlobName = blobName,
				VersionTimestamp = (blob == null ? DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc) : blob.VersionTimestamp),
				SequenceNumber = new long?((long)0),
				FileName = Guid.NewGuid().ToString(),
				IsIncrementalCopy = new bool?(copyType == FECopyType.Incremental),
				SnapshotCount = (copyType != FECopyType.Incremental || blob == null ? 0 : blob.SnapshotCount)
			};
			PageBlob pageBlob1 = pageBlob;
			DbBlobObject.CopyCommonProperties(blob, pageBlob1);
			return pageBlob1;
		}

		internal static BlockBlob CloneBlobInstanceForAsyncCopyBlob(string accountName, string containerName, string blobName, BlockBlob blob)
		{
			BlockBlob blockBlob = new BlockBlob()
			{
				AccountName = accountName,
				ContainerName = containerName,
				BlobName = blobName,
				VersionTimestamp = (blob == null ? DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc) : blob.VersionTimestamp),
				DirectoryPath = BlockBlobDataManager.CreateUniqueDirectoryLoadBalanced(),
				HasBlock = new bool?(false),
				IsCommitted = new bool?(true),
				UncommittedBlockIdLength = null,
				SnapshotCount = 0
			};
			BlockBlob blockBlob1 = blockBlob;
			DbBlobObject.CopyCommonProperties(blob, blockBlob1);
			return blockBlob1;
		}

		private string CopyBlockBlobDataIfRequired(BlockBlob src, BlockBlob dest)
		{
			if (string.Compare(src.AccountName, dest.AccountName, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(src.ContainerName, dest.ContainerName, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(src.BlobName, dest.BlobName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return null;
			}
			BlockBlobMetaInfo blockBlobMetaInfo = new BlockBlobMetaInfo(src.AccountName, src.ContainerName, src.BlobName, src.DirectoryPath);
			Logger<INormalAndDebugLogger>.Instance.InfoDebug.Log("BlockBlob: CopyBlockBlobDataIfRequired Copying blob data");
			return BlockBlobDataManager.MakeBlobDataCopy(blockBlobMetaInfo);
		}

		internal static void CopyCommonProperties(Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob srcBlob, Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob retBlob)
		{
			if (srcBlob != null)
			{
				retBlob.LeaseDuration = srcBlob.LeaseDuration;
				retBlob.LeaseEndTime = srcBlob.LeaseEndTime;
				retBlob.LeaseId = srcBlob.LeaseId;
				retBlob.LeaseState = srcBlob.LeaseState;
				retBlob.GenerationId = srcBlob.GenerationId;
			}
		}

		public void Dispose()
		{
		}

		public void EndAbortCopyBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public CopyBlobOperationInfo EndAsynchronousCopyBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<CopyBlobOperationInfo> copyBlobOperationInfo = (AsyncIteratorContext<CopyBlobOperationInfo>)asyncResult;
			copyBlobOperationInfo.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			copyBlobOperationInfo.ResultData = this.GetCopyBlobOperationInfo();
			return copyBlobOperationInfo.ResultData;
		}

		public virtual CrcStream EndGetBlob(IAsyncResult asyncResult)
		{
			CrcStream crcStream = this._realBlobObject.EndGetBlob(asyncResult);
			this._blob = ((DbBlobObject)this._realBlobObject)._blob;
			this.LeaseInfo = this._realBlobObject.LeaseInfo;
			return crcStream;
		}

		public virtual void EndPutBlob(IAsyncResult asyncResult)
		{
			throw new InvalidOperationException();
		}

		public virtual DateTime EndSnapshotBlob(IAsyncResult ar)
		{
			DateTime dateTime = this._realBlobObject.EndSnapshotBlob(ar);
			this._blob = ((DbBlobObject)this._realBlobObject)._blob;
			this.LeaseInfo = this._realBlobObject.LeaseInfo;
			return dateTime;
		}

		private static void EvaluateConditionsAndThrowException(Dictionary<Condition, bool> expressionList, bool? isForSource)
		{
			bool item = true;
			HttpStatusCode? nullable = null;
			if (expressionList.ContainsKey(Condition.IfLastModificationTimeMatch))
			{
				item = expressionList[Condition.IfLastModificationTimeMatch];
			}
			if (expressionList.ContainsKey(Condition.IfNotModifiedSinceTime))
			{
				item = (!item ? false : expressionList[Condition.IfNotModifiedSinceTime]);
			}
			if (!item)
			{
				throw new ConditionNotMetException(null, isForSource, null, nullable);
			}
			if (expressionList.ContainsKey(Condition.IfLastModificationTimeMismatch))
			{
				bool flag = expressionList[Condition.IfLastModificationTimeMismatch];
				if (expressionList.ContainsKey(Condition.IfModifiedSinceTime))
				{
					flag = (flag ? true : expressionList[Condition.IfModifiedSinceTime]);
				}
				if (!flag)
				{
					nullable = new HttpStatusCode?(HttpStatusCode.NotModified);
				}
				item = (!item ? false : flag);
			}
			if (expressionList.ContainsKey(Condition.IfModifiedSinceTime))
			{
				bool item1 = expressionList[Condition.IfModifiedSinceTime];
				if (expressionList.ContainsKey(Condition.IfLastModificationTimeMismatch))
				{
					item1 = (item1 ? true : expressionList[Condition.IfLastModificationTimeMismatch]);
				}
				if (!item1)
				{
					nullable = new HttpStatusCode?(HttpStatusCode.NotModified);
				}
				item = (!item ? false : item1);
			}
			if (!item)
			{
				throw new ConditionNotMetException(null, isForSource, null, nullable);
			}
		}

		private CopyBlobOperationInfo GetCopyBlobOperationInfo()
		{
			if (this._blobMetadata == null)
			{
				return null;
			}
			CopyBlobOperationInfo copyBlobOperationInfo = new CopyBlobOperationInfo()
			{
				CopyId = new Guid(this._blobMetadata.CopyId),
				CopyStatus = this._blobMetadata.CopyStatus
			};
			return copyBlobOperationInfo;
		}

		private IEnumerator<IAsyncResult> GetPropertiesImpl(BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					this.LoadContainer(dbContext);
					Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = this.LoadBlob(dbContext);
					BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(blob, DateTime.UtcNow);
					DbBlobObject.CheckConditionsAndReturnResetRequired(blob, blobLeaseInfo, condition, null, false);
					this._blob = blob;
					this.LeaseInfo = blobLeaseInfo;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobObject.GetProperties"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		protected bool IsRequestAtleastFeb12()
		{
			return this._blobServiceVersion >= BlobServiceVersion.Feb12;
		}

		private Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob LoadBlob(DevelopmentStorageDbDataContext context)
		{
			return this.LoadBlob(context, false);
		}

		private Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob LoadBlob(DevelopmentStorageDbDataContext context, bool allowUncommittedBlob)
		{
			Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = this.TryLoadBlob(context);
			if (blob == null)
			{
				throw new BlobNotFoundException();
			}
			BlockBlob blockBlob = blob as BlockBlob;
			if (blockBlob != null && !allowUncommittedBlob && !blockBlob.IsCommitted.Value)
			{
				throw new BlobNotFoundException();
			}
			return blob;
		}

		protected BlockBlob LoadBlockBlob(DevelopmentStorageDbDataContext context)
		{
			return this.LoadBlockBlob(context, false);
		}

		protected BlockBlob LoadBlockBlob(DevelopmentStorageDbDataContext context, bool allowUncommittedBlob)
		{
			string str;
			BlockBlob blockBlob = this.TryLoadBlockBlob(context, out str);
			if (blockBlob == null)
			{
				throw new BlobNotFoundException();
			}
			if (!allowUncommittedBlob && !blockBlob.IsCommitted.Value)
			{
				throw new BlobNotFoundException();
			}
			return blockBlob;
		}

		protected Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer LoadContainer(DevelopmentStorageDbDataContext context)
		{
			Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer blobContainer = (
				from c in context.BlobContainers
				where (c.AccountName == this._blob.AccountName) && (c.ContainerName == this._blob.ContainerName)
				select c).FirstOrDefault<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer>();
			if (blobContainer == null)
			{
				throw new ContainerNotFoundException();
			}
			return blobContainer;
		}

		protected PageBlob LoadPageBlob(DevelopmentStorageDbDataContext context)
		{
			PageBlob pageBlob = this.TryLoadPageBlob(context);
			if (pageBlob == null)
			{
				throw new BlobNotFoundException();
			}
			return pageBlob;
		}

		private static Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob MakeBlob(BlobType blobType)
		{
			switch (blobType)
			{
				case BlobType.None:
				{
					return new BaseBlob();
				}
				case BlobType.ListBlob:
				{
					return new BlockBlob();
				}
				case BlobType.IndexBlob:
				{
					return new PageBlob();
				}
				case BlobType.AppendBlob:
				{
					throw new FeatureNotSupportedByEmulatorException("Append Blob");
				}
			}
			throw new ArgumentOutOfRangeException("blobType", (object)blobType, "Unknown blob type");
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginAcquireLease(LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.AcquireLease", callback, state);
			asyncIteratorContext.Begin(this.AcquireLeaseImpl(leaseType, leaseDuration, proposedLeaseId, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginBreakLease(TimeSpan? leaseBreakPeriod, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.BreakLease", callback, state);
			asyncIteratorContext.Begin(this.BreakLeaseImpl(leaseBreakPeriod, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginChangeLease(Guid leaseId, Guid proposedLeaseId, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.ChangeLease", callback, state);
			asyncIteratorContext.Begin(this.ChangeLeaseImpl(leaseId, proposedLeaseId, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginGetProperties(BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginReleaseLease(Guid leaseId, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.ReleaseLease", callback, state);
			asyncIteratorContext.Begin(this.ReleaseLeaseImpl(leaseId, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginRenewLease(LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.RenewLease", callback, state);
			asyncIteratorContext.Begin(this.RenewLeaseImpl(leaseType, leaseId, leaseDuration, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginSetApplicationMetadata(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.SetApplicationMetadata", callback, state);
			asyncIteratorContext.Begin(this.SetApplicationMetadataImpl(metadata, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginSetExpiryTime(DateTime expiryTime, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.SetExpiryTime", callback, state);
			asyncIteratorContext.Begin(this.SetExpiryTimeImpl(expiryTime, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginSetProperties(string contentType, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.SetProperties", callback, state);
			asyncIteratorContext.Begin(this.SetPropertiesImpl(contentType, maxBlobSize, serviceMetadata, applicationMetadata, sequenceNumberUpdate, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginSetProperties(string contentType, long? maxBlobSize, NameValueCollection serviceMetadata, ServiceMetadataUpdatePolicy serviceMetadataPolicy, string[] serviceMetadataPolicyArguments, NameValueCollection applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.SetProperties", callback, state);
			asyncIteratorContext.Begin(this.SetPropertiesImpl(contentType, maxBlobSize, serviceMetadata, serviceMetadataPolicy, serviceMetadataPolicyArguments, applicationMetadata, sequenceNumberUpdate, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginSetServiceMetadata(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobObject.SetServiceMetadata", callback, state);
			asyncIteratorContext.Begin(this.SetServiceMetadataImpl(metadata, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.BeginSynchronousCopyBlob(string sourceAccount, IBlobObject sourceBlob, DateTime? expiryTime, byte[] applicationMetadata, OverwriteOption overwriteOption, IBlobObjectCondition sourceCondition, IBlobObjectCondition destinationCondition, UriString copySource, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<CopyBlobOperationInfo> asyncIteratorContext = new AsyncIteratorContext<CopyBlobOperationInfo>("DbBlobObject.SynchronousCopyBlob", callback, state);
			asyncIteratorContext.Begin(this.SynchronousCopyBlobImpl(sourceAccount, sourceBlob, expiryTime, applicationMetadata, overwriteOption, sourceCondition, destinationCondition, copySource, asyncIteratorContext));
			return asyncIteratorContext;
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndAcquireLease(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndBreakLease(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndChangeLease(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndGetProperties(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndReleaseLease(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndRenewLease(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndSetApplicationMetadata(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndSetExpiryTime(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndSetProperties(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndSetServiceMetadata(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		CopyBlobOperationInfo Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.EndSynchronousCopyBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<CopyBlobOperationInfo> copyBlobOperationInfo = (AsyncIteratorContext<CopyBlobOperationInfo>)asyncResult;
			copyBlobOperationInfo.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			copyBlobOperationInfo.ResultData = this.GetCopyBlobOperationInfo();
			return copyBlobOperationInfo.ResultData;
		}

		private IEnumerator<IAsyncResult> ReleaseLeaseImpl(Guid leaseId, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					this.LoadContainer(dbContext);
					Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob nullable = this.LoadBlob(dbContext, true);
					DbBlobObject.CheckCopyState(nullable);
					DateTime utcNow = DateTime.UtcNow;
					BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(nullable, utcNow);
					DbBlobObject.CheckConditionsExceptLease(nullable, condition, null);
					if (blobLeaseInfo.Id.HasValue && blobLeaseInfo.Id.Value != leaseId)
					{
						throw new LeaseLostException();
					}
					if (blobLeaseInfo.State.Value == LeaseState.Available)
					{
						throw new LeaseLostException();
					}
					nullable.LeaseEndTime = new DateTime?(utcNow);
					nullable.LeaseState = 0;
					nullable.IsLeaseOp = true;
					this.SubmitLeaseChanges(dbContext);
					blobLeaseInfo.SetBlob(nullable, utcNow);
					this._blob = nullable;
					this.LeaseInfo = blobLeaseInfo;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobObject.ReleaseLease"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> RenewLeaseImpl(LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			// 
			// Current member / type: System.Collections.Generic.IEnumerator`1<System.IAsyncResult> Microsoft.WindowsAzure.DevelopmentStorage.Store.DbBlobObject::RenewLeaseImpl(Microsoft.Cis.Services.Nephos.Common.Storage.LeaseType,System.Guid,System.TimeSpan,Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObjectCondition,AsyncHelper.AsyncIteratorContext`1<AsyncHelper.NoResults>)
			// File path: C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\Microsoft.Azure.DevelopmentStorage.Store.dll
			// 
			// Product version: 2017.3.1005.3
			// Exception in: System.Collections.Generic.IEnumerator<System.IAsyncResult> RenewLeaseImpl(Microsoft.Cis.Services.Nephos.Common.Storage.LeaseType,System.Guid,System.TimeSpan,Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObjectCondition,AsyncHelper.AsyncIteratorContext<AsyncHelper.NoResults>)
			// 
			// The given key was not present in the dictionary.
			//    at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
			//    at ..() in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\GotoElimination\GotoCancelation.cs:line 61
			//    at ..() in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\GotoElimination\GotoCancelation.cs:line 35
			//    at ..(DecompilationContext ,  ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\GotoElimination\GotoCancelation.cs:line 26
			//    at ..(MethodBody ,  , ILanguage ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\DecompilationPipeline.cs:line 88
			//    at ..(MethodBody , ILanguage ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\DecompilationPipeline.cs:line 70
			//    at Telerik.JustDecompiler.Decompiler.Extensions.( , ILanguage , MethodBody , DecompilationContext& ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\Extensions.cs:line 95
			//    at Telerik.JustDecompiler.Decompiler.Extensions.(MethodBody , ILanguage , DecompilationContext& ,  ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\Extensions.cs:line 58
			//    at ..(ILanguage , MethodDefinition ,  ) in C:\Builds\556\Behemoth\ReleaseBranch Production Build NT\Sources\OpenSource\Cecil.Decompiler\Decompiler\WriterContextServices\BaseWriterContextService.cs:line 117
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com

		}

		protected void ResetBlobLeaseToAvailable(Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob, bool shouldReset)
		{
			if (shouldReset)
			{
				blob.LeaseDuration = TimeSpan.Zero;
				blob.LeaseEndTime = null;
				blob.LeaseId = null;
				blob.LeaseState = 0;
			}
		}

		private static DateTime RoundOffDateTimeToMillis(DateTime value)
		{
			value = new DateTime(value.Ticks / (long)10000 * (long)10000, DateTimeKind.Utc);
			if (value >= SqlDateTime.MaxValue.Value)
			{
				value = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
			}
			return value;
		}

		private IEnumerator<IAsyncResult> SetApplicationMetadataImpl(byte[] metadata, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					this.LoadContainer(dbContext);
					Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = this.LoadBlob(dbContext);
					BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(blob, DateTime.UtcNow);
					DbBlobObject.CheckCopyState(blob);
					bool flag = DbBlobObject.CheckConditionsAndReturnResetRequired(blob, blobLeaseInfo, condition, null, true);
					StorageStampHelpers.ValidateApplicationMetadata(metadata);
					blob.Metadata = metadata;
					this.ResetBlobLeaseToAvailable(blob, flag);
					dbContext.SubmitChanges();
					blobLeaseInfo.SetBlob(blob, blobLeaseInfo.LeaseInfoValidAt);
					this._blob = blob;
					this.LeaseInfo = blobLeaseInfo;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobObject.SetApplicationMetadata"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		internal void SetBlobNoneType()
		{
			this._blob.BlobType = BlobType.None;
		}

		private IEnumerator<IAsyncResult> SetExpiryTimeImpl(DateTime expiryTime, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					this.LoadContainer(dbContext);
					Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = this.LoadBlob(dbContext, condition.IsIncludingUncommittedBlobs);
					BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(blob, DateTime.UtcNow);
					DbBlobObject.CheckConditionsAndReturnResetRequired(blob, blobLeaseInfo, condition, null, false);
					if (blob.VersionTimestamp >= SqlDateTime.MaxValue.Value)
					{
						if (!condition.IsDeletingOnlySnapshots)
						{
							DbBlobObject.CheckCopyState(blob);
							DbBlobObject.CheckConditionsAndReturnResetRequired(blob, blobLeaseInfo, condition, null, true);
						}
						dbContext.DeleteSnapshots(blob.AccountName, blob.ContainerName, blob.BlobName, new bool?(condition.IsDeletingOnlySnapshots), new bool?(condition.IsRequiringNoSnapshots));
					}
					else
					{
						dbContext.Blobs.DeleteOnSubmit(blob);
					}
					dbContext.SubmitChanges();
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobObject.SetExpiryTime"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		protected static void SetMaxBlobSize(Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob, long? maxBlobSize)
		{
			if (maxBlobSize.HasValue)
			{
				if (blob.BlobType != BlobType.IndexBlob)
				{
					throw new InvalidBlobTypeException();
				}
				((PageBlob)blob).MaxSize = maxBlobSize;
			}
		}

		private IEnumerator<IAsyncResult> SetPropertiesImpl(string contentType, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						this.LoadContainer(dbContext);
						Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = this.LoadBlob(dbContext);
						BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(blob, DateTime.UtcNow);
						DbBlobObject.CheckCopyState(blob);
						bool flag = DbBlobObject.CheckConditionsAndReturnResetRequired(blob, blobLeaseInfo, condition, null, true);
						if (contentType != null)
						{
							blob.ContentType = contentType;
						}
						if (blob.BlobType == BlobType.IndexBlob && maxBlobSize.HasValue && ((PageBlob)blob).MaxSize.Value > maxBlobSize.Value)
						{
							DbPageBlobObject.ClearPageHelper(dbContext, (PageBlob)blob, maxBlobSize.Value, ((PageBlob)blob).MaxSize.Value);
						}
						DbBlobObject.SetMaxBlobSize(blob, maxBlobSize);
						DbBlobObject.SetSequenceNumber(blob, sequenceNumberUpdate);
						if (serviceMetadata != null)
						{
							blob.ServiceMetadata = serviceMetadata;
						}
						StorageStampHelpers.ValidateApplicationMetadata(applicationMetadata);
						if (applicationMetadata != null)
						{
							blob.Metadata = applicationMetadata;
						}
						this.ResetBlobLeaseToAvailable(blob, flag);
						dbContext.SubmitChanges();
						dbContext.Refresh(RefreshMode.OverwriteCurrentValues, blob);
						transactionScope.Complete();
						blobLeaseInfo.SetBlob(blob, blobLeaseInfo.LeaseInfoValidAt);
						this._blob = blob;
						this.LeaseInfo = blobLeaseInfo;
					}
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobObject.SetProperties"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> SetPropertiesImpl(string contentType, long? maxBlobSize, NameValueCollection serviceMetadata, ServiceMetadataUpdatePolicy serviceMetadataPolicy, string[] serviceMetadataPolicyArguments, NameValueCollection applicationMetadata, ISequenceNumberUpdate sequenceNumberUpdate, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			throw new NotImplementedException("Set Properties behavior with Keep and Remove isn't implemented yet");
		}

		private void SetRealBlobObject()
		{
			IBlobObject dbPageBlobObject;
			if (this._blob.BlobType == BlobType.None)
			{
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					this.LoadContainer(dbContext);
					this._blob = this.LoadBlob(dbContext);
				}
			}
			if (this._blob.BlobType == BlobType.IndexBlob)
			{
				dbPageBlobObject = new DbPageBlobObject(this._storageManager, this._blob, this._blobServiceVersion);
			}
			else
			{
				dbPageBlobObject = new DbListBlobObject(this._storageManager, this._blob, this._blobServiceVersion);
			}
			this._realBlobObject = dbPageBlobObject;
			this._realBlobObject.Timeout = this.Timeout;
		}

		protected static void SetSequenceNumber(Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob, ISequenceNumberUpdate sequenceNumberUpdate)
		{
			long? nullable;
			if (sequenceNumberUpdate != null)
			{
				if (blob.BlobType != BlobType.IndexBlob)
				{
					throw new InvalidBlobTypeException();
				}
				switch (sequenceNumberUpdate.UpdateType)
				{
					case SequenceNumberUpdateType.Update:
					{
						((PageBlob)blob).SequenceNumber = new long?(sequenceNumberUpdate.SequenceNumber);
						return;
					}
					case SequenceNumberUpdateType.Max:
					{
						PageBlob pageBlob = (PageBlob)blob;
						long? sequenceNumber = ((PageBlob)blob).SequenceNumber;
						pageBlob.SequenceNumber = new long?(Math.Max(sequenceNumber.Value, sequenceNumberUpdate.SequenceNumber));
						break;
					}
					case SequenceNumberUpdateType.Increment:
					{
						PageBlob pageBlob1 = (PageBlob)blob;
						long? sequenceNumber1 = pageBlob1.SequenceNumber;
						long num = sequenceNumberUpdate.SequenceNumber;
						if (sequenceNumber1.HasValue)
						{
							nullable = new long?(sequenceNumber1.GetValueOrDefault() + num);
						}
						else
						{
							nullable = null;
						}
						pageBlob1.SequenceNumber = nullable;
						return;
					}
					default:
					{
						return;
					}
				}
			}
		}

		private IEnumerator<IAsyncResult> SetServiceMetadataImpl(byte[] metadata, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					this.LoadContainer(dbContext);
					Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = this.LoadBlob(dbContext);
					bool flag = false;
					BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(blob, DateTime.UtcNow);
					DbBlobObject.CheckCopyState(blob);
					flag = DbBlobObject.CheckConditionsAndReturnResetRequired(blob, blobLeaseInfo, condition, null, true);
					this.ResetBlobLeaseToAvailable(blob, flag);
					blob.ServiceMetadata = metadata;
					dbContext.SubmitChanges();
					blobLeaseInfo.SetBlob(blob, blobLeaseInfo.LeaseInfoValidAt);
					this._blob = blob;
					this.LeaseInfo = blobLeaseInfo;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobObject.SetServiceMetadata"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private void SubmitLeaseChanges(DevelopmentStorageDbDataContext context)
		{
			try
			{
				context.SubmitChanges();
			}
			catch (ChangeConflictException changeConflictException)
			{
				throw new ServerBusyException(true);
			}
		}

		private IEnumerator<IAsyncResult> SynchronousCopyBlobImpl(string sourceAccount, IBlobObject sourceBlob2, DateTime? expiryTime, byte[] applicationMetadata, OverwriteOption overwriteOption, IBlobObjectCondition sourceCondition, IBlobObjectCondition destinationCondition, UriString copySource, AsyncIteratorContext<CopyBlobOperationInfo> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						if (applicationMetadata != null && (int)applicationMetadata.Length > 8192)
						{
							throw new ArgumentOutOfRangeException("applicationMetadata", "Metadata length exceeds the specified limit");
						}
						string str = null;
						string directoryPath = null;
						DateTime utcNow = DateTime.UtcNow;
						BlobLeaseInfo blobLeaseInfo = null;
						bool flag = false;
						long value = (long)0;
						StorageStampHelpers.CheckBlobName(this._blob.BlobName, this._blob.ContainerName);
						this.LoadContainer(dbContext);
						Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = (
							from b in dbContext.Blobs
							where (b.AccountName == sourceAccount) && (b.ContainerName == sourceBlob2.ContainerName) && (b.BlobName == sourceBlob2.Name) && (b.VersionTimestamp == sourceBlob2.Snapshot)
							select b).FirstOrDefault<Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob>();
						if (blob == null)
						{
							throw new BlobNotFoundException();
						}
						if (blob is PageBlob)
						{
							bool? isIncrementalCopy = ((PageBlob)blob).IsIncrementalCopy;
							if ((!isIncrementalCopy.GetValueOrDefault() ? false : isIncrementalCopy.HasValue))
							{
								throw new CopySourceCannotBeIncrementalCopyBlobException();
							}
						}
						if (sourceBlob2.ContainerName == this._blob.ContainerName && sourceBlob2.Name == this._blob.BlobName && overwriteOption == OverwriteOption.CreateNewOnly)
						{
							throw new BlobAlreadyExistsException();
						}
						DbBlobObject.CheckConditionsAndReturnResetRequired(blob, new BlobLeaseInfo(blob, utcNow), sourceCondition, new bool?(true), false);
						Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob pageBlob = this.TryLoadBlob(dbContext);
						if (pageBlob == null)
						{
							if (overwriteOption == OverwriteOption.UpdateExistingOnly)
							{
								if (destinationCondition == null || !destinationCondition.LeaseId.HasValue)
								{
									throw new ConditionNotMetException("", new bool?(false), null);
								}
								throw new LeaseNotPresentException();
							}
						}
						else if (blob.BlobType != pageBlob.BlobType)
						{
							throw new InvalidBlobTypeException("Source Blob type is different than the blob destination blob type");
						}
						if (!(blob is BlockBlob))
						{
							if (pageBlob != null)
							{
								if (overwriteOption == OverwriteOption.CreateNewOnly)
								{
									throw new BlobAlreadyExistsException();
								}
								blobLeaseInfo = new BlobLeaseInfo(pageBlob, utcNow);
								DbBlobObject.CheckCopyState(pageBlob);
								flag = DbBlobObject.CheckConditionsAndReturnResetRequired(pageBlob, blobLeaseInfo, destinationCondition, new bool?(false), true);
							}
							else
							{
								pageBlob = new PageBlob()
								{
									AccountName = this._blob.AccountName,
									ContainerName = this._blob.ContainerName,
									BlobName = this._blob.BlobName,
									VersionTimestamp = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc),
									SequenceNumber = new long?((long)0),
									FileName = Guid.NewGuid().ToString()
								};
								dbContext.Blobs.InsertOnSubmit(pageBlob);
							}
							((PageBlob)pageBlob).MaxSize = ((PageBlob)blob).MaxSize;
							if (((PageBlob)blob).FileName != ((PageBlob)pageBlob).FileName)
							{
								File.Copy(DbPageBlobObject.GetFilePath(((PageBlob)blob).FileName), DbPageBlobObject.GetFilePath(((PageBlob)pageBlob).FileName), true);
							}
							value = ((PageBlob)pageBlob).MaxSize.Value;
						}
						else
						{
							if (pageBlob != null)
							{
								if (((BlockBlob)pageBlob).IsCommitted.Value && overwriteOption == OverwriteOption.CreateNewOnly)
								{
									throw new BlobAlreadyExistsException();
								}
								blobLeaseInfo = new BlobLeaseInfo(pageBlob, utcNow);
								DbBlobObject.CheckCopyState(pageBlob);
								flag = DbBlobObject.CheckConditionsAndReturnResetRequired(pageBlob, blobLeaseInfo, destinationCondition, new bool?(false), true);
							}
							else
							{
								pageBlob = new BlockBlob()
								{
									AccountName = this._blob.AccountName,
									ContainerName = this._blob.ContainerName,
									BlobName = this._blob.BlobName,
									VersionTimestamp = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc),
									IsCommitted = ((BlockBlob)blob).IsCommitted
								};
								dbContext.Blobs.InsertOnSubmit(pageBlob);
							}
							((BlockBlob)pageBlob).HasBlock = ((BlockBlob)blob).HasBlock;
							str = this.CopyBlockBlobDataIfRequired((BlockBlob)blob, (BlockBlob)pageBlob);
							directoryPath = ((BlockBlob)blob).DirectoryPath;
							value = blob.ContentLength;
						}
						this.CheckInfiniteLeaseOrFail(blobLeaseInfo, true);
						pageBlob.ContentLength = blob.ContentLength;
						pageBlob.ContentType = blob.ContentType;
						pageBlob.ContentMD5 = blob.ContentMD5;
						pageBlob.GenerationId = Guid.NewGuid().ToString();
						pageBlob.SnapshotCount = 0;
						BlobServiceMetaData rawString = (blob.ServiceMetadata == null ? BlobServiceMetaData.GetInstance() : BlobServiceMetaData.GetInstance(blob.ServiceMetadata));
						if (!this.IsRequestAtleastFeb12())
						{
							rawString.CopyId = null;
							rawString.CopySource = null;
							rawString.CopyStatus = null;
							rawString.CopyProgressOffset = null;
							rawString.CopyProgressTotal = null;
							rawString.CopyCompletionTime = null;
						}
						else
						{
							rawString.CopyId = Guid.NewGuid().ToString();
							rawString.CopySource = copySource.RawString;
							rawString.CopyStatus = "success";
							rawString.CopyProgressOffset = value.ToString();
							rawString.CopyProgressTotal = value.ToString();
							rawString.CopyCompletionTime = new long?(DateTime.UtcNow.ToFileTimeUtc());
						}
						pageBlob.Metadata = (applicationMetadata != null ? applicationMetadata : blob.Metadata);
						pageBlob.SequenceNumber = blob.SequenceNumber;
						pageBlob.ServiceMetadata = rawString.GetMetadata();
						this.ResetBlobLeaseToAvailable(pageBlob, flag);
						dbContext.SubmitChanges();
						if (!(blob is BlockBlob))
						{
							dbContext.CopyPageBlob(sourceAccount, this._blob.AccountName, blob.ContainerName, blob.BlobName, new DateTime?(blob.VersionTimestamp), this._blob.ContainerName, this._blob.BlobName);
						}
						else
						{
							dbContext.CopyBlockBlob(sourceAccount, this._blob.AccountName, blob.ContainerName, blob.BlobName, new DateTime?(blob.VersionTimestamp), this._blob.ContainerName, this._blob.BlobName, directoryPath, str);
						}
						dbContext.Refresh(RefreshMode.OverwriteCurrentValues, pageBlob);
						transactionScope.Complete();
						blobLeaseInfo = new BlobLeaseInfo(pageBlob, utcNow);
						this._blob = pageBlob;
						this.LeaseInfo = blobLeaseInfo;
						if (this.IsRequestAtleastFeb12())
						{
							this._blobMetadata = rawString;
						}
					}
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobObject.CopyBlob"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob TryLoadBlob(DevelopmentStorageDbDataContext context)
		{
			StorageStampHelpers.CheckAccountName(this._blob.AccountName);
			StorageStampHelpers.CheckBlobName(this._blob.BlobName, this._blob.ContainerName);
			Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = (
				from b in context.Blobs
				where (b.AccountName == this._blob.AccountName) && (b.ContainerName == this._blob.ContainerName) && (b.BlobName == this._blob.BlobName) && (b.VersionTimestamp == this._blob.VersionTimestamp)
				select b).FirstOrDefault<Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob>();
			return blob;
		}

		protected BlockBlob TryLoadBlockBlob(DevelopmentStorageDbDataContext context, out string existingDirectory)
		{
			BlockBlob blockBlob;
			existingDirectory = null;
			IQueryable<Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob> blobs = 
				from b in context.Blobs
				where (b.AccountName == this._blob.AccountName) && (b.ContainerName == this._blob.ContainerName) && (b.BlobName == this._blob.BlobName)
				select b;
			using (IEnumerator<Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob> enumerator = blobs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob current = enumerator.Current;
					if (!(current is BlockBlob))
					{
						throw new InvalidBlobTypeException();
					}
					existingDirectory = ((BlockBlob)current).DirectoryPath;
					if (DbBlobObject.RoundOffDateTimeToMillis(current.VersionTimestamp) != DbBlobObject.RoundOffDateTimeToMillis(this._blob.VersionTimestamp))
					{
						continue;
					}
					blockBlob = (BlockBlob)current;
					return blockBlob;
				}
				return null;
			}
			return blockBlob;
		}

		protected PageBlob TryLoadPageBlob(DevelopmentStorageDbDataContext context)
		{
			Microsoft.WindowsAzure.DevelopmentStorage.Store.Blob blob = this.TryLoadBlob(context);
			if (blob == null)
			{
				return null;
			}
			if (!(blob is PageBlob))
			{
				throw new InvalidBlobTypeException();
			}
			return (PageBlob)blob;
		}
	}
}