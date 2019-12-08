using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Table.Service.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public abstract class TableDataServiceBase<T> : DataService<T>, IServiceProvider
	where T : IUtilityTableDataContext
	{
		public const int MaxBatchSize = 100;

		public bool PremiumTableAccountRequest;

		private IUtilityTableDataContext dataSource;

		private TableProtocolHead tableProtocolHead;

		public TableDataServiceBase(TableProtocolHead tableProtocolHead, IAccountIdentifier identifier, IUtilityTableDataContextFactory factory, bool operationIsConditional, Dictionary<string, string> continuationToken, RequestStartedCallback requestStartedCallback, CheckPermissionDelegate checkPermissionCallback, QueryRowCommandPropertiesAvailableCallback queryRowCommandPropertiesAvailableCallback, ContinuationTokenAvailableCallback continuationTokenAvailableCallback)
		{
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			if (tableProtocolHead == null)
			{
				throw new ArgumentNullException("tableProtocolHead");
			}
			this.dataSource = factory.CreateDataContext(identifier, tableProtocolHead.ServiceOperationContext.AccountName, tableProtocolHead.RequestRestVersion);
			this.dataSource.ApiVersion = tableProtocolHead.RequestRestVersion;
			this.dataSource.OldMetricsTableNamesDeprecated = tableProtocolHead.IsRequestVersionAtLeast("2013-08-15");
			this.dataSource.IsBatchRequest = tableProtocolHead.IsBatchRequest();
			this.dataSource.OperationIsConditional = operationIsConditional;
			this.dataSource.ContinuationToken = continuationToken;
			this.dataSource.RequestStartedCallback = requestStartedCallback;
			this.dataSource.CheckPermissionCallback = checkPermissionCallback;
			this.dataSource.QueryRowCommandPropertiesAvailableCallback = queryRowCommandPropertiesAvailableCallback;
			this.dataSource.ContinuationTokenAvailableCallback = continuationTokenAvailableCallback;
			this.tableProtocolHead = tableProtocolHead;
			if (VersioningHelper.CompareVersions(tableProtocolHead.RequestRestVersion, "2016-05-31") >= 0)
			{
				this.dataSource.Timeout = tableProtocolHead.operationContext.RemainingTimeout();
			}
		}

		protected override T CreateDataSource()
		{
			return (T)this.dataSource;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId="Microsoft.Cis.Services.Nephos.Common.NephosAssertionException.Assert(System.Boolean,System.String)")]
		public object GetService(Type serviceType)
		{
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			NephosAssertionException.Assert(this.dataSource != null, "DataSource has not been created yet, but IServiceProvider.GetService was called");
			if (!(serviceType == typeof(IDataServiceUpdateProvider)) && !(serviceType == typeof(IDataServiceQueryProvider)) && !(serviceType == typeof(IDataServiceMetadataProvider)) && !(serviceType == typeof(IUpdatable)))
			{
				return null;
			}
			return this.dataSource;
		}

		protected override void HandleException(HandleExceptionArgs args)
		{
			Exception exception = args.Exception;
			IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
			object[] logString = new object[] { exception.GetLogString() };
			error.Log("TableDataServiceHost.ProcessException. Rethrowing exception: {0}", logString);
			exception = this.tableProtocolHead.TransformExceptionInternal(exception);
			if (exception is ArgumentOutOfRangeException)
			{
				exception = new TableServiceArgumentOutOfRangeException(exception.Message, exception);
			}
			else if (exception is ArgumentException)
			{
				exception = new TableServiceArgumentException(exception.Message, exception);
			}
			else if (exception is OverflowException)
			{
				exception = new TableServiceOverflowException(exception.Message, exception);
			}
			else if (exception is UriFormatException)
			{
				exception = new TableServiceArgumentException(exception.Message, exception);
			}
			NephosErrorDetails errorDetailsForException = this.tableProtocolHead.GetErrorDetailsForException(exception);
			string userSafeErrorMessage = errorDetailsForException.UserSafeErrorMessage;
			IUtilityTableDataContext currentDataSource = (IUtilityTableDataContext)this.dataSource.CurrentDataSource;
			if (this.tableProtocolHead.IsBatchRequest() && currentDataSource.FailedCommandIndex >= 0)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] failedCommandIndex = new object[] { currentDataSource.FailedCommandIndex, userSafeErrorMessage };
				userSafeErrorMessage = string.Format(invariantCulture, "{0}:{1}", failedCommandIndex);
			}
			args.Exception = new DataServiceException((int)errorDetailsForException.StatusEntry.StatusCodeHttp, errorDetailsForException.StatusEntry.StatusId, userSafeErrorMessage, null, args.Exception);
			this.tableProtocolHead.OnErrorInAstoriaProcessing(errorDetailsForException);
		}

		public static void InitializeCommonServiceConfig(DataServiceConfiguration config)
		{
			config.MaxBatchCount = 1;
			config.MaxChangesetCount = 100;
			config.MaxExpandCount = 0;
			config.MaxExpandDepth = 0;
			config.MaxObjectCountOnInsert = 2147483647;
			config.MaxResultsPerCollection = 2147483647;
			config.EnableTypeConversion = false;
			config.DataServiceBehavior.AcceptCountRequests = false;
			config.UseVerboseErrors = TableProtocolHead.HttpProcessorConfigurationDefaultInstance.IncludeInternalDetailsInErrorResponses;
			config.SetEntitySetAccessRule("*", EntitySetRights.All);
		}

		protected override void OnStartProcessingRequest(ProcessRequestArgs args)
		{
			if (args.IsBatchOperation)
			{
				TableProtocolHead batchInnerOperationCount = this.tableProtocolHead;
				batchInnerOperationCount.BatchInnerOperationCount = batchInnerOperationCount.BatchInnerOperationCount + 1;
				if (!this.tableProtocolHead.IsRequestVersionAtLeast("2015-12-11"))
				{
					this.tableProtocolHead.EnsureSupportedRequestContentType();
					string item = args.OperationContext.RequestHeaders["Accept"];
					if (!string.IsNullOrWhiteSpace(item))
					{
						int num = item.IndexOf(',');
						if (num != -1)
						{
							item = item.Substring(0, num);
						}
						this.tableProtocolHead.EnsureSupportedMediaType(item);
					}
				}
				this.tableProtocolHead.OverrideRequestContentType = args.OperationContext.RequestHeaders["Content-Type"];
				this.tableProtocolHead.OverrideResponseContentType = args.OperationContext.RequestHeaders["Accept"];
			}
			if (this.tableProtocolHead.IsRequestVersionAtLeast("2015-12-11") && (this.tableProtocolHead.BatchInnerOperationCount > 0 || !args.OperationContext.IsBatchRequest))
			{
				this.tableProtocolHead.EnsureContentTypeHeaderValuePostJul15(args.OperationContext.RequestHeaders["Content-Type"]);
				string queryStringValue = args.OperationContext.GetQueryStringValue("$format");
				if (!this.tableProtocolHead.CheckWhetherToSkipAcceptHeaderCheckPostJul15(queryStringValue))
				{
					this.tableProtocolHead.EnsureAcceptHeaderValuePostJul15(args.OperationContext.RequestHeaders["Accept"]);
				}
			}
			this.dataSource.OnStartProcessingRequest(args);
			base.OnStartProcessingRequest(args);
		}
	}
}