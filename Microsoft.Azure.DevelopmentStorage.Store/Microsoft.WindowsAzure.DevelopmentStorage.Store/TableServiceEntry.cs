using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Table.Service.DataModel;
using Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Table.Service.TableManager;
using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[ServiceEntryClass]
	public sealed class TableServiceEntry : ServiceRequestHandler, IServiceEntry, IDisposable
	{
		private const string TableHostSuffixDomainsConfigParameterName = "UriHostSuffixes";

		private static IUtilityTableDataContextFactory SharedTableDataFactory;

		private static IStorageManager SharedStorageManager;

		private AuthenticationManager authenticationManager;

		private ITableManager tableManager;

		public override Microsoft.Cis.Services.Nephos.Common.ServiceType ServiceType
		{
			get
			{
				return Microsoft.Cis.Services.Nephos.Common.ServiceType.TableService;
			}
		}

		public override IStorageManager StorageManagerProvider
		{
			get
			{
				return TableServiceEntry.SharedStorageManager;
			}
		}

		public TableServiceEntry()
		{
		}

		public override void AcceptRequest(RequestContext requestContext, Microsoft.Cis.Services.Nephos.Common.ServiceType serviceType)
		{
			requestContext.ServiceType = serviceType;
			PerRequestStorageManager perRequestStorageManager = new PerRequestStorageManager(TableServiceEntry.SharedStorageManager, requestContext.OperationStatus);
			AuthenticationManager authenticationManager = XFETableAuthenticationManager.CreateAuthenticationManager(TableServiceEntry.SharedStorageManager);
			AuthorizationManager authorizationManager = NephosAuthorizationManager.CreateAuthorizationManager(TableServiceEntry.SharedStorageManager, false);
			ITableManager operationStatus = TableManager.CreateTableManager(authorizationManager, TableServiceEntry.SharedTableDataFactory, TableServiceEntry.SharedStorageManager);
			operationStatus.Initialize();
			operationStatus.OperationStatus = requestContext.OperationStatus;
			IProcessor processor = TableProtocolHead.Create(requestContext, perRequestStorageManager, authenticationManager, operationStatus, TableProtocolHead.HttpProcessorConfigurationDefaultInstance, new TransformExceptionDelegate(SqlExceptionManager.TransformSqlException), null);
			ServiceRequestHandler.DispatchRequest(processor);
		}

		public override void AcceptRequest(RequestContext requestContext)
		{
			this.AcceptRequest(requestContext, this.ServiceType);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public void Initialize(IServiceEntrySink sink)
		{
			if (sink == null)
			{
				throw new ArgumentNullException("sink");
			}
			Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("[Nephos.Table] { Initialize");
			TableProtocolHead.HttpProcessorConfigurationDefaultInstance = HttpProcessorConfiguration.LoadDefaultHttpProcessorConfiguration(sink, "UriHostSuffixes", null);
			TableServiceEntry.SharedStorageManager = new DbStorageManager();
			this.authenticationManager = XFETableAuthenticationManager.CreateAuthenticationManager(TableServiceEntry.SharedStorageManager);
			TableServiceEntry.SharedTableDataFactory = new DbTableDataContextFactory();
			this.tableManager = TableManager.CreateTableManager(NephosAuthorizationManager.CreateAuthorizationManager(TableServiceEntry.SharedStorageManager, false), TableServiceEntry.SharedTableDataFactory, TableServiceEntry.SharedStorageManager);
			sink.RegisterRestHandler(this);
		}

		public void Start()
		{
			TableServiceEntry.SharedStorageManager.Initialize();
			this.tableManager.Initialize();
		}

		public void Stop()
		{
			TableServiceEntry.SharedStorageManager.Shutdown();
			TableServiceEntry.SharedStorageManager.Dispose();
			TableServiceEntry.SharedStorageManager = null;
			this.tableManager.Shutdown();
			this.tableManager.Dispose();
			this.tableManager = null;
		}
	}
}