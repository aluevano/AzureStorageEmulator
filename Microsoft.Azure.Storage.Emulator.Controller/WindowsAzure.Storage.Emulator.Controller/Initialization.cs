using Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	internal class Initialization
	{
		private const int LocalDBProbeTimeout = 20;

		private const string DefaultSqlInstanceName = "(localdb)\\MSSQLLocalDB";

		private string server;

		private Dictionary<string, string> endpoints = new Dictionary<string, string>();

		private string username;

		private bool forceCreate;

		private bool skipCreate;

		private bool autoDetect;

		private bool reservePorts;

		private bool unreservePorts;

		internal Initialization(bool forceCreate, bool skipCreate, bool autodetect, string server, string sqlInstance, bool reservePorts, bool unreservePorts)
		{
			this.forceCreate = forceCreate;
			this.skipCreate = skipCreate;
			this.autoDetect = autodetect;
			this.server = server;
			this.reservePorts = reservePorts;
			this.unreservePorts = unreservePorts;
			foreach (KeyValuePair<string, StorageEmulatorConfigService> service in StorageEmulatorConfigCache.Configuration.Services)
			{
				string key = service.Key;
				string url = service.Value.Url;
				this.endpoints.Add(key, url);
			}
			this.username = WindowsIdentity.GetCurrent().Name;
			if (sqlInstance != null)
			{
				if (sqlInstance.Equals("."))
				{
					sqlInstance = string.Empty;
				}
				this.server = string.Concat(".\\", sqlInstance);
			}
		}

		private StorageEmulatorUpdatableConfiguration AutoDetectAndUpdate()
		{
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration;
			if (!StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(true, out storageEmulatorUpdatableConfiguration))
			{
				storageEmulatorUpdatableConfiguration = new StorageEmulatorUpdatableConfiguration();
			}
			this.LogMessage(Resource.LocalDBCheckForInstall, new object[0]);
			DBProbe dBProbe = new DBProbe((string message) => this.LogMessage(message, new object[0]), (string message) => this.LogMessage(message, new object[0]));
			string str = this.CheckForLocalDB();
			if (str == null)
			{
				this.LogMessage(Resource.LocalDBInstallNotFound, new object[0]);
				try
				{
					if (!dBProbe.ProbeInstance("localhost\\SQLExpress", 2))
					{
						storageEmulatorUpdatableConfiguration.SqlInstance = string.Empty;
					}
					else
					{
						storageEmulatorUpdatableConfiguration.SqlInstance = "localhost\\SQLExpress";
					}
				}
				catch (ArgumentException argumentException)
				{
					this.LogError("SQL default instance name was rejected: {0}", new object[] { "localhost\\SQLExpress" });
					storageEmulatorUpdatableConfiguration.SqlInstance = string.Empty;
				}
			}
			else
			{
				try
				{
					this.LogMessage(Resource.LocalDBInstallFound, new object[0]);
					if (!dBProbe.ProbeInstanceWithRetry(str, 20, 10))
					{
						storageEmulatorUpdatableConfiguration.SqlInstance = string.Empty;
						this.LogError(Resource.LocalDBInstalledButNotRunning, new object[0]);
					}
					else
					{
						storageEmulatorUpdatableConfiguration.SqlInstance = str;
					}
				}
				catch (ArgumentException argumentException1)
				{
					this.LogError("LocalDB default instance name was rejected: {0}", new object[] { str });
					storageEmulatorUpdatableConfiguration.SqlInstance = string.Empty;
					this.LogError(Resource.LocalDBInstalledButNotRunning, new object[0]);
				}
			}
			this.server = storageEmulatorUpdatableConfiguration.SqlInstance;
			storageEmulatorUpdatableConfiguration.WriteToUserProfile();
			return storageEmulatorUpdatableConfiguration;
		}

		private string CheckForLocalDB()
		{
			DBProbe dBProbe = new DBProbe((string message) => this.LogMessage(message, new object[0]), (string message) => this.LogMessage(message, new object[0]));
			string str = "(localdb)\\MSSQLLocalDB";
			try
			{
				if (dBProbe.ProbeInstance(str, 20))
				{
					return str;
				}
			}
			catch (ArgumentException argumentException)
			{
				this.LogError("LocalDB default instance name was rejected: {0}", new object[] { str });
			}
			return null;
		}

		private void CreateDatabaseAndGrantAccess(IStorageContext context)
		{
			string empty = string.Empty;
			context.SetDatabaseNameToStorageEmulator();
			string creatingDatabase = Resource.CreatingDatabase;
			object[] databaseName = new object[] { context.DatabaseName, context.InstanceName };
			this.LogMessage(creatingDatabase, databaseName);
			context.CreateDatabase(this.forceCreate);
			this.LogNewline();
			this.LogMessage(Resource.GrantingDatabaseAccess, new object[] { this.username });
			empty = context.GrantDbAccess(this.username);
			this.LogMessage(Resource.GrantedDatabaseAccess, new object[] { this.username });
			if (empty != "dbo")
			{
				try
				{
					this.LogMessage(Resource.AddingDatabaseRole, new object[] { this.username });
					context.AddRoleMember(empty);
					this.LogMessage(Resource.AddedDatabaseRole, new object[] { this.username });
				}
				catch (SqlException sqlException1)
				{
					SqlException sqlException = sqlException1;
					if (sqlException.Number != 15410)
					{
						throw;
					}
					else
					{
						this.LogMessage(sqlException.Message, new object[0]);
					}
				}
			}
		}

		private IStorageContext GetContext()
		{
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration;
			if (this.autoDetect)
			{
				this.LogMessage(Resource.AutoDetectRequested, new object[0]);
				storageEmulatorUpdatableConfiguration = this.AutoDetectAndUpdate();
			}
			else if (!string.IsNullOrEmpty(this.server))
			{
				this.LogMessage(Resource.ServerSpecified, new object[0]);
				storageEmulatorUpdatableConfiguration = this.ProbeInstanceAndUpdate(this.server);
			}
			else if (!StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(out storageEmulatorUpdatableConfiguration))
			{
				this.LogMessage(Resource.NoConfigurationAutoDetecting, new object[0]);
				storageEmulatorUpdatableConfiguration = this.AutoDetectAndUpdate();
			}
			else if (string.IsNullOrEmpty(storageEmulatorUpdatableConfiguration.SqlInstance))
			{
				this.LogMessage(Resource.EmptyInstanceAutoDetecting, new object[0]);
				storageEmulatorUpdatableConfiguration = this.AutoDetectAndUpdate();
			}
			if (string.IsNullOrEmpty(storageEmulatorUpdatableConfiguration.SqlInstance))
			{
				this.LogErrorAndThrowException(EmulatorErrorCode.NoSqlInstanceFound, null);
			}
			this.LogMessage(Resource.FoundSqlInstance, new object[] { storageEmulatorUpdatableConfiguration.SqlInstance });
			return new StorageContext()
			{
				InstanceName = storageEmulatorUpdatableConfiguration.SqlInstance
			};
		}

		internal void LaunchInitializationElevated()
		{
			if (this.server != null && this.server.EndsWith("\\\\"))
			{
				this.server = this.server.Substring(0, this.server.Length - 1);
			}
			Process process = new Process()
			{
				EnableRaisingEvents = true
			};
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("init");
			stringBuilder.Append(" /inprocess");
			if (this.forceCreate)
			{
				stringBuilder.Append(" /forcecreate");
			}
			if (this.skipCreate)
			{
				stringBuilder.Append(" /skipcreate");
			}
			if (this.autoDetect)
			{
				stringBuilder.Append(" /autodetect");
			}
			if (this.reservePorts)
			{
				stringBuilder.Append(" /reserveports");
			}
			if (this.unreservePorts)
			{
				stringBuilder.Append(" /unreserveports");
			}
			if (!string.IsNullOrWhiteSpace(this.server))
			{
				stringBuilder.AppendFormat(" /server \"{0}", this.server);
			}
			ProcessWrapper processWrapper = new ProcessWrapper("AzureStorageEmulator.exe", stringBuilder.ToString());
			process.StartInfo = processWrapper.StartInfo;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.Verb = "runas";
			try
			{
				process.Start();
				process.WaitForExit();
			}
			catch (Win32Exception win32Exception1)
			{
				Win32Exception win32Exception = win32Exception1;
				throw new EmulatorException(EmulatorErrorCode.UnknownError, win32Exception.Message, win32Exception);
			}
			EmulatorErrorCode exitCode = (EmulatorErrorCode)process.ExitCode;
			if (!Enum.IsDefined(typeof(EmulatorErrorCode), (int)exitCode))
			{
				exitCode = EmulatorErrorCode.UnknownError;
			}
			if (exitCode != EmulatorErrorCode.Success)
			{
				throw new EmulatorException(exitCode);
			}
		}

		private void LogError(string format, params object[] args)
		{
			Console.Error.WriteLine(format, args);
		}

		private void LogErrorAndThrowException(EmulatorErrorCode errorCode, string errorMessage = null)
		{
			EmulatorException emulatorException = new EmulatorException(errorCode, errorMessage);
			this.LogError(emulatorException.Message, new object[0]);
			this.LogError(Resource.ActionsFailed, new object[0]);
			throw emulatorException;
		}

		private void LogMessage(string format, params object[] args)
		{
			Console.WriteLine(format, args);
		}

		private void LogNewline()
		{
			Console.WriteLine();
		}

		internal void PerformEmulatorInitialization()
		{
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration;
			IStorageContext context = null;
			try
			{
				using (EventWaitHandle eventWaitHandle = EventWaitHandle.OpenExisting("DevelopmentStorage-7580AFBB-2BEC-4269-B083-46C1361A69B0"))
				{
					this.LogErrorAndThrowException(EmulatorErrorCode.StopRequired, null);
				}
			}
			catch (WaitHandleCannotBeOpenedException waitHandleCannotBeOpenedException)
			{
			}
			if (this.reservePorts || this.unreservePorts)
			{
				try
				{
					foreach (string value in this.endpoints.Values)
					{
						if (!this.unreservePorts)
						{
							HttpReservation.ModifyReservation(value, this.username, true);
							string addedReservation = Resource.AddedReservation;
							object[] objArray = new object[] { value, this.username };
							this.LogMessage(addedReservation, objArray);
						}
						else
						{
							if (!HttpReservation.TryDeleteReservation(value, this.username))
							{
								continue;
							}
							string removedReservation = Resource.RemovedReservation;
							object[] objArray1 = new object[] { value, this.username };
							this.LogMessage(removedReservation, objArray1);
						}
					}
				}
				catch (HttpReservationException httpReservationException)
				{
					string str = string.Format(Resource.ReservationFailed, httpReservationException.Message);
					this.LogErrorAndThrowException(EmulatorErrorCode.ReservationFailed, str);
				}
				catch (IdentityNotMappedException identityNotMappedException1)
				{
					IdentityNotMappedException identityNotMappedException = identityNotMappedException1;
					string str1 = string.Format(Resource.ReservationFailedIncorrectUser, this.username, identityNotMappedException.Message);
					this.LogErrorAndThrowException(EmulatorErrorCode.ReservationFailedIncorrectUser, str1);
				}
				this.LogNewline();
			}
			if (!this.skipCreate)
			{
				context = this.GetContext();
				try
				{
					this.CreateDatabaseAndGrantAccess(context);
					if (!StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(out storageEmulatorUpdatableConfiguration))
					{
						storageEmulatorUpdatableConfiguration = new StorageEmulatorUpdatableConfiguration();
					}
					storageEmulatorUpdatableConfiguration.SqlInstance = this.server;
				}
				catch (SqlException sqlException1)
				{
					SqlException sqlException = sqlException1;
					string str2 = string.Format(Resource.DatabaseCreationFailed, context.DatabaseName, sqlException.Message);
					this.LogErrorAndThrowException(EmulatorErrorCode.DatabaseCreationFailed, str2);
				}
				catch (InvalidOperationException invalidOperationException1)
				{
					InvalidOperationException invalidOperationException = invalidOperationException1;
					string str3 = string.Format(Resource.DatabaseCreationFailed, context.DatabaseName, invalidOperationException.Message);
					this.LogErrorAndThrowException(EmulatorErrorCode.DatabaseCreationFailed, str3);
				}
				catch (IOException oException1)
				{
					IOException oException = oException1;
					string str4 = string.Format(Resource.DatabaseCreationFailed, context.DatabaseName, oException.Message);
					this.LogErrorAndThrowException(EmulatorErrorCode.DatabaseCreationFailed, str4);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					string str5 = string.Format(Resource.DatabaseCreationFailed, context.DatabaseName, exception.Message);
					this.LogErrorAndThrowException(EmulatorErrorCode.DatabaseCreationFailed, str5);
				}
				this.LogNewline();
			}
			this.LogMessage(Resource.ActionsSucceeded, new object[0]);
		}

		private StorageEmulatorUpdatableConfiguration ProbeInstanceAndUpdate(string instanceName)
		{
			StorageEmulatorUpdatableConfiguration storageEmulatorUpdatableConfiguration;
			if (!StorageEmulatorUpdatableConfiguration.TryGetFromUserProfile(true, out storageEmulatorUpdatableConfiguration))
			{
				storageEmulatorUpdatableConfiguration = new StorageEmulatorUpdatableConfiguration();
			}
			if (!string.IsNullOrEmpty(instanceName))
			{
				this.LogMessage(Resource.UserSpecifiedInstance, new object[0]);
				DBProbe dBProbe = new DBProbe((string message) => this.LogMessage(message, new object[0]), (string message) => this.LogMessage(message, new object[0]));
				try
				{
					if (!dBProbe.ProbeInstanceWithRetry(instanceName, 20, 10))
					{
						this.LogErrorAndThrowException(EmulatorErrorCode.UserSpecifiedSqlInstanceNotFound, null);
					}
					else
					{
						storageEmulatorUpdatableConfiguration.SqlInstance = instanceName;
						this.server = storageEmulatorUpdatableConfiguration.SqlInstance;
						storageEmulatorUpdatableConfiguration.WriteToUserProfile();
					}
				}
				catch (ArgumentException argumentException)
				{
					this.LogError("SQL instance name is rejected as malformed: {0}", new object[] { instanceName });
					this.LogErrorAndThrowException(EmulatorErrorCode.UserSpecifiedSqlInstanceNotFound, null);
				}
			}
			return storageEmulatorUpdatableConfiguration;
		}
	}
}