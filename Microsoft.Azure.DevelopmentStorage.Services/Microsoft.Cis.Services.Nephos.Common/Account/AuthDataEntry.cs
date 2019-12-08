using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Account
{
	public class AuthDataEntry
	{
		private string keyName;

		private byte[] authValue;

		private SecretKeyPermissions permissions;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		public byte[] AuthValue
		{
			get
			{
				return this.authValue;
			}
			set
			{
				this.authValue = value;
			}
		}

		public string KeyName
		{
			get
			{
				return this.keyName;
			}
			set
			{
				this.keyName = value;
			}
		}

		public SecretKeyPermissions Permissions
		{
			get
			{
				return this.permissions;
			}
			set
			{
				this.permissions = value;
			}
		}

		public AuthDataEntry()
		{
		}

		public AuthDataEntry(string keyName, byte[] authValue) : this(keyName, authValue, SecretKeyPermissions.Full)
		{
		}

		public AuthDataEntry(string keyName, byte[] authValue, SecretKeyPermissions permissions)
		{
			this.keyName = keyName;
			this.authValue = authValue;
			this.permissions = permissions;
		}
	}
}