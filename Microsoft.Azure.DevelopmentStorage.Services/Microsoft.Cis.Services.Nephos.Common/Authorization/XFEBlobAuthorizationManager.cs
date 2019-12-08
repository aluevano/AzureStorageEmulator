using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	public class XFEBlobAuthorizationManager : NephosAuthorizationManager
	{
		public override bool PublicAccessEnabled
		{
			get
			{
				return true;
			}
		}

		protected internal XFEBlobAuthorizationManager(IStorageManager storageManager, bool checkResourceAcl) : base(storageManager, checkResourceAcl)
		{
		}

		protected internal override AuthorizationResult AuthorizeAccountSignedAccessRequest(AccountSasAccessIdentifier signedRequestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters)
		{
			AuthorizationResult authorizationResult = new AuthorizationResult(false, AuthorizationFailureReason.AccessPermissionFailure);
			return base.AuthorizeAccountSignedAccessRequest(signedRequestor, resourceAccount, resourceContainer, resourceIdentifier, requestedPermission, requestedSasParameters);
		}

		protected internal override IEnumerable<IAsyncResult> AuthorizePublicAccess(AsyncIteratorContext<AuthorizationResult> context, AuthorizationResult authorizationResult, IStorageManager storageManager, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, PermissionLevel permission, Duration d, TimeSpan timeout)
		{
			using (IBlobContainer blobContainer = storageManager.CreateBlobContainerInstance(resourceAccount, resourceContainer))
			{
				blobContainer.Timeout = d.Remaining(timeout);
				IAsyncResult asyncResult = blobContainer.BeginGetProperties(ContainerPropertyNames.ServiceMetadata, null, context.GetResumeCallback(), context.GetResumeState("NephosAuthorizationManager.AuthorizeRequestImpl"));
				yield return asyncResult;
				try
				{
					blobContainer.EndGetProperties(asyncResult);
				}
				catch (ContainerNotFoundException containerNotFoundException)
				{
					throw new ContainerUnauthorizedException(resourceAccount, resourceContainer, resourceIdentifier, requestor, permission, containerNotFoundException);
				}
				catch (ServerBusyException serverBusyException)
				{
					Logger<IRestProtocolHeadLogger>.Instance.ErrorDebug.Log("Got ServerBusy while trying to get container Acl settings during Authorization so not continuing");
					throw;
				}
				catch (TimeoutException timeoutException)
				{
					Logger<IRestProtocolHeadLogger>.Instance.ErrorDebug.Log("Request timed out while trying to get container Acl settings during Authorization so not continuing");
					throw;
				}
				catch (StorageManagerException storageManagerException1)
				{
					StorageManagerException storageManagerException = storageManagerException1;
					IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
					verboseDebug.Log("Got and swallowed exception when accessing acl setting for authorization: {0}", new object[] { storageManagerException });
					authorizationResult.FailureReason = AuthorizationFailureReason.AccessPermissionFailure;
					authorizationResult.Authorized = false;
					context.ResultData = authorizationResult;
					goto Label0;
				}
				string str = (new ContainerAclSettings(blobContainer.ServiceMetadata)).PublicAccessLevel;
				if (!string.IsNullOrEmpty(str))
				{
					if (string.IsNullOrEmpty(resourceIdentifier))
					{
						if (Comparison.StringEqualsIgnoreCase(str, "container") || Comparison.StringEqualsIgnoreCase(str, bool.TrueString))
						{
							authorizationResult.FailureReason = AuthorizationFailureReason.NotApplicable;
							authorizationResult.Authorized = true;
							context.ResultData = authorizationResult;
							goto Label0;
						}
					}
					else if (Comparison.StringEqualsIgnoreCase(str, "blob") || Comparison.StringEqualsIgnoreCase(str, "container") || Comparison.StringEqualsIgnoreCase(str, bool.TrueString))
					{
						authorizationResult.FailureReason = AuthorizationFailureReason.NotApplicable;
						authorizationResult.Authorized = true;
						context.ResultData = authorizationResult;
						goto Label0;
					}
					authorizationResult.FailureReason = AuthorizationFailureReason.AccessPermissionFailure;
					authorizationResult.Authorized = false;
					context.ResultData = authorizationResult;
				}
				else
				{
					authorizationResult.FailureReason = AuthorizationFailureReason.AccessPermissionFailure;
					authorizationResult.Authorized = false;
					context.ResultData = authorizationResult;
				}
			}
		Label0:
			yield break;
		}

		protected internal override AuthorizationResult AuthorizeResourceSignedAccessRequest(SignedAccessAccountIdentifier signedRequestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASPermission requestedSignedPermission)
		{
			AuthorizationResult authorizationResult;
			AuthorizationResult authorizationResult1 = new AuthorizationResult(false, AuthorizationFailureReason.AccessPermissionFailure);
			if ((requestedSignedPermission & (SASPermission.Update | SASPermission.Process)) != SASPermission.None)
			{
				throw new ArgumentException(string.Format("Signed permission is not well formed. Signed permission: {0}", requestedSignedPermission), "signedPermission");
			}
			if ((requestedSignedPermission & signedRequestor.SignedAccessPermission) != requestedSignedPermission)
			{
				authorizationResult1.FailureReason = AuthorizationFailureReason.PermissionMismatch;
				authorizationResult1.Authorized = false;
				return authorizationResult1;
			}
			List<SASAccessRestriction>.Enumerator enumerator = signedRequestor.SignedAccessRestrictions.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SASAccessRestriction current = enumerator.Current;
					if (string.IsNullOrEmpty(resourceAccount) || !resourceAccount.Equals(current.AccessPath.AccountName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(resourceContainer) || !resourceContainer.Equals(current.AccessPath.ContainerName, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					if (current.AccessLevel != SASAccessLevel.Container)
					{
						if (string.IsNullOrEmpty(resourceIdentifier) || !resourceIdentifier.Equals(current.AccessPath.RemainingPart, StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						authorizationResult1.FailureReason = AuthorizationFailureReason.NotApplicable;
						authorizationResult1.Authorized = true;
						authorizationResult = authorizationResult1;
						return authorizationResult;
					}
					else
					{
						authorizationResult1.FailureReason = AuthorizationFailureReason.NotApplicable;
						authorizationResult1.Authorized = true;
						authorizationResult = authorizationResult1;
						return authorizationResult;
					}
				}
				authorizationResult1.FailureReason = AuthorizationFailureReason.AccessPermissionFailureSAS;
				authorizationResult1.Authorized = false;
				return authorizationResult1;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
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
			return new XFEBlobAuthorizationManager(storageManager, checkResourceAcl);
		}
	}
}