using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public abstract class Container : IContainer, IDisposable
	{
		private IContainer container;

		public IStorageAccount Account
		{
			get
			{
				IStorageAccount account;
				try
				{
					account = this.container.Account;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return account;
			}
		}

		public byte[] ApplicationMetadata
		{
			get
			{
				byte[] applicationMetadata;
				try
				{
					applicationMetadata = this.container.ApplicationMetadata;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return applicationMetadata;
			}
			set
			{
				try
				{
					this.container.ApplicationMetadata = value;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		public string ContainerName
		{
			get
			{
				string containerName;
				try
				{
					containerName = this.container.ContainerName;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return containerName;
			}
		}

		public abstract Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType ContainerType
		{
			get;
		}

		protected IContainer InternalContainer
		{
			get
			{
				return this.container;
			}
		}

		public DateTime? LastModificationTime
		{
			get
			{
				DateTime? lastModificationTime;
				try
				{
					lastModificationTime = this.container.LastModificationTime;
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
					leaseInfo = this.container.LeaseInfo;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return leaseInfo;
			}
		}

		public virtual Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get
			{
				return this.container.OperationStatus;
			}
			set
			{
				this.container.OperationStatus = value;
			}
		}

		public virtual Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get
			{
				return this.container.ProviderInjection;
			}
			set
			{
				this.container.ProviderInjection = value;
			}
		}

		public byte[] ServiceMetadata
		{
			get
			{
				byte[] serviceMetadata;
				try
				{
					serviceMetadata = this.container.ServiceMetadata;
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
					this.container.ServiceMetadata = value;
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
				return this.container.Timeout;
			}
			set
			{
				try
				{
					this.container.Timeout = StorageStampHelpers.AdjustTimeoutRange(value);
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		protected Container(IContainer container)
		{
			this.container = container;
		}

		public IAsyncResult BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncCallback callback, object state)
		{
			return this.BeginGetProperties(propertyNames, condition, CacheRefreshOptions.None, callback, state);
		}

		public IAsyncResult BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, AsyncCallback callback, object state)
		{
			return this.BeginGetProperties(propertyNames, condition, cacheRefreshOptions, true, callback, state);
		}

		public IAsyncResult BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, bool shouldUpdateCacheEntryOnRefresh, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobContainer.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, cacheRefreshOptions, shouldUpdateCacheEntryOnRefresh, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IAsyncResult BeginGetPropertiesNoCacheSynchronization(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, bool shouldUpdateCacheEntryOnRefresh, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobContainer.GetPropertiesNoCacheSynchronization", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesNoCacheSynchronizationImpl(propertyNames, condition, cacheRefreshOptions, shouldUpdateCacheEntryOnRefresh, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealBlobContainer.SetProperties", callback, state);
			asyncIteratorContext.Begin(this.SetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.container.Dispose();
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

		private void EndGetPropertiesNoCacheSynchronization(IAsyncResult ar)
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

		private IEnumerator<IAsyncResult> GetPropertiesImpl(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, bool shouldUpdateCacheEntryOnRefresh, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.Account.BeginGetProperties(AccountPropertyNames.None, null, context.GetResumeCallback(), context.GetResumeState("Container.GetPropertiesImpl"));
			yield return asyncResult;
			this.Account.EndGetProperties(asyncResult);
			asyncResult = this.BeginGetPropertiesNoCacheSynchronization(propertyNames, condition, cacheRefreshOptions, shouldUpdateCacheEntryOnRefresh, context.GetResumeCallback(), context.GetResumeState("Container.GetPropertiesImpl"));
			yield return asyncResult;
			this.EndGetPropertiesNoCacheSynchronization(asyncResult);
		}

		private IEnumerator<IAsyncResult> GetPropertiesNoCacheSynchronizationImpl(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, bool shouldUpdateCacheEntryOnRefresh, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.container.BeginGetProperties(propertyNames, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BlobContainer.GetPropertiesImpl"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.container.EndGetProperties(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> SetPropertiesImpl(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			try
			{
				asyncResult = this.container.BeginSetProperties(propertyNames, Helpers.Convert(condition), context.GetResumeCallback(), context.GetResumeState("BlobContainer.SetProperties"));
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.container.EndSetProperties(asyncResult);
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
		}
	}
}