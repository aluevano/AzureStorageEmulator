using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public struct AccountPropertyNames
	{
		public readonly static AccountPropertyNames None;

		public readonly static AccountPropertyNames All;

		public readonly static AccountPropertyNames ServiceMetadata;

		public readonly static AccountPropertyNames SecretKeys;

		public AccountLevelPropertyNames PropertyNames;

		public AccountServiceMetadataPropertyNames ServiceMetadataPropertyNames;

		static AccountPropertyNames()
		{
			AccountPropertyNames.None = new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)0));
			AccountPropertyNames.All = new AccountPropertyNames(AccountLevelPropertyNames.SecretKeys, (AccountServiceMetadataPropertyNames)((long)2013773824));
			AccountPropertyNames.ServiceMetadata = new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)2013773824));
			AccountPropertyNames.SecretKeys = new AccountPropertyNames(AccountLevelPropertyNames.SecretKeys, (AccountServiceMetadataPropertyNames)((long)0));
		}

		public AccountPropertyNames(AccountLevelPropertyNames propertyNames, AccountServiceMetadataPropertyNames serviceMetadataPropertyNames)
		{
			this.PropertyNames = propertyNames;
			this.ServiceMetadataPropertyNames = serviceMetadataPropertyNames;
		}

		public override string ToString()
		{
			return string.Format("PropertyNames=0x{0:X}, ServiceMetadataPropertyNames=0x{1:X}", this.PropertyNames, this.ServiceMetadataPropertyNames);
		}
	}
}