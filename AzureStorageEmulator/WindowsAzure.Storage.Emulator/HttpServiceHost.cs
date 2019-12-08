using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.WindowsAzure.DevelopmentStorage.Store;
using Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.Emulator
{
	internal class HttpServiceHost : IHttpServiceHost, IDisposable, IServiceEntrySink
	{
		private int isStarted;

		private bool disposed;

		private string serviceName;

		private ServiceRequestHandler serviceRequestHandler;

		private HttpListener listener;

		private Thread listenerThread;

		private IServiceEntry serviceEntry;

		public string UrlBase
		{
			get;
			set;
		}

		public HttpServiceHost(string serviceName)
		{
			this.isStarted = 0;
			this.serviceName = serviceName;
			this.UrlBase = string.Empty;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			if (disposing)
			{
				if (this.listener != null)
				{
					if (this.listener.IsListening)
					{
						this.listener.Stop();
					}
					this.listener.Close();
					this.listener = null;
				}
				if (this.serviceEntry != null)
				{
					this.serviceEntry.Dispose();
					this.serviceEntry = null;
					this.serviceRequestHandler = null;
				}
			}
			this.disposed = true;
		}

		~HttpServiceHost()
		{
			this.Dispose(false);
		}

		public string GetConfigurationParameter(string name)
		{
			string empty = string.Empty;
			string str = name;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "NephosAllowPathStyleUris")
				{
					name = "AllowPathStyleUris";
				}
				else if (str1 == "NephosIncludeInternalDetailsInErrorResponses")
				{
					name = "IncludeInternalDetailsInErrorResponses";
				}
				else if (str1 == "Nephos.TableUriHostSuffixes")
				{
					return "localhost";
				}
			}
			try
			{
				Type type = StorageEmulatorConfigCache.Configuration.Services[this.serviceName].GetType();
				PropertyInfo property = type.GetProperty(name);
				empty = property.GetValue(StorageEmulatorConfigCache.Configuration.Services[this.serviceName], null).ToString();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Configuration Parameter: {0} not found", new object[] { name });
				throw new Exception(string.Format("Configuration Parameter: {0} not found", name), exception);
			}
			return empty;
		}

		public bool IsRunning()
		{
			if (this.listener == null)
			{
				return false;
			}
			return this.listener.IsListening;
		}

		public bool IsTenantDevFabric()
		{
			return true;
		}

		public void RegisterRestHandler(ServiceRequestHandler handler)
		{
		}

		public void Restart()
		{
			this.Stop();
			this.Start();
		}

		private void Run()
		{
			Interlocked.CompareExchange(ref this.isStarted, 1, 0);
			while (this.isStarted == 1)
			{
				try
				{
					HttpListenerContext context = this.listener.GetContext();
					Trace.ActivityId = LoggerProvider.Instance.CreateActivityId(true);
					Trace.Metadata = new Dictionary<string, object>();
					EndToEndLogging.LogActivityStart(Logger<IRestProtocolHeadLogger>.Instance.Verbose, "RDStorageRequest");
					this.serviceRequestHandler.AcceptRequest(new RequestContext(context, DateTime.UtcNow));
				}
				catch (Exception exception)
				{
					Logger<IRestProtocolHeadLogger>.Instance.Error.Log(exception.Message);
				}
			}
		}

		public void Start()
		{
			if (this.isStarted == 1)
			{
				return;
			}
			if (this.listener == null)
			{
				this.listener = new HttpListener();
			}
			if (this.serviceEntry == null)
			{
				try
				{
					string str = this.serviceName;
					string str1 = str;
					if (str != null)
					{
						if (str1 == "Blob")
						{
							this.serviceEntry = new BlobServiceEntry();
						}
						else if (str1 == "Queue")
						{
							this.serviceEntry = new QueueServiceEntry();
						}
						else
						{
							if (str1 != "Table")
							{
								throw new Exception("Blob, Queue & Table configuration missing");
							}
							this.serviceEntry = new TableServiceEntry();
						}
						this.serviceRequestHandler = this.serviceEntry as ServiceRequestHandler;
						this.serviceEntry.Initialize(this);
						this.serviceEntry.Start();
						this.listener.Prefixes.Add(this.UrlBase);
						this.listener.Start();
						this.listenerThread = new Thread(new ThreadStart(this.Run));
						this.listenerThread.Start();
						return;
					}
					throw new Exception("Blob, Queue & Table configuration missing");
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.Stop();
					Logger<IRestProtocolHeadLogger>.Instance.Error.Log(exception.Message);
					throw;
				}
			}
		}

		public void Stop()
		{
			if (this.listener != null)
			{
				Interlocked.CompareExchange(ref this.isStarted, 0, 1);
				if (this.listener.IsListening)
				{
					this.listener.Stop();
				}
				this.listener.Close();
				this.listener = null;
			}
			if (this.serviceEntry != null)
			{
				this.serviceEntry.Stop();
				this.serviceEntry = null;
				this.serviceRequestHandler = null;
			}
			if (this.listenerThread != null)
			{
				this.listenerThread.Abort();
				this.listenerThread = null;
			}
		}
	}
}