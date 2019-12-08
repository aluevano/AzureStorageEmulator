using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
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
	internal class DbStorageAccount : IStorageAccount, IDisposable
	{
		private readonly TimeSpan DEFAULTTIMEOUT = TimeSpan.FromSeconds(30);

		private Account _account;

		private bool _isSecondaryAccess;

		private DateTime? _lastModificationTime;

		private AccountServiceMetadata _accountServiceMetadata;

		private AccountPermissions? _accountPermissions;

		private static char[] s_specialCharactersForLike;

		public string ClusterName
		{
			get;
			set;
		}

		public bool IsSecondaryAccess
		{
			get
			{
				return this._isSecondaryAccess;
			}
			set
			{
				this._isSecondaryAccess = value;
			}
		}

		public DateTime? LastModificationTime
		{
			get
			{
				return this._lastModificationTime;
			}
			set
			{
				this._lastModificationTime = value;
			}
		}

		AccountPermissions? Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.Permissions
		{
			get
			{
				return this._accountPermissions;
			}
			set
			{
				this._accountPermissions = value;
			}
		}

		SecretKeyListV3 Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.SecretKeysV3
		{
			get
			{
				SecretKeyListV3 secretKeyListV3 = new SecretKeyListV3()
				{
					new SecretKeyV3("key1", this._account.SecretKey, SecretKeyPermissions.Full),
					new SecretKeyV3("key2", this._account.SecondaryKey, SecretKeyPermissions.Full)
				};
				return secretKeyListV3;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public string Name
		{
			get
			{
				return this._account.Name;
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

		public AccountServiceMetadata ServiceMetadata
		{
			get
			{
				return this._accountServiceMetadata;
			}
			set
			{
				this._accountServiceMetadata = value;
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

		static DbStorageAccount()
		{
			DbStorageAccount.s_specialCharactersForLike = new char[] { '%', '\u005F', '[' };
		}

		internal DbStorageAccount(DbStorageStamp stamp, string accountName) : this(stamp, new Account()
		{
			Name = accountName
		})
		{
		}

		internal DbStorageAccount(DbStorageStamp stamp, Account account)
		{
			this.StorageManager = stamp.StorageManager;
			this._account = account;
			this.OperationStatus = stamp.OperationStatus;
			this._accountPermissions = new AccountPermissions?(AccountPermissions.Full);
			this.Timeout = this.DEFAULTTIMEOUT;
		}

		private IEnumerator<IAsyncResult> CreateQueueContainerImpl(string containerName, DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncIteratorContext<IQueueContainer> context)
		{
			IStringDataEventStream verboseDebug = Logger<INormalAndDebugLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { containerName, expiryTime, serviceMetadata, applicationMetadata, this.Timeout };
			verboseDebug.Log("CreateQueueContainerImpl({0},{1},{2},{3},{4})", objArray);
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<DbQueueContainer>((TimeSpan param0) => {
				Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = new Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer()
				{
					AccountName = this._account.Name,
					QueueName = containerName,
					ServiceMetadata = serviceMetadata,
					Metadata = applicationMetadata
				};
				DbQueueContainer dbQueueContainer = new DbQueueContainer(this, queueContainer);
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					dbContext.QueueContainers.InsertOnSubmit(queueContainer);
					dbContext.SubmitChanges();
				}
				return dbQueueContainer;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("StorageAccount.CreateQueueContainer"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<DbQueueContainer>(asyncResult);
		}

		private IEnumerator<IAsyncResult> DeleteQueueContainerImpl(string containerName, IContainerCondition conditions, AsyncIteratorContext<NoResults> context)
		{
			IStringDataEventStream verboseDebug = Logger<INormalAndDebugLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { containerName, this.Timeout };
			verboseDebug.Log("DeleteQueueContainerImpl({0},{1})", objArray);
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = new Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer()
					{
						AccountName = this._account.Name,
						QueueName = containerName
					};
					queueContainer = DbQueueContainer.LoadQueueContainer(dbContext, queueContainer);
					DbQueueContainer.CheckQueueContainerCondition(queueContainer, conditions);
					dbContext.QueueContainers.DeleteOnSubmit(queueContainer);
					dbContext.SubmitChanges();
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("StorageAccount.DeleteBlobContainerImpl"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
		}

		internal static string FixTildeInPrefix(string prefix)
		{
			if (Environment.Version.Major >= 4 || prefix.IndexOfAny(DbStorageAccount.s_specialCharactersForLike) >= 0)
			{
				return prefix;
			}
			return prefix.Replace("~", "~~");
		}

		private IEnumerator<IAsyncResult> GetPropertiesImpl(AccountPropertyNames propertyNames, IAccountCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IStringDataEventStream verboseDebug = Logger<INormalAndDebugLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { propertyNames, condition, this.Timeout };
			verboseDebug.Log("StorageAccount.GetPropertiesImpl({0},{1},{2})", objArray);
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					StorageStampHelpers.CheckAccountName(this._account.Name);
					Account account = this.LoadAccount(dbContext);
					this._account = account;
					if (account.SecondaryReadEnabled)
					{
						if (this._accountServiceMetadata == null)
						{
							this._accountServiceMetadata = new AccountServiceMetadata();
						}
						this._accountServiceMetadata.SecondaryReadEnabled = new bool?(true);
					}
					if (propertyNames.ServiceMetadataPropertyNames.HasFlag((AccountServiceMetadataPropertyNames)((long)32768)))
					{
						if (this._accountServiceMetadata == null)
						{
							this._accountServiceMetadata = new AccountServiceMetadata();
						}
						if (account.BlobServiceSettings == null)
						{
							this._accountServiceMetadata.BlobAnalyticsSettings = new AnalyticsSettings();
						}
						else
						{
							this._accountServiceMetadata.BlobAnalyticsSettings = ServiceSettingsSerializer.DeSerialize(account.BlobServiceSettings);
						}
						this._accountServiceMetadata.BlobGeoReplicationStats = this.LoadGeoReplicationStats(dbContext);
					}
					if (propertyNames.ServiceMetadataPropertyNames.HasFlag((AccountServiceMetadataPropertyNames)((long)65536)))
					{
						if (this._accountServiceMetadata == null)
						{
							this._accountServiceMetadata = new AccountServiceMetadata();
						}
						if (account.QueueServiceSettings == null)
						{
							this._accountServiceMetadata.QueueAnalyticsSettings = new AnalyticsSettings();
						}
						else
						{
							this._accountServiceMetadata.QueueAnalyticsSettings = ServiceSettingsSerializer.DeSerialize(account.QueueServiceSettings);
						}
						this._accountServiceMetadata.QueueGeoReplicationStats = this.LoadGeoReplicationStats(dbContext);
					}
					if (propertyNames.ServiceMetadataPropertyNames.HasFlag((AccountServiceMetadataPropertyNames)((long)131072)))
					{
						if (this._accountServiceMetadata == null)
						{
							this._accountServiceMetadata = new AccountServiceMetadata();
						}
						if (account.TableServiceSettings == null)
						{
							this._accountServiceMetadata.TableAnalyticsSettings = new AnalyticsSettings();
						}
						else
						{
							this._accountServiceMetadata.TableAnalyticsSettings = ServiceSettingsSerializer.DeSerialize(account.TableServiceSettings);
						}
						this._accountServiceMetadata.TableGeoReplicationStats = this.LoadGeoReplicationStats(dbContext);
					}
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("StorageAccount.GetPropertiesImpl"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> ListBlobContainersImpl(string containerNamePrefix, ContainerPropertyNames propertyNames, string separator, string containerKeyStart, IContainerCondition condition, int maxContainerNames, AsyncIteratorContext<IBlobContainerCollection> context)
		{
			IStringDataEventStream verboseDebug = Logger<INormalAndDebugLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { containerNamePrefix, propertyNames, separator, containerKeyStart, condition, maxContainerNames, this.Timeout };
			verboseDebug.Log("ListBlobContainersImpl({0},{1},{2},{3},{4},{5},{6})", objArray);
			string str = null;
			if (containerKeyStart != null)
			{
				string[] strArrays = containerKeyStart.Split(new char[] { '/' });
				if ((int)strArrays.Length != 3)
				{
					throw new ArgumentOutOfRangeException("containerKeyStart", "The marker is not well formed.");
				}
				string str1 = strArrays[1];
				str = strArrays[2];
				if (str1 != this.Name)
				{
					throw new ArgumentException(string.Format("Account name '{0}' specified in the container key start does not match the account being listed '{1}'", str1, this.Name), "containerKeyStart");
				}
			}
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<DbBlobContainerCollection>((TimeSpan param0) => {
				DbBlobContainerCollection dbBlobContainerCollections;
				StorageStampHelpers.ValidateMaxContainers(maxContainerNames);
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					IQueryable<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer> blobContainers = this.MakeListBlobContainersQuery(containerNamePrefix, separator, str, condition, maxContainerNames, dbContext);
					dbBlobContainerCollections = this.ReadListBlobContainersResult(containerNamePrefix, separator, maxContainerNames, blobContainers);
				}
				return dbBlobContainerCollections;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("StorageStamp.ListBlobContainersImpl"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<DbBlobContainerCollection>(asyncResult);
		}

		private IEnumerator<IAsyncResult> ListQueueContainersImpl(string containerNamePrefix, ContainerPropertyNames propertyNames, string separator, string containerNameStart, IContainerCondition condition, int maxQueueNames, AsyncIteratorContext<IQueueContainerCollection> context)
		{
			IStringDataEventStream verboseDebug = Logger<INormalAndDebugLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { containerNamePrefix, propertyNames, separator, containerNameStart, condition, maxQueueNames, this.Timeout };
			verboseDebug.Log("ListQueueContainersImpl({0},{1},{2},{3},{4},{5},{6})", objArray);
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<DbQueueContainerCollection>((TimeSpan param0) => {
				DbQueueContainerCollection dbQueueContainerCollections;
				StorageStampHelpers.ValidateMaxContainers(maxQueueNames);
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					IQueryable<Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer> queueContainers = this.MakeListQueueContainersQuery(containerNamePrefix, separator, containerNameStart, condition, maxQueueNames, dbContext);
					dbQueueContainerCollections = this.ReadListQueueContainersResult(containerNamePrefix, separator, maxQueueNames, queueContainers);
				}
				return dbQueueContainerCollections;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("StorageStamp.ListQueueContainersImpl"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<DbQueueContainerCollection>(asyncResult);
		}

		private Account LoadAccount(DevelopmentStorageDbDataContext dataContext)
		{
			Account account = (
				from a in dataContext.Accounts
				where a.Name == this._account.Name
				select a).FirstOrDefault<Account>();
			if (account == null)
			{
				throw new AccountNotFoundException();
			}
			return account;
		}

		private GeoReplicationStats LoadGeoReplicationStats(DevelopmentStorageDbDataContext dbContext)
		{
			DateTime dbUTCDate = dbContext.GetDbUTCDate();
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref dbUTCDate);
			GeoReplicationStats geoReplicationStat = new GeoReplicationStats()
			{
				LastSyncTime = new DateTime?(dbUTCDate),
				Status = new GeoReplicationStatus?(GeoReplicationStatus.Live)
			};
			return geoReplicationStat;
		}

		private IQueryable<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer> MakeListBlobContainersQuery(string containerNamePrefix, string separator, string containerNameStart, IContainerCondition condition, int maxContainerNames, DevelopmentStorageDbDataContext dataContext)
		{
			IQueryable<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer> blobContainers = 
				from c in dataContext.BlobContainers
				where c.AccountName == this._account.Name
				select c;
			if (!string.IsNullOrEmpty(separator))
			{
				blobContainers = 
					from c in dataContext.GetSummaryBlobContainers(this._account.Name, containerNamePrefix, separator)
					select c;
			}
			else if (!string.IsNullOrEmpty(containerNamePrefix))
			{
				blobContainers = 
					from c in blobContainers
					where c.ContainerName.StartsWith(DbStorageAccount.FixTildeInPrefix(containerNamePrefix))
					select c;
			}
			if (!string.IsNullOrEmpty(containerNameStart))
			{
				blobContainers = 
					from c in blobContainers
					where c.ContainerName.CompareTo(containerNameStart) >= 0
					select c;
			}
			if (condition != null)
			{
				if (condition.IfModifiedSinceTime.HasValue)
				{
					blobContainers = 
						from c in blobContainers
						where c.LastModificationTime > condition.IfModifiedSinceTime.Value
						select c;
				}
				if (condition.IfNotModifiedSinceTime.HasValue)
				{
					blobContainers = 
						from c in blobContainers
						where c.LastModificationTime <= condition.IfNotModifiedSinceTime.Value
						select c;
				}
			}
			if (maxContainerNames != 0)
			{
				blobContainers = blobContainers.Take<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer>(maxContainerNames + 1);
			}
			return blobContainers;
		}

		private IQueryable<Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer> MakeListQueueContainersQuery(string containerNamePrefix, string separator, string containerNameStart, IContainerCondition condition, int maxQueueNames, DevelopmentStorageDbDataContext dataContext)
		{
			IQueryable<Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer> queueContainers = 
				from c in dataContext.QueueContainers
				where c.AccountName == this._account.Name
				select c;
			if (!string.IsNullOrEmpty(separator))
			{
				queueContainers = 
					from c in dataContext.GetSummaryQueueContainers(this._account.Name, containerNamePrefix, separator)
					select c;
			}
			else if (!string.IsNullOrEmpty(containerNamePrefix))
			{
				queueContainers = 
					from c in queueContainers
					where c.QueueName.StartsWith(DbStorageAccount.FixTildeInPrefix(containerNamePrefix))
					select c;
			}
			if (!string.IsNullOrEmpty(containerNameStart))
			{
				queueContainers = 
					from c in queueContainers
					where c.QueueName.CompareTo(containerNameStart) >= 0
					select c;
			}
			if (condition != null)
			{
				if (condition.IfModifiedSinceTime.HasValue)
				{
					queueContainers = 
						from c in queueContainers
						where c.LastModificationTime > condition.IfModifiedSinceTime.Value
						select c;
				}
				if (condition.IfNotModifiedSinceTime.HasValue)
				{
					queueContainers = 
						from c in queueContainers
						where c.LastModificationTime <= condition.IfNotModifiedSinceTime.Value
						select c;
				}
			}
			if (maxQueueNames != 0)
			{
				queueContainers = queueContainers.Take<Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer>(maxQueueNames + 1);
			}
			return queueContainers;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.BeginCreateQueueContainer(string queueName, DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IQueueContainer> asyncIteratorContext = new AsyncIteratorContext<IQueueContainer>("StorageAccount.CreateQueueContainer", callback, state);
			asyncIteratorContext.Begin(this.CreateQueueContainerImpl(queueName, expiryTime, serviceMetadata, applicationMetadata, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.BeginCreateTableContainer(string tableName, DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncCallback callback, object state)
		{
			throw new NotImplementedException();
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.BeginDeleteQueueContainer(string queueName, IContainerCondition conditions, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("StorageAccount.DeleteQueueContainer", callback, state);
			asyncIteratorContext.Begin(this.DeleteQueueContainerImpl(queueName, conditions, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.BeginDeleteTableContainer(string tableName, IContainerCondition conditions, AsyncCallback callback, object state)
		{
			throw new NotImplementedException();
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.BeginGetProperties(AccountPropertyNames propertyNames, IAccountCondition conditions, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("StorageAccount.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, conditions, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.BeginListBlobContainers(string containerName, ContainerPropertyNames propertyNames, string separator, string containerKeyStart, IContainerCondition condition, int maxContainerNames, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IBlobContainerCollection> asyncIteratorContext = new AsyncIteratorContext<IBlobContainerCollection>("StorageAccount.ListBlobContainers", callback, state);
			asyncIteratorContext.Begin(this.ListBlobContainersImpl(containerName, propertyNames, separator, containerKeyStart, condition, maxContainerNames, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.BeginListQueueContainers(string containerName, ContainerPropertyNames propertyNames, string separator, string containerNameStart, IContainerCondition condition, int maxContainerNames, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IQueueContainerCollection> asyncIteratorContext = new AsyncIteratorContext<IQueueContainerCollection>("StorageAccount.ListQueueContainers", callback, state);
			asyncIteratorContext.Begin(this.ListQueueContainersImpl(containerName, propertyNames, separator, containerNameStart, condition, maxContainerNames, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.BeginListTableContainers(string containerName, ContainerPropertyNames propertyNames, string separator, string containerNameStart, IContainerCondition condition, int maxContainerNames, AsyncCallback callback, object state)
		{
			throw new NotSupportedException();
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.BeginSetProperties(AccountPropertyNames propertyNames, IAccountCondition conditions, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("StorageAccount.SetProperties", callback, state);
			asyncIteratorContext.Begin(this.SetPropertiesImpl(propertyNames, conditions, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IBlobContainer Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.CreateBlobContainerInstance(string containerName)
		{
			return new DbBlobContainer(this, containerName);
		}

		IQueueContainer Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.CreateQueueContainerInstance(string queueName)
		{
			return new DbQueueContainer(this, queueName);
		}

		ITableContainer Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.CreateTableContainerInstance(string tableName)
		{
			return new DbTableContainer(this, tableName);
		}

		IQueueContainer Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.EndCreateQueueContainer(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<IQueueContainer> asyncIteratorContext = (AsyncIteratorContext<IQueueContainer>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		ITableContainer Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.EndCreateTableContainer(IAsyncResult ar)
		{
			throw new NotImplementedException();
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.EndDeleteQueueContainer(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.EndDeleteTableContainer(IAsyncResult ar)
		{
			throw new NotImplementedException();
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.EndGetProperties(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		IBlobContainerCollection Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.EndListBlobContainers(IAsyncResult asyncResult)
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

		IQueueContainerCollection Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.EndListQueueContainers(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<IQueueContainerCollection> asyncIteratorContext = (AsyncIteratorContext<IQueueContainerCollection>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		ITableContainerCollection Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.EndListTableContainers(IAsyncResult ar)
		{
			throw new NotSupportedException();
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IStorageAccount.EndSetProperties(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private void PreserveUnchangedAnalyticsSettings(AnalyticsSettings newSettings, AnalyticsSettings oldSettings)
		{
			if (newSettings != null && oldSettings != null)
			{
				if (!newSettings.MinuteMetricsElementRead)
				{
					newSettings.MinuteMetricsType = oldSettings.MinuteMetricsType;
					newSettings.MinuteMetricsVersion = oldSettings.MinuteMetricsVersion;
					newSettings.IsMinuteMetricsRetentionPolicyEnabled = oldSettings.IsMinuteMetricsRetentionPolicyEnabled;
					newSettings.MinuteMetricsRetentionInDays = oldSettings.MinuteMetricsRetentionInDays;
				}
				if (!newSettings.CorsRulesElementRead)
				{
					newSettings.CorsRulesSerializedString = oldSettings.CorsRulesSerializedString;
				}
				if (!newSettings.IsAnalyticsVersionAtLeastV3)
				{
					return;
				}
				if (!newSettings.MetricsElementRead)
				{
					newSettings.MetricsType = oldSettings.MetricsType;
					newSettings.MetricsVersion = oldSettings.MetricsVersion;
					newSettings.IsMetricsRetentionPolicyEnabled = oldSettings.IsMetricsRetentionPolicyEnabled;
					newSettings.MetricsRetentionInDays = oldSettings.MetricsRetentionInDays;
				}
				if (!newSettings.LoggingElementRead)
				{
					newSettings.LogType = oldSettings.LogType;
					newSettings.IsLogRetentionPolicyEnabled = oldSettings.IsLogRetentionPolicyEnabled;
					newSettings.LogVersion = oldSettings.LogVersion;
					newSettings.LogRetentionInDays = oldSettings.LogRetentionInDays;
				}
				if (!newSettings.DefaultServiceVersionElementRead)
				{
					newSettings.DefaultRESTVersion = oldSettings.DefaultRESTVersion;
				}
			}
		}

		private DbBlobContainerCollection ReadListBlobContainersResult(string containerNamePrefix, string separator, int maxContainerNames, IQueryable<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer> containers)
		{
			int num = 0;
			string str = null;
			List<IBlobContainer> blobContainers = new List<IBlobContainer>();
			foreach (Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer container in containers)
			{
				if (!string.IsNullOrEmpty(separator))
				{
					container.ContainerName = DbBlobContainer.GetSummaryName(container.ContainerName, containerNamePrefix, separator);
				}
				if (maxContainerNames != 0)
				{
					num++;
					if (num > maxContainerNames)
					{
						str = string.Concat("/", this.Name, "/", container.ContainerName);
						break;
					}
				}
				blobContainers.Add(new DbBlobContainer(this, container, true));
			}
			return new DbBlobContainerCollection(blobContainers, str);
		}

		private DbQueueContainerCollection ReadListQueueContainersResult(string containerNamePrefix, string separator, int maxQueueNames, IQueryable<Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer> queues)
		{
			int num = 0;
			string queueName = null;
			List<IQueueContainer> queueContainers = new List<IQueueContainer>();
			foreach (Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queue in queues)
			{
				if (!string.IsNullOrEmpty(separator))
				{
					queue.QueueName = DbBlobContainer.GetSummaryName(queue.QueueName, containerNamePrefix, separator);
				}
				if (maxQueueNames != 0)
				{
					num++;
					if (num > maxQueueNames)
					{
						queueName = queue.QueueName;
						break;
					}
				}
				queueContainers.Add(new DbQueueContainer(this, queue));
			}
			return new DbQueueContainerCollection(queueContainers, queueName);
		}

		private IEnumerator<IAsyncResult> SetPropertiesImpl(AccountPropertyNames propertyNames, IAccountCondition conditions, AsyncIteratorContext<NoResults> context)
		{
			IStringDataEventStream infoDebug = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
			object[] objArray = new object[] { propertyNames, conditions, this.Timeout };
			infoDebug.Log("SetPropertiesImpl({0},{1},{2})", objArray);
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					StorageStampHelpers.CheckAccountName(this._account.Name);
					Account value = this.LoadAccount(dbContext);
					this._account = value;
					AnalyticsSettings analyticsSetting = null;
					AnalyticsSettings blobAnalyticsSettings = null;
					AccountServiceMetadataPropertyNames serviceMetadataPropertyNames = propertyNames.ServiceMetadataPropertyNames;
					if (serviceMetadataPropertyNames <= AccountServiceMetadataPropertyNames.QueueAnalyticsSettings)
					{
						if (serviceMetadataPropertyNames == AccountServiceMetadataPropertyNames.BlobAnalyticsSettings)
						{
							blobAnalyticsSettings = this.ServiceMetadata.BlobAnalyticsSettings;
							if (blobAnalyticsSettings != null)
							{
								if (value.BlobServiceSettings != null)
								{
									analyticsSetting = ServiceSettingsSerializer.DeSerialize(value.BlobServiceSettings);
									this.PreserveUnchangedAnalyticsSettings(blobAnalyticsSettings, analyticsSetting);
								}
								value.BlobServiceSettings = ServiceSettingsSerializer.Serialize(blobAnalyticsSettings);
								dbContext.SubmitChanges();
							}
						}
						else if (serviceMetadataPropertyNames == AccountServiceMetadataPropertyNames.QueueAnalyticsSettings)
						{
							blobAnalyticsSettings = this.ServiceMetadata.QueueAnalyticsSettings;
							if (blobAnalyticsSettings != null)
							{
								if (value.QueueServiceSettings != null)
								{
									analyticsSetting = ServiceSettingsSerializer.DeSerialize(value.QueueServiceSettings);
									this.PreserveUnchangedAnalyticsSettings(blobAnalyticsSettings, analyticsSetting);
								}
								value.QueueServiceSettings = ServiceSettingsSerializer.Serialize(blobAnalyticsSettings);
								dbContext.SubmitChanges();
							}
						}
					}
					else if (serviceMetadataPropertyNames == AccountServiceMetadataPropertyNames.TableAnalyticsSettings)
					{
						blobAnalyticsSettings = this.ServiceMetadata.TableAnalyticsSettings;
						if (blobAnalyticsSettings != null)
						{
							if (value.TableServiceSettings != null)
							{
								analyticsSetting = ServiceSettingsSerializer.DeSerialize(value.TableServiceSettings);
								this.PreserveUnchangedAnalyticsSettings(blobAnalyticsSettings, analyticsSetting);
							}
							value.TableServiceSettings = ServiceSettingsSerializer.Serialize(blobAnalyticsSettings);
							dbContext.SubmitChanges();
						}
					}
					else if (serviceMetadataPropertyNames == AccountServiceMetadataPropertyNames.SecondaryReadEnabled)
					{
						bool? secondaryReadEnabled = this.ServiceMetadata.SecondaryReadEnabled;
						if (secondaryReadEnabled.HasValue)
						{
							value.SecondaryReadEnabled = secondaryReadEnabled.Value;
							dbContext.SubmitChanges();
						}
					}
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("StorageAccount.SetPropertiesImpl"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}
	}
}