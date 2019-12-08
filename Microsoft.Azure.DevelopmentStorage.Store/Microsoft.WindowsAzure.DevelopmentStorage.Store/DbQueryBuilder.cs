using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.UtilityComputing;
using System;
using System.Linq;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbQueryBuilder
	{
		public DbQueryBuilder()
		{
		}

		public static IQueryable<UtilityRow> CreateRowQuery(string accountName, string tableName, DbTableDataContext context)
		{
			DbTableRowQueryProvider<UtilityRow> dbTableRowQueryProvider = new DbTableRowQueryProvider<UtilityRow>(context, accountName, tableName);
			if (context.SignedAccountIdentifier != null && (context.SignedAccountIdentifier.StartingPartitionKey != null || context.SignedAccountIdentifier.StartingRowKey != null || context.SignedAccountIdentifier.EndingPartitionKey != null || context.SignedAccountIdentifier.EndingRowKey != null))
			{
				KeyBounds keyBound = new KeyBounds()
				{
					MinPartitionKey = context.SignedAccountIdentifier.StartingPartitionKey,
					MinRowKey = context.SignedAccountIdentifier.StartingRowKey,
					MaxPartitionKey = context.SignedAccountIdentifier.EndingPartitionKey,
					MaxRowKey = context.SignedAccountIdentifier.EndingRowKey
				};
				dbTableRowQueryProvider.SASKeyBounds = keyBound;
			}
			return new DbTableRowQueryable<UtilityRow>(dbTableRowQueryProvider);
		}

		public static IQueryable<UtilityTable> CreateTableQuery(string accountName, DbTableDataContext context)
		{
			return new DbTableRowQueryable<UtilityTable>(new DbTableRowQueryProvider<UtilityTable>(context, accountName, "Tables"));
		}
	}
}