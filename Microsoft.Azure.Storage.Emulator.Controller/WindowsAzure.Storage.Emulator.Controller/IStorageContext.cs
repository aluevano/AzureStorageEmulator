using System;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	public interface IStorageContext
	{
		string DatabaseName
		{
			get;
		}

		string InstanceName
		{
			get;
			set;
		}

		void AddRoleMember(string databaseUserName);

		bool CheckDbExists();

		void CleanBlobData();

		void CleanQueueData();

		void CleanTableData();

		void CreateDatabase(bool force);

		string GetBlockBlobRoot();

		string GetPageBlobRoot();

		string GrantDbAccess(string user);

		void InitializeLogger();

		void Log(string msg);

		void LogException(Exception error);

		void SetDatabaseNameToStorageEmulator();

		void ShrinkDatabase();
	}
}