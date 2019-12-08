using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.WindowsAzure.DevelopmentStorage.Store;
using Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	public class StorageContext : IStorageContext
	{
		private const string AddDBRoleCommand = "USE {0} EXEC sp_addrolemember N'{1}', N'{2}'";

		private const string CheckLogins = "USE {0} EXEC sp_helplogins N'{1}'";

		private const string GrantDBAccessCommand = "USE {0} EXEC sp_grantdbaccess N'{1}', N'{2}'";

		private static Random rnd;

		public string DatabaseName
		{
			get
			{
				return DevelopmentStorageDbDataContext.DatabaseName;
			}
		}

		public string InstanceName
		{
			get
			{
				return DevelopmentStorageDbDataContext.SqlInstance;
			}
			set
			{
				DevelopmentStorageDbDataContext.SqlInstance = value;
			}
		}

		static StorageContext()
		{
			StorageContext.rnd = new Random();
		}

		public StorageContext()
		{
		}

		public void AddRoleMember(string databaseUserName)
		{
			using (SqlConnection sqlConnection = new SqlConnection(DevelopmentStorageDbDataContext.GetConnectionString()))
			{
				sqlConnection.Open();
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] databaseName = new object[] { DevelopmentStorageDbDataContext.DatabaseName, "db_datareader", databaseUserName };
				using (SqlCommand sqlCommand = new SqlCommand(string.Format(invariantCulture, "USE {0} EXEC sp_addrolemember N'{1}', N'{2}'", databaseName), sqlConnection))
				{
					sqlCommand.ExecuteNonQuery();
					CultureInfo cultureInfo = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { DevelopmentStorageDbDataContext.DatabaseName, "db_datawriter", databaseUserName };
					sqlCommand.CommandText = string.Format(cultureInfo, "USE {0} EXEC sp_addrolemember N'{1}', N'{2}'", objArray);
					sqlCommand.ExecuteNonQuery();
				}
			}
		}

		public bool CheckDbExists()
		{
			return DevelopmentStorageDbDataContext.GetDbContext().DatabaseExists();
		}

		public void CleanBlobData()
		{
			try
			{
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					dbContext.BlobContainers.DeleteAllOnSubmit<BlobContainer>(dbContext.BlobContainers);
					dbContext.Blobs.DeleteAllOnSubmit<Blob>(dbContext.Blobs);
					dbContext.BlocksData.DeleteAllOnSubmit<BlockData>(dbContext.BlocksData);
					dbContext.CommittedBlocks.DeleteAllOnSubmit<CommittedBlock>(dbContext.CommittedBlocks);
					dbContext.CurrentPages.DeleteAllOnSubmit<CurrentPage>(dbContext.CurrentPages);
					dbContext.Pages.DeleteAllOnSubmit<Page>(dbContext.Pages);
					dbContext.SubmitChanges();
					this.ShrinkDatabase();
				}
				StorageContext.CleanupFilesAndSubDirectores(DevelopmentStorageDbDataContext.PageBlobRoot);
				StorageContext.CleanupFilesAndSubDirectores(DevelopmentStorageDbDataContext.BlockBlobRoot);
			}
			catch (Exception exception)
			{
				throw new EmulatorException(EmulatorErrorCode.FailedToClearData, exception);
			}
		}

		public void CleanQueueData()
		{
			try
			{
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					dbContext.QueueContainers.DeleteAllOnSubmit<QueueContainer>(dbContext.QueueContainers);
					dbContext.QueueMessages.DeleteAllOnSubmit<QueueMessage>(dbContext.QueueMessages);
					dbContext.SubmitChanges();
					this.ShrinkDatabase();
				}
			}
			catch (Exception exception)
			{
				throw new EmulatorException(EmulatorErrorCode.FailedToClearData, exception);
			}
		}

		public void CleanTableData()
		{
			try
			{
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					dbContext.TableContainers.DeleteAllOnSubmit<TableContainer>(dbContext.TableContainers);
					dbContext.TableRows.DeleteAllOnSubmit<TableRow>(dbContext.TableRows);
					dbContext.SubmitChanges();
				}
			}
			catch (Exception exception)
			{
				throw new EmulatorException(EmulatorErrorCode.FailedToClearData, exception);
			}
		}

		private static void CleanupFilesAndSubDirectores(string dir)
		{
			string[] files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < (int)files.Length; i++)
			{
				string str = files[i];
				try
				{
					File.Delete(str);
				}
				catch (IOException oException)
				{
				}
			}
			string[] directories = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
			for (int j = 0; j < (int)directories.Length; j++)
			{
				string str1 = directories[j];
				try
				{
					Directory.Delete(str1, true);
				}
				catch (IOException oException1)
				{
				}
			}
			string[] strArrays = Directory.GetDirectories(dir, "*", SearchOption.AllDirectories);
			for (int k = 0; k < (int)strArrays.Length; k++)
			{
				string str2 = strArrays[k];
				try
				{
					Directory.Delete(str2, true);
				}
				catch (IOException oException2)
				{
				}
			}
		}

		internal EmulatorStorageAccount CreateAccount(string accountName, string primaryKey, string secondaryKey, bool secondaryReadEnabled)
		{
			Account account;
			byte[] numArray = (primaryKey == null ? StorageContext.GenerateKey() : Convert.FromBase64String(primaryKey));
			byte[] numArray1 = (secondaryKey == null ? StorageContext.GenerateKey() : Convert.FromBase64String(secondaryKey));
			using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
			{
				using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
				{
					IEnumerable<Account> accounts = 
						from a in dbContext.Accounts
						where a.Name == accountName
						select a;
					if (accounts != null && accounts.Count<Account>() > 0)
					{
						throw new EmulatorException(EmulatorErrorCode.AccountAlreadyExists);
					}
					IEnumerable<DeletedAccount> deletedAccounts = 
						from a in dbContext.DeletedAccounts
						where a.Name == accountName
						select a;
					if (deletedAccounts != null && deletedAccounts.Count<DeletedAccount>() > 0)
					{
						DeletedAccount deletedAccount = deletedAccounts.Single<DeletedAccount>();
						if ((deletedAccount.DeletionTime + Constants.AccountRecreationTimeLimit) > DateTime.UtcNow)
						{
							throw new EmulatorException(EmulatorErrorCode.NeedToWaitForAccountDeletion);
						}
						dbContext.DeletedAccounts.DeleteOnSubmit(deletedAccount);
					}
					Account account1 = new Account()
					{
						Name = accountName,
						SecretKey = numArray,
						SecondaryKey = numArray1,
						SecondaryReadEnabled = secondaryReadEnabled
					};
					account = account1;
					dbContext.Accounts.InsertOnSubmit(account);
					dbContext.SubmitChanges();
					transactionScope.Complete();
				}
			}
			EmulatorStorageAccount emulatorStorageAccount = new EmulatorStorageAccount(account.Name, Convert.ToBase64String(account.SecretKey), Convert.ToBase64String(account.SecondaryKey), account.SecondaryReadEnabled);
			return emulatorStorageAccount;
		}

		public void CreateDatabase(bool force)
		{
			DevelopmentStorageDbDataContext.CreateDatabase(force);
		}

		internal bool DeleteAccount(string accountName)
		{
			using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
			{
				using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
				{
					IEnumerable<Account> accounts = 
						from a in dbContext.Accounts
						where a.Name == accountName
						select a;
					if (accounts == null || accounts.Count<Account>() == 0)
					{
						throw new EmulatorException(EmulatorErrorCode.AccountNotFound);
					}
					foreach (StorageEmulatorConfigAccount value in StorageEmulatorConfigCache.Configuration.Accounts.Values)
					{
						if (!value.Name.Equals(accountName))
						{
							continue;
						}
						throw new EmulatorException(EmulatorErrorCode.CannotDeleteDefaultAccount);
					}
					DeletedAccount deletedAccount = new DeletedAccount()
					{
						Name = accountName,
						DeletionTime = DateTime.UtcNow
					};
					DeletedAccount deletedAccount1 = deletedAccount;
					dbContext.Accounts.DeleteOnSubmit(accounts.Single<Account>());
					dbContext.DeletedAccounts.InsertOnSubmit(deletedAccount1);
					dbContext.SubmitChanges();
					transactionScope.Complete();
				}
			}
			return true;
		}

		private static byte[] GenerateKey()
		{
			byte[] numArray = new byte[66];
			StorageContext.rnd.NextBytes(numArray);
			return numArray;
		}

		internal EmulatorStorageAccount GetAccount(string accountName)
		{
			EmulatorStorageAccount emulatorStorageAccount;
			using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
			{
				IEnumerable<Account> accounts = 
					from a in dbContext.Accounts
					where a.Name == accountName
					select a;
				if (accounts == null || accounts.Count<Account>() == 0)
				{
					throw new EmulatorException(EmulatorErrorCode.AccountNotFound);
				}
				Account account = accounts.Single<Account>();
				EmulatorStorageAccount emulatorStorageAccount1 = new EmulatorStorageAccount(account.Name, Convert.ToBase64String(account.SecretKey), Convert.ToBase64String(account.SecondaryKey), account.SecondaryReadEnabled);
				emulatorStorageAccount = emulatorStorageAccount1;
			}
			return emulatorStorageAccount;
		}

		public string GetBlockBlobRoot()
		{
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration;
			StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(out storageEmulatorUpdatableConfiguration);
			if (storageEmulatorUpdatableConfiguration == null)
			{
				throw new EmulatorException(EmulatorErrorCode.InitializationRequired);
			}
			string blockBlobRoot = storageEmulatorUpdatableConfiguration.BlockBlobRoot;
			if (!Directory.Exists(blockBlobRoot))
			{
				Directory.CreateDirectory(blockBlobRoot);
			}
			return blockBlobRoot;
		}

		public string GetPageBlobRoot()
		{
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration;
			StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(out storageEmulatorUpdatableConfiguration);
			if (storageEmulatorUpdatableConfiguration == null)
			{
				throw new EmulatorException(EmulatorErrorCode.InitializationRequired);
			}
			string pageBlobRoot = storageEmulatorUpdatableConfiguration.PageBlobRoot;
			if (!Directory.Exists(pageBlobRoot))
			{
				Directory.CreateDirectory(pageBlobRoot);
			}
			return pageBlobRoot;
		}

		public string GrantDbAccess(string user)
		{
			string str;
			string empty = string.Empty;
			using (SqlConnection sqlConnection = new SqlConnection(DevelopmentStorageDbDataContext.GetConnectionString()))
			{
				sqlConnection.Open();
				using (SqlCommand sqlCommand = new SqlCommand(string.Format("USE {0} EXEC sp_helplogins N'{1}'", DevelopmentStorageDbDataContext.DatabaseName, user), sqlConnection))
				{
					SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
					DataSet dataSet = new DataSet();
					sqlDataAdapter.Fill(dataSet);
					foreach (DataRow row in dataSet.Tables[1].Rows)
					{
						if (!(row.Field<string>("DBName").Trim() == Constants.EmulatorDBName) || !(row.Field<string>("UserOrAlias").Trim() == "User"))
						{
							continue;
						}
						empty = row.Field<string>("UserName").Trim();
					}
				}
				if (string.IsNullOrEmpty(empty))
				{
					char[] chrArray = new char[] { '\\' };
					string[] strArrays = user.Split(chrArray, 2);
					empty = ((int)strArrays.Length != 2 ? user : strArrays[1]);
					using (SqlCommand sqlCommand1 = new SqlCommand(string.Format("USE {0} EXEC sp_grantdbaccess N'{1}', N'{2}'", DevelopmentStorageDbDataContext.DatabaseName, user, empty), sqlConnection))
					{
						sqlCommand1.ExecuteNonQuery();
					}
				}
				str = empty;
			}
			return str;
		}

		public void InitializeLogger()
		{
			string storageEmulatorDBName = StorageEmulatorUpdatableConfiguration.GetStorageEmulatorDBName();
			string sqlInstance = StorageEmulatorUpdatableConfiguration.GetSqlInstance();
			DevelopmentStorageDbDataContext.LogDirectory = StorageEmulatorUpdatableConfiguration.GetLogPath();
			DevelopmentStorageDbDataContext.SqlInstance = sqlInstance;
			DevelopmentStorageDbDataContext.DatabaseName = storageEmulatorDBName;
			DevelopmentStorageDbDataContext.PageBlobRoot = this.GetPageBlobRoot();
			DevelopmentStorageDbDataContext.BlockBlobRoot = this.GetBlockBlobRoot();
			DevelopmentStorageDbDataContext.LinqToSqlLogStream = new LoggerProviderTextWriter();
			if (StorageEmulatorUpdatableConfiguration.GetLoggingEnabled())
			{
				LoggerProvider.Instance = new FileLoggerProvider();
				return;
			}
			LoggerProvider.Instance = new DummyLoggerProvider();
		}

		internal IEnumerable<EmulatorStorageAccount> ListAccounts()
		{
			List<Account> list;
			using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
			{
				IEnumerable<Account> accounts = 
					from a in dbContext.Accounts
					select a;
				list = accounts.ToList<Account>();
			}
			List<EmulatorStorageAccount> emulatorStorageAccounts = new List<EmulatorStorageAccount>();
			foreach (Account account in list)
			{
				EmulatorStorageAccount emulatorStorageAccount = new EmulatorStorageAccount(account.Name, Convert.ToBase64String(account.SecretKey), Convert.ToBase64String(account.SecondaryKey), account.SecondaryReadEnabled);
				emulatorStorageAccounts.Add(emulatorStorageAccount);
			}
			return emulatorStorageAccounts;
		}

		public void Log(string msg)
		{
			Logger<IRestProtocolHeadLogger>.Instance.Info.Log(msg);
		}

		public void LogException(Exception error)
		{
			if (!(error is LoggerException))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log(error.Message);
			}
		}

		internal string[] RegenerateKey(string accountName, KeyType keyType)
		{
			byte[] numArray = StorageContext.GenerateKey();
			string base64String = Convert.ToBase64String(numArray);
			string[] strArrays = new string[2];
			using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
			{
				using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
				{
					IEnumerable<Account> accounts = 
						from a in dbContext.Accounts
						where a.Name == accountName
						select a;
					if (accounts == null || accounts.Count<Account>() == 0)
					{
						throw new EmulatorException(EmulatorErrorCode.AccountNotFound);
					}
					Account account = accounts.Single<Account>();
					switch (keyType)
					{
						case KeyType.Primary:
						{
							account.SecretKey = numArray;
							strArrays[0] = base64String;
							strArrays[1] = Convert.ToBase64String(account.SecondaryKey);
							break;
						}
						case KeyType.Secondary:
						{
							account.SecondaryKey = numArray;
							strArrays[0] = Convert.ToBase64String(account.SecretKey);
							strArrays[1] = base64String;
							break;
						}
					}
					dbContext.SubmitChanges();
					transactionScope.Complete();
				}
			}
			return strArrays;
		}

		public void SetDatabaseNameToStorageEmulator()
		{
			DevelopmentStorageDbDataContext.DatabaseName = Constants.EmulatorDBName;
		}

		public void ShrinkDatabase()
		{
			using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
			{
				try
				{
					dbContext.ExecuteCommand(string.Concat("DBCC SHRINKDATABASE(N'", dbContext.Connection.Database, "')"), new object[0]);
					dbContext.ExecuteCommand(string.Concat("DBCC SHRINKFILE (N'", dbContext.Connection.Database, "' , EMPTYFILE)"), new object[0]);
					dbContext.ExecuteCommand(string.Concat("DBCC SHRINKFILE (N'", dbContext.Connection.Database, "_log' , EMPTYFILE)"), new object[0]);
				}
				catch (SqlException sqlException)
				{
				}
			}
		}

		internal EmulatorStorageAccount UpdateAccount(string accountName, bool? secondaryReadEnabled)
		{
			EmulatorStorageAccount emulatorStorageAccount;
			using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
			{
				using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
				{
					IEnumerable<Account> accounts = 
						from a in dbContext.Accounts
						where a.Name == accountName
						select a;
					if (accounts == null || accounts.Count<Account>() == 0)
					{
						throw new EmulatorException(EmulatorErrorCode.AccountNotFound);
					}
					Account account = accounts.Single<Account>();
					Account account1 = account;
					bool? nullable = secondaryReadEnabled;
					account1.SecondaryReadEnabled = (nullable.HasValue ? nullable.GetValueOrDefault() : account.SecondaryReadEnabled);
					emulatorStorageAccount = new EmulatorStorageAccount(account.Name, Convert.ToBase64String(account.SecretKey), Convert.ToBase64String(account.SecondaryKey), account.SecondaryReadEnabled);
					dbContext.SubmitChanges();
					transactionScope.Complete();
				}
			}
			return emulatorStorageAccount;
		}
	}
}