using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	public abstract class AuthorizationManager
	{
		public abstract Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		public abstract bool PublicAccessEnabled
		{
			get;
		}

		protected AuthorizationManager()
		{
		}

		public static AuthorizationResult AuthorizeAccountSignedAccessRequest(AccountSasAccessIdentifier signedRequestor, string resourceAccount, SASAuthorizationParameters requestedSasParameters)
		{
			AuthorizationResult authorizationResult = new AuthorizationResult(false, AuthorizationFailureReason.AccessPermissionFailure);
			if (string.IsNullOrEmpty(resourceAccount) || !resourceAccount.Equals(signedRequestor.AccountName, StringComparison.OrdinalIgnoreCase))
			{
				authorizationResult.FailureReason = AuthorizationFailureReason.UnauthorizedAccountSasRequest;
				authorizationResult.Authorized = false;
				return authorizationResult;
			}
			if ((requestedSasParameters.SignedResourceType & signedRequestor.SignedResourceType) != requestedSasParameters.SignedResourceType)
			{
				authorizationResult.FailureReason = AuthorizationFailureReason.ResourceTypeMismatch;
				authorizationResult.Authorized = false;
				return authorizationResult;
			}
			if ((requestedSasParameters.SignedPermission & signedRequestor.SignedAccessPermission) != requestedSasParameters.SignedPermission)
			{
				authorizationResult.FailureReason = AuthorizationFailureReason.PermissionMismatch;
				authorizationResult.Authorized = false;
				return authorizationResult;
			}
			authorizationResult.FailureReason = AuthorizationFailureReason.NotApplicable;
			authorizationResult.Authorized = true;
			return authorizationResult;
		}

		protected internal virtual AuthorizationResult AuthorizeAccountSignedAccessRequest(AccountSasAccessIdentifier signedRequestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters)
		{
			return AuthorizationManager.AuthorizeAccountSignedAccessRequest(signedRequestor, resourceAccount, requestedSasParameters);
		}

		private static AuthorizationResult AuthorizeProtocol(SasProtocol signedProtocol, RequestContext requestContext)
		{
			AuthorizationResult authorizationResult = new AuthorizationResult(true, AuthorizationFailureReason.NotApplicable);
			if (signedProtocol == SasProtocol.All || requestContext.RequestUrl.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase) && signedProtocol == SasProtocol.Https)
			{
				return authorizationResult;
			}
			authorizationResult.FailureReason = AuthorizationFailureReason.ProtocolMismatch;
			authorizationResult.Authorized = false;
			return authorizationResult;
		}

		protected internal abstract IEnumerable<IAsyncResult> AuthorizePublicAccess(AsyncIteratorContext<AuthorizationResult> context, AuthorizationResult authorizationResult, IStorageManager storageManager, string resourceAccount, string resourceContainer, string resourceIdentifier, IAccountIdentifier requestor, PermissionLevel permission, Duration d, TimeSpan timeout);

		protected internal abstract AuthorizationResult AuthorizeResourceSignedAccessRequest(SignedAccessAccountIdentifier signedRequestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASPermission requestedSignedPermission);

		private static AuthorizationResult AuthorizeSignedService(SasService signedService, RequestContext requestContext)
		{
			AuthorizationResult authorizationResult = new AuthorizationResult(true, AuthorizationFailureReason.NotApplicable);
			if (signedService != SasService.None)
			{
				switch (requestContext.ServiceType)
				{
					case ServiceType.BlobService:
					{
						if ((signedService & SasService.Blob) == SasService.Blob)
						{
							break;
						}
						authorizationResult.FailureReason = AuthorizationFailureReason.ServiceMismatch;
						authorizationResult.Authorized = false;
						return authorizationResult;
					}
					case ServiceType.QueueService:
					{
						if ((signedService & SasService.Queue) == SasService.Queue)
						{
							break;
						}
						authorizationResult.FailureReason = AuthorizationFailureReason.ServiceMismatch;
						authorizationResult.Authorized = false;
						return authorizationResult;
					}
					case ServiceType.TableService:
					{
						if ((signedService & SasService.Table) == SasService.Table)
						{
							break;
						}
						authorizationResult.FailureReason = AuthorizationFailureReason.ServiceMismatch;
						authorizationResult.Authorized = false;
						return authorizationResult;
					}
					case ServiceType.LocationAccountService:
					case ServiceType.LocationService:
					{
						authorizationResult.FailureReason = AuthorizationFailureReason.ServiceMismatch;
						authorizationResult.Authorized = false;
						return authorizationResult;
					}
					case ServiceType.FileService:
					{
						if ((signedService & SasService.File) == SasService.File)
						{
							break;
						}
						authorizationResult.FailureReason = AuthorizationFailureReason.ServiceMismatch;
						authorizationResult.Authorized = false;
						return authorizationResult;
					}
					default:
					{
						authorizationResult.FailureReason = AuthorizationFailureReason.ServiceMismatch;
						authorizationResult.Authorized = false;
						return authorizationResult;
					}
				}
			}
			return authorizationResult;
		}

		private static AuthorizationResult AuthorizeSourceIp(IPAddressRange signedIP, RequestContext requestContext)
		{
			IPAddressRange pAddressRange;
			AuthorizationResult authorizationResult = new AuthorizationResult(true, AuthorizationFailureReason.NotApplicable);
			if (signedIP == null || IPAddressRange.IsContainedInRange(requestContext.ClientIP.Address, signedIP, out pAddressRange))
			{
				return authorizationResult;
			}
			authorizationResult.FailureReason = AuthorizationFailureReason.SourceIPMismatch;
			authorizationResult.Authorized = false;
			return authorizationResult;
		}

		public abstract IAsyncResult BeginAuthorizeRequest(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel permission, TimeSpan timeout, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginAuthorizeRequest(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters, TimeSpan timeout, AsyncCallback callback, object state);

		public abstract IAsyncResult BeginAuthorizeRequest(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters, AuthorizationInformation authorizationInfo, TimeSpan timeout, AsyncCallback callback, object state);

		public IAsyncResult BeginCheckAccess(IAccountIdentifier identifier, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel permissionLevel, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealServiceManager.Authorize", callback, state);
			asyncIteratorContext.Begin(this.CheckAccessImpl(identifier, resourceAccount, resourceContainer, resourceIdentifier, permissionLevel, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginCheckAccess(IAccountIdentifier identifier, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealServiceManager.Authorize", callback, state);
			asyncIteratorContext.Begin(this.CheckAccessImpl(identifier, resourceAccount, resourceContainer, resourceIdentifier, requestedPermission, requestedSasParameters, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginCheckAccess(IAccountIdentifier identifier, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters, AuthorizationInformation authorizationInfo, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealServiceManager.Authorize", callback, state);
			asyncIteratorContext.Begin(this.CheckAccessImpl(identifier, resourceAccount, resourceContainer, resourceIdentifier, requestedPermission, requestedSasParameters, authorizationInfo, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IEnumerator<IAsyncResult> CheckAccessImpl(IAccountIdentifier identifier, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel permissionLevel, TimeSpan timeout, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.BeginAuthorizeRequest(identifier, resourceAccount, resourceContainer, resourceIdentifier, permissionLevel, timeout, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.AuthorizeImpl"));
			yield return asyncResult;
			AuthorizationResult authorizationResult = this.EndAuthorizeRequest(asyncResult);
			if (!authorizationResult.Authorized)
			{
				if (identifier == null || !identifier.IsSecondaryAccess || !AuthorizationManager.IsWritePermission(permissionLevel))
				{
					throw new NephosUnauthorizedAccessException(resourceAccount, resourceContainer, resourceIdentifier, identifier, permissionLevel, authorizationResult.FailureReason);
				}
				throw new SecondaryWriteNotAllowedException();
			}
		}

		private IEnumerator<IAsyncResult> CheckAccessImpl(IAccountIdentifier identifier, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters, TimeSpan timeout, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.BeginAuthorizeRequest(identifier, resourceAccount, resourceContainer, resourceIdentifier, requestedPermission, requestedSasParameters, timeout, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.AuthorizeImpl"));
			yield return asyncResult;
			AuthorizationResult authorizationResult = this.EndAuthorizeRequest(asyncResult);
			if (!authorizationResult.Authorized)
			{
				if (identifier == null || !identifier.IsSecondaryAccess || !AuthorizationManager.IsWritePermission(requestedPermission))
				{
					throw new NephosUnauthorizedAccessException(resourceAccount, resourceContainer, resourceIdentifier, identifier, requestedPermission, requestedSasParameters.SignedPermission, authorizationResult.FailureReason);
				}
				throw new SecondaryWriteNotAllowedException();
			}
		}

		private IEnumerator<IAsyncResult> CheckAccessImpl(IAccountIdentifier identifier, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requestedPermission, SASAuthorizationParameters requestedSasParameters, AuthorizationInformation authorizationInfo, TimeSpan timeout, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.BeginAuthorizeRequest(identifier, resourceAccount, resourceContainer, resourceIdentifier, requestedPermission, requestedSasParameters, authorizationInfo, timeout, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.AuthorizeImpl"));
			yield return asyncResult;
			AuthorizationResult authorizationResult = this.EndAuthorizeRequest(asyncResult);
			if (!authorizationResult.Authorized)
			{
				if (identifier == null || !identifier.IsSecondaryAccess || !AuthorizationManager.IsWritePermission(requestedPermission))
				{
					throw new NephosUnauthorizedAccessException(resourceAccount, resourceContainer, resourceIdentifier, identifier, requestedPermission, requestedSasParameters.SignedPermission, authorizationResult.FailureReason);
				}
				throw new SecondaryWriteNotAllowedException();
			}
		}

		public SASAuthorizationParameters CheckAccessWithMultiplePermissions(IAccountIdentifier requestor, string resourceAccount, string resourceContainer, string resourceIdentifier, PermissionLevel requiredPermission, SasType supportedSasTypes, SasResourceType requiredResourceType, List<SASPermission> requiredSasPermissions, TimeSpan timeout)
		{
			SASAuthorizationParameters sASAuthorizationParameter;
			AuthorizationResult authorizationResult = null;
			Duration startingNow = Duration.StartingNow;
			SASAuthorizationParameters sASAuthorizationParameter1 = new SASAuthorizationParameters()
			{
				SupportedSasTypes = supportedSasTypes,
				SignedResourceType = requiredResourceType
			};
			SASAuthorizationParameters current = sASAuthorizationParameter1;
			List<SASPermission>.Enumerator enumerator = requiredSasPermissions.GetEnumerator();
			try
			{
				do
				{
					if (!enumerator.MoveNext())
					{
						break;
					}
					current.SignedPermission = enumerator.Current;
					IAsyncResult asyncResult = this.BeginAuthorizeRequest(requestor, resourceAccount, resourceContainer, resourceIdentifier, requiredPermission, current, startingNow.Remaining(timeout), null, null);
					authorizationResult = this.EndAuthorizeRequest(asyncResult);
					if (!authorizationResult.Authorized)
					{
						continue;
					}
					sASAuthorizationParameter = current;
					return sASAuthorizationParameter;
				}
				while (authorizationResult.Authorized || authorizationResult.FailureReason == AuthorizationFailureReason.PermissionMismatch);
				if (requestor == null || !requestor.IsSecondaryAccess || !AuthorizationManager.IsWritePermission(requiredPermission))
				{
					throw new NephosUnauthorizedAccessException(resourceAccount, resourceContainer, resourceIdentifier, requestor, requiredPermission, authorizationResult.FailureReason);
				}
				throw new SecondaryWriteNotAllowedException();
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return sASAuthorizationParameter;
		}

		public abstract AuthorizationResult EndAuthorizeRequest(IAsyncResult asyncResult);

		public void EndCheckAccess(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private static bool IsWritePermission(PermissionLevel permission)
		{
			if (permission == PermissionLevel.Read)
			{
				return false;
			}
			return permission != PermissionLevel.ReadAcl;
		}

		public static AuthorizationResult PreAuthorizeRequest(IAccountIdentifier requestor, RequestContext requestContext)
		{
			AuthorizationResult authorizationResult = new AuthorizationResult(true, AuthorizationFailureReason.NotApplicable);
			if (requestor is SignedAccessAccountIdentifier)
			{
				SignedAccessAccountIdentifier signedAccessAccountIdentifier = requestor as SignedAccessAccountIdentifier;
				authorizationResult = AuthorizationManager.AuthorizeSourceIp(signedAccessAccountIdentifier.SignedIP, requestContext);
				if (!authorizationResult.Authorized)
				{
					return authorizationResult;
				}
				authorizationResult = AuthorizationManager.AuthorizeProtocol(signedAccessAccountIdentifier.SignedProtocol, requestContext);
				if (!authorizationResult.Authorized)
				{
					return authorizationResult;
				}
			}
			else if (requestor is AccountSasAccessIdentifier)
			{
				AccountSasAccessIdentifier accountSasAccessIdentifier = requestor as AccountSasAccessIdentifier;
				authorizationResult = AuthorizationManager.AuthorizeSourceIp(accountSasAccessIdentifier.SignedIP, requestContext);
				if (!authorizationResult.Authorized)
				{
					return authorizationResult;
				}
				authorizationResult = AuthorizationManager.AuthorizeProtocol(accountSasAccessIdentifier.SignedProtocol, requestContext);
				if (!authorizationResult.Authorized)
				{
					return authorizationResult;
				}
				authorizationResult = AuthorizationManager.AuthorizeSignedService(accountSasAccessIdentifier.SignedService, requestContext);
				if (!authorizationResult.Authorized)
				{
					return authorizationResult;
				}
			}
			authorizationResult.FailureReason = AuthorizationFailureReason.NotApplicable;
			authorizationResult.Authorized = true;
			return authorizationResult;
		}
	}
}