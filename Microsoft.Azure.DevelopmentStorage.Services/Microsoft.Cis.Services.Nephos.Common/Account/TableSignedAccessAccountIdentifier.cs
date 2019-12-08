using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Account
{
	public class TableSignedAccessAccountIdentifier : SignedAccessAccountIdentifier
	{
		public string EndingPartitionKey
		{
			get;
			private set;
		}

		public string EndingRowKey
		{
			get;
			private set;
		}

		public string StartingPartitionKey
		{
			get;
			private set;
		}

		public string StartingRowKey
		{
			get;
			private set;
		}

		public string TableName
		{
			get;
			private set;
		}

		public TableSignedAccessAccountIdentifier(IAccountIdentifier identifier, string tableName, string startingPartitionKey, string startingRowKey, string endingPartitionKey, string endingRowKey) : base(identifier)
		{
			this.TableName = tableName;
			this.StartingPartitionKey = startingPartitionKey;
			this.StartingRowKey = startingRowKey;
			this.EndingPartitionKey = endingPartitionKey;
			this.EndingRowKey = endingRowKey;
		}

		public TableSignedAccessAccountIdentifier(IStorageAccount account, string tableName, string startingPartitionKey, string startingRowKey, string endingPartitionKey, string endingRowKey, SecretKeyPermissions keyPermissions) : base(account, keyPermissions)
		{
			this.TableName = tableName;
			this.StartingPartitionKey = startingPartitionKey;
			this.StartingRowKey = startingRowKey;
			this.EndingPartitionKey = endingPartitionKey;
			this.EndingRowKey = endingRowKey;
		}
	}
}