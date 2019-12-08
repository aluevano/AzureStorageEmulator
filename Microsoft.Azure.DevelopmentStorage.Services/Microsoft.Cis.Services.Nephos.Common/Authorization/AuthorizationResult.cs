using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	public class AuthorizationResult
	{
		private bool authorized;

		private AuthorizationFailureReason failureReason;

		public bool Authorized
		{
			get
			{
				return this.authorized;
			}
			set
			{
				this.authorized = value;
			}
		}

		public AuthorizationFailureReason FailureReason
		{
			get
			{
				return this.failureReason;
			}
			set
			{
				this.failureReason = value;
			}
		}

		public AuthorizationResult(bool authorized, AuthorizationFailureReason failureReason)
		{
			this.authorized = authorized;
			this.failureReason = failureReason;
		}
	}
}