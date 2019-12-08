using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Account
{
	public class SignedAccessAccountIdentifier : AccountIdentifier
	{
		public SASPermission SignedAccessPermission
		{
			get;
			set;
		}

		public List<SASAccessRestriction> SignedAccessRestrictions
		{
			get;
			set;
		}

		public IPAddressRange SignedIP
		{
			get;
			set;
		}

		public SasProtocol SignedProtocol
		{
			get;
			set;
		}

		public SignedAccessAccountIdentifier(string accountName, bool accountIsAdmin, AccountPermissions accountPermissions, SASPermission signedAccessPermission, List<SASAccessRestriction> signedAccessRestrictions) : base(accountName, accountIsAdmin, accountPermissions)
		{
			this.SignedAccessPermission = signedAccessPermission;
			this.SignedAccessRestrictions = signedAccessRestrictions;
			this.SignedProtocol = SasProtocol.All;
		}

		public SignedAccessAccountIdentifier(IAccountIdentifier identifier) : base(identifier)
		{
			if (!identifier.IsAnonymous.HasValue)
			{
				base.IsAnonymous = new bool?(true);
			}
			this.SignedAccessRestrictions = new List<SASAccessRestriction>();
			this.SignedProtocol = SasProtocol.All;
		}

		public SignedAccessAccountIdentifier(IStorageAccount account, SecretKeyPermissions keyPermission) : base(account, keyPermission)
		{
			this.SignedAccessRestrictions = new List<SASAccessRestriction>();
			this.SignedProtocol = SasProtocol.All;
		}

		public override void Initialize(SignedAccessHelper helper)
		{
			if (helper.SignedPermission.HasValue)
			{
				this.SignedAccessPermission = helper.SignedPermission.Value;
			}
			this.SignedAccessRestrictions = helper.AccessRestrictions;
			this.SignedIP = helper.SignedIP;
			this.SignedProtocol = helper.SignedProtocol;
		}
	}
}