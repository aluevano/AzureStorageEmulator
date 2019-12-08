using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Account
{
	public abstract class AccountAccessRulesIdentifier : AccountIdentifier
	{
		protected AccountAccessRulesIdentifier(string accountName, bool accountIsAdmin, AccountPermissions accountPermissions) : base(accountName, accountIsAdmin, accountPermissions, SecretKeyPermissions.Full, false)
		{
		}

		protected AccountAccessRulesIdentifier(string accountName, bool accountIsAdmin, AccountPermissions accountPermissions, SecretKeyPermissions keyPermissions, bool isAnonymous) : base(accountName, accountIsAdmin, accountPermissions, keyPermissions, isAnonymous)
		{
		}

		public abstract void Authorize(string verb, string resource, string subscription, string accountName);
	}
}