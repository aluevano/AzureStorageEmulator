using System;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class SecretKeyListV3 : List<SecretKeyV3>, ICloneable
	{
		public const int MIN_DEFAULT_KEY_COUNT = 1;

		public const int MAX_DEFAULT_KEY_COUNT = 2;

		public const int MAX_SYSTEM_KEY_COUNT = 2;

		public const int MAX_CUSTOM_KEY_COUNT = 4;

		public SecretKeyListV3()
		{
		}

		public object Clone()
		{
			SecretKeyListV3 secretKeyListV3 = new SecretKeyListV3();
			foreach (SecretKeyV3 secretKeyV3 in this)
			{
				secretKeyListV3.Add((SecretKeyV3)secretKeyV3.Clone());
			}
			return secretKeyListV3;
		}

		public SecretKeyV3 GetSecretKeyByName(string keyName)
		{
			SecretKeyV3 secretKeyV3;
			List<SecretKeyV3>.Enumerator enumerator = base.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SecretKeyV3 current = enumerator.Current;
					if (current.Name != keyName)
					{
						continue;
					}
					secretKeyV3 = current;
					return secretKeyV3;
				}
				return null;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return secretKeyV3;
		}

		public bool HasValidValues(out string errorMessage)
		{
			int num = 0;
			int num1 = 0;
			foreach (SecretKeyV3 secretKeyV3 in this)
			{
				if (!secretKeyV3.IsDefault())
				{
					num1++;
				}
				else
				{
					num++;
				}
			}
			if (num < 1)
			{
				errorMessage = string.Format("At least {0} default secret key(s) need to be specified. Found {1} secret keys.", 1, num);
				return false;
			}
			if (num > 2)
			{
				errorMessage = string.Format("At most {0} default secret key(s) are allowed. Found {1} secret keys.", 2, num);
				return false;
			}
			if (num1 <= 4)
			{
				if (!SecretKeyValidator.ContainsUniqueKeyNames(this, out errorMessage))
				{
					return false;
				}
				return true;
			}
			errorMessage = string.Format("At most {0} custom secret key(s) are allowed. Found {1} secret keys.", 4, num1);
			return false;
		}
	}
}