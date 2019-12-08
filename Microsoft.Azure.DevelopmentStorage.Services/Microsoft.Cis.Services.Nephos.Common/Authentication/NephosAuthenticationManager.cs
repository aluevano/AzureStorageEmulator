using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public class NephosAuthenticationManager : AuthenticationManager
	{
		private static Dictionary<ServiceType, Dictionary<SupportedAuthScheme, AuthenticationMethod>> serviceAuthMethodsTable;

		private static Dictionary<ServiceType, Dictionary<SupportedAuthScheme, SchemeParamsParserMethod>> serviceSchemeParamsParserTable;

		private IStorageManager storageManager;

		private AuthenticationInformation authInfo;

		public override Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		static NephosAuthenticationManager()
		{
			NephosAuthenticationManager.serviceAuthMethodsTable = new Dictionary<ServiceType, Dictionary<SupportedAuthScheme, AuthenticationMethod>>();
			NephosAuthenticationManager.serviceSchemeParamsParserTable = new Dictionary<ServiceType, Dictionary<SupportedAuthScheme, SchemeParamsParserMethod>>();
			Dictionary<SupportedAuthScheme, AuthenticationMethod> supportedAuthSchemes = new Dictionary<SupportedAuthScheme, AuthenticationMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) }
			};
			NephosAuthenticationManager.serviceAuthMethodsTable.Add(ServiceType.StampXlsService, supportedAuthSchemes);
			Dictionary<SupportedAuthScheme, SchemeParamsParserMethod> supportedAuthSchemes1 = new Dictionary<SupportedAuthScheme, SchemeParamsParserMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) }
			};
			NephosAuthenticationManager.serviceSchemeParamsParserTable.Add(ServiceType.StampXlsService, supportedAuthSchemes1);
			supportedAuthSchemes = new Dictionary<SupportedAuthScheme, AuthenticationMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) }
			};
			NephosAuthenticationManager.serviceAuthMethodsTable.Add(ServiceType.StampService, supportedAuthSchemes);
			supportedAuthSchemes1 = new Dictionary<SupportedAuthScheme, SchemeParamsParserMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) }
			};
			NephosAuthenticationManager.serviceSchemeParamsParserTable.Add(ServiceType.StampService, supportedAuthSchemes1);
			supportedAuthSchemes = new Dictionary<SupportedAuthScheme, AuthenticationMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) }
			};
			NephosAuthenticationManager.serviceAuthMethodsTable.Add(ServiceType.LocationService, supportedAuthSchemes);
			supportedAuthSchemes1 = new Dictionary<SupportedAuthScheme, SchemeParamsParserMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) }
			};
			NephosAuthenticationManager.serviceSchemeParamsParserTable.Add(ServiceType.LocationService, supportedAuthSchemes1);
			supportedAuthSchemes = new Dictionary<SupportedAuthScheme, AuthenticationMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) }
			};
			NephosAuthenticationManager.serviceAuthMethodsTable.Add(ServiceType.LocationAccountService, supportedAuthSchemes);
			supportedAuthSchemes1 = new Dictionary<SupportedAuthScheme, SchemeParamsParserMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) }
			};
			NephosAuthenticationManager.serviceSchemeParamsParserTable.Add(ServiceType.LocationAccountService, supportedAuthSchemes1);
			supportedAuthSchemes = new Dictionary<SupportedAuthScheme, AuthenticationMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) },
				{ SupportedAuthScheme.SharedKeyLite, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) },
				{ SupportedAuthScheme.SignedKey, new AuthenticationMethod(XFEBlobAuthenticationManager.SignedKeyAuthenticate) }
			};
			NephosAuthenticationManager.serviceAuthMethodsTable.Add(ServiceType.BlobService, supportedAuthSchemes);
			supportedAuthSchemes1 = new Dictionary<SupportedAuthScheme, SchemeParamsParserMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) },
				{ SupportedAuthScheme.SharedKeyLite, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) },
				{ SupportedAuthScheme.SignedKey, new SchemeParamsParserMethod(XFEBlobAuthenticationManager.SignedKeyParamsParser) }
			};
			NephosAuthenticationManager.serviceSchemeParamsParserTable.Add(ServiceType.BlobService, supportedAuthSchemes1);
			supportedAuthSchemes = new Dictionary<SupportedAuthScheme, AuthenticationMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) },
				{ SupportedAuthScheme.SharedKeyLite, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) }
			};
			NephosAuthenticationManager.serviceAuthMethodsTable.Add(ServiceType.FileService, supportedAuthSchemes);
			supportedAuthSchemes1 = new Dictionary<SupportedAuthScheme, SchemeParamsParserMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) },
				{ SupportedAuthScheme.SharedKeyLite, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) }
			};
			NephosAuthenticationManager.serviceSchemeParamsParserTable.Add(ServiceType.FileService, supportedAuthSchemes1);
			supportedAuthSchemes = new Dictionary<SupportedAuthScheme, AuthenticationMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) },
				{ SupportedAuthScheme.SharedKeyLite, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) },
				{ SupportedAuthScheme.SignedKey, new AuthenticationMethod(XFEQueueAuthenticationManager.SignedKeyAuthenticate) }
			};
			NephosAuthenticationManager.serviceAuthMethodsTable.Add(ServiceType.QueueService, supportedAuthSchemes);
			supportedAuthSchemes1 = new Dictionary<SupportedAuthScheme, SchemeParamsParserMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) },
				{ SupportedAuthScheme.SharedKeyLite, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) },
				{ SupportedAuthScheme.SignedKey, new SchemeParamsParserMethod(XFEQueueAuthenticationManager.SignedKeyParamsParser) }
			};
			NephosAuthenticationManager.serviceSchemeParamsParserTable.Add(ServiceType.QueueService, supportedAuthSchemes1);
			supportedAuthSchemes = new Dictionary<SupportedAuthScheme, AuthenticationMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) },
				{ SupportedAuthScheme.SharedKeyLite, new AuthenticationMethod(NephosAuthenticationManager.SharedKeyAuthenticate) },
				{ SupportedAuthScheme.SignedKey, new AuthenticationMethod(XFETableAuthenticationManager.SignedKeyAuthenticate) }
			};
			NephosAuthenticationManager.serviceAuthMethodsTable.Add(ServiceType.TableService, supportedAuthSchemes);
			supportedAuthSchemes1 = new Dictionary<SupportedAuthScheme, SchemeParamsParserMethod>()
			{
				{ SupportedAuthScheme.SharedKey, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) },
				{ SupportedAuthScheme.SharedKeyLite, new SchemeParamsParserMethod(NephosAuthenticationManager.SharedKeyParamsParser) },
				{ SupportedAuthScheme.SignedKey, new SchemeParamsParserMethod(XFETableAuthenticationManager.SignedKeyParamsParser) }
			};
			NephosAuthenticationManager.serviceSchemeParamsParserTable.Add(ServiceType.TableService, supportedAuthSchemes1);
		}

		private NephosAuthenticationManager(IStorageManager manager)
		{
			this.storageManager = manager;
			this.authInfo = null;
		}

		private NephosAuthenticationManager(AuthenticationInformation authInfo)
		{
			this.authInfo = authInfo;
			this.storageManager = null;
		}

		public override IAuthenticationResult Authenticate(RequestContext requestContext, NephosUriComponents uriComponents, AuthenticationManager.GetStringToSignCallback getStringToSignCallback, TimeSpan timeout)
		{
			return this.EndAuthenticate(this.BeginAuthenticate(null, requestContext, uriComponents, getStringToSignCallback, timeout, null, null));
		}

		private IEnumerator<IAsyncResult> AuthenticateImpl(IStorageAccount storageAccount, RequestContext requestContext, NephosUriComponents uriComponents, AuthenticationManager.GetStringToSignCallback getStringToSignCallback, TimeSpan timeout, AsyncIteratorContext<IAuthenticationResult> context)
		{
			SupportedAuthScheme? nullable;
			IStorageAccount operationStatus;
			context.ResultData = null;
			if (getStringToSignCallback == null)
			{
				throw new ArgumentNullException("getStringToSignCallback");
			}
			string str = null;
			string str1 = null;
			bool flag = false;
			NephosAuthenticationManager.ParseSignatureParametersFromAuthorizationHeader(requestContext, uriComponents, false, out str, out str1, out nullable, out flag);
			if (!flag)
			{
				NephosAssertionException.Assert(nullable.HasValue, "Request Authentication Scheme should have a value");
				IAccountIdentifier accountIdentifier = null;
				AuthenticationInformation authInfoForScheme = null;
				if (this.storageManager != null)
				{
					if (storageAccount == null || !storageAccount.Name.Equals(str))
					{
						try
						{
							operationStatus = this.storageManager.CreateAccountInstance(str);
							if (requestContext != null)
							{
								operationStatus.OperationStatus = requestContext.OperationStatus;
							}
						}
						catch (ArgumentOutOfRangeException argumentOutOfRangeException)
						{
							throw new AuthenticationFailureException(string.Format(CultureInfo.InvariantCulture, "The account name is invalid.", new object[0]));
						}
						operationStatus.Timeout = timeout;
						IAsyncResult asyncResult = operationStatus.BeginGetProperties(AccountPropertyNames.All, null, context.GetResumeCallback(), context.GetResumeState("NephosAuthenticationManager.AuthenticateImpl"));
						yield return asyncResult;
						try
						{
							operationStatus.EndGetProperties(asyncResult);
						}
						catch (AccountNotFoundException accountNotFoundException1)
						{
							AccountNotFoundException accountNotFoundException = accountNotFoundException1;
							CultureInfo invariantCulture = CultureInfo.InvariantCulture;
							object[] objArray = new object[] { str };
							throw new AuthenticationFailureException(string.Format(invariantCulture, "Cannot find the claimed account when trying to GetProperties for the account {0}.", objArray), accountNotFoundException);
						}
						catch (Exception exception1)
						{
							Exception exception = exception1;
							IStringDataEventStream warning = Logger<IRestProtocolHeadLogger>.Instance.Warning;
							warning.Log("Rethrow exception when trying to GetProperties for the account {0}: {1}", new object[] { str, exception });
							throw;
						}
					}
					else
					{
						operationStatus = storageAccount;
						if (requestContext != null)
						{
							operationStatus.OperationStatus = requestContext.OperationStatus;
						}
					}
					authInfoForScheme = NephosAuthenticationManager.GetAuthInfoForScheme(operationStatus, nullable.Value, requestContext, uriComponents, false);
					if (authInfoForScheme == null)
					{
						if (storageAccount != operationStatus)
						{
							operationStatus.Dispose();
						}
						CultureInfo cultureInfo = CultureInfo.InvariantCulture;
						object[] name = new object[] { nullable, operationStatus.Name };
						throw new AuthenticationFailureException(string.Format(cultureInfo, "Authentication scheme {0} is not supported by account {1}.", name));
					}
					SecretKeyPermissions permissions = SecretKeyPermissions.Full;
					if (authInfoForScheme.NamedKeyAuthData != null)
					{
						permissions = authInfoForScheme.NamedKeyAuthData.Permissions;
					}
					accountIdentifier = new AccountIdentifier(operationStatus, permissions);
					if (storageAccount != operationStatus)
					{
						operationStatus.Dispose();
					}
				}
				string str2 = null;
				try
				{
					str2 = getStringToSignCallback(requestContext, uriComponents, nullable.Value);
				}
				catch (NotSupportedException notSupportedException)
				{
					CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
					object[] objArray1 = new object[] { nullable };
					throw new AuthenticationFailureException(string.Format(invariantCulture1, "Authentication scheme {0} is not supported.", objArray1));
				}
				AuthenticationMethod item = NephosAuthenticationManager.serviceAuthMethodsTable[requestContext.ServiceType][nullable.Value];
				item(str2, str1, authInfoForScheme);
				NephosAssertionException.Assert(accountIdentifier != null);
				context.ResultData = new AuthenticationResult(accountIdentifier, null, false);
			}
			else
			{
				context.ResultData = new AuthenticationResult(AuthenticationManager.AnonymousAccount, null, false);
			}
		}

		public override IAsyncResult BeginAuthenticate(IStorageAccount storageAccount, RequestContext requestContext, NephosUriComponents uriComponents, AuthenticationManager.GetStringToSignCallback getStringToSignCallback, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IAuthenticationResult> asyncIteratorContext = new AsyncIteratorContext<IAuthenticationResult>("NephosAuthenticationManager.Authenticate", callback, state);
			asyncIteratorContext.Begin(this.AuthenticateImpl(storageAccount, requestContext, uriComponents, getStringToSignCallback, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public static AuthenticationManager CreateAuthenticationManager(IStorageManager manager)
		{
			if (manager == null)
			{
				throw new ArgumentNullException("manager");
			}
			return new NephosAuthenticationManager(manager);
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

		private static AuthenticationInformation GetAuthInfoForScheme(IStorageAccount account, SupportedAuthScheme authScheme, RequestContext requestContext, NephosUriComponents uriComponents, bool isInterStampAuthentication)
		{
			if (authScheme != SupportedAuthScheme.SharedKey && authScheme != SupportedAuthScheme.SharedKeyLite && authScheme != SupportedAuthScheme.SignedKey)
			{
				return null;
			}
			Collection<AuthDataEntry> authDataEntries = new Collection<AuthDataEntry>();
			if (account.SecretKeysV3 == null)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Critical.Log("Attempting to authenticate against account {0} which does not have secretKeyListV3", new object[] { account.Name });
			}
			else
			{
				foreach (SecretKeyV3 secretKeysV3 in account.SecretKeysV3)
				{
					authDataEntries.Add(new AuthDataEntry(secretKeysV3.Name, secretKeysV3.Value, secretKeysV3.Permissions));
				}
			}
			return new AuthenticationInformation("SharedKey", authScheme, authDataEntries, requestContext, uriComponents, isInterStampAuthentication);
		}

		public override string ParseAccountNameFromAuthorizationHeader(RequestContext requestContext, NephosUriComponents uriComponents)
		{
			SupportedAuthScheme? nullable;
			string str = null;
			string str1 = null;
			bool flag = false;
			NephosAuthenticationManager.ParseSignatureParametersFromAuthorizationHeader(requestContext, uriComponents, true, out str, out str1, out nullable, out flag);
			return str;
		}

		public static void ParseSignatureParametersFromAuthorizationHeader(RequestContext requestContext, NephosUriComponents uriComponents, bool swallowException, out string accountName, out string signature, out SupportedAuthScheme? requestAuthScheme, out bool isAnonymousAccount)
		{
			string authorizationScheme;
			string authorizationSchemeParameters;
			accountName = null;
			signature = null;
			isAnonymousAccount = false;
			requestAuthScheme = null;
			try
			{
				try
				{
					authorizationScheme = requestContext.AuthorizationScheme;
					authorizationSchemeParameters = requestContext.AuthorizationSchemeParameters;
				}
				catch (HttpRequestHeaderNotFoundException httpRequestHeaderNotFoundException)
				{
					isAnonymousAccount = true;
					return;
				}
				catch (HttpRequestInvalidHeaderException httpRequestInvalidHeaderException1)
				{
					HttpRequestInvalidHeaderException httpRequestInvalidHeaderException = httpRequestInvalidHeaderException1;
					throw new InvalidAuthenticationInfoException(httpRequestInvalidHeaderException.Message, httpRequestInvalidHeaderException);
				}
				catch (HttpRequestDuplicateHeaderException httpRequestDuplicateHeaderException1)
				{
					HttpRequestDuplicateHeaderException httpRequestDuplicateHeaderException = httpRequestDuplicateHeaderException1;
					throw new InvalidAuthenticationInfoException(httpRequestDuplicateHeaderException.Message, httpRequestDuplicateHeaderException);
				}
				DateTime? nephosOrStandardDateHeader = null;
				try
				{
					nephosOrStandardDateHeader = HttpRequestAccessorCommon.GetNephosOrStandardDateHeader(requestContext.RequestHeaders);
				}
				catch (ArgumentException argumentException)
				{
					throw new AuthenticationFailureException("The Date header in the request is incorrect.", argumentException);
				}
				if (!nephosOrStandardDateHeader.HasValue)
				{
					throw new AuthenticationFailureException("Request date header not specified");
				}
				if (nephosOrStandardDateHeader.Value.Ticks < DateTime.UtcNow.Subtract(ParameterLimits.DateHeaderLag).Ticks)
				{
					DateTime value = nephosOrStandardDateHeader.Value;
					throw new AuthenticationFailureException(string.Format("Request date header too old: '{0}'", HttpUtilities.ConvertDateTimeToHttpString(value.ToUniversalTime())));
				}
				try
				{
					requestAuthScheme = new SupportedAuthScheme?((SupportedAuthScheme)Enum.Parse(typeof(SupportedAuthScheme), authorizationScheme, true));
				}
				catch (ArgumentException argumentException1)
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { authorizationScheme };
					throw new AuthenticationFailureException(string.Format(invariantCulture, "Authentication scheme {0} is not supported", objArray));
				}
				if (!requestAuthScheme.Equals(SupportedAuthScheme.SharedKey) && !requestAuthScheme.Equals(SupportedAuthScheme.SharedKeyLite) && !requestAuthScheme.Equals(SupportedAuthScheme.SignedKey))
				{
					CultureInfo cultureInfo = CultureInfo.InvariantCulture;
					object[] objArray1 = new object[] { authorizationScheme };
					throw new AuthenticationFailureException(string.Format(cultureInfo, "Authentication scheme {0} is not supported", objArray1));
				}
				NephosAssertionException.Assert(requestAuthScheme.HasValue, "Authentication scheme in request has no value");
				NephosAuthenticationManager.serviceSchemeParamsParserTable[requestContext.ServiceType][requestAuthScheme.Value](requestAuthScheme.Value, authorizationSchemeParameters, uriComponents, out accountName, out signature);
			}
			catch (Exception exception)
			{
				if (!swallowException)
				{
					throw;
				}
			}
		}

		private static AuthDataEntry SharedKeyAuthenticate(string stringToSign, string requestSignature, AuthenticationInformation authInfo)
		{
			AuthDataEntry item;
			object[] objArray;
			object[] objArray1;
			IStringDataEventStream authenticationFailure;
			CultureInfo invariantCulture;
			if (string.IsNullOrEmpty(stringToSign))
			{
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				object[] authScheme = new object[] { authInfo.AuthScheme };
				throw new AuthenticationFailureException(string.Format(cultureInfo, "String to sign for auth scheme {0} cannot be null or empty.", authScheme));
			}
			NephosAssertionException.Assert(!string.IsNullOrEmpty(requestSignature));
			try
			{
				Convert.FromBase64String(requestSignature);
			}
			catch (FormatException formatException)
			{
				throw new InvalidAuthenticationInfoException("Signature is not a valid base64 string.", formatException);
			}
			Collection<AuthDataEntry> sharedKeys = SharedKeyAuthInfoHelper.GetSharedKeys(authInfo);
			if (sharedKeys.Count > 0)
			{
				HMAC authValue = HMACCryptoCache.Instance.Acquire(sharedKeys[0].AuthValue);
				try
				{
					int num = 0;
					while (num < sharedKeys.Count)
					{
						authValue.Key = sharedKeys[num].AuthValue;
						string str = MessageHashFunctions.ComputeMacWithSpecificAlgorithm(authValue, stringToSign);
						if (!AuthenticationManager.AreSignaturesEqual(str, requestSignature))
						{
							IStringDataEventStream infoDebug = Logger<IRestProtocolHeadLogger>.Instance.InfoDebug;
							object[] objArray2 = new object[] { stringToSign, str, requestSignature };
							infoDebug.Log("Authentication Debug. stringToSign: {0}, computedSignature: {1}, requestSignature: {2}", objArray2);
							IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.AuthenticationFailure;
							object[] objArray3 = new object[] { num + 1 };
							stringDataEventStream.Log("SecurityWarning: Authentication attempt failed against key {0}.", objArray3);
							num++;
						}
						else
						{
							item = sharedKeys[num];
							return item;
						}
					}
					authenticationFailure = Logger<IRestProtocolHeadLogger>.Instance.AuthenticationFailure;
					objArray = new object[] { stringToSign };
					authenticationFailure.Log("SecurityWarning: Authentication failed due to signature mismatch. Server's stringtosign value is {0}", objArray);
					invariantCulture = CultureInfo.InvariantCulture;
					objArray1 = new object[] { requestSignature, stringToSign };
					throw new AuthenticationFailureException(string.Format(invariantCulture, "The MAC signature found in the HTTP request '{0}' is not the same as any computed signature. Server used following string to sign: '{1}'.", objArray1));
				}
				finally
				{
					HMACCryptoCache.Instance.Release(authValue);
				}
				return item;
			}
			authenticationFailure = Logger<IRestProtocolHeadLogger>.Instance.AuthenticationFailure;
			objArray = new object[] { stringToSign };
			authenticationFailure.Log("SecurityWarning: Authentication failed due to signature mismatch. Server's stringtosign value is {0}", objArray);
			invariantCulture = CultureInfo.InvariantCulture;
			objArray1 = new object[] { requestSignature, stringToSign };
			throw new AuthenticationFailureException(string.Format(invariantCulture, "The MAC signature found in the HTTP request '{0}' is not the same as any computed signature. Server used following string to sign: '{1}'.", objArray1));
		}

		public static void SharedKeyParamsParser(SupportedAuthScheme scheme, string parameter, NephosUriComponents uriComponents, out string accountName, out string signature)
		{
			NephosAssertionException.Assert((scheme.Equals(SupportedAuthScheme.SharedKey) ? true : scheme.Equals(SupportedAuthScheme.SharedKeyLite)));
			NephosAssertionException.Assert(!string.IsNullOrEmpty(parameter));
			string[] strArrays = parameter.Split(HttpRequestAccessorCommon.colonDelimiter, StringSplitOptions.RemoveEmptyEntries);
			if ((int)strArrays.Length != 2)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { "Authorization", string.Format("{0} {1}", scheme, parameter) };
				throw new InvalidAuthenticationInfoException(string.Format(invariantCulture, "{0} value '{1}' is invalid.", objArray));
			}
			accountName = strArrays[0];
			signature = strArrays[1];
		}
	}
}