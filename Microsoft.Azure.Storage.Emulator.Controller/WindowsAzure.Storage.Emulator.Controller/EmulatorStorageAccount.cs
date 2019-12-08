using System;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	public class EmulatorStorageAccount
	{
		private string accountName;

		private string primaryKey;

		private string secondaryKey;

		private bool secondaryReadEnabled;

		public string AccountName
		{
			get
			{
				return this.accountName;
			}
			set
			{
				this.accountName = value;
			}
		}

		public string PrimaryKey
		{
			get
			{
				return this.primaryKey;
			}
			set
			{
				this.primaryKey = value;
			}
		}

		public string SecondaryKey
		{
			get
			{
				return this.secondaryKey;
			}
			set
			{
				this.secondaryKey = value;
			}
		}

		public bool SecondaryReadEnabled
		{
			get
			{
				return this.secondaryReadEnabled;
			}
			set
			{
				this.secondaryReadEnabled = value;
			}
		}

		public EmulatorStorageAccount(string accountName, string primaryKey, string secondaryKey, bool secondaryReadEnabled)
		{
			this.accountName = accountName;
			this.primaryKey = primaryKey;
			this.secondaryKey = secondaryKey;
			this.secondaryReadEnabled = secondaryReadEnabled;
		}
	}
}