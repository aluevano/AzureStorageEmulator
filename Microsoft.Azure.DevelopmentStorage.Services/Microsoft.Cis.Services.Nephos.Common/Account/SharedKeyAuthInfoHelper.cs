using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Microsoft.Cis.Services.Nephos.Common.Account
{
	public static class SharedKeyAuthInfoHelper
	{
		public static Collection<AuthDataEntry> GetSharedKeys(AuthenticationInformation authInfo)
		{
			if (authInfo == null)
			{
				throw new ArgumentNullException("authInfo");
			}
			if (string.Compare(authInfo.AuthScheme, SupportedAuthScheme.SharedKey.ToString(), StringComparison.OrdinalIgnoreCase) != 0)
			{
				throw new ArgumentException("Supplied authentication information is not for shared key scheme!", "authInfo");
			}
			string authKeyName = authInfo.AuthKeyName;
			Collection<AuthDataEntry> authDataEntries = new Collection<AuthDataEntry>();
			if (string.IsNullOrEmpty(authKeyName))
			{
				foreach (AuthDataEntry authDatum in authInfo.AuthData)
				{
					if (!SecretKeyV3.IsDefaultKeyName(authDatum.KeyName))
					{
						continue;
					}
					authDataEntries.Add(authDatum);
				}
			}
			else if (authInfo.NamedKeyAuthData != null)
			{
				authDataEntries.Add(authInfo.NamedKeyAuthData);
			}
			if (authDataEntries.Count == 0)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { authKeyName ?? "N/A" };
				throw new AuthenticationFailureException(string.Format(invariantCulture, "Could not find any keys to use for authentication. Key name specified is '{0}'", objArray));
			}
			return authDataEntries;
		}
	}
}