using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class SecretKeyValidator
	{
		private const int SECRET_KEY_NAME_MIN_LENGTH = 5;

		private const int SECRET_KEY_NAME_MAX_LENGTH = 20;

		private const int SECRET_KEY_VALUE_MIN_LENGTH = 16;

		private const int SECRET_KEY_VALUE_MAX_LENGTH = 256;

		internal static SecretKeyValidator.SameSecretKeyValueComparer s_keyValueComparer;

		private readonly static Regex alphaNumberic;

		static SecretKeyValidator()
		{
			SecretKeyValidator.s_keyValueComparer = new SecretKeyValidator.SameSecretKeyValueComparer();
			SecretKeyValidator.alphaNumberic = new Regex("^[a-z0-9]+$");
		}

		public SecretKeyValidator()
		{
		}

		internal static bool ContainsUniqueKeyNames(SecretKeyListV3 keys, out string errorMessage)
		{
			bool flag;
			HashSet<string> strs = new HashSet<string>();
			List<SecretKeyV3>.Enumerator enumerator = keys.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SecretKeyV3 current = enumerator.Current;
					if (strs.Add(current.Name))
					{
						continue;
					}
					errorMessage = string.Format("More than one secret key is named {0}.", current.Name);
					flag = false;
					return flag;
				}
				errorMessage = null;
				return true;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return flag;
		}

		internal static bool IsKeyNameLengthValid(string keyName, out string errorMessage, int minLength = 5, int maxLength = 20)
		{
			if (keyName == null)
			{
				errorMessage = "SecretKeys: key name must be non-empty";
				return false;
			}
			if (keyName.Length <= maxLength && keyName.Length >= minLength)
			{
				errorMessage = null;
				return true;
			}
			errorMessage = string.Format("SecretKeys: key name must be at least {0} characters and at most {1} characters", minLength, maxLength);
			return false;
		}

		internal static bool IsKeyNameLowerCaseAlphaNumeric(string keyName, out string errorMessage)
		{
			if (!SecretKeyValidator.alphaNumberic.IsMatch(keyName))
			{
				errorMessage = "SecretKeys: key name must be lower case and alpha-numeric";
				return false;
			}
			errorMessage = null;
			return true;
		}

		internal static bool IsKeyValueLengthValid(byte[] keyValue, out string errorMessage, int minLength = 16, int maxLength = 256)
		{
			if (keyValue == null)
			{
				errorMessage = "SecretKeys: key value cannot be null";
				return false;
			}
			if ((int)keyValue.Length <= maxLength && (int)keyValue.Length >= minLength)
			{
				errorMessage = null;
				return true;
			}
			errorMessage = string.Format("SecretKeys: key value must be at least {0} bytes and at most {1} bytes.", minLength, maxLength);
			return false;
		}

		internal class SameSecretKeyValueComparer : EqualityComparer<byte[]>
		{
			public SameSecretKeyValueComparer()
			{
			}

			public override bool Equals(byte[] value1, byte[] value2)
			{
				if (value1 == null ^ value2 == null)
				{
					return false;
				}
				if (value1 != null)
				{
					if ((int)value1.Length != (int)value2.Length)
					{
						return false;
					}
					for (int i = 0; i < (int)value1.Length; i++)
					{
						if (value1[i] != value2[i])
						{
							return false;
						}
					}
				}
				return true;
			}

			public override int GetHashCode(byte[] value)
			{
				int length = (int)value.Length;
				for (int i = Math.Min((int)value.Length, 4) - 1; i >= 0; i--)
				{
					length <<= 8;
					length |= value[i];
				}
				return length;
			}
		}
	}
}