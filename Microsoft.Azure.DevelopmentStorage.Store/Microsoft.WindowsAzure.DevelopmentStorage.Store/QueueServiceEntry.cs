using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC;
using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[ServiceEntryClass]
	public sealed class QueueServiceEntry : ServiceRequestHandler, IServiceEntry, IDisposable
	{
		private const string QueueHostSuffixDomainsConfigParameterName = "UriHostSuffixes";

		private AbstractQueueManager queueManager;

		private AuthenticationManager authenticationManager;

		private static IStorageManager SharedStorageManager;

		public override Microsoft.Cis.Services.Nephos.Common.ServiceType ServiceType
		{
			get
			{
				return Microsoft.Cis.Services.Nephos.Common.ServiceType.QueueService;
			}
		}

		public override IStorageManager StorageManagerProvider
		{
			get
			{
				return QueueServiceEntry.SharedStorageManager;
			}
		}

		public QueueServiceEntry()
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
			PerRequestStorageManager perRequestStorageManager = new PerRequestStorageManager(QueueServiceEntry.SharedStorageManager, context.OperationStatus);
			AuthenticationManager authenticationManager = XFEQueueAuthenticationManager.CreateAuthenticationManager(QueueServiceEntry.SharedStorageManager);
			AuthorizationManager authorizationManager = XFEQueueAuthorizationManager.CreateAuthorizationManager(QueueServiceEntry.SharedStorageManager, false);
			AbstractQueueManager dbQueueManager = new DbQueueManager(QueueServiceEntry.SharedStorageManager, authorizationManager)
			{
				OperationStatus = context.OperationStatus
			};
			IProcessor processor = QueueProtocolHead.Create(context, perRequestStorageManager, authenticationManager, dbQueueManager, QueueProtocolHead.HttpProcessorConfigurationDefaultInstance, new TransformExceptionDelegate(SqlExceptionManager.TransformSqlException), null);
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
					processor.Dispose();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					IStringDataEventStream unhandledException = Logger<IRestProtocolHeadLogger>.Instance.UnhandledException;
					object[] objArray = new object[] { exception.ToString() };
					unhandledException.Log("BeginProcess threw exception {0}", objArray);
					processor.Dispose();
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
			Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("[Nephos.Queue] { Initialize");
			QueueServiceEntry.SharedStorageManager = new DbStorageManager();
			QueueProtocolHead.HttpProcessorConfigurationDefaultInstance = HttpProcessorConfiguration.LoadDefaultHttpProcessorConfiguration(sink, "UriHostSuffixes", null);
			this.authenticationManager = XFEQueueAuthenticationManager.CreateAuthenticationManager(QueueServiceEntry.SharedStorageManager);
			this.queueManager = new DbQueueManager(QueueServiceEntry.SharedStorageManager, XFEQueueAuthorizationManager.CreateAuthorizationManager(QueueServiceEntry.SharedStorageManager, false));
			sink.RegisterRestHandler(this);
			Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("[DevelopmentStorage.Queue] } Initialize");
		}

		void Microsoft.Cis.Services.Nephos.Common.Protocols.Rest.IServiceEntry.Start()
		{
			QueueServiceEntry.SharedStorageManager.Initialize();
		}

		void Microsoft.Cis.Services.Nephos.Common.Protocols.Rest.IServiceEntry.Stop()
		{
			QueueServiceEntry.SharedStorageManager.Shutdown();
			QueueServiceEntry.SharedStorageManager.Dispose();
			QueueServiceEntry.SharedStorageManager = null;
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