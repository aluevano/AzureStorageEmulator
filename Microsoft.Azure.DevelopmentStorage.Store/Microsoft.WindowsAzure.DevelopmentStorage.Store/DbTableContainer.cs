using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Storage;
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
	internal class DbTableContainer : ITableContainer, IContainer, IDisposable
	{
		private Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer _tableContainer;

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
				return this._tableContainer.Metadata;
			}
			set
			{
				this._tableContainer.Metadata = value;
			}
		}

		public string ContainerName
		{
			get
			{
				return this._tableContainer.CasePreservedTableName;
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType ContainerType
		{
			get
			{
				return Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.TableContainer;
			}
		}

		public DateTime? LastModificationTime
		{
			get
			{
				return new DateTime?(this._tableContainer.LastModificationTime);
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

		public byte[] ServiceMetadata
		{
			get
			{
				return this._tableContainer.ServiceMetadata;
			}
			set
			{
				this._tableContainer.ServiceMetadata = value;
			}
		}

		internal DbStorageManager StorageManager
		{
			get;
			private set;
		}

		public TimeSpan Timeout
		{
			get;
			set;
		}

		internal DbTableContainer(DbStorageAccount account, string tableName) : this(account, new Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer()
		{
			AccountName = account.Name,
			TableName = tableName.ToLowerInvariant(),
			CasePreservedTableName = tableName
		})
		{
		}

		internal DbTableContainer(DbStorageAccount account, Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer container)
		{
			StorageStampHelpers.CheckContainerName(container.CasePreservedTableName, Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.TableContainer, false);
			this.StorageManager = account.StorageManager;
			this._tableContainer = container;
			this.OperationStatus = account.OperationStatus;
			this.Account = account;
		}

		public IAsyncResult BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbTableContainer.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbTableContainer.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, bool shouldUpdateCacheEntryOnRefresh, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbTableContainer.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbTableContainer.SetProperties", callback, state);
			asyncIteratorContext.Begin(this.SetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		internal static void CheckTableContainerCondition(Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer tableContainer, IContainerCondition condition)
		{
			if (condition != null && condition.IfModifiedSinceTime.HasValue && condition.IfModifiedSinceTime.Value >= tableContainer.LastModificationTime)
			{
				throw new ConditionNotMetException(null, null, null);
			}
			if (condition != null && condition.IfNotModifiedSinceTime.HasValue && condition.IfNotModifiedSinceTime.Value < tableContainer.LastModificationTime)
			{
				throw new ConditionNotMetException(null, null, null);
			}
		}

		public void Dispose()
		{
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
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan remaining) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer tableContainer = this.LoadTableContainer(dbContext);
					DbTableContainer.CheckTableContainerCondition(tableContainer, condition);
					this._tableContainer = tableContainer;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbTableContainer.GetPropertiesImpl"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer LoadTableContainer(DevelopmentStorageDbDataContext context)
		{
			return DbTableContainer.LoadTableContainer(context, this._tableContainer);
		}

		internal static Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer LoadTableContainer(DevelopmentStorageDbDataContext context, Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer tableContainer)
		{
			StorageStampHelpers.CheckContainerName(tableContainer.CasePreservedTableName, Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.TableContainer, false);
			Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer tableContainer1 = (
				from c in context.TableContainers
				where (c.AccountName == tableContainer.AccountName) && (c.TableName == tableContainer.TableName)
				select c).FirstOrDefault<Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer>();
			if (tableContainer1 == null)
			{
				throw new ContainerNotFoundException();
			}
			return tableContainer1;
		}

		private IEnumerator<IAsyncResult> SetPropertiesImpl(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan remaining) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer applicationMetadata = this.LoadTableContainer(dbContext);
					if ((propertyNames & ContainerPropertyNames.ApplicationMetadata) != ContainerPropertyNames.None)
					{
						StorageStampHelpers.ValidateApplicationMetadata(this.ApplicationMetadata);
						applicationMetadata.Metadata = this.ApplicationMetadata;
					}
					if ((propertyNames & ContainerPropertyNames.ServiceMetadata) != ContainerPropertyNames.None)
					{
						applicationMetadata.ServiceMetadata = ((IContainer)this).ServiceMetadata;
					}
					dbContext.SubmitChanges();
					this._tableContainer = applicationMetadata;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbTableContainer.SetProperties"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}
	}
}