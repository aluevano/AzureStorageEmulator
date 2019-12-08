using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	public class XFEQueueAuthorizationManager : NephosAuthorizationManager
	{
		protected internal XFEQueueAuthorizationManager(IStorageManager storageManager, bool checkResourceAcl) : base(storageManager, checkResourceAcl)
		{
		}

		protected internal override AuthorizationResult AuthorizeResourceSignedAccessRequest(SignedAccessAccountIdentifier signedRequestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASPermission requestedSignedPermission)
		{
			AuthorizationResult authorizationResult = new AuthorizationResult(false, AuthorizationFailureReason.AccessPermissionFailure);
			if ((requestedSignedPermission & ~29) != SASPermission.None)
			{
				throw new ArgumentException(string.Format("Signed permission is not well formed. Signed permission: {0}", requestedSignedPermission), "signedPermission");
			}
			if ((requestedSignedPermission & signedRequestor.SignedAccessPermission) != requestedSignedPermission)
			{
				authorizationResult.FailureReason = AuthorizationFailureReason.PermissionMismatch;
				authorizationResult.Authorized = false;
				return authorizationResult;
			}
			authorizationResult.FailureReason = AuthorizationFailureReason.NotApplicable;
			authorizationResult.Authorized = true;
			return authorizationResult;
		}

		public override IAsyncResult BeginAuthorizeRequest(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel permission, TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (requestor is SignedAccessAccountIdentifier || requestor is AccountSasAccessIdentifier)
			{
				throw new NephosUnauthorizedAccessException("Signed access not supported for this request", resourceAccount, resourceContainer, resourceIdentifier, requestor, permission, AuthorizationFailureReason.InvalidOperationSAS);
			}
			return base.BeginSharedKeyAuthorization(requestor, resourceAccount, resourceContainer, resourceIdentifier, permission, new AuthorizationInformation(), timeout, callback, state);
		}

		public static new AuthorizationManager CreateAuthorizationManager(IStorageManager storageManager, bool checkResourceAcl)
		{
			if (checkResourceAcl && storageManager == null)
			{
				throw new ArgumentNullException("storageManager");
			}
			return new XFEQueueAuthorizationManager(storageManager, checkResourceAcl);
		}
	}
}