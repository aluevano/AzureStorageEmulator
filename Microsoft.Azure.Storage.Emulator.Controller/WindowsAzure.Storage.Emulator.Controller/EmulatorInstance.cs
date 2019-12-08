using Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	public class EmulatorInstance
	{
		private static Random rnd;

		public static EmulatorInstance CommonInstance
		{
			get;
			private set;
		}

		public EmulatorStatus Status
		{
			get
			{
				StorageEmulatorConfigService storageEmulatorConfigService;
				StorageEmulatorConfigService storageEmulatorConfigService1;
				StorageEmulatorConfigService storageEmulatorConfigService2;
				string url;
				string str;
				string url1;
				StorageEmulatorConfig configuration = StorageEmulatorConfigCache.Configuration;
				configuration.Services.TryGetValue("Blob", out storageEmulatorConfigService);
				configuration.Services.TryGetValue("Queue", out storageEmulatorConfigService1);
				configuration.Services.TryGetValue("Table", out storageEmulatorConfigService2);
				EmulatorStatus emulatorStatu = new EmulatorStatus()
				{
					IsRunning = EmulatorProcessController.IsRunning()
				};
				EmulatorStatus emulatorStatu1 = emulatorStatu;
				if (storageEmulatorConfigService == null)
				{
					url = null;
				}
				else
				{
					url = storageEmulatorConfigService.Url;
				}
				emulatorStatu1.BlobEndpoint = url;
				EmulatorStatus emulatorStatu2 = emulatorStatu;
				if (storageEmulatorConfigService1 == null)
				{
					str = null;
				}
				else
				{
					str = storageEmulatorConfigService1.Url;
				}
				emulatorStatu2.QueueEndpoint = str;
				EmulatorStatus emulatorStatu3 = emulatorStatu;
				if (storageEmulatorConfigService2 == null)
				{
					url1 = null;
				}
				else
				{
					url1 = storageEmulatorConfigService2.Url;
				}
				emulatorStatu3.TableEndpoint = url1;
				return emulatorStatu;
			}
		}

		static EmulatorInstance()
		{
			EmulatorInstance.rnd = new Random();
			EmulatorInstance.CommonInstance = new EmulatorInstance();
		}

		private EmulatorInstance()
		{
		}

		public void ClearData(EmulatorServiceType serviceType)
		{
			StorageContext storageContext = new StorageContext();
			storageContext.InitializeLogger();
			if ((serviceType & EmulatorServiceType.Blob) == EmulatorServiceType.Blob)
			{
				storageContext.CleanBlobData();
			}
			if ((serviceType & EmulatorServiceType.Queue) == EmulatorServiceType.Queue)
			{
				storageContext.CleanQueueData();
			}
			if ((serviceType & EmulatorServiceType.Table) == EmulatorServiceType.Table)
			{
				storageContext.CleanTableData();
			}
		}

		public EmulatorStorageAccount CreateAccount(string accountName, string primaryKey = null, string secondaryKey = null, bool secondaryReadEnabled = true)
		{
			this.ValidateArguments(accountName, primaryKey, secondaryKey);
			StorageContext storageContext = new StorageContext();
			storageContext.InitializeLogger();
			return storageContext.CreateAccount(accountName, primaryKey, secondaryKey, secondaryReadEnabled);
		}

		public bool DeleteAccount(string accountName)
		{
			this.ValidateAccountName(accountName);
			StorageContext storageContext = new StorageContext();
			storageContext.InitializeLogger();
			return storageContext.DeleteAccount(accountName);
		}

		public EmulatorStorageAccount GetAccount(string accountName)
		{
			this.ValidateAccountName(accountName);
			StorageContext storageContext = new StorageContext();
			storageContext.InitializeLogger();
			return storageContext.GetAccount(accountName);
		}

		public void Initialize(string serverName = null, string sqlInstanceName = null, bool forceCreate = false, bool skipCreate = false, bool autoDetectDatabase = false, bool runInCurrentProcess = false, bool reservePorts = false, bool unreservePorts = false)
		{
			Initialization initialization = new Initialization(forceCreate, skipCreate, autoDetectDatabase, serverName, sqlInstanceName, reservePorts, unreservePorts);
			if (!runInCurrentProcess && (reservePorts || unreservePorts))
			{
				initialization.LaunchInitializationElevated();
				return;
			}
			initialization.PerformEmulatorInitialization();
		}

		private bool IsAlphaNumericLowerCase(string s)
		{
			return (new Regex("^[a-z0-9]*$")).IsMatch(s);
		}

		public IEnumerable<EmulatorStorageAccount> ListAccounts()
		{
			StorageContext storageContext = new StorageContext();
			storageContext.InitializeLogger();
			return storageContext.ListAccounts();
		}

		public string[] RegenerateKey(string accountName, KeyType keyType)
		{
			this.ValidateAccountName(accountName);
			this.ValidateKeyType(keyType);
			StorageContext storageContext = new StorageContext();
			storageContext.InitializeLogger();
			return storageContext.RegenerateKey(accountName, keyType);
		}

		public void Start()
		{
			EmulatorProcessController.EnsureRunning(60000);
		}

		public void Stop()
		{
			EmulatorProcessController.Shutdown();
		}

		public EmulatorStorageAccount UpdateAccount(string accountName, bool? secondaryReadEnabled = null)
		{
			this.ValidateAccountName(accountName);
			StorageContext storageContext = new StorageContext();
			storageContext.InitializeLogger();
			return storageContext.UpdateAccount(accountName, secondaryReadEnabled);
		}

		private void ValidateAccountName(string accountName)
		{
			if (accountName == null || accountName.Length < 3 || accountName.Length > 24 || !this.IsAlphaNumericLowerCase(accountName))
			{
				throw new ArgumentException("Account name should be 3-24 characters long, numbers and lower-case letters only.", "accountName");
			}
		}

		private void ValidateArguments(string accountName, string primaryKey, string secondaryKey)
		{
			this.ValidateAccountName(accountName);
			this.ValidateKey(primaryKey, true);
			this.ValidateKey(secondaryKey, false);
		}

		private void ValidateKey(string key, bool isPrimary)
		{
			if (key != null)
			{
				string str = (isPrimary ? "primaryKey" : "secondaryKey");
				try
				{
					Convert.FromBase64String(key);
				}
				catch (FormatException formatException)
				{
					throw new ArgumentException("Key should be base64 encoded and 88 characters long.", str, formatException);
				}
				if (key.Length != 88)
				{
					throw new ArgumentException("Key should be base64 encoded and 88 characters long.", str);
				}
			}
		}

		private void ValidateKeyType(KeyType keyType)
		{
			switch (keyType)
			{
				case KeyType.Primary:
				case KeyType.Secondary:
				{
					return;
				}
				default:
				{
					throw new ArgumentException("Key type should be primary or secondary.");
				}
			}
		}
	}
}