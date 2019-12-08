using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class StorageAccount : IStorageAccount, IDisposable
	{
		private IStorageAccount internalAccount;

		public string ClusterName
		{
			get
			{
				return this.internalAccount.ClusterName;
			}
			set
			{
				this.internalAccount.ClusterName = value;
			}
		}

		public bool IsSecondaryAccess
		{
			get
			{
				bool isSecondaryAccess;
				try
				{
					isSecondaryAccess = this.internalAccount.IsSecondaryAccess;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return isSecondaryAccess;
			}
			set
			{
				try
				{
					this.internalAccount.IsSecondaryAccess = value;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		public DateTime? LastModificationTime
		{
			get
			{
				DateTime? lastModificationTime;
				try
				{
					lastModificationTime = this.internalAccount.LastModificationTime;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return lastModificationTime;
			}
			set
			{
				try
				{
					this.internalAccount.LastModificationTime = value;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		public string Name
		{
			get
			{
				string name;
				try
				{
					name = this.internalAccount.Name;
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
				return this.internalAccount.OperationStatus;
			}
			set
			{
				this.internalAccount.OperationStatus = value;
			}
		}

		public AccountPermissions? Permissions
		{
			get
			{
				AccountPermissions? nullable;
				try
				{
					nullable = new AccountPermissions?(this.internalAccount.Permissions.Value);
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return nullable;
			}
			set
			{
				try
				{
					this.internalAccount.Permissions = value;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get
			{
				return this.internalAccount.ProviderInjection;
			}
			set
			{
				this.internalAccount.ProviderInjection = value;
			}
		}

		public SecretKeyListV3 SecretKeysV3
		{
			get
			{
				SecretKeyListV3 secretKeysV3;
				try
				{
					secretKeysV3 = this.internalAccount.SecretKeysV3;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return secretKeysV3;
			}
			set
			{
				try
				{
					this.internalAccount.SecretKeysV3 = value;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		public AccountServiceMetadata ServiceMetadata
		{
			get
			{
				AccountServiceMetadata serviceMetadata;
				try
				{
					serviceMetadata = this.internalAccount.ServiceMetadata;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return serviceMetadata;
			}
			set
			{
				try
				{
					this.internalAccount.ServiceMetadata = value;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		public TimeSpan Timeout
		{
			get
			{
				return this.internalAccount.Timeout;
			}
			set
			{
				try
				{
					this.internalAccount.Timeout = StorageStampHelpers.AdjustTimeoutRange(value);
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		internal StorageAccount(IStorageAccount account)
		{
			this.internalAccount = account;
		}

		public IAsyncResult BeginCreateQueueContainer(string queueName, DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IQueueContainer> asyncIteratorContext = new AsyncIteratorContext<IQueueContainer>("StorageAccount.CreateQueueContainer", callback, state);
			asyncIteratorContext.Begin(this.CreateQueueContainerImpl(queueName, expiryTime, serviceMetadata, applicationMetadata, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginCreateTableContainer(string tableName, DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ITableContainer> asyncIteratorContext = new AsyncIteratorContext<ITableContainer>("StorageAccount.CreateTableContainer", callback, state);
			asyncIteratorContext.Begin(this.CreateTableContainerImpl(tableName, expiryTime, serviceMetadata, applicationMetadata, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginDeleteQueueContainer(string queueName, IContainerCondition conditions, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("StorageAccount.DeleteQueueContainer", callback, state);
			asyncIteratorContext.Begin(this.DeleteQueueContainerImpl(queueName, conditions, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginDeleteTableContainer(string tableName, IContainerCondition conditions, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("StorageAccount.DeleteTableContainer", callback, state);
			asyncIteratorContext.Begin(this.DeleteTableContainerImpl(tableName, conditions, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetProperties(AccountPropertyNames propertyNames, IAccountCondition condition, AsyncCallback callback, object state)
		{
			return this.BeginGetProperties(propertyNames, condition, CacheRefreshOptions.None, Environment.TickCount & 2147483647, callback, state);
		}

		public IAsyncResult BeginGetProperties(AccountPropertyNames propertyNames, IAccountCondition condition, CacheRefreshOptions cacheRefreshOptions, int localCacheEntryTimeInTicks, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("StorageAccount.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, cacheRefreshOptions, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginListBlobContainers(string containerName, ContainerPropertyNames propertyNames, string separator, string containerNameStart, IContainerCondition condition, int maxContainerNames, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IBlobContainerCollection> asyncIteratorContext = new AsyncIteratorContext<IBlobContainerCollection>("StorageAccount.ListBlobContainers", callback, state);
			asyncIteratorContext.Begin(this.ListBlobContainersImpl(containerName, propertyNames, separator, containerNameStart, condition, maxContainerNames, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginListQueueContainers(string queueName, ContainerPropertyNames propertyNames, string separator, string queueNameStart, IContainerCondition condition, int maxContainerNames, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IQueueContainerCollection> asyncIteratorContext = new AsyncIteratorContext<IQueueContainerCollection>("StorageAccount.ListQueueContainers", callback, state);
			asyncIteratorContext.Begin(this.ListQueueContainersImpl(queueName, propertyNames, separator, queueNameStart, condition, maxContainerNames, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginListTableContainers(string tableName, ContainerPropertyNames propertyNames, string separator, string tableNameStart, IContainerCondition condition, int maxContainerNames, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ITableContainerCollection> asyncIteratorContext = new AsyncIteratorContext<ITableContainerCollection>("StorageAccount.ListTableContainers", callback, state);
			asyncIteratorContext.Begin(this.ListTableContainersImpl(tableName, propertyNames, separator, tableNameStart, condition, maxContainerNames, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetProperties(AccountPropertyNames propertyNames, IAccountCondition conditions, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("StorageAccount.SetProperties", callback, state);
			asyncIteratorContext.Begin(this.SetPropertiesImpl(propertyNames, conditions, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IBlobContainer CreateBlobContainerInstance(string containerName)
		{
			IBlobContainer blobContainer;
			try
			{
				IBlobContainer operationStatus = this.internalAccount.CreateBlobContainerInstance(containerName);
				operationStatus.OperationStatus = this.OperationStatus;
				operationStatus.ProviderInjection = this.ProviderInjection;
				blobContainer = new BlobContainer(operationStatus);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			return blobContainer;
		}

		private IEnumerator<IAsyncResult> CreateQueueContainerImpl(string queueName, DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncIteratorContext<IQueueContainer> context)
		{
			IAsyncResult asyncResult;
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { queueName, expiryTime, serviceMetadata, applicationMetadata, this.Timeout };
			verboseDebug.Log("CreateQueueContainerImpl({0},{1},{2},{3},{4})", objArray);
			try
			{
				asyncResult = this.internalAccount.BeginCreateQueueContainer(queueName, StorageStampHelpers.AdjustNullableDatetimeRange(expiryTime), serviceMetadata, applicationMetadata, context.GetResumeCallback(), context.GetResumeState("StorageAccount.CreateQueueContainerImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				IQueueContainer queueContainer = this.internalAccount.EndCreateQueueContainer(asyncResult);
				context.ResultData = new QueueContainer(queueContainer);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		public IQueueContainer CreateQueueContainerInstance(string queueName)
		{
			IQueueContainer queueContainer;
			try
			{
				IQueueContainer operationStatus = this.internalAccount.CreateQueueContainerInstance(queueName);
				operationStatus.OperationStatus = this.OperationStatus;
				queueContainer = new QueueContainer(operationStatus);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			return queueContainer;
		}

		private IEnumerator<IAsyncResult> CreateTableContainerImpl(string tableName, DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncIteratorContext<ITableContainer> context)
		{
			IAsyncResult asyncResult;
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { tableName, expiryTime, serviceMetadata, applicationMetadata, this.Timeout };
			verboseDebug.Log("CreateTableContainerImpl({0},{1},{2},{3},{4})", objArray);
			try
			{
				asyncResult = this.internalAccount.BeginCreateTableContainer(tableName, StorageStampHelpers.AdjustNullableDatetimeRange(expiryTime), serviceMetadata, applicationMetadata, context.GetResumeCallback(), context.GetResumeState("StorageAccount.CreateTableContainerImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				ITableContainer tableContainer = this.internalAccount.EndCreateTableContainer(asyncResult);
				context.ResultData = new TableContainer(tableContainer);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		public ITableContainer CreateTableContainerInstance(string tableName)
		{
			ITableContainer tableContainer;
			try
			{
				ITableContainer operationStatus = this.internalAccount.CreateTableContainerInstance(tableName);
				operationStatus.OperationStatus = this.OperationStatus;
				tableContainer = new TableContainer(operationStatus);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			return tableContainer;
		}

		private IEnumerator<IAsyncResult> DeleteQueueContainerImpl(string queueName, IContainerCondition conditions, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { queueName, this.Timeout };
			verboseDebug.Log("DeleteQueueContainerImpl({0},{1})", objArray);
			try
			{
				asyncResult = this.internalAccount.BeginDeleteQueueContainer(queueName, Helpers.Convert(conditions), context.GetResumeCallback(), context.GetResumeState("StorageAccount.DeleteQueueContainerImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.internalAccount.EndDeleteQueueContainer(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> DeleteTableContainerImpl(string tableName, IContainerCondition conditions, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { tableName, this.Timeout };
			verboseDebug.Log("DeleteTableContainerImpl({0},{1})", objArray);
			try
			{
				asyncResult = this.internalAccount.BeginDeleteTableContainer(tableName, Helpers.Convert(conditions), context.GetResumeCallback(), context.GetResumeState("StorageAccount.DeleteTableContainerImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.internalAccount.EndDeleteTableContainer(asyncResult);
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
				this.internalAccount.Dispose();
			}
		}

		public IQueueContainer EndCreateQueueContainer(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<IQueueContainer> asyncIteratorContext = (AsyncIteratorContext<IQueueContainer>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public ITableContainer EndCreateTableContainer(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<ITableContainer> asyncIteratorContext = (AsyncIteratorContext<ITableContainer>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public void EndDeleteQueueContainer(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndDeleteTableContainer(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
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

		public IBlobContainerCollection EndListBlobContainers(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<IBlobContainerCollection> asyncIteratorContext = (AsyncIteratorContext<IBlobContainerCollection>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public IQueueContainerCollection EndListQueueContainers(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<IQueueContainerCollection> asyncIteratorContext = (AsyncIteratorContext<IQueueContainerCollection>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public ITableContainerCollection EndListTableContainers(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<ITableContainerCollection> asyncIteratorContext = (AsyncIteratorContext<ITableContainerCollection>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
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

		private IEnumerator<IAsyncResult> GetPropertiesImpl(AccountPropertyNames propertyNames, IAccountCondition condition, CacheRefreshOptions cacheRefreshOptions, AsyncIteratorContext<NoResults> context)
		{
			return this.GetPropertiesImplOld(propertyNames, condition, cacheRefreshOptions, context);
		}

		private IEnumerator<IAsyncResult> GetPropertiesImplOld(AccountPropertyNames propertyNames, IAccountCondition condition, CacheRefreshOptions cacheRefreshOptions, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.internalAccount.BeginGetProperties(propertyNames, condition, context.GetResumeCallback(), context.GetResumeState("StorageAccount.GetPropertiesImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.internalAccount.EndGetProperties(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> ListBlobContainersImpl(string containerName, ContainerPropertyNames propertyNames, string separator, string containerNameStart, IContainerCondition condition, int maxContainerNames, AsyncIteratorContext<IBlobContainerCollection> context)
		{
			IAsyncResult asyncResult;
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { containerName, propertyNames, separator, containerNameStart, condition, maxContainerNames, this.Timeout };
			verboseDebug.Log("ListBlobContainersImpl({0},{1},{2},{3},{4},{5},{6})", objArray);
			try
			{
				asyncResult = this.internalAccount.BeginListBlobContainers(containerName, propertyNames, separator, containerNameStart, Helpers.Convert(condition), maxContainerNames, context.GetResumeCallback(), context.GetResumeState("StorageStamp.ListBlobContainersImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				IBlobContainerCollection blobContainerCollections = this.internalAccount.EndListBlobContainers(asyncResult);
				context.ResultData = new BlobContainerCollection(blobContainerCollections);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> ListQueueContainersImpl(string queueName, ContainerPropertyNames propertyNames, string separator, string queueNameStart, IContainerCondition condition, int maxContainerNames, AsyncIteratorContext<IQueueContainerCollection> context)
		{
			IAsyncResult asyncResult;
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { queueName, propertyNames, separator, queueNameStart, condition, maxContainerNames, this.Timeout };
			verboseDebug.Log("ListQueueContainersImpl({0},{1},{2},{3},{4},{5},{6})", objArray);
			try
			{
				asyncResult = this.internalAccount.BeginListQueueContainers(queueName, propertyNames, separator, queueNameStart, Helpers.Convert(condition), maxContainerNames, context.GetResumeCallback(), context.GetResumeState("StorageStamp.ListQueueContainersImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				IQueueContainerCollection queueContainerCollections = this.internalAccount.EndListQueueContainers(asyncResult);
				context.ResultData = new QueueContainerCollection(queueContainerCollections);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> ListTableContainersImpl(string tableName, ContainerPropertyNames propertyNames, string separator, string tableNameStart, IContainerCondition condition, int maxContainerNames, AsyncIteratorContext<ITableContainerCollection> context)
		{
			IAsyncResult asyncResult;
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { tableName, propertyNames, separator, tableNameStart, condition, maxContainerNames, this.Timeout };
			verboseDebug.Log("ListTableContainersImpl({0},{1},{2},{3},{4},{5},{6})", objArray);
			try
			{
				asyncResult = this.internalAccount.BeginListTableContainers(tableName, propertyNames, separator, tableNameStart, Helpers.Convert(condition), maxContainerNames, context.GetResumeCallback(), context.GetResumeState("StorageStamp.ListTableContainersImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				ITableContainerCollection tableContainerCollections = this.internalAccount.EndListTableContainers(asyncResult);
				context.ResultData = new TableContainerCollection(tableContainerCollections);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> SetPropertiesImpl(AccountPropertyNames propertyNames, IAccountCondition conditions, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { propertyNames, conditions, this.Timeout };
			verboseDebug.Log("SetPropertiesImpl({0},{1},{2})", objArray);
			try
			{
				asyncResult = this.internalAccount.BeginSetProperties(propertyNames, null, context.GetResumeCallback(), context.GetResumeState("StorageAccount.SetPropertiesImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.internalAccount.EndSetProperties(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}
	}
}