using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Account
{
	public class AccountSasAccessIdentifier : AccountIdentifier
	{
		public SASPermission SignedAccessPermission
		{
			get;
			private set;
		}

		public IPAddressRange SignedIP
		{
			get;
			private set;
		}

		public SasProtocol SignedProtocol
		{
			get;
			private set;
		}

		public SasResourceType SignedResourceType
		{
			get;
			private set;
		}

		public SasService SignedService
		{
			get;
			private set;
		}

		public AccountSasAccessIdentifier(IStorageAccount account, SecretKeyPermissions keyPermission) : base(account, keyPermission)
		{
			this.SignedAccessPermission = SASPermission.None;
			this.SignedProtocol = SasProtocol.All;
		}

		public override void Initialize(SignedAccessHelper helper)
		{
			AccountSasHelper accountSasHelper = (AccountSasHelper)helper;
			this.SignedAccessPermission = accountSasHelper.SignedPermission.Value;
			this.SignedResourceType = accountSasHelper.SignedResourceType;
			this.SignedService = accountSasHelper.SignedService;
			this.SignedIP = accountSasHelper.SignedIP;
			this.SignedProtocol = accountSasHelper.SignedProtocol;
		}
	}
}