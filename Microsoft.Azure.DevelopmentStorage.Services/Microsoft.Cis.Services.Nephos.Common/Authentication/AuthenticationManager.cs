using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public abstract class AuthenticationManager
	{
		private readonly static IAccountIdentifier anonymousAccount;

		public static IAccountIdentifier AnonymousAccount
		{
			get
			{
				return Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.anonymousAccount;
			}
		}

		public abstract Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		static AuthenticationManager()
		{
			Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.anonymousAccount = new AccountIdentifier("anonymous918b409cd64240648287969f2827f0c1", false, AccountPermissions.Read, SecretKeyPermissions.Read, true);
		}

		protected AuthenticationManager()
		{
		}

		public static bool AreSignaturesEqual(string signature1, string signature2)
		{
			if (signature1.Length != signature2.Length)
			{
				return false;
			}
			int num = 1;
			for (int i = 0; i < signature1.Length; i++)
			{
				num = num & (signature1[i] == signature2[i] ? 1 : 0);
			}
			return num == 1;
		}

		public abstract IAuthenticationResult Authenticate(RequestContext requestContext, NephosUriComponents uriComponents, Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.GetStringToSignCallback getStringToSignCallback, TimeSpan timeout);

		public abstract IAsyncResult BeginAuthenticate(IStorageAccount storageAccount, RequestContext requestContext, NephosUriComponents uriComponents, Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.GetStringToSignCallback getStringToSignCallback, TimeSpan timeout, AsyncCallback callback, object state);

		public abstract IAuthenticationResult EndAuthenticate(IAsyncResult asyncResult);

		public static bool IsAccountSasAccess(NameValueCollection queryParameters)
		{
			string item = queryParameters["sv"];
			if (string.IsNullOrWhiteSpace(queryParameters["ss"]))
			{
				return false;
			}
			return !VersioningHelper.IsPreApril15OrInvalidVersion(item);
		}

		public static bool IsAnonymousAccess(RequestContext requestContext)
		{
			if (Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.IsInvalidAccess(requestContext) || Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.IsAuthenticatedAccess(requestContext))
			{
				return false;
			}
			return !Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.IsSignatureAccess(requestContext);
		}

		public static bool IsAuthenticatedAccess(RequestContext requestContext)
		{
			return requestContext.RequestHeaders.Get("Authorization") != null;
		}

		public static bool IsAuthenticatedAccess(RequestContext requestContext, string expectedAuthScheme)
		{
			string authorizationScheme;
			if (!Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.IsAuthenticatedAccess(requestContext))
			{
				return false;
			}
			try
			{
				authorizationScheme = requestContext.AuthorizationScheme;
				string authorizationSchemeParameters = requestContext.AuthorizationSchemeParameters;
			}
			catch (HttpRequestInvalidHeaderException httpRequestInvalidHeaderException1)
			{
				HttpRequestInvalidHeaderException httpRequestInvalidHeaderException = httpRequestInvalidHeaderException1;
				throw new InvalidAuthenticationInfoException(httpRequestInvalidHeaderException.Message, httpRequestInvalidHeaderException);
			}
			catch (HttpRequestDuplicateHeaderException httpRequestDuplicateHeaderException)
			{
				throw new InvalidAuthenticationInfoException("Duplicate authorization headers found", httpRequestDuplicateHeaderException);
			}
			return authorizationScheme == expectedAuthScheme;
		}

		public static bool IsInvalidAccess(RequestContext requestContext)
		{
			if (requestContext.QueryParameters["sig"] == null)
			{
				return false;
			}
			return requestContext.RequestHeaders.Get("Authorization") != null;
		}

		public static bool IsSignatureAccess(RequestContext requestContext)
		{
			return requestContext.QueryParameters["sig"] != null;
		}

		public abstract string ParseAccountNameFromAuthorizationHeader(RequestContext requestContext, NephosUriComponents uriComponents);

		public delegate string GetStringToSignCallback(RequestContext requestContext, NephosUriComponents uriComponents, SupportedAuthScheme requestAuthScheme);
	}
}