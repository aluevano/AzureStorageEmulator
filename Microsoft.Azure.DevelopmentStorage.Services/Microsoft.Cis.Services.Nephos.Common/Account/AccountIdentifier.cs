using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Account
{
	public class AccountIdentifier : IAccountIdentifier
	{
		public string AccountName
		{
			get
			{
				return JustDecompileGenerated_get_AccountName();
			}
			set
			{
				JustDecompileGenerated_set_AccountName(value);
			}
		}

		private string JustDecompileGenerated_AccountName_k__BackingField;

		public string JustDecompileGenerated_get_AccountName()
		{
			return this.JustDecompileGenerated_AccountName_k__BackingField;
		}

		private void JustDecompileGenerated_set_AccountName(string value)
		{
			this.JustDecompileGenerated_AccountName_k__BackingField = value;
		}

		public bool CanBypassAccountNetworkAcls
		{
			get;
			set;
		}

		public bool IsAdmin
		{
			get
			{
				return JustDecompileGenerated_get_IsAdmin();
			}
			set
			{
				JustDecompileGenerated_set_IsAdmin(value);
			}
		}

		private bool JustDecompileGenerated_IsAdmin_k__BackingField;

		public bool JustDecompileGenerated_get_IsAdmin()
		{
			return this.JustDecompileGenerated_IsAdmin_k__BackingField;
		}

		private void JustDecompileGenerated_set_IsAdmin(bool value)
		{
			this.JustDecompileGenerated_IsAdmin_k__BackingField = value;
		}

		public bool? IsAnonymous
		{
			get
			{
				return JustDecompileGenerated_get_IsAnonymous();
			}
			set
			{
				JustDecompileGenerated_set_IsAnonymous(value);
			}
		}

		private bool? JustDecompileGenerated_IsAnonymous_k__BackingField;

		public bool? JustDecompileGenerated_get_IsAnonymous()
		{
			return this.JustDecompileGenerated_IsAnonymous_k__BackingField;
		}

		protected void JustDecompileGenerated_set_IsAnonymous(bool? value)
		{
			this.JustDecompileGenerated_IsAnonymous_k__BackingField = value;
		}

		public bool IsDeleteAllowed
		{
			get
			{
				if (this.IsFullPermissions)
				{
					return true;
				}
				if ((this.Permissions & AccountPermissions.Delete) != AccountPermissions.Delete)
				{
					return false;
				}
				return (this.KeyUsedPermissions & SecretKeyPermissions.Delete) == SecretKeyPermissions.Delete;
			}
		}

		public bool IsFullControlAllowed
		{
			get
			{
				return this.IsFullPermissions;
			}
		}

		private bool IsFullPermissions
		{
			get
			{
				if ((this.Permissions & AccountPermissions.Full) != AccountPermissions.Full)
				{
					return false;
				}
				return (this.KeyUsedPermissions & SecretKeyPermissions.Full) == SecretKeyPermissions.Full;
			}
		}

		public bool IsKeyDisabled
		{
			get
			{
				return this.KeyUsedPermissions == SecretKeyPermissions.None;
			}
		}

		public bool IsReadAllowed
		{
			get
			{
				if (this.IsFullPermissions)
				{
					return true;
				}
				if ((this.Permissions & AccountPermissions.Read) != AccountPermissions.Read)
				{
					return false;
				}
				return (this.KeyUsedPermissions & SecretKeyPermissions.Read) == SecretKeyPermissions.Read;
			}
		}

		public bool IsSecondaryAccess
		{
			get;
			set;
		}

		public bool IsWriteAllowed
		{
			get
			{
				if (this.IsFullPermissions)
				{
					return true;
				}
				if ((this.Permissions & AccountPermissions.Write) != AccountPermissions.Write)
				{
					return false;
				}
				return (this.KeyUsedPermissions & SecretKeyPermissions.Write) == SecretKeyPermissions.Write;
			}
		}

		public bool IsWriteAllowedBySecretKey
		{
			get
			{
				return (this.KeyUsedPermissions & SecretKeyPermissions.Write) == SecretKeyPermissions.Write;
			}
		}

		public SecretKeyPermissions KeyUsedPermissions
		{
			get
			{
				return JustDecompileGenerated_get_KeyUsedPermissions();
			}
			set
			{
				JustDecompileGenerated_set_KeyUsedPermissions(value);
			}
		}

		private SecretKeyPermissions JustDecompileGenerated_KeyUsedPermissions_k__BackingField;

		public SecretKeyPermissions JustDecompileGenerated_get_KeyUsedPermissions()
		{
			return this.JustDecompileGenerated_KeyUsedPermissions_k__BackingField;
		}

		private void JustDecompileGenerated_set_KeyUsedPermissions(SecretKeyPermissions value)
		{
			this.JustDecompileGenerated_KeyUsedPermissions_k__BackingField = value;
		}

		public AccountPermissions Permissions
		{
			get
			{
				return JustDecompileGenerated_get_Permissions();
			}
			set
			{
				JustDecompileGenerated_set_Permissions(value);
			}
		}

		private AccountPermissions JustDecompileGenerated_Permissions_k__BackingField;

		public AccountPermissions JustDecompileGenerated_get_Permissions()
		{
			return this.JustDecompileGenerated_Permissions_k__BackingField;
		}

		private void JustDecompileGenerated_set_Permissions(AccountPermissions value)
		{
			this.JustDecompileGenerated_Permissions_k__BackingField = value;
		}

		public AccountIdentifier(string accountName, bool accountIsAdmin, AccountPermissions accountPermissions) : this(accountName, accountIsAdmin, accountPermissions, SecretKeyPermissions.Full, false)
		{
		}

		public AccountIdentifier(string accountName, bool accountIsAdmin, AccountPermissions accountPermissions, SecretKeyPermissions keyPermissions, bool isAnonymous)
		{
			this.AccountName = accountName;
			this.IsAdmin = accountIsAdmin;
			this.Permissions = accountPermissions;
			this.KeyUsedPermissions = keyPermissions;
			this.IsAnonymous = new bool?(isAnonymous);
		}

		public AccountIdentifier(IStorageAccount account, SecretKeyPermissions keyUsedPermissions)
		{
			if (account == null)
			{
				throw new ArgumentNullException("account", "account cannot be null");
			}
			NephosAssertionException.Assert(account.Permissions.HasValue, "account.Permissions cannot be null");
			this.AccountName = account.Name;
			this.IsSecondaryAccess = account.IsSecondaryAccess;
			this.Permissions = account.Permissions.Value;
			this.KeyUsedPermissions = keyUsedPermissions;
		}

		public AccountIdentifier(IAccountIdentifier identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("src");
			}
			this.AccountName = identifier.AccountName;
			this.Permissions = identifier.Permissions;
			this.KeyUsedPermissions = identifier.KeyUsedPermissions;
			this.IsAnonymous = identifier.IsAnonymous;
		}

		public virtual void Initialize(SignedAccessHelper helper)
		{
		}
	}
}