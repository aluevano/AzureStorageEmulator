using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration;
using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Storage.Emulator
{
	internal class ServiceManager
	{
		private static ServiceManager instance;

		private static object syncLock;

		private Dictionary<string, IHttpServiceHost> httpServiceHosts;

		private bool disposed;

		public static ServiceManager Instance
		{
			get
			{
				if (ServiceManager.instance == null)
				{
					lock (ServiceManager.syncLock)
					{
						if (ServiceManager.instance == null)
						{
							ServiceManager.instance = new ServiceManager();
						}
					}
				}
				return ServiceManager.instance;
			}
		}

		static ServiceManager()
		{
			ServiceManager.syncLock = new object();
		}

		private ServiceManager()
		{
			this.Initialize();
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
			if (disposing && this.httpServiceHosts != null)
			{
				this.httpServiceHosts.Clear();
				this.httpServiceHosts = null;
			}
			this.disposed = true;
		}

		~ServiceManager()
		{
			this.Dispose(false);
		}

		public ServiceStatusInfo GetServiceStatus(string serviceName, Exception ex)
		{
			ServiceStatusInfo serviceStatusInfo = new ServiceStatusInfo()
			{
				ServiceName = serviceName,
				Error = ex
			};
			if (this.httpServiceHosts == null || !this.httpServiceHosts.ContainsKey(serviceName))
			{
				serviceStatusInfo.IsRunning = false;
				serviceStatusInfo.ServiceEndpoint = StorageEmulatorConfigCache.Configuration.Services[serviceName].Url;
			}
			else
			{
				serviceStatusInfo.IsRunning = this.httpServiceHosts[serviceName].IsRunning();
				serviceStatusInfo.ServiceEndpoint = this.httpServiceHosts[serviceName].UrlBase;
			}
			return serviceStatusInfo;
		}

		public List<ServiceStatusInfo> GetServiceStatusAll()
		{
			List<ServiceStatusInfo> serviceStatusInfos = new List<ServiceStatusInfo>();
			foreach (string key in StorageEmulatorConfigCache.Configuration.Services.Keys)
			{
				serviceStatusInfos.Add(this.GetServiceStatus(key, null));
			}
			return serviceStatusInfos;
		}

		private void HandleServiceException(string serviceName, ServiceStatusInfo serviceStatus)
		{
			ServiceController.Context.LogException(serviceStatus.Error);
			if (this.httpServiceHosts.ContainsKey(serviceName))
			{
				this.httpServiceHosts.Remove(serviceName);
			}
			throw serviceStatus.Error;
		}

		private void Initialize()
		{
			this.httpServiceHosts = new Dictionary<string, IHttpServiceHost>();
		}

		public bool IsServiceRunning(string serviceName)
		{
			if (!this.httpServiceHosts.ContainsKey(serviceName))
			{
				return false;
			}
			return this.httpServiceHosts[serviceName].IsRunning();
		}

		public ServiceStatusInfo StartService(string serviceName)
		{
			ServiceStatusInfo serviceStatus;
			try
			{
				ServiceController.Context.Log(string.Format("Starting Service: {0}", serviceName));
				if (!this.httpServiceHosts.ContainsKey(serviceName))
				{
					this.httpServiceHosts.Add(serviceName, new HttpServiceHost(serviceName));
					this.httpServiceHosts[serviceName].UrlBase = StorageEmulatorConfigCache.Configuration.Services[serviceName].Url;
				}
				this.httpServiceHosts[serviceName].Start();
				serviceStatus = this.GetServiceStatus(serviceName, null);
			}
			catch (Exception exception)
			{
				serviceStatus = this.GetServiceStatus(serviceName, exception);
				this.HandleServiceException(serviceName, serviceStatus);
			}
			return serviceStatus;
		}

		public ServiceStatusInfo StopService(string serviceName)
		{
			ServiceStatusInfo serviceStatus;
			try
			{
				ServiceController.Context.Log(string.Format("Stopping Service: {0}", serviceName));
				if (this.httpServiceHosts.ContainsKey(serviceName))
				{
					this.httpServiceHosts[serviceName].Stop();
					this.httpServiceHosts[serviceName].Dispose();
					this.httpServiceHosts.Remove(serviceName);
				}
				serviceStatus = this.GetServiceStatus(serviceName, null);
			}
			catch (Exception exception)
			{
				serviceStatus = this.GetServiceStatus(serviceName, exception);
				this.HandleServiceException(serviceName, serviceStatus);
			}
			return serviceStatus;
		}
	}
}