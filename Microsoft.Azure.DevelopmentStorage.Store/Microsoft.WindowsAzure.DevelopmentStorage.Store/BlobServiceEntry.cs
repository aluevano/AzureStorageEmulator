using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[ServiceEntryClass]
	public sealed class BlobServiceEntry : ServiceRequestHandler, IServiceEntry, IDisposable
	{
		private const string BlobHostSuffixDomainsConfigParameterName = "UriHostSuffixes";

		private static IStorageManager SharedStorageManager;

		private static ServiceManagerConfiguration SharedConfig;

		private AuthenticationManager authenticationManager;

		private ServiceManager serviceManager;

		public override Microsoft.Cis.Services.Nephos.Common.ServiceType ServiceType
		{
			get
			{
				return Microsoft.Cis.Services.Nephos.Common.ServiceType.BlobService;
			}
		}

		public override IStorageManager StorageManagerProvider
		{
			get
			{
				return BlobServiceEntry.SharedStorageManager;
			}
		}

		public BlobServiceEntry()
		{
		}

		public override void AcceptRequest(RequestContext requestContext)
		{
			this.AcceptRequest(requestContext, this.ServiceType);
		}

		public override void AcceptRequest(RequestContext context, Microsoft.Cis.Services.Nephos.Common.ServiceType serviceType)
		{
			Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("Accepting request");
			context.ServiceType = serviceType;
			PerRequestStorageManager perRequestStorageManager = new PerRequestStorageManager(BlobServiceEntry.SharedStorageManager, context.OperationStatus);
			AuthenticationManager operationStatus = XFEBlobAuthenticationManager.CreateAuthenticationManager(perRequestStorageManager, false);
			operationStatus.OperationStatus = context.OperationStatus;
			AuthorizationManager authorizationManager = XFEBlobAuthorizationManager.CreateAuthorizationManager(perRequestStorageManager, true);
			authorizationManager.OperationStatus = context.OperationStatus;
			ServiceManager serviceManager = RealServiceManager.CreateServiceManager(authorizationManager, perRequestStorageManager, BlobServiceEntry.SharedConfig);
			serviceManager.OperationStatus = context.OperationStatus;
			IProcessor processor = HttpRestProcessor.Create(context, perRequestStorageManager, operationStatus, serviceManager, HttpRestProcessor.HttpProcessorConfigurationDefaultInstance, new TransformExceptionDelegate(SqlExceptionManager.TransformSqlException), null);
			bool flag = true;
			try
			{
				try
				{
					processor.BeginProcess(new AsyncCallback(this.ProcessAsyncCallback), processor);
					flag = false;
				}
				catch (NephosAssertionException nephosAssertionException1)
				{
					NephosAssertionException nephosAssertionException = nephosAssertionException1;
					IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
					object[] str = new object[] { nephosAssertionException.ToString() };
					error.Log("ASSERTION in BeginProcess: {0}", str);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					IStringDataEventStream unhandledException = Logger<IRestProtocolHeadLogger>.Instance.UnhandledException;
					object[] objArray = new object[] { exception.ToString() };
					unhandledException.Log("BeginProcess threw exception {0}", objArray);
				}
			}
			finally
			{
				if (flag)
				{
					processor.Dispose();
				}
			}
			Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("Returning from AcceptRequest method.");
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		void Microsoft.Cis.Services.Nephos.Common.Protocols.Rest.IServiceEntry.Initialize(IServiceEntrySink sink)
		{
			Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("[Nephos.Storage] { Initialize");
			HttpRestProcessor.HttpProcessorConfigurationDefaultInstance = HttpProcessorConfiguration.LoadDefaultHttpProcessorConfiguration(sink, "UriHostSuffixes", null);
			BlobServiceEntry.SharedStorageManager = new DbStorageManager();
			BlobServiceEntry.SharedConfig = new ServiceManagerConfiguration();
			this.authenticationManager = XFEBlobAuthenticationManager.CreateAuthenticationManager(BlobServiceEntry.SharedStorageManager, false);
			this.serviceManager = RealServiceManager.CreateServiceManager(XFEBlobAuthorizationManager.CreateAuthorizationManager(BlobServiceEntry.SharedStorageManager, true), BlobServiceEntry.SharedStorageManager, BlobServiceEntry.SharedConfig);
			StorageStamp.TranslateException = new TranslateExceptionDelegate(SqlExceptionManager.ReThrowException);
			BlockBlobGarbageCollector.Initialize();
			Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("[Nephos.Storage] } Initialize");
			sink.RegisterRestHandler(this);
		}

		void Microsoft.Cis.Services.Nephos.Common.Protocols.Rest.IServiceEntry.Start()
		{
			BlobServiceEntry.SharedStorageManager.Initialize();
		}

		void Microsoft.Cis.Services.Nephos.Common.Protocols.Rest.IServiceEntry.Stop()
		{
			if (BlobServiceEntry.SharedStorageManager != null)
			{
				BlobServiceEntry.SharedStorageManager.Shutdown();
				BlobServiceEntry.SharedStorageManager.Dispose();
				BlobServiceEntry.SharedStorageManager = null;
			}
		}

		private void ProcessAsyncCallback(IAsyncResult ar)
		{
			Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("In the process end callback");
			using (IProcessor asyncState = (IProcessor)ar.AsyncState)
			{
				try
				{
					asyncState.EndProcess(ar);
				}
				catch (NephosAssertionException nephosAssertionException1)
				{
					NephosAssertionException nephosAssertionException = nephosAssertionException1;
					IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
					object[] str = new object[] { nephosAssertionException.ToString() };
					error.Log("ASSERTION: {0}", str);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					IStringDataEventStream unhandledException = Logger<IRestProtocolHeadLogger>.Instance.UnhandledException;
					object[] objArray = new object[] { exception.ToString() };
					unhandledException.Log("EXCEPTION thrown: {0}", objArray);
				}
			}
		}
	}
}