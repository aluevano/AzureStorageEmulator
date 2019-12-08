using Microsoft.Cis.Services.Nephos.Common.Account;
using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.DataModel
{
	public interface IUtilityTableDataContextFactory
	{
		IUtilityTableDataContext CreateDataContext(IAccountIdentifier accountIdentifier, string resourceOwner, string restVersion);
	}
}