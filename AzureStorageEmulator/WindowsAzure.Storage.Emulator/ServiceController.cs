using Microsoft.WindowsAzure.DevelopmentStorage.Store;
using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.Emulator
{
	internal static class ServiceController
	{
		private static object controllerLock;

		private static IStorageContext context;

		private static bool isInitialized;

		public static IStorageContext Context
		{
			get
			{
				return ServiceController.context;
			}
			private set
			{
				ServiceController.context = value;
				ServiceController.isInitialized = true;
			}
		}

		static ServiceController()
		{
			ServiceController.controllerLock = new object();
			ServiceController.isInitialized = false;
		}

		public static void DestroyServices()
		{
			foreach (string key in StorageEmulatorConfigCache.Configuration.Services.Keys)
			{
				ServiceController.StopService(key);
			}
		}

		private static DateTime GetCurrentDateTime()
		{
			DateTime utcNow = DateTime.UtcNow;
			DateTime dateTime = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second, 0, DateTimeKind.Utc);
			return dateTime;
		}

		public static void InitializeServices(Action<string> showErrorMessage, IStorageContext context)
		{
			try
			{
				ServiceController.Context = context;
				ServiceController.Context.InitializeLogger();
				ServiceController.SyncronizeAccountsFromConfigFile();
			}
			catch (Exception exception)
			{
				showErrorMessage(exception.Message);
			}
			foreach (string key in StorageEmulatorConfigCache.Configuration.Services.Keys)
			{
				try
				{
					ServiceController.StartService(key);
				}
				catch (Exception exception2)
				{
					Exception exception1 = exception2;
					ServiceController.StopService(key);
					showErrorMessage(exception1.Message);
					throw new EmulatorException(EmulatorErrorCode.StartFailed);
				}
			}
		}

		public static ServiceStatusInfo StartService(string serviceName)
		{
			ServiceStatusInfo serviceStatusInfo;
			lock (ServiceController.controllerLock)
			{
				ServiceStatusInfo serviceStatusInfo1 = null;
				serviceStatusInfo1 = ServiceManager.Instance.StartService(serviceName);
				ServiceController.ServiceStatusChanged(serviceStatusInfo1);
				serviceStatusInfo = serviceStatusInfo1;
			}
			return serviceStatusInfo;
		}

		public static ServiceStatusInfo StopService(string serviceName)
		{
			ServiceStatusInfo serviceStatusInfo;
			lock (ServiceController.controllerLock)
			{
				ServiceStatusInfo serviceStatusInfo1 = ServiceManager.Instance.StopService(serviceName);
				ServiceController.ServiceStatusChanged(serviceStatusInfo1);
				serviceStatusInfo = serviceStatusInfo1;
			}
			return serviceStatusInfo;
		}

		private static void SyncronizeAccountsFromConfigFile()
		{
			foreach (StorageEmulatorConfigAccount value in StorageEmulatorConfigCache.Configuration.Accounts.Values)
			{
				ServiceController.GetCurrentDateTime();
				bool flag = false;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Account secretKey = (
						from existingAccount in dbContext.Accounts
						where existingAccount.Name == value.Name
						select existingAccount).FirstOrDefault<Account>();
					if (secretKey == null)
					{
						Account account = new Account()
						{
							Name = value.Name,
							SecondaryReadEnabled = true
						};
						secretKey = account;
						flag = true;
					}
					secretKey.SecretKey = Convert.FromBase64String(value.AuthKey);
					secretKey.SecondaryKey = secretKey.SecretKey;
					if (flag)
					{
						dbContext.Accounts.InsertOnSubmit(secretKey);
					}
					dbContext.SubmitChanges();
				}
			}
		}

		public static event StatusEventHandler ServiceStatusChanged;
	}
}