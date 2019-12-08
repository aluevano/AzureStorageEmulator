using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	public enum AuthorizationFailureReason
	{
		NotApplicable = 0,
		AccessPermissionFailure = 2,
		QuotaPermissionFailure = 3,
		AccountDisabled = 4,
		KeyDisabled = 5,
		AccountPermissionFailureSAS = 11,
		AccessPermissionFailureSAS = 12,
		QuotaPermissionFailureSAS = 13,
		InvalidOperationSAS = 14,
		SourceIPMismatch = 15,
		ProtocolMismatch = 16,
		ServiceMismatch = 17,
		PermissionMismatch = 18,
		ResourceTypeMismatch = 19,
		UnauthorizedAccountSasRequest = 20,
		UnauthorizedBlobOverwrite = 21
	}
}