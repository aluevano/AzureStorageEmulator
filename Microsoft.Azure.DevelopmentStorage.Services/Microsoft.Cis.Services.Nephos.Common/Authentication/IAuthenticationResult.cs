using Microsoft.Cis.Services.Nephos.Common.Account;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public interface IAuthenticationResult
	{
		IAccountIdentifier AccountIdentifier
		{
			get;
		}

		string AuthenticationVersion
		{
			get;
		}

		bool IsSignatureAccess
		{
			get;
		}
	}
}