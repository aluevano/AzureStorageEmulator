using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public class XFEBlobAuthenticationManager : AuthenticationManager
	{
		private NephosAuthenticationManager nephosAuthenticationManager;

		private IStorageManager storageManager;

		protected bool DenyUnversionedAuthenticatedAccess
		{
			get;
			set;
		}

		public override Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get
			{
				return this.nephosAuthenticationManager.OperationStatus;
			}
			set
			{
				this.nephosAuthenticationManager.OperationStatus = value;
			}
		}

		private XFEBlobAuthenticationManager(IStorageManager manager)
		{
			this.storageManager = manager;
			this.nephosAuthenticationManager = NephosAuthenticationManager.CreateAuthenticationManager(manager) as NephosAuthenticationManager;
			NephosAssertionException.Assert(this.nephosAuthenticationManager != null);
		}

		public override IAuthenticationResult Authenticate(RequestContext requestContext, NephosUriComponents uriComponents, AuthenticationManager.GetStringToSignCallback getStringToSignCallback, TimeSpan timeout)
		{
			return this.EndAuthenticate(this.BeginAuthenticate(null, requestContext, uriComponents, getStringToSignCallback, timeout, null, null));
		}

		private IEnumerator<IAsyncResult> AuthenticateImpl(IStorageAccount storageAccount, RequestContext requestContext, NephosUriComponents uriComponents, AuthenticationManager.GetStringToSignCallback getStringToSignCallback, TimeSpan timeout, AsyncIteratorContext<IAuthenticationResult> context)
		{
			bool flag;
			SignedAccessHelper blobSignedAccessHelper;
			IStorageAccount operationStatus;
			ContainerAclSettings containerAclSetting;
			Duration startingNow = Duration.StartingNow;
			NameValueCollection queryParameters = requestContext.QueryParameters;
			if (AuthenticationManager.IsInvalidAccess(requestContext))
			{
				throw new InvalidAuthenticationInfoException("Ambiguous authentication scheme credentials providedRequest contains authentication credentials for signed access and authenticated access");
			}
			bool flag1 = AuthenticationManager.IsAuthenticatedAccess(requestContext);
			bool flag2 = AuthenticationManager.IsSignatureAccess(requestContext);
			flag = (!flag1 ? false : AuthenticationManager.IsAuthenticatedAccess(requestContext, "SignedKey"));
			bool flag3 = flag;
			bool flag4 = (flag1 ? false : !flag2);
			bool flag5 = string.IsNullOrEmpty(requestContext.RequestHeaderRestVersion);
			if (this.DenyUnversionedAuthenticatedAccess && flag5 && flag1)
			{
				throw new AuthenticationFailureException(string.Format(CultureInfo.InvariantCulture, "Unversioned authenticated access is not allowed.", new object[0]));
			}
			if ((!flag1 || flag3) && !flag4)
			{
				NephosAssertionException.Assert((flag2 ? true : flag3));
				bool flag6 = (flag2 ? false : flag3);
				if (!AuthenticationManager.IsAccountSasAccess(requestContext.QueryParameters))
				{
					blobSignedAccessHelper = new BlobSignedAccessHelper(requestContext, uriComponents);
				}
				else
				{
					if (flag6)
					{
						throw new AuthenticationFailureException("SignedKey is not supported with account-level SAS.");
					}
					blobSignedAccessHelper = new AccountSasHelper(requestContext, uriComponents);
				}
				blobSignedAccessHelper.ParseAccessPolicyFields(flag6);
				blobSignedAccessHelper.PerformSignedAccessAuthenticationFirstPhaseValidations();
				AccountIdentifier signedAccessAccountIdentifier = null;
				if (!flag6)
				{
					byte[] sign = blobSignedAccessHelper.ComputeUrlDecodedUtf8EncodedStringToSign();
					if (storageAccount == null || !string.Equals(storageAccount.Name, uriComponents.AccountName))
					{
						try
						{
							operationStatus = this.storageManager.CreateAccountInstance(uriComponents.AccountName);
							if (requestContext != null)
							{
								operationStatus.OperationStatus = requestContext.OperationStatus;
							}
						}
						catch (ArgumentOutOfRangeException argumentOutOfRangeException)
						{
							throw new AuthenticationFailureException(string.Format(CultureInfo.InvariantCulture, "The account name is invalid.", new object[0]));
						}
						operationStatus.Timeout = startingNow.Remaining(timeout);
						IAsyncResult asyncResult = operationStatus.BeginGetProperties(AccountPropertyNames.All, null, context.GetResumeCallback(), context.GetResumeState("XFEBlobAuthenticationManager.AuthenticateImpl"));
						yield return asyncResult;
						try
						{
							operationStatus.EndGetProperties(asyncResult);
						}
						catch (AccountNotFoundException accountNotFoundException1)
						{
							AccountNotFoundException accountNotFoundException = accountNotFoundException1;
							CultureInfo invariantCulture = CultureInfo.InvariantCulture;
							object[] name = new object[] { operationStatus.Name };
							throw new AuthenticationFailureException(string.Format(invariantCulture, "Cannot find the claimed account when trying to GetProperties for the account {0}.", name), accountNotFoundException);
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							IStringDataEventStream warning = Logger<IRestProtocolHeadLogger>.Instance.Warning;
							object[] objArray = new object[] { operationStatus.Name, exception };
							warning.Log("Rethrow exception when trying to GetProperties for the account {0}: {1}", objArray);
							throw;
						}
					}
					else
					{
						operationStatus = storageAccount;
					}
					if (!blobSignedAccessHelper.ComputeSignatureAndCompare(sign, operationStatus.SecretKeysV3))
					{
						throw new AuthenticationFailureException(string.Concat("Signature did not match. String to sign used was ", (new UTF8Encoding()).GetString(sign)));
					}
					NephosAssertionException.Assert(blobSignedAccessHelper.KeyUsedForSigning != null, "Key used for signing cannot be null");
					signedAccessAccountIdentifier = blobSignedAccessHelper.CreateAccountIdentifier(operationStatus);
					if (storageAccount != operationStatus)
					{
						operationStatus.Dispose();
					}
				}
				else
				{
					IAsyncResult asyncResult1 = this.nephosAuthenticationManager.BeginAuthenticate(storageAccount, requestContext, uriComponents, getStringToSignCallback, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("XFEBlobAuthenticationManager.AuthenticateImpl"));
					yield return asyncResult1;
					IAuthenticationResult authenticationResult = this.nephosAuthenticationManager.EndAuthenticate(asyncResult1);
					signedAccessAccountIdentifier = new SignedAccessAccountIdentifier(authenticationResult.AccountIdentifier);
				}
				if (blobSignedAccessHelper.IsRevocableAccess)
				{
					using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(uriComponents.AccountName, uriComponents.ContainerName))
					{
						ContainerPropertyNames containerPropertyName = ContainerPropertyNames.ServiceMetadata;
						if (requestContext != null)
						{
							blobContainer.OperationStatus = requestContext.OperationStatus;
						}
						blobContainer.Timeout = startingNow.Remaining(timeout);
						IAsyncResult asyncResult2 = blobContainer.BeginGetProperties(containerPropertyName, null, context.GetResumeCallback(), context.GetResumeState("XFEBlobAuthenticationManager.AuthenticateImpl"));
						yield return asyncResult2;
						try
						{
							blobContainer.EndGetProperties(asyncResult2);
						}
						catch (Exception exception3)
						{
							Exception exception2 = exception3;
							if (exception2 is ContainerNotFoundException)
							{
								throw new AuthenticationFailureException("Error locating SAS identifier", exception2);
							}
							IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Warning;
							object[] accountName = new object[] { uriComponents.AccountName, uriComponents.ContainerName, exception2 };
							stringDataEventStream.Log("Rethrow exception when trying to fetch SAS identifier account {0} container {1} : {2}", accountName);
							throw;
						}
						try
						{
							containerAclSetting = new ContainerAclSettings(blobContainer.ServiceMetadata);
						}
						catch (MetadataFormatException metadataFormatException1)
						{
							MetadataFormatException metadataFormatException = metadataFormatException1;
							throw new NephosStorageDataCorruptionException(string.Format("Error decoding Acl setting for container {0}", uriComponents.ContainerName), metadataFormatException);
						}
					}
					try
					{
						blobSignedAccessHelper.ValidateAndDeriveEffectiveAccessPolicy(blobSignedAccessHelper.LocateSasIdentifier(containerAclSetting.SASIdentifiers));
						blobSignedAccessHelper.PerformSignedAccessAuthenticationSecondPhaseValidations();
						signedAccessAccountIdentifier.Initialize(blobSignedAccessHelper);
						context.ResultData = new AuthenticationResult(signedAccessAccountIdentifier, blobSignedAccessHelper.SignedVersion, true);
					}
					catch (FormatException formatException)
					{
						throw new AuthenticationFailureException("Signature fields not well formed.", formatException);
					}
				}
				else
				{
					signedAccessAccountIdentifier.Initialize(blobSignedAccessHelper);
					context.ResultData = new AuthenticationResult(signedAccessAccountIdentifier, blobSignedAccessHelper.SignedVersion, true);
				}
			}
			else
			{
				IAsyncResult asyncResult3 = this.nephosAuthenticationManager.BeginAuthenticate(storageAccount, requestContext, uriComponents, getStringToSignCallback, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("XFEBlobAuthenticationManager.AuthenticateImpl"));
				yield return asyncResult3;
				context.ResultData = this.nephosAuthenticationManager.EndAuthenticate(asyncResult3);
			}
		}

		public override IAsyncResult BeginAuthenticate(IStorageAccount storageAccount, RequestContext requestContext, NephosUriComponents uriComponents, AuthenticationManager.GetStringToSignCallback getStringToSignCallback, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IAuthenticationResult> asyncIteratorContext = new AsyncIteratorContext<IAuthenticationResult>("XFEBlobAuthenticationManager.Authenticate", callback, state);
			asyncIteratorContext.Begin(this.AuthenticateImpl(storageAccount, requestContext, uriComponents, getStringToSignCallback, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public static AuthenticationManager CreateAuthenticationManager(IStorageManager manager, bool denyUnversionedAuthenticatedAccess = false)
		{
			if (manager == null)
			{
				throw new ArgumentNullException("manager");
			}
			return new XFEBlobAuthenticationManager(manager)
			{
				DenyUnversionedAuthenticatedAccess = denyUnversionedAuthenticatedAccess
			};
		}

		public override IAuthenticationResult EndAuthenticate(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<IAuthenticationResult> asyncIteratorContext = (AsyncIteratorContext<IAuthenticationResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override string ParseAccountNameFromAuthorizationHeader(RequestContext requestContext, NephosUriComponents uriComponents)
		{
			return this.nephosAuthenticationManager.ParseAccountNameFromAuthorizationHeader(requestContext, uriComponents);
		}

		public static AuthDataEntry SignedKeyAuthenticate(string stringToSign, string requestSignature, AuthenticationInformation authInfo)
		{
			AuthDataEntry authDataEntry;
			NephosAssertionException.Assert(!string.IsNullOrEmpty(stringToSign));
			NephosAssertionException.Assert(!string.IsNullOrEmpty(requestSignature));
			NephosAssertionException.Assert(authInfo != null);
			RequestContext requestContext = authInfo.RequestContext;
			NephosUriComponents uriComponents = authInfo.UriComponents;
			NameValueCollection queryParameters = requestContext.QueryParameters;
			string item = queryParameters["sv"];
			byte[] sign = BlobSignedAccessHelper.ComputeUrlDecodedUtf8EncodedStringToSign(queryParameters, uriComponents);
			using (IEnumerator<AuthDataEntry> enumerator = SharedKeyAuthInfoHelper.GetSharedKeys(authInfo).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					AuthDataEntry current = enumerator.Current;
					byte[] numArray = SASUtilities.ComputeSignedKey(sign, current.AuthValue);
					if (!SASUtilities.ComputeSignatureAndCompare((new UTF8Encoding()).GetBytes(stringToSign), numArray, requestSignature))
					{
						continue;
					}
					authDataEntry = current;
					return authDataEntry;
				}
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { requestSignature, stringToSign };
				throw new AuthenticationFailureException(string.Format(invariantCulture, "The MAC signature found in the HTTP request '{0}' is not the same as any computed signature. Server used following string to sign: '{1}'.", objArray));
			}
			return authDataEntry;
		}

		[SuppressMessage("Anvil.Nullptr", "26501", Justification="NephosAssertionException.Assert does not return.")]
		public static void SignedKeyParamsParser(SupportedAuthScheme scheme, string parameter, NephosUriComponents uriComponents, out string accountName, out string signature)
		{
			NephosAssertionException.Assert(scheme.Equals(SupportedAuthScheme.SignedKey));
			NephosAssertionException.Assert((uriComponents == null ? false : !string.IsNullOrEmpty(uriComponents.AccountName)));
			if (string.IsNullOrEmpty(parameter))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { "Authorization", string.Format("{0} {1}", scheme, parameter) };
				throw new InvalidAuthenticationInfoException(string.Format(invariantCulture, "{0} value '{1}' is invalid.", objArray));
			}
			accountName = uriComponents.AccountName;
			signature = parameter;
		}
	}
}