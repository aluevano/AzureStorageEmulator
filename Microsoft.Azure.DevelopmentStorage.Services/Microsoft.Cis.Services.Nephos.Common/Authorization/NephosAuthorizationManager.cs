using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	public class NephosAuthorizationManager : AuthorizationManager
	{
		private readonly static char[] slashSeparator;

		private readonly IStorageManager storageManager;

		private readonly bool checkResourceAcl;

		public override Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		public override bool PublicAccessEnabled
		{
			get
			{
				return false;
			}
		}

		static NephosAuthorizationManager()
		{
			NephosAuthorizationManager.slashSeparator = new char[] { '/' };
		}

		private NephosAuthorizationManager()
		{
			NephosAssertionException.Fail("NephosAuthorizationManager constructor with no parameters cannot be called");
		}

		protected NephosAuthorizationManager(IStorageManager storageManager, bool checkResourceAcl)
		{
			this.storageManager = storageManager;
			this.checkResourceAcl = checkResourceAcl;
		}

		protected internal override IEnumerable<IAsyncResult> AuthorizePublicAccess(AsyncIteratorContext<AuthorizationResult> context, AuthorizationResult authorizationResult, IStorageManager storageManager, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, PermissionLevel permission, Duration d, TimeSpan timeout)
		{
			throw new NotSupportedException("Public access not supported");
		}

		private IEnumerator<IAsyncResult> AuthorizeRequestImpl(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters, AuthorizationInformation authorizationInfo, TimeSpan timeout, AsyncIteratorContext<AuthorizationResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			IAsyncResult asyncResult = this.BeginSharedKeyAuthorization(requestor, resourceAccount, resourceContainer, resourceIdentifier, requestedPermission, authorizationInfo, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("NephosAuthorizationManager.AuthorizeRequestImpl"));
			yield return asyncResult;
			AuthorizationResult authorizationResult = this.EndAuthorizeRequest(asyncResult);
			bool authorized = authorizationResult.Authorized;
			if (!authorized || !(requestor is SignedAccessAccountIdentifier) && !(requestor is AccountSasAccessIdentifier))
			{
				context.ResultData = authorizationResult;
			}
			else
			{
				NephosAssertionException.Assert((requestor is SignedAccessAccountIdentifier ? true : requestor is AccountSasAccessIdentifier));
				if (!(requestor is SignedAccessAccountIdentifier))
				{
					if (!(requestor is AccountSasAccessIdentifier))
					{
						throw new NephosUnauthorizedAccessException("Signed access not supported for this request", resourceAccount, resourceContainer, resourceIdentifier, requestor, requestedPermission, AuthorizationFailureReason.InvalidOperationSAS);
					}
					if ((requestedSasParameters.SupportedSasTypes & SasType.AccountSas) != SasType.AccountSas)
					{
						throw new NephosUnauthorizedAccessException("Account signed access not supported for this request", resourceAccount, resourceContainer, resourceIdentifier, requestor, requestedPermission, AuthorizationFailureReason.InvalidOperationSAS);
					}
					if (requestedSasParameters.SignedPermission == SASPermission.None)
					{
						throw new ArgumentException("signedPerrmission");
					}
					authorizationResult = this.AuthorizeAccountSignedAccessRequest(requestor as AccountSasAccessIdentifier, resourceAccount, resourceContainer, resourceIdentifier, requestedPermission, requestedSasParameters);
					context.ResultData = authorizationResult;
				}
				else
				{
					if ((requestedSasParameters.SupportedSasTypes & SasType.ResourceSas) != SasType.ResourceSas)
					{
						throw new NephosUnauthorizedAccessException("Signed access not supported for this request", resourceAccount, resourceContainer, resourceIdentifier, requestor, requestedPermission, AuthorizationFailureReason.InvalidOperationSAS);
					}
					if (requestedSasParameters.SignedPermission == SASPermission.None)
					{
						throw new ArgumentException("signedPerrmission");
					}
					authorizationResult = this.AuthorizeResourceSignedAccessRequest(requestor as SignedAccessAccountIdentifier, resourceAccount, resourceContainer, resourceIdentifier, requestedPermission, requestedSasParameters.SignedPermission);
					context.ResultData = authorizationResult;
				}
			}
		}

		protected internal override AuthorizationResult AuthorizeResourceSignedAccessRequest(SignedAccessAccountIdentifier signedRequestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASPermission requestedSignedPermission)
		{
			throw new NotSupportedException("Signed access not supported");
		}

		public override IAsyncResult BeginAuthorizeRequest(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel permission, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return this.BeginSharedKeyAuthorization(requestor, resourceAccount, resourceContainer, resourceIdentifier, permission, new AuthorizationInformation(), timeout, callback, state);
		}

		public override IAsyncResult BeginAuthorizeRequest(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<AuthorizationResult> asyncIteratorContext = new AsyncIteratorContext<AuthorizationResult>("OwnerAdminAuthorizationManager.AuthorizeRequest", callback, state);
			asyncIteratorContext.Begin(this.AuthorizeRequestImpl(requestor, resourceAccount, resourceContainer, resourceIdentifier, requestedPermission, requestedSasParameters, new AuthorizationInformation(), timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginAuthorizeRequest(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters, AuthorizationInformation authorizationInfo, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<AuthorizationResult> asyncIteratorContext = new AsyncIteratorContext<AuthorizationResult>("OwnerAdminAuthorizationManager.AuthorizeRequest", callback, state);
			asyncIteratorContext.Begin(this.AuthorizeRequestImpl(requestor, resourceAccount, resourceContainer, resourceIdentifier, requestedPermission, requestedSasParameters, authorizationInfo, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		protected internal IAsyncResult BeginSharedKeyAuthorization(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel permission, AuthorizationInformation authorizationInfo, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<AuthorizationResult> asyncIteratorContext = new AsyncIteratorContext<AuthorizationResult>("OwnerAdminAuthorizationManager.AuthorizeRequest", callback, state);
			asyncIteratorContext.Begin(this.SharedKeyAuthorizationImpl(requestor, resourceAccount, resourceContainer, resourceIdentifier, permission, authorizationInfo, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public static AuthorizationManager CreateAuthorizationManager(IStorageManager storageManager, bool checkResourceAcl)
		{
			if (checkResourceAcl && storageManager == null)
			{
				throw new ArgumentNullException("storageManager");
			}
			return new NephosAuthorizationManager(storageManager, checkResourceAcl);
		}

		public override AuthorizationResult EndAuthorizeRequest(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<AuthorizationResult> asyncIteratorContext = (AsyncIteratorContext<AuthorizationResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		private IEnumerator<IAsyncResult> SharedKeyAuthorizationImpl(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel permission, AuthorizationInformation authorizationInfo, TimeSpan timeout, AsyncIteratorContext<AuthorizationResult> context)
		{
			PermissionLevel permissionLevel;
			PermissionLevel permissionLevel1;
			PermissionLevel permissionLevel2;
			PermissionLevel permissionLevel3;
			PermissionLevel permissionLevel4;
			IEnumerable<IAsyncResult> asyncResults;
			Duration startingNow = Duration.StartingNow;
			AuthorizationResult authorizationResult = new AuthorizationResult(false, AuthorizationFailureReason.AccessPermissionFailure);
			PermissionLevel permissionLevel5 = PermissionLevel.Read | PermissionLevel.Write | PermissionLevel.Delete | PermissionLevel.ReadAcl | PermissionLevel.WriteAcl | PermissionLevel.FullControl | PermissionLevel.Owner | PermissionLevel.ReadDelete | PermissionLevel.ReadWrite | PermissionLevel.ReadWriteDelete | PermissionLevel.WriteDelete | PermissionLevel.ReadAclWriteAcl;
			if ((int)(permission & ~permissionLevel5) != 0 || (int)permission == 0)
			{
				throw new ArgumentException("permission", string.Format("permission is not well formed. Permission: {0}", permission));
			}
			if (requestor == null)
			{
				throw new ArgumentNullException("requestor");
			}
			if (!requestor.IsKeyDisabled)
			{
				PermissionLevel permissionLevel6 = permission & (PermissionLevel.Read | PermissionLevel.Write | PermissionLevel.Delete | PermissionLevel.ReadAcl | PermissionLevel.WriteAcl | PermissionLevel.FullControl | PermissionLevel.ReadDelete | PermissionLevel.ReadWrite | PermissionLevel.ReadWriteDelete | PermissionLevel.WriteDelete | PermissionLevel.ReadAclWriteAcl);
				if (!string.IsNullOrEmpty(resourceAccount) && resourceAccount.Equals(requestor.AccountName, StringComparison.OrdinalIgnoreCase))
				{
					PermissionLevel permissionLevel7 = (PermissionLevel)0;
					if ((permissionLevel6 & PermissionLevel.Read) == PermissionLevel.Read)
					{
						PermissionLevel permissionLevel8 = permissionLevel7;
						if (requestor.IsReadAllowed)
						{
							permissionLevel4 = PermissionLevel.Read;
						}
						else
						{
							permissionLevel4 = (PermissionLevel)0;
						}
						permissionLevel7 = permissionLevel8 | permissionLevel4;
					}
					if ((permissionLevel6 & PermissionLevel.Write) == PermissionLevel.Write && requestor.IsWriteAllowed)
					{
						permissionLevel7 |= PermissionLevel.Write;
					}
					if ((permissionLevel6 & PermissionLevel.Delete) == PermissionLevel.Delete)
					{
						PermissionLevel permissionLevel9 = permissionLevel7;
						if (requestor.IsDeleteAllowed)
						{
							permissionLevel3 = PermissionLevel.Delete;
						}
						else
						{
							permissionLevel3 = (PermissionLevel)0;
						}
						permissionLevel7 = permissionLevel9 | permissionLevel3;
					}
					if ((permissionLevel6 & PermissionLevel.FullControl) == PermissionLevel.FullControl)
					{
						PermissionLevel permissionLevel10 = permissionLevel7;
						if (requestor.IsFullControlAllowed)
						{
							permissionLevel2 = PermissionLevel.FullControl;
						}
						else
						{
							permissionLevel2 = (PermissionLevel)0;
						}
						permissionLevel7 = permissionLevel10 | permissionLevel2;
					}
					if ((permissionLevel6 & PermissionLevel.ReadAcl) == PermissionLevel.ReadAcl)
					{
						PermissionLevel permissionLevel11 = permissionLevel7;
						if (requestor.IsReadAllowed)
						{
							permissionLevel1 = PermissionLevel.ReadAcl;
						}
						else
						{
							permissionLevel1 = (PermissionLevel)0;
						}
						permissionLevel7 = permissionLevel11 | permissionLevel1;
					}
					if ((permissionLevel6 & PermissionLevel.WriteAcl) == PermissionLevel.WriteAcl)
					{
						PermissionLevel permissionLevel12 = permissionLevel7;
						if (requestor.IsWriteAllowed)
						{
							permissionLevel = PermissionLevel.WriteAcl;
						}
						else
						{
							permissionLevel = (PermissionLevel)0;
						}
						permissionLevel7 = permissionLevel12 | permissionLevel;
					}
					if (permissionLevel7 != permissionLevel6)
					{
						authorizationResult.FailureReason = AuthorizationFailureReason.PermissionMismatch;
						authorizationResult.Authorized = false;
					}
					else
					{
						authorizationResult.FailureReason = AuthorizationFailureReason.NotApplicable;
						authorizationResult.Authorized = true;
					}
					context.ResultData = authorizationResult;
				}
				else if (!this.checkResourceAcl)
				{
					authorizationResult.FailureReason = AuthorizationFailureReason.AccessPermissionFailure;
					authorizationResult.Authorized = false;
					context.ResultData = authorizationResult;
				}
				else if (!this.PublicAccessEnabled)
				{
					authorizationResult.FailureReason = AuthorizationFailureReason.AccessPermissionFailure;
					authorizationResult.Authorized = false;
					context.ResultData = authorizationResult;
				}
				else if ((permission & PermissionLevel.Owner) == PermissionLevel.Owner)
				{
					authorizationResult.FailureReason = AuthorizationFailureReason.AccessPermissionFailure;
					authorizationResult.Authorized = false;
					context.ResultData = authorizationResult;
				}
				else if (permissionLevel6 != PermissionLevel.Read)
				{
					authorizationResult.FailureReason = AuthorizationFailureReason.AccessPermissionFailure;
					authorizationResult.Authorized = false;
					context.ResultData = authorizationResult;
				}
				else if (!string.IsNullOrEmpty(resourceContainer) || !string.IsNullOrEmpty(resourceIdentifier))
				{
					try
					{
						asyncResults = this.AuthorizePublicAccess(context, authorizationResult, this.storageManager, resourceAccount, resourceContainer, resourceIdentifier, requestor, permission, startingNow, timeout);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						Logger<IRestProtocolHeadLogger>.Instance.ErrorDebug.Log("Can't authorize public access using ACL");
						throw exception;
					}
					foreach (IAsyncResult asyncResult in asyncResults)
					{
						yield return asyncResult;
					}
				}
				else
				{
					authorizationResult.FailureReason = AuthorizationFailureReason.AccessPermissionFailure;
					authorizationResult.Authorized = false;
					context.ResultData = authorizationResult;
				}
			}
			else
			{
				authorizationResult.FailureReason = AuthorizationFailureReason.KeyDisabled;
				authorizationResult.Authorized = false;
				context.ResultData = authorizationResult;
			}
		}
	}
}