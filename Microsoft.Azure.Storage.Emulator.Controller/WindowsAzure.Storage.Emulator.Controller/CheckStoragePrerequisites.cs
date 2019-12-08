using Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Principal;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	public class CheckStoragePrerequisites
	{
		public CheckStoragePrerequisites()
		{
		}

		internal static PrerequisiteCheckResult CheckDbAccess(string sqlInstance)
		{
			PrerequisiteCheckResult prerequisiteCheckResult;
			PrerequisiteCheckResult prerequisiteCheckResult1 = PrerequisiteCheckResult.None;
			string masterConnectionString = StorageEmulatorUpdatableConfiguration.GetMasterConnectionString();
			string storageEmulatorDBName = StorageEmulatorUpdatableConfiguration.GetStorageEmulatorDBName();
			List<string> strs = new List<string>();
			string name = WindowsIdentity.GetCurrent().Name;
			char[] chrArray = new char[] { '\\' };
			string[] strArrays = name.Split(chrArray, 2);
			if ((int)strArrays.Length == 2)
			{
				string str = strArrays[1];
			}
			else
			{
				string str1 = strArrays[0];
			}
			using (SqlConnection sqlConnection = new SqlConnection(masterConnectionString))
			{
				sqlConnection.Open();
				string str2 = string.Concat("SELECT HAS_DBACCESS('", storageEmulatorDBName, "')");
				using (SqlCommand sqlCommand = new SqlCommand(str2, sqlConnection))
				{
					object obj = sqlCommand.ExecuteScalar();
					if (!(obj is int))
					{
						prerequisiteCheckResult = PrerequisiteCheckResult.NoDatabaseAccess;
						return prerequisiteCheckResult;
					}
					else if ((int)obj != 1)
					{
						prerequisiteCheckResult = PrerequisiteCheckResult.NoDatabaseAccess;
						return prerequisiteCheckResult;
					}
				}
				string str3 = string.Concat("USE ", storageEmulatorDBName, " SELECT IS_MEMBER ('db_datareader'),IS_MEMBER ('db_datawriter')");
				int item = 0;
				int num = 0;
				using (SqlCommand sqlCommand1 = new SqlCommand(str3, sqlConnection))
				{
					using (SqlDataReader sqlDataReader = sqlCommand1.ExecuteReader())
					{
						if (sqlDataReader.HasRows)
						{
							while (sqlDataReader.Read())
							{
								item = (int)sqlDataReader[0];
								num = (int)sqlDataReader[1];
							}
						}
						prerequisiteCheckResult1 = (item != 1 || num != 1 ? PrerequisiteCheckResult.NoDatabaseAccess : PrerequisiteCheckResult.HasDatabaseAccess);
					}
				}
				return prerequisiteCheckResult1;
			}
			return prerequisiteCheckResult;
		}

		private static bool CheckPortReservation(Action<string> showErrorMessage)
		{
			Uri uri;
			bool flag;
			HttpListener httpListener = new HttpListener();
			foreach (string key in StorageEmulatorConfigCache.Configuration.Services.Keys)
			{
				string url = StorageEmulatorConfigCache.Configuration.Services[key].Url;
				if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
				{
					CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
					object[] objArray = new object[] { url };
					showErrorMessage(string.Format(currentUICulture, "Invalid uri '{0}' in configuration", objArray));
					flag = false;
					return flag;
				}
				else
				{
					httpListener.Prefixes.Add(url);
				}
			}
			try
			{
				try
				{
					httpListener.Start();
					flag = true;
					return flag;
				}
				catch (HttpListenerException httpListenerException1)
				{
					HttpListenerException httpListenerException = httpListenerException1;
					if (httpListenerException.ErrorCode == 183)
					{
						showErrorMessage("Port conflict with existing application.");
						flag = false;
						return flag;
					}
					else if (httpListenerException.ErrorCode == 5)
					{
						flag = false;
						return flag;
					}
				}
				return true;
			}
			finally
			{
				if (httpListener.IsListening)
				{
					httpListener.Stop();
				}
			}
			return flag;
		}

		public static bool CheckPrerequisites(Action<string> showErrorMessage, out bool fixWithInit)
		{
			string str;
			fixWithInit = false;
			PrerequisiteCheckResult prerequisiteCheckResult = CheckStoragePrerequisites.CheckSqlInstanceAndDatabase(out str);
			if (prerequisiteCheckResult == PrerequisiteCheckResult.DatabaseNotInstalledOrLoginFailed || prerequisiteCheckResult == PrerequisiteCheckResult.SqlExpressInstanceNotInstalledOrRunning)
			{
				fixWithInit = true;
				return false;
			}
			prerequisiteCheckResult = PrerequisiteCheckResult.None;
			prerequisiteCheckResult = CheckStoragePrerequisites.CheckDbAccess(str);
			if (prerequisiteCheckResult == PrerequisiteCheckResult.NoDatabaseAccess)
			{
				fixWithInit = true;
				return false;
			}
			fixWithInit = false;
			return CheckStoragePrerequisites.CheckPortReservation(showErrorMessage);
		}

		internal static PrerequisiteCheckResult CheckSqlInstanceAndDatabase(out string sqlInstance)
		{
			PrerequisiteCheckResult prerequisiteCheckResult = PrerequisiteCheckResult.None;
			string storageEmulatorDBName = StorageEmulatorUpdatableConfiguration.GetStorageEmulatorDBName();
			sqlInstance = StorageEmulatorUpdatableConfiguration.GetSqlInstance();
			string masterConnectionString = StorageEmulatorUpdatableConfiguration.GetMasterConnectionString();
			int num = 10;
			bool flag = true;
			while (flag && num > 0)
			{
				flag = false;
				num--;
				using (SqlConnection sqlConnection = new SqlConnection(masterConnectionString))
				{
					try
					{
						try
						{
							Trace.WriteLine("Attempting to connect to db");
							sqlConnection.Open();
							using (SqlCommand sqlCommand = new SqlCommand("SELECT name FROM sys.databases WHERE name = @name", sqlConnection))
							{
								sqlCommand.Parameters.Add(new SqlParameter("@name", storageEmulatorDBName));
								using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
								{
									prerequisiteCheckResult = (!sqlDataReader.HasRows ? PrerequisiteCheckResult.DatabaseNotInstalledOrLoginFailed : PrerequisiteCheckResult.DatabaseInstalled);
								}
							}
						}
						catch (SqlException sqlException)
						{
							if (sqlException.Number != 4060)
							{
								prerequisiteCheckResult = PrerequisiteCheckResult.SqlExpressInstanceNotInstalledOrRunning;
								if (sqlInstance.Contains("(localdb)"))
								{
									flag = true;
								}
							}
							else
							{
								prerequisiteCheckResult = PrerequisiteCheckResult.DatabaseNotInstalledOrLoginFailed;
							}
						}
					}
					finally
					{
						if (sqlConnection != null)
						{
							Trace.WriteLine(string.Format("Connection state is: {0}", sqlConnection.State));
							SqlConnection.ClearPool(sqlConnection);
						}
					}
				}
			}
			return prerequisiteCheckResult;
		}
	}
}