using Microsoft.Cis.Services.Nephos.Common.Account;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public class AuthenticationResult : IAuthenticationResult
	{
		private IAccountIdentifier accountIdentifier;

		private string authenticationVersion;

		private bool isSignatureAccess;

		public IAccountIdentifier AccountIdentifier
		{
			get
			{
				return this.accountIdentifier;
			}
		}

		public string AuthenticationVersion
		{
			get
			{
				return this.authenticationVersion;
			}
		}

		public bool IsSignatureAccess
		{
			get
			{
				return this.isSignatureAccess;
			}
		}

		public AuthenticationResult(IAccountIdentifier accountIdentifier, string authenticationVersion, bool isSignatureAccess)
		{
			this.accountIdentifier = accountIdentifier;
			this.authenticationVersion = authenticationVersion;
			this.isSignatureAccess = isSignatureAccess;
		}
	}
}