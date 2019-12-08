using AsyncHelper;
using AsyncHelper.Streams;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Service;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using Microsoft.Cis.Services.Nephos.Table.Service.DataModel;
using Microsoft.Cis.Services.Nephos.Table.Service.TableManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class TableManager : BaseServiceManager, ITableManager, IDisposable
	{
		private const int MaxRequestBodySize = 4194304;

		private const int ContentIdSearchSize = 102400;

		private const int MaxContentIdValueLength = 100;

		private IUtilityTableDataContextFactory utilityTableDataContextFactory;

		public Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		public TableManager(AuthorizationManager authorizationManager, IUtilityTableDataContextFactory tableDataFactory, IStorageManager storageManager)
		{
			this.authorizationManager = authorizationManager;
			this.utilityTableDataContextFactory = tableDataFactory;
			this.storageManager = storageManager;
			Logger<IRestProtocolHeadLogger>.Instance.Info.Log("NOT using custom threadpool for blocking calls");
		}

		public IAsyncResult BeginGetTableAcl(IAccountIdentifier identifier, string account, string tableName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ContainerAclSettings> asyncIteratorContext = new AsyncIteratorContext<ContainerAclSettings>("TableManager.GetTableAcl", callback, state);
			asyncIteratorContext.Begin(this.GetTableAclImpl(identifier, account, tableName, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetTableServiceProperties(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<AnalyticsSettings> asyncIteratorContext = new AsyncIteratorContext<AnalyticsSettings>("TableManager.GetTableServiceProperties", callback, state);
			asyncIteratorContext.Begin(this.GetTableServicePropertiesImpl(identifier, ownerAccountName, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetTableServiceStats(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<GeoReplicationStats> asyncIteratorContext = new AsyncIteratorContext<GeoReplicationStats>("TableManager.GetTableServiceProperties", callback, state);
			asyncIteratorContext.Begin(this.GetTableServiceStatsImpl(identifier, ownerAccountName, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginPerformOperation(IAccountIdentifier accountId, string accountName, IDataServiceHost host, RequestStartedCallback requestStartedCallback, CheckPermissionDelegate checkPermissionCallback, QueryRowCommandPropertiesAvailableCallback queryRowCommandPropertiesAvailableCallback, Dictionary<string, string> continuationToken, ContinuationTokenAvailableCallback continuationTokenAvailableCallback, bool operationIsConditional, RequestContext requestContext, AsyncCallback callback, object state)
		{
			TableProtocolHead tableProtocolHead = host as TableProtocolHead;
			NephosAssertionException.Assert(tableProtocolHead != null, "Expecting host to be instance of TableProtocolHead.");
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("TableManager.PerformOperation", callback, state);
			asyncIteratorContext.Begin(this.PerformOperationImpl(accountId, accountName, tableProtocolHead, requestStartedCallback, checkPermissionCallback, queryRowCommandPropertiesAvailableCallback, continuationToken, continuationTokenAvailableCallback, operationIsConditional, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetTableAcl(IAccountIdentifier identifier, string account, string tableName, ContainerAclSettings acl, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("TableManager.SetTableAcl", callback, state);
			asyncIteratorContext.Begin(this.SetTableAclImpl(identifier, account, tableName, acl, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetTableServiceProperties(IAccountIdentifier identifier, string ownerAccountName, AnalyticsSettings settings, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("TableManager.SetTableServiceProperties", callback, state);
			asyncIteratorContext.Begin(this.SetTableServicePropertiesImpl(identifier, ownerAccountName, settings, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public static Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest.TableManager CreateTableManager(AuthorizationManager authorizationManager, IUtilityTableDataContextFactory tableDataFactory, IStorageManager storageManager)
		{
			return new Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest.TableManager(authorizationManager, tableDataFactory, storageManager);
		}

		private void DispatchRequestToAstoria(IAccountIdentifier identifier, TableProtocolHead tableProtocolHead, RequestStartedCallback requestStartedCallback, CheckPermissionDelegate checkPermissionCallback, QueryRowCommandPropertiesAvailableCallback queryRowCommandPropertiesAvailableCallback, Dictionary<string, string> continuationToken, ContinuationTokenAvailableCallback continuationTokenAvailableCallback, bool operationIsConditional)
		{
			Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Deferring request to Astoria.");
			TableDataServiceBase<IUtilityTableDataContext> tableDataServiceV3 = null;
			if (tableProtocolHead.IsRequestVersionAtLeast("2013-08-15"))
			{
				tableDataServiceV3 = new TableDataServiceV3<IUtilityTableDataContext>(tableProtocolHead, identifier, this.utilityTableDataContextFactory, operationIsConditional, continuationToken, requestStartedCallback, checkPermissionCallback, queryRowCommandPropertiesAvailableCallback, continuationTokenAvailableCallback);
			}
			else if (!tableProtocolHead.IsRequestVersionAtLeast("2011-08-18"))
			{
				tableDataServiceV3 = new TableDataServiceV1<IUtilityTableDataContext>(tableProtocolHead, identifier, this.utilityTableDataContextFactory, operationIsConditional, continuationToken, requestStartedCallback, checkPermissionCallback, queryRowCommandPropertiesAvailableCallback, continuationTokenAvailableCallback);
			}
			else
			{
				tableDataServiceV3 = new TableDataServiceV2<IUtilityTableDataContext>(tableProtocolHead, identifier, this.utilityTableDataContextFactory, operationIsConditional, continuationToken, requestStartedCallback, checkPermissionCallback, queryRowCommandPropertiesAvailableCallback, continuationTokenAvailableCallback);
			}
			tableDataServiceV3.AttachHost(tableProtocolHead);
			tableDataServiceV3.ProcessRequest();
			Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Astoria finished processing request.");
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public ContainerAclSettings EndGetTableAcl(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<ContainerAclSettings> asyncIteratorContext = (AsyncIteratorContext<ContainerAclSettings>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public AnalyticsSettings EndGetTableServiceProperties(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<AnalyticsSettings> asyncIteratorContext = (AsyncIteratorContext<AnalyticsSettings>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public GeoReplicationStats EndGetTableServiceStats(IAsyncResult ar)
		{
			return ar.End<GeoReplicationStats>(RethrowableWrapperBehavior.NoWrap);
		}

		public void EndPerformOperation(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndSetTableAcl(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndSetTableServiceProperties(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private string GetResourceString(string account)
		{
			string[] strArrays = new string[] { "", account };
			return string.Join("/", strArrays);
		}

		protected IEnumerator<IAsyncResult> GetTableAclImpl(IAccountIdentifier identifier, string account, string tableName, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ContainerAclSettings> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(tableName))
			{
				throw new ArgumentException("tableName", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetTableAcl");
			}
			if (identifier is TableSignedAccessAccountIdentifier || identifier is AccountSasAccessIdentifier)
			{
				throw new NephosUnauthorizedAccessException("Signed access not supported for this request", AuthorizationFailureReason.InvalidOperationSAS);
			}
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, tableName, null, PermissionLevel.ReadAcl, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("TableManager.GetTableAclImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			string str = account;
			string str1 = tableName;
			using (IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(str))
			{
				using (ITableContainer tableContainer = storageAccount.CreateTableContainerInstance(str1))
				{
					ContainerPropertyNames containerPropertyName = ContainerPropertyNames.LastModificationTime | ContainerPropertyNames.ServiceMetadata;
					tableContainer.Timeout = startingNow.Remaining(timeout);
					asyncResult = tableContainer.BeginGetProperties(containerPropertyName, null, CacheRefreshOptions.SkipAllCache, context.GetResumeCallback(), context.GetResumeState("TableManager.GetTableAclImpl"));
					yield return asyncResult;
					tableContainer.EndGetProperties(asyncResult);
					try
					{
						context.ResultData = new ContainerAclSettings(tableContainer.ServiceMetadata);
					}
					catch (MetadataFormatException metadataFormatException1)
					{
						MetadataFormatException metadataFormatException = metadataFormatException1;
						throw new NephosStorageDataCorruptionException(string.Format("Error decoding Acl setting for {0}", RealServiceManager.GetResourceString(account, tableName)), metadataFormatException);
					}
				}
			}
		}

		private IEnumerator<IAsyncResult> GetTableServicePropertiesImpl(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<AnalyticsSettings> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(ownerAccountName))
			{
				throw new ArgumentException("ownerAccountName", "Cannot be null");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetTableServiceProperties");
			}
			if (identifier is TableSignedAccessAccountIdentifier)
			{
				throw new NephosUnauthorizedAccessException("Signed access not supported for this request", AuthorizationFailureReason.InvalidOperationSAS);
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, ownerAccountName, null, null, PermissionLevel.Read | PermissionLevel.Owner, sASAuthorizationParameter1, timeout, context.GetResumeCallback(), context.GetResumeState("TableManager.GetTableServicePropertiesImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(ownerAccountName);
			storageAccount.Timeout = timeout;
			AccountCondition accountCondition = new AccountCondition(false, false, storageAccount.LastModificationTime, null);
			asyncResult = storageAccount.BeginGetProperties(new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)131072)), accountCondition, context.GetResumeCallback(), context.GetResumeState("TableManager.GetTableServicePropertiesImpl"));
			yield return asyncResult;
			storageAccount.EndGetProperties(asyncResult);
			context.ResultData = storageAccount.ServiceMetadata.TableAnalyticsSettings;
		}

		private IEnumerator<IAsyncResult> GetTableServiceStatsImpl(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<GeoReplicationStats> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(ownerAccountName))
			{
				throw new ArgumentException("ownerAccountName", "Cannot be null");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetTableServiceStats");
			}
			if (identifier is TableSignedAccessAccountIdentifier)
			{
				throw new NephosUnauthorizedAccessException("Signed access not supported for this request", AuthorizationFailureReason.InvalidOperationSAS);
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, ownerAccountName, null, null, PermissionLevel.Read | PermissionLevel.Owner, sASAuthorizationParameter1, timeout, context.GetResumeCallback(), context.GetResumeState("TableManager.GetTableServiceStatsImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(ownerAccountName);
			storageAccount.Timeout = timeout;
			AccountCondition accountCondition = new AccountCondition(false, false, storageAccount.LastModificationTime, null);
			asyncResult = storageAccount.BeginGetProperties(new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)536870912)), accountCondition, context.GetResumeCallback(), context.GetResumeState("TableManager.GetTableServicePropertiesImpl"));
			yield return asyncResult;
			storageAccount.EndGetProperties(asyncResult);
			context.ResultData = storageAccount.ServiceMetadata.TableGeoReplicationStats;
		}

		public void Initialize()
		{
		}

		private IEnumerator<IAsyncResult> PerformOperationImpl(IAccountIdentifier identifier, string accountName, TableProtocolHead tableProtocolHead, RequestStartedCallback requestStartedCallback, CheckPermissionDelegate checkPermissionCallback, QueryRowCommandPropertiesAvailableCallback queryRowCommandPropertiesAvailableCallback, Dictionary<string, string> continuationToken, ContinuationTokenAvailableCallback continuationTokenAvailableCallback, bool operationIsConditional, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			TimeSpan maxValue = TimeSpan.MaxValue;
			if (VersioningHelper.CompareVersions(tableProtocolHead.RequestRestVersion, "2016-05-31") >= 0)
			{
				maxValue = tableProtocolHead.operationContext.RemainingTimeout();
			}
			IAsyncResult workerThread = this.authorizationManager.BeginCheckAccess(identifier, accountName, null, null, PermissionLevel.Owner, maxValue, context.GetResumeCallback(), context.GetResumeState("AuthorizationManager.BeginCheckAccess"));
			yield return workerThread;
			this.authorizationManager.EndCheckAccess(workerThread);
			using (Stream bufferPoolMemoryStream = new BufferPoolMemoryStream(65536))
			{
				bool flag = false;
				if (tableProtocolHead.ShouldReadRequestBody)
				{
					long requestContentLength = tableProtocolHead.RequestContentLength;
					if (requestContentLength > (long)4194304)
					{
						IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
						object[] objArray = new object[] { requestContentLength, 4194304 };
						verbose.Log("Content-Length is out of range. Content-Length={0} MaxRequestBodySize={1}", objArray);
						if (!tableProtocolHead.IsBatchRequest())
						{
							if (VersioningHelper.CompareVersions(tableProtocolHead.RequestRestVersion, "2013-08-15") < 0)
							{
								throw new RequestTooLargeException();
							}
							throw new RequestEntityTooLargeException(new long?((long)4194304));
						}
						flag = true;
						requestContentLength = (long)102400;
					}
					else if (requestContentLength == (long)-1)
					{
						requestContentLength = (long)4194305;
					}
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Started asynchronously reading from input stream");
					if (VersioningHelper.CompareVersions(tableProtocolHead.RequestRestVersion, "2016-05-31") >= 0)
					{
						maxValue = tableProtocolHead.operationContext.RemainingTimeout();
					}
					workerThread = AsyncStreamCopy.BeginAsyncStreamCopy(tableProtocolHead.RequestStream, bufferPoolMemoryStream, requestContentLength, 65536, maxValue, context.GetResumeCallback(), context.GetResumeState("BeginAsyncStreamCopy"));
					yield return workerThread;
					long num = AsyncStreamCopy.EndAsyncStreamCopy(workerThread);
					bufferPoolMemoryStream.Seek((long)0, SeekOrigin.Begin);
					tableProtocolHead.RequestStream = bufferPoolMemoryStream;
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Finished reading from input stream");
					workerThread = AsyncHelpers.BeginSwitchToWorkerThread(context.GetResumeCallback(), context.GetResumeState("PerformOperationImpl"));
					yield return workerThread;
					AsyncHelpers.EndSwitchToWorkerThread(workerThread);
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Switched to a WorkerThread");
					if (num > (long)4194304)
					{
						IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
						object[] requestContentLength1 = new object[] { tableProtocolHead.RequestContentLength, 4194304, num };
						stringDataEventStream.Log("Request body is too long. Content-Length={0} MaxRequestBodySize={1} BytesRead={2}", requestContentLength1);
						if (!tableProtocolHead.IsBatchRequest())
						{
							if (VersioningHelper.CompareVersions(tableProtocolHead.RequestRestVersion, "2013-08-15") < 0)
							{
								throw new RequestTooLargeException();
							}
							throw new RequestEntityTooLargeException(new long?((long)4194304));
						}
						flag = true;
					}
				}
				if (!flag)
				{
					this.DispatchRequestToAstoria(identifier, tableProtocolHead, requestStartedCallback, checkPermissionCallback, queryRowCommandPropertiesAvailableCallback, continuationToken, continuationTokenAvailableCallback, operationIsConditional);
				}
				else
				{
					Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Batch request body is too long. Sending error batch response now.");
					if (VersioningHelper.CompareVersions(tableProtocolHead.RequestRestVersion, "2013-08-15") >= 0)
					{
						throw new RequestEntityTooLargeException(new long?((long)4194304));
					}
					string str = tableProtocolHead.ReadContentIdFromRequest(100, 102400);
					TableProtocolHead tableProtocolHead1 = tableProtocolHead;
					string str1 = str;
					if (str1 == null)
					{
						str1 = "1";
					}
					tableProtocolHead1.SendBatchRequestTooLargeResponse(str1);
				}
			}
		}

		protected IEnumerator<IAsyncResult> SetTableAclImpl(IAccountIdentifier identifier, string account, string tableName, ContainerAclSettings acl, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			byte[] numArray;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(tableName))
			{
				throw new ArgumentException("tableName", "Cannot be null or empty");
			}
			if (acl == null)
			{
				throw new ArgumentNullException("acl");
			}
			if (acl.SASIdentifiers == null)
			{
				throw new ArgumentNullException("sasidentifiers");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SetTableAcl");
			}
			RemainingTime remainingTime = new RemainingTime(timeout);
			if (identifier is TableSignedAccessAccountIdentifier || identifier is AccountSasAccessIdentifier)
			{
				throw new NephosUnauthorizedAccessException("Signed access not supported for this request", AuthorizationFailureReason.InvalidOperationSAS);
			}
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, tableName, null, PermissionLevel.WriteAcl, remainingTime, context.GetResumeCallback(), context.GetResumeState("TableManager.SetTableAclImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(account))
			{
				using (ITableContainer tableContainer = storageAccount.CreateTableContainerInstance(tableName))
				{
					acl.EncodeToServiceMetadata(out numArray);
					ContainerCondition containerCondition = null;
					tableContainer.ServiceMetadata = numArray;
					tableContainer.Timeout = startingNow.Remaining(timeout);
					asyncResult = tableContainer.BeginSetProperties(ContainerPropertyNames.ServiceMetadata, containerCondition, context.GetResumeCallback(), context.GetResumeState("TableManager.SetTableAclImpl"));
					yield return asyncResult;
					tableContainer.EndSetProperties(asyncResult);
				}
			}
		}

		private IEnumerator<IAsyncResult> SetTableServicePropertiesImpl(IAccountIdentifier identifier, string ownerAccountName, AnalyticsSettings settings, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(ownerAccountName))
			{
				throw new ArgumentException("ownerAccountName", "Cannot be null");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SetTableServiceProperties");
			}
			if (identifier is TableSignedAccessAccountIdentifier)
			{
				throw new NephosUnauthorizedAccessException("Signed access not supported for this request", AuthorizationFailureReason.InvalidOperationSAS);
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, ownerAccountName, null, null, PermissionLevel.Write | PermissionLevel.Owner, sASAuthorizationParameter1, timeout, context.GetResumeCallback(), context.GetResumeState("TableManager.SetTableServicePropertiesImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount accountServiceMetadatum = null;
			AccountCondition accountCondition = new AccountCondition(false, false, null, null);
			accountServiceMetadatum = this.storageManager.CreateAccountInstance(ownerAccountName);
			accountServiceMetadatum.ServiceMetadata = new AccountServiceMetadata()
			{
				TableAnalyticsSettings = settings
			};
			accountServiceMetadatum.Timeout = timeout;
			asyncResult = accountServiceMetadatum.BeginSetProperties(new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)131072)), accountCondition, context.GetResumeCallback(), context.GetResumeState("TableManager.SetTableServicePropertiesImpl"));
			yield return asyncResult;
			accountServiceMetadatum.EndSetProperties(asyncResult);
		}

		public void Shutdown()
		{
		}
	}
}