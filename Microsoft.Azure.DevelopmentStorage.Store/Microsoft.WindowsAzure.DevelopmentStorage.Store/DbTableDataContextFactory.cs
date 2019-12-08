using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Table.Service.DataModel;
using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public class DbTableDataContextFactory : IUtilityTableDataContextFactory
	{
		public DbTableDataContextFactory()
		{
		}

		public static void ClearCache()
		{
		}

		public IUtilityTableDataContext CreateDataContext(IAccountIdentifier accountIdentifier, string resourceOwner, string restVersion)
		{
			return new DbTableDataContext(accountIdentifier, resourceOwner)
			{
				ApiVersion = restVersion
			};
		}
	}
}