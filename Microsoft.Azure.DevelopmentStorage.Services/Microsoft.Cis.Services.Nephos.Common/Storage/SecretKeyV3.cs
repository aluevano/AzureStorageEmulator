using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[DataContract]
	public class SecretKeyV3 : ICloneable
	{
		public const string DEFAULT_KEY_NAME_1 = "key1";

		public const string DEFAULT_KEY_NAME_2 = "key2";

		[DataMember]
		public string Name
		{
			get;
			private set;
		}

		[DataMember]
		public SecretKeyPermissions Permissions
		{
			get;
			private set;
		}

		[DataMember]
		public byte[] Value
		{
			get;
			private set;
		}

		public SecretKeyV3(string keyName, byte[] keyValue, SecretKeyPermissions keyPermissions)
		{
			this.Name = keyName;
			this.Value = keyValue;
			this.Permissions = keyPermissions;
		}

		public object Clone()
		{
			string str = (string)this.Name.Clone();
			byte[] numArray = (byte[])this.Value.Clone();
			return new SecretKeyV3(str, numArray, this.Permissions);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || this.GetType() != obj.GetType())
			{
				return false;
			}
			SecretKeyV3 secretKeyV3 = (SecretKeyV3)obj;
			if (!this.Name.Equals(secretKeyV3.Name))
			{
				return false;
			}
			if (!SecretKeyValidator.s_keyValueComparer.Equals(this.Value, secretKeyV3.Value))
			{
				return false;
			}
			if (this.Permissions != secretKeyV3.Permissions)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return this.GetHashCode();
		}

		public bool IsDefault()
		{
			return this.Name.Length == 4;
		}

		public static bool IsDefaultKeyName(string keyName)
		{
			if (keyName == "key1")
			{
				return true;
			}
			return keyName == "key2";
		}

		public bool IsValid(out string errorMessage)
		{
			errorMessage = null;
			if (SecretKeyV3.IsDefaultKeyName(this.Name))
			{
				if (this.Permissions != SecretKeyPermissions.Full)
				{
					errorMessage = string.Format("SecretKeys: default keys must have full permissions instead of {0}", this.Permissions.ToString());
					return false;
				}
				if (!SecretKeyValidator.IsKeyValueLengthValid(this.Value, out errorMessage, 16, 256))
				{
					return false;
				}
				return true;
			}
			if (!SecretKeyValidator.IsKeyNameLengthValid(this.Name, out errorMessage, 5, 20))
			{
				return false;
			}
			if (!SecretKeyValidator.IsKeyNameLowerCaseAlphaNumeric(this.Name, out errorMessage))
			{
				return false;
			}
			if (!SecretKeyValidator.IsKeyValueLengthValid(this.Value, out errorMessage, 16, 256))
			{
				return false;
			}
			return true;
		}
	}
}