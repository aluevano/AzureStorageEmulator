using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	public interface IAuthorizationResult
	{
		bool Authorized
		{
			get;
		}

		AuthorizationFailureReason FailureReason
		{
			get;
		}
	}
}