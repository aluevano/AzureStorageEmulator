using System;
using System.Collections;
using System.Data.SqlClient;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	public class DBProbe
	{
		private Action<string> logMessage;

		private Action<string> logDebug;

		public DBProbe(Action<string> logMessage, Action<string> logDebug)
		{
			this.logMessage = logMessage;
			this.logDebug = logDebug;
		}

		public bool ProbeInstance(string instanceName, int timeout)
		{
			bool flag;
			if (this.logMessage != null)
			{
				this.logMessage(string.Format(Resource.ProbingSqlInstance, instanceName));
			}
			string str = string.Format("Data Source={0};Integrated security=SSPI;timeout={1};", instanceName, timeout);
			try
			{
				using (SqlConnection sqlConnection = new SqlConnection(str))
				{
					try
					{
						sqlConnection.Open();
					}
					finally
					{
						SqlConnection.ClearPool(sqlConnection);
					}
				}
				return true;
			}
			catch (SqlException sqlException1)
			{
				SqlException sqlException = sqlException1;
				if (this.logDebug != null)
				{
					this.logDebug(string.Format("Caught exception while probing for SQL endpoint. {0}", sqlException.Message));
					if (sqlException.Errors != null)
					{
						this.logDebug(string.Format("Number of SqlErrors Reported: {0}", sqlException.Errors.Count));
						foreach (SqlError error in sqlException.Errors)
						{
							this.logDebug(string.Format("SqlError: {0}", error.ToString()));
						}
					}
				}
				flag = false;
			}
			return flag;
		}

		public bool ProbeInstanceWithRetry(string instanceName, int timeout, int numRetries)
		{
			int num = numRetries;
			bool flag = this.ProbeInstance(instanceName, timeout);
			while (!flag && num > 0)
			{
				if (this.logMessage != null)
				{
					this.logMessage(string.Format(Resource.FormatRetryProbe, numRetries - num + 1, numRetries));
				}
				flag = this.ProbeInstance(instanceName, timeout);
				num--;
			}
			return flag;
		}
	}
}