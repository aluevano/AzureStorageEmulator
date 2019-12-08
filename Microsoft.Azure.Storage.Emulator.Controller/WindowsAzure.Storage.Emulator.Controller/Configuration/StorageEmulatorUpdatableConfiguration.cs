using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration
{
	[XmlRoot("StorageEmulator")]
	public class StorageEmulatorUpdatableConfiguration
	{
		private const string DefaultLogPath = "Logs";

		private const string DefaultBlockBlobPath = "BlockBlobRoot";

		private const string DefaultPageBlobPath = "PageBlobRoot";

		private const string AppDataDirectory = "AzureStorageEmulator";

		private readonly static string EmulatorConfigFilename;

		private static StorageEmulatorUpdatableConfiguration cachedConfig;

		[XmlElement("BlockBlobRoot")]
		public string BlockBlobRoot
		{
			get;
			set;
		}

		[XmlElement("LoggingEnabled")]
		public bool LoggingEnabled
		{
			get;
			set;
		}

		[XmlElement("LogPath")]
		public string LogPath
		{
			get;
			set;
		}

		[XmlElement("PageBlobRoot")]
		public string PageBlobRoot
		{
			get;
			set;
		}

		[XmlElement("SQLInstance")]
		public string SqlInstance
		{
			get;
			set;
		}

		static StorageEmulatorUpdatableConfiguration()
		{
			StorageEmulatorUpdatableConfiguration.EmulatorConfigFilename = string.Format("AzureStorageEmulator.{0}.config", "5.2");
		}

		public StorageEmulatorUpdatableConfiguration()
		{
			this.SqlInstance = "localhost\\SQLExpress";
			string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AzureStorageEmulator");
			this.PageBlobRoot = Path.Combine(str, "PageBlobRoot");
			this.BlockBlobRoot = Path.Combine(str, "BlockBlobRoot");
			this.LogPath = Path.Combine(str, "Logs");
			this.LoggingEnabled = false;
		}

		private static string GetConfigFilePath()
		{
			DirectoryInfo directoryInfo = null;
			try
			{
				directoryInfo = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AzureStorageEmulator"));
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				throw new InvalidOperationException(string.Format("Unable to create folder {0} :{1}", directoryInfo.FullName, exception.Message), exception);
			}
			return Path.Combine(directoryInfo.FullName, StorageEmulatorUpdatableConfiguration.EmulatorConfigFilename);
		}

		public static object GetConfigurationSection(string name)
		{
			object section = ConfigurationManager.GetSection(name);
			if (section == null)
			{
				string str = ProcessWrapper.ResolveBinaryFullPath("AzureStorageEmulator.exe");
				section = ConfigurationManager.OpenExeConfiguration(str).GetSection(name);
			}
			return section;
		}

		public static string GetDbConnectionString()
		{
			string sqlInstance = "localhost\\SQLExpress";
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration = null;
			if (StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(out storageEmulatorUpdatableConfiguration))
			{
				sqlInstance = storageEmulatorUpdatableConfiguration.SqlInstance;
			}
			return string.Format("Server={0};Integrated security=SSPI;database={1}", sqlInstance, Constants.EmulatorDBName);
		}

		public static bool GetLoggingEnabled()
		{
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration;
			if (!StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(out storageEmulatorUpdatableConfiguration))
			{
				storageEmulatorUpdatableConfiguration = new StorageEmulatorUpdatableConfiguration();
			}
			return storageEmulatorUpdatableConfiguration.LoggingEnabled;
		}

		public static string GetLogPath()
		{
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration;
			if (!StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(out storageEmulatorUpdatableConfiguration))
			{
				storageEmulatorUpdatableConfiguration = new StorageEmulatorUpdatableConfiguration();
			}
			if (!Directory.Exists(storageEmulatorUpdatableConfiguration.LogPath))
			{
				Directory.CreateDirectory(storageEmulatorUpdatableConfiguration.LogPath);
			}
			return storageEmulatorUpdatableConfiguration.LogPath;
		}

		public static string GetMasterConnectionString()
		{
			string sqlInstance = "localhost\\SQLExpress";
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration = null;
			if (StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(out storageEmulatorUpdatableConfiguration))
			{
				sqlInstance = storageEmulatorUpdatableConfiguration.SqlInstance;
			}
			return string.Format("Server={0};Integrated security=SSPI;database=master", sqlInstance);
		}

		public static string GetSqlInstance()
		{
			string sqlInstance = null;
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration = null;
			if (StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(out storageEmulatorUpdatableConfiguration))
			{
				sqlInstance = storageEmulatorUpdatableConfiguration.SqlInstance;
			}
			if (sqlInstance == null)
			{
				using (SqlConnection sqlConnection = new SqlConnection(StorageEmulatorUpdatableConfiguration.GetDbConnectionString()))
				{
					if (sqlInstance == null)
					{
						sqlInstance = sqlConnection.DataSource;
					}
				}
			}
			return sqlInstance;
		}

		public static string GetStorageEmulatorDBConnectionString()
		{
			string sqlInstance = null;
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration = null;
			if (StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(out storageEmulatorUpdatableConfiguration))
			{
				sqlInstance = storageEmulatorUpdatableConfiguration.SqlInstance;
			}
			string database = null;
			using (SqlConnection sqlConnection = new SqlConnection(StorageEmulatorUpdatableConfiguration.GetDbConnectionString()))
			{
				if (sqlInstance == null)
				{
					sqlInstance = sqlConnection.DataSource;
				}
				database = sqlConnection.Database;
			}
			return string.Format("Server={0};Integrated security=SSPI;database={1}", sqlInstance, database);
		}

		public static string GetStorageEmulatorDBName()
		{
			return Constants.EmulatorDBName;
		}

		public static bool TryGetFromUserProfile(out StorageEmulatorUpdatableConfiguration result)
		{
			return StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(false, out result);
		}

		public static bool TryGetFromUserProfile(bool forceRefresh, out StorageEmulatorUpdatableConfiguration result)
		{
			result = null;
			if (!forceRefresh && StorageEmulatorUpdatableConfiguration.cachedConfig != null)
			{
				result = StorageEmulatorUpdatableConfiguration.cachedConfig;
				return true;
			}
			string configFilePath = StorageEmulatorUpdatableConfiguration.GetConfigFilePath();
			if (!File.Exists(configFilePath))
			{
				return false;
			}
			FileStream fileStream = File.OpenRead(configFilePath);
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration = (StorageEmulatorUpdatableConfiguration)(new XmlSerializer(typeof(StorageEmulatorUpdatableConfiguration))).Deserialize(fileStream);
			fileStream.Close();
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration1 = storageEmulatorUpdatableConfiguration;
			StorageEmulatorUpdatableConfiguration.cachedConfig = storageEmulatorUpdatableConfiguration1;
			result = storageEmulatorUpdatableConfiguration1;
			return true;
		}

		public void WriteToUserProfile()
		{
			StorageEmulatorUpdatableConfiguration.cachedConfig = this;
			using (FileStream fileStream = File.Create(StorageEmulatorUpdatableConfiguration.GetConfigFilePath()))
			{
				(new XmlSerializer(typeof(StorageEmulatorUpdatableConfiguration))).Serialize(fileStream, this);
			}
		}
	}
}