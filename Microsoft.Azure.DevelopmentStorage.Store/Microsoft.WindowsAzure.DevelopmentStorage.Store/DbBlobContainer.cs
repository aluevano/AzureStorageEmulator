using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbBlobContainer : IBlobContainer, IBaseBlobContainer, IContainer, IDisposable
	{
		private Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer _container;

		public IStorageAccount Account
		{
			get
			{
				return JustDecompileGenerated_get_Account();
			}
			set
			{
				JustDecompileGenerated_set_Account(value);
			}
		}

		private IStorageAccount JustDecompileGenerated_Account_k__BackingField;

		public IStorageAccount JustDecompileGenerated_get_Account()
		{
			return this.JustDecompileGenerated_Account_k__BackingField;
		}

		private void JustDecompileGenerated_set_Account(IStorageAccount value)
		{
			this.JustDecompileGenerated_Account_k__BackingField = value;
		}

		public byte[] ApplicationMetadata
		{
			get
			{
				return this._container.Metadata;
			}
			set
			{
				StorageStampHelpers.ValidateApplicationMetadata(value);
				this._container.Metadata = value;
			}
		}

		public string ContainerName
		{
			get
			{
				return this._container.ContainerName;
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

		private void JustDecompileGenerated_set_LeaseInfo(ILeaseInfo value)
		{
			this.JustDecompileGenerated_LeaseInfo_k__BackingField = value;
		}

		Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.ContainerType
		{
			get
			{
				return Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.BlobContainer;
			}
		}

		DateTime? Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.LastModificationTime
		{
			get
			{
				return new DateTime?(this._container.LastModificationTime);
			}
		}

		byte[] Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.ServiceMetadata
		{
			get
			{
				return this._container.ServiceMetadata;
			}
			set
			{
				StorageStampHelpers.ValidateServiceMetadata(value);
				this._container.ServiceMetadata = value;
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

		public DbStorageManager StorageManager
		{
			get;
			private set;
		}

		public TimeSpan Timeout
		{
			get;
			set;
		}

		public DbBlobContainer(DbStorageAccount account, string containerName) : this(account, new Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer()
		{
			AccountName = account.Name,
			ContainerName = containerName
		})
		{
		}

		public DbBlobContainer(DbStorageAccount account, Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer container)
		{
			StorageStampHelpers.CheckContainerName(container.ContainerName, Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.BlobContainer, false);
			this.StorageManager = account.StorageManager;
			this._container = container;
			this.OperationStatus = account.OperationStatus;
			this.Account = account;
		}

		public DbBlobContainer(DbStorageAccount account, Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer container, bool shouldGenerateLeaseInfo) : this(account, container)
		{
			if (shouldGenerateLeaseInfo)
			{
				this.LeaseInfo = new ContainerLeaseInfo(container, DateTime.UtcNow);
			}
		}

		private IEnumerator<IAsyncResult> AcquireLeaseImpl(LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, IContainerCondition condition, bool updateLastModificationTime, AsyncIteratorContext<NoResults> context)
		{
			// 
			// Current member / type: System.Collections.Generic.IEnumerator`1<System.IAsyncResult> Microsoft.WindowsAzure.DevelopmentStorage.Store.DbBlobContainer::AcquireLeaseImpl(Microsoft.Cis.Services.Nephos.Common.Storage.LeaseType,System.TimeSpan,System.Nullable`1<System.Guid>,Microsoft.Cis.Services.Nephos.Common.Storage.IContainerCondition,System.Boolean,AsyncHelper.AsyncIteratorContext`1<AsyncHelper.NoResults>)
			// File path: C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\Microsoft.Azure.DevelopmentStorage.Store.dll
			// 
			// Product version: 2017.3.1005.3
			// Exception in: System.Collections.Generic.IEnumerator<System.IAsyncResult> AcquireLeaseImpl(Microsoft.Cis.Services.Nephos.Common.Storage.LeaseType,System.TimeSpan,System.Nullable<System.Guid>,Microsoft.Cis.Services.Nephos.Common.Storage.IContainerCondition,System.Boolean,AsyncHelper.AsyncIteratorContext<AsyncHelper.NoResults>)
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

		public IAsyncResult BeginSetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.SetProperties", callback, state);
			asyncIteratorContext.Begin(this.SetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IEnumerator<IAsyncResult> BreakLeaseImpl(TimeSpan? leaseBreakPeriod, IContainerCondition condition, bool updateLastModificationTime, AsyncIteratorContext<NoResults> context)
		{
			// 
			// Current member / type: System.Collections.Generic.IEnumerator`1<System.IAsyncResult> Microsoft.WindowsAzure.DevelopmentStorage.Store.DbBlobContainer::BreakLeaseImpl(System.Nullable`1<System.TimeSpan>,Microsoft.Cis.Services.Nephos.Common.Storage.IContainerCondition,System.Boolean,AsyncHelper.AsyncIteratorContext`1<AsyncHelper.NoResults>)
			// File path: C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\Microsoft.Azure.DevelopmentStorage.Store.dll
			// 
			// Product version: 2017.3.1005.3
			// Exception in: System.Collections.Generic.IEnumerator<System.IAsyncResult> BreakLeaseImpl(System.Nullable<System.TimeSpan>,Microsoft.Cis.Services.Nephos.Common.Storage.IContainerCondition,System.Boolean,AsyncHelper.AsyncIteratorContext<AsyncHelper.NoResults>)
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

		private IEnumerator<IAsyncResult> ChangeLeaseImpl(Guid leaseId, Guid proposedLeaseId, IContainerCondition condition, bool updateLastModificationTime, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer nullable = this.LoadBlobContainer(dbContext);
					DbBlobContainer.CheckBlobContainerConditionWithoutLeaseCondition(nullable, condition);
					DateTime utcNow = DateTime.UtcNow;
					ContainerLeaseInfo containerLeaseInfo = new ContainerLeaseInfo(nullable, utcNow);
					switch (containerLeaseInfo.State.Value)
					{
						case LeaseState.Available:
						{
							throw new LeaseNotPresentException();
						}
						case LeaseState.Leased:
						{
							if (!(leaseId != containerLeaseInfo.Id.Value) || !(proposedLeaseId != containerLeaseInfo.Id.Value))
							{
								break;
							}
							throw new LeaseHeldException();
						}
						case LeaseState.Expired:
						{
							throw new LeaseHeldException();
						}
						case LeaseState.Breaking:
						{
							if (!containerLeaseInfo.Id.HasValue || !(leaseId != containerLeaseInfo.Id.Value) || !(proposedLeaseId != containerLeaseInfo.Id.Value))
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
					nullable.IsLeaseOp = !updateLastModificationTime;
					dbContext.SubmitChanges();
					containerLeaseInfo.SetBlobContainer(nullable, utcNow);
					this._container = nullable;
					this.LeaseInfo = containerLeaseInfo;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobContainer.ChangeLease"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		internal static void CheckBlobContainerCondition(Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer blobContainer, IContainerCondition condition, ContainerLeaseInfo leaseInfo)
		{
			DbBlobContainer.CheckBlobContainerCondition(blobContainer, condition, null, true, leaseInfo);
		}

		internal static void CheckBlobContainerCondition(Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer blobContainer, IContainerCondition condition, bool? isForSource, bool shouldCheckLease, ContainerLeaseInfo leaseInfo)
		{
			if (condition != null && condition.IfModifiedSinceTime.HasValue && condition.IfModifiedSinceTime.Value >= blobContainer.LastModificationTime)
			{
				throw new ConditionNotMetException(null, isForSource, null);
			}
			if (condition != null && condition.IfNotModifiedSinceTime.HasValue && condition.IfNotModifiedSinceTime.Value < blobContainer.LastModificationTime)
			{
				throw new ConditionNotMetException(null, isForSource, null);
			}
			if (shouldCheckLease && condition != null && condition.LeaseId.HasValue && leaseInfo != null)
			{
				LeaseState? state = leaseInfo.State;
				LeaseState valueOrDefault = state.GetValueOrDefault();
				if (state.HasValue)
				{
					switch (valueOrDefault)
					{
						case LeaseState.Available:
						case LeaseState.Broken:
						{
							if (!leaseInfo.Id.HasValue)
							{
								throw new LeaseNotPresentException();
							}
							throw new LeaseLostException();
						}
						case LeaseState.Expired:
						{
							throw new LeaseLostException();
						}
					}
				}
				if (leaseInfo.Id.Value != condition.LeaseId.Value)
				{
					throw new LeaseHeldException();
				}
			}
		}

		internal static void CheckBlobContainerConditionWithoutLeaseCondition(Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer blobContainer, IContainerCondition condition)
		{
			DbBlobContainer.CheckBlobContainerCondition(blobContainer, condition, null, false, null);
		}

		private IEnumerator<IAsyncResult> CreateContainerImpl(DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncIteratorContext<NoResults> context)
		{
			IStringDataEventStream verboseDebug = Logger<INormalAndDebugLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { expiryTime, serviceMetadata, applicationMetadata, this.Timeout };
			verboseDebug.Log("CreateContainerImpl({0},{1},{2},{3})", objArray);
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				StorageStampHelpers.CheckContainerName(this._container.ContainerName, Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.BlobContainer, false);
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					this._container.ServiceMetadata = serviceMetadata;
					this._container.Metadata = applicationMetadata;
					dbContext.BlobContainers.InsertOnSubmit(this._container);
					dbContext.SubmitChanges();
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobContainer.CreateContainerImpl"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> DeleteContainerImpl(IContainerCondition conditions, Guid? leaseId, AsyncIteratorContext<NoResults> context)
		{
			object obj;
			IStringDataEventStream verboseDebug = Logger<INormalAndDebugLogger>.Instance.VerboseDebug;
			object[] timeout = new object[2];
			object[] objArray = timeout;
			obj = (leaseId.HasValue ? leaseId.Value.ToString() : "NULL");
			objArray[0] = obj;
			timeout[1] = this.Timeout;
			verboseDebug.Log("DeleteContainerImpl({0},{1})", timeout);
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer blobContainer = this.LoadBlobContainer(dbContext);
					ContainerLeaseInfo containerLeaseInfo = new ContainerLeaseInfo(blobContainer, DateTime.UtcNow);
					DbBlobContainer.CheckBlobContainerCondition(blobContainer, conditions, containerLeaseInfo);
					LeaseState? state = containerLeaseInfo.State;
					LeaseState valueOrDefault = state.GetValueOrDefault();
					if (state.HasValue)
					{
						switch (valueOrDefault)
						{
							case LeaseState.Available:
							case LeaseState.Broken:
							{
								if (!leaseId.HasValue)
								{
									break;
								}
								if (!containerLeaseInfo.Id.HasValue)
								{
									throw new LeaseNotPresentException();
								}
								throw new LeaseLostException();
							}
							case LeaseState.Leased:
							case LeaseState.Breaking:
							{
								if (leaseId.HasValue && !(leaseId.Value != containerLeaseInfo.Id.Value))
								{
									break;
								}
								throw new LeaseHeldException();
							}
							case LeaseState.Expired:
							{
								if (!leaseId.HasValue)
								{
									break;
								}
								throw new LeaseLostException();
							}
						}
					}
					dbContext.BlobContainers.DeleteOnSubmit(blobContainer);
					dbContext.SubmitChanges();
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobContainer.DeleteContainerImpl"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		public void Dispose()
		{
		}

		public void EndSetProperties(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private IEnumerator<IAsyncResult> GetPropertiesImpl(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer blobContainer = this.LoadBlobContainer(dbContext);
					ContainerLeaseInfo containerLeaseInfo = new ContainerLeaseInfo(blobContainer, DateTime.UtcNow);
					DbBlobContainer.CheckBlobContainerCondition(blobContainer, condition, containerLeaseInfo);
					this._container = blobContainer;
					this.LeaseInfo = containerLeaseInfo;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobContainer.GetProperties"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		internal static string GetSummaryName(string name, string prefix, string separator, out bool hasSeparator)
		{
			hasSeparator = false;
			int num = name.IndexOf(separator, (prefix == null ? 0 : prefix.Length));
			if (num <= 0)
			{
				return name;
			}
			hasSeparator = true;
			return name.Substring(0, num + separator.Length);
		}

		internal static string GetSummaryName(string name, string prefix, string separator)
		{
			bool flag = false;
			return DbBlobContainer.GetSummaryName(name, prefix, separator, out flag);
		}

		internal static bool IsInfiniteLease(TimeSpan ts, bool isInput)
		{
			if (isInput)
			{
				return ts == RealServiceManager.LeaseDurationInfinite;
			}
			return ts == TimeSpan.FromSeconds(4294967295);
		}

		private IEnumerator<IAsyncResult> ListBlobsImpl(string blobNamePrefix, BlobPropertyNames propertyNames, string separator, string blobNameStart, DateTime? snapshotStart, IBlobObjectCondition condition, int maxBlobNames, BlobServiceVersion version, AsyncIteratorContext<IBlobObjectCollection> context)
		{
			IStringDataEventStream verboseDebug = Logger<INormalAndDebugLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { blobNamePrefix, propertyNames, separator, blobNameStart, snapshotStart, condition, maxBlobNames, this.Timeout };
			verboseDebug.Log("ListBlobsImpl({0},{1},{2},{3},{4},{5},{6},{7})", objArray);
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<DbBlobObjectCollection>((TimeSpan param0) => {
				DbBlobObjectCollection dbBlobObjectCollections;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					this.LoadBlobContainer(dbContext);
					IQueryable<Blob> blobs = this.MakeListBlobsQuery(blobNamePrefix, blobNameStart, snapshotStart, separator, condition, maxBlobNames, dbContext);
					dbBlobObjectCollections = this.ReadListBlobsResult(blobNamePrefix, separator, maxBlobNames, blobs);
				}
				return dbBlobObjectCollections;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobContainer.ListBlobs"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<DbBlobObjectCollection>(asyncResult);
		}

		private Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer LoadBlobContainer(DevelopmentStorageDbDataContext context)
		{
			return DbBlobContainer.LoadBlobContainer(context, this._container);
		}

		internal static Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer LoadBlobContainer(DevelopmentStorageDbDataContext context, Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer container)
		{
			StorageStampHelpers.CheckContainerName(container.ContainerName, Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.BlobContainer, false);
			Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer blobContainer = (
				from c in context.BlobContainers
				where (c.AccountName == container.AccountName) && (c.ContainerName == container.ContainerName)
				select c).FirstOrDefault<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer>();
			if (blobContainer == null)
			{
				throw new ContainerNotFoundException();
			}
			return blobContainer;
		}

		private IQueryable<Blob> MakeListBlobsQuery(string blobNamePrefix, string blobNameStart, DateTime? snapshotStart, string separator, IBlobObjectCondition condition, int maxBlobNames, DevelopmentStorageDbDataContext dataContext)
		{
			IQueryable<Blob> blobs = 
				from b in dataContext.Blobs
				where (b.AccountName == this._container.AccountName) && (b.ContainerName == this._container.ContainerName)
				select b;
			if (condition != null)
			{
				if (!condition.IsIncludingPageBlobs)
				{
					blobs = 
						from b in blobs
						where b.BlobTypeInt == 1
						select b;
				}
				if (!condition.IsIncludingUncommittedBlobs)
				{
					blobs = 
						from b in blobs
						where ((BlockBlob)b).IsCommitted == null || ((BlockBlob)b).IsCommitted.Value
						select b;
				}
			}
			if (!string.IsNullOrEmpty(separator))
			{
				blobs = 
					from b in dataContext.GetSummaryBlobs(this._container.AccountName, this._container.ContainerName, blobNamePrefix, separator)
					select b;
			}
			else if (!string.IsNullOrEmpty(blobNamePrefix))
			{
				blobs = 
					from b in blobs
					where b.BlobName.StartsWith(DbStorageAccount.FixTildeInPrefix(blobNamePrefix))
					select b;
			}
			if (blobNameStart != null)
			{
				blobs = (!snapshotStart.HasValue ? 
					from b in blobs
					where b.BlobName.CompareTo(blobNameStart) >= 0
					select b : 
					from b in blobs
					where b.BlobName.CompareTo(blobNameStart) > 0 || b.BlobName.CompareTo(blobNameStart) == 0 && (b.VersionTimestamp >= snapshotStart.Value)
					select b);
			}
			if (condition != null)
			{
				if (condition.IfModifiedSinceTime.HasValue)
				{
					blobs = 
						from b in blobs
						where b.LastModificationTime > (DateTime?)condition.IfModifiedSinceTime.Value
						select b;
				}
				if (condition.IfNotModifiedSinceTime.HasValue)
				{
					blobs = 
						from b in blobs
						where b.LastModificationTime <= (DateTime?)condition.IfNotModifiedSinceTime.Value
						select b;
				}
				if (!condition.IsIncludingSnapshots)
				{
					blobs = 
						from b in blobs
						where b.VersionTimestamp == StorageStampHelpers.RootBlobSnapshotVersion
						select b;
				}
			}
			if (maxBlobNames != 0)
			{
				blobs = (
					from x in blobs
					orderby x.AccountName
					orderby x.ContainerName
					orderby x.BlobName
					select x).Take<Blob>(maxBlobNames + 1);
			}
			return blobs;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.BeginAcquireLease(LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.AcquireLease", callback, state);
			asyncIteratorContext.Begin(this.AcquireLeaseImpl(leaseType, leaseDuration, proposedLeaseId, condition, updateLastModificationTime, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.BeginBreakLease(TimeSpan? leaseBreakPeriod, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.BreakLease", callback, state);
			asyncIteratorContext.Begin(this.BreakLeaseImpl(leaseBreakPeriod, condition, updateLastModificationTime, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.BeginChangeLease(Guid leaseId, Guid proposedLeaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.ChangeLease", callback, state);
			asyncIteratorContext.Begin(this.ChangeLeaseImpl(leaseId, proposedLeaseId, condition, updateLastModificationTime, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.BeginCreateContainer(DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.CreateContainer", callback, state);
			asyncIteratorContext.Begin(this.CreateContainerImpl(expiryTime, serviceMetadata, applicationMetadata, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.BeginDeleteContainer(IContainerCondition conditions, Guid? leaseId, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.DeleteContainer", callback, state);
			asyncIteratorContext.Begin(this.DeleteContainerImpl(conditions, leaseId, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.BeginReleaseLease(Guid leaseId, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.ReleaseLease", callback, state);
			asyncIteratorContext.Begin(this.ReleaseLeaseImpl(leaseId, condition, updateLastModificationTime, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.BeginRenewLease(LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, IContainerCondition condition, bool updateLastModificationTime, bool useContainerNotFoundError, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.RenewLease", callback, state);
			asyncIteratorContext.Begin(this.RenewLeaseImpl(leaseType, leaseId, leaseDuration, condition, updateLastModificationTime, asyncIteratorContext));
			return asyncIteratorContext;
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.EndAcquireLease(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.EndBreakLease(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.EndChangeLease(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.EndCreateContainer(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.EndDeleteContainer(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.EndReleaseLease(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IBaseBlobContainer.EndRenewLease(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.BeginListBlobs(string blobNamePrefix, BlobPropertyNames propertyNames, string separator, string blobNameStart, DateTime? snapshotStart, IBlobObjectCondition condition, int maxBlobNames, BlobServiceVersion version, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IBlobObjectCollection> asyncIteratorContext = new AsyncIteratorContext<IBlobObjectCollection>("DbBlobContainer.ListBlobs", callback, state);
			asyncIteratorContext.Begin(this.ListBlobsImpl(blobNamePrefix, propertyNames, separator, blobNameStart, snapshotStart, condition, maxBlobNames, version, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IListBlobObject Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.CreateAppendBlobInstance(string blobName, DateTime snapshot, BlobServiceVersion blobServiceVersion)
		{
			throw new FeatureNotSupportedByEmulatorException("Append Blob");
		}

		IBlobObject Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.CreateBaseBlobObjectInstance(string blobName, DateTime snapshot, BlobServiceVersion blobServiceVersion)
		{
			return new DbBlobObject(this, blobName, snapshot, BlobType.None, blobServiceVersion);
		}

		IListBlobObject Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.CreateBlockBlobInstance(string blobName, DateTime snapshot, BlobServiceVersion blobServiceVersion)
		{
			return new DbListBlobObject(this, blobName, snapshot, blobServiceVersion);
		}

		IIndexBlobObject Microsoft.Cis.Services.Nephos.Common.Storage.IBlobContainer.CreateIndexBlobObjectInstance(string blobName, DateTime snapshot, BlobServiceVersion blobServiceVersion)
		{
			return new DbPageBlobObject(this, blobName, snapshot, blobServiceVersion);
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

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, bool shouldUpdateCacheEntryOnRefresh, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbBlobContainer.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.EndGetProperties(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private DbBlobObjectCollection ReadListBlobsResult(string blobNamePrefix, string separator, int maxBlobNames, IQueryable<Blob> blobs)
		{
			DbBlobObject dbListBlobObject;
			int num = 0;
			string blobName = null;
			DateTime? nullable = null;
			List<IBlobObject> blobObjects = new List<IBlobObject>();
			foreach (Blob blob in blobs)
			{
				bool flag = false;
				if (!string.IsNullOrEmpty(separator))
				{
					blob.BlobName = DbBlobContainer.GetSummaryName(blob.BlobName, blobNamePrefix, separator, out flag);
				}
				if (maxBlobNames != 0)
				{
					num++;
					if (num > maxBlobNames)
					{
						blobName = blob.BlobName;
						nullable = new DateTime?(blob.VersionTimestamp);
						break;
					}
				}
				if (blob.BlobType == BlobType.ListBlob)
				{
					dbListBlobObject = new DbListBlobObject(this.StorageManager, blob, BlobServiceVersion.Sept09);
				}
				else
				{
					dbListBlobObject = new DbPageBlobObject(this.StorageManager, blob, BlobServiceVersion.Sept09);
				}
				DbBlobObject dbBlobObject = dbListBlobObject;
				blobObjects.Add(dbBlobObject);
				if (!flag)
				{
					continue;
				}
				dbBlobObject.SetBlobNoneType();
			}
			return new DbBlobObjectCollection(blobObjects, null, blobName, nullable);
		}

		private IEnumerator<IAsyncResult> ReleaseLeaseImpl(Guid leaseId, IContainerCondition condition, bool updateLastModificationTime, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer nullable = this.LoadBlobContainer(dbContext);
					DbBlobContainer.CheckBlobContainerConditionWithoutLeaseCondition(nullable, condition);
					DateTime utcNow = DateTime.UtcNow;
					ContainerLeaseInfo containerLeaseInfo = new ContainerLeaseInfo(nullable, utcNow);
					if (containerLeaseInfo.Id.HasValue && containerLeaseInfo.Id.Value != leaseId)
					{
						throw new LeaseLostException();
					}
					if (containerLeaseInfo.State.Value == LeaseState.Available)
					{
						throw new LeaseLostException();
					}
					nullable.LeaseEndTime = new DateTime?(utcNow);
					nullable.LeaseState = 0;
					nullable.IsLeaseOp = !updateLastModificationTime;
					dbContext.SubmitChanges();
					containerLeaseInfo.SetBlobContainer(nullable, utcNow);
					this._container = nullable;
					this.LeaseInfo = containerLeaseInfo;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobContainer.ReleaseLease"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> RenewLeaseImpl(LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, IContainerCondition condition, bool updateLastModificationTime, AsyncIteratorContext<NoResults> context)
		{
			// 
			// Current member / type: System.Collections.Generic.IEnumerator`1<System.IAsyncResult> Microsoft.WindowsAzure.DevelopmentStorage.Store.DbBlobContainer::RenewLeaseImpl(Microsoft.Cis.Services.Nephos.Common.Storage.LeaseType,System.Guid,System.TimeSpan,Microsoft.Cis.Services.Nephos.Common.Storage.IContainerCondition,System.Boolean,AsyncHelper.AsyncIteratorContext`1<AsyncHelper.NoResults>)
			// File path: C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\Microsoft.Azure.DevelopmentStorage.Store.dll
			// 
			// Product version: 2017.3.1005.3
			// Exception in: System.Collections.Generic.IEnumerator<System.IAsyncResult> RenewLeaseImpl(Microsoft.Cis.Services.Nephos.Common.Storage.LeaseType,System.Guid,System.TimeSpan,Microsoft.Cis.Services.Nephos.Common.Storage.IContainerCondition,System.Boolean,AsyncHelper.AsyncIteratorContext<AsyncHelper.NoResults>)
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

		private IEnumerator<IAsyncResult> SetPropertiesImpl(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer>((TimeSpan param0) => {
				Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer blobContainer;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer serviceMetadata = this.LoadBlobContainer(dbContext);
					ContainerLeaseInfo containerLeaseInfo = new ContainerLeaseInfo(serviceMetadata, DateTime.UtcNow);
					DbBlobContainer.CheckBlobContainerCondition(serviceMetadata, condition, containerLeaseInfo);
					if ((propertyNames & ContainerPropertyNames.ServiceMetadata) != ContainerPropertyNames.None)
					{
						serviceMetadata.ServiceMetadata = ((IContainer)this).ServiceMetadata;
					}
					if ((propertyNames & ContainerPropertyNames.ApplicationMetadata) != ContainerPropertyNames.None)
					{
						serviceMetadata.Metadata = this.ApplicationMetadata;
					}
					dbContext.SubmitChanges();
					this.LeaseInfo = containerLeaseInfo;
					blobContainer = serviceMetadata;
				}
				return blobContainer;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbBlobContainer.SetProperties"));
			yield return asyncResult;
			this._container = this.StorageManager.AsyncProcessor.EndExecute<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer>(asyncResult);
		}
	}
}