using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public abstract class SignedAccessHelper
	{
		private readonly static int Sha256HashSize;

		public abstract List<SASAccessRestriction> AccessRestrictions
		{
			get;
		}

		public bool IsRevocableAccess
		{
			get
			{
				return !string.IsNullOrEmpty(this.SignedIdentifier);
			}
		}

		public string KeyName
		{
			get;
			protected set;
		}

		public SecretKeyV3 KeyUsedForSigning
		{
			get;
			protected set;
		}

		public NameValueCollection QueryParams
		{
			get;
			private set;
		}

		public Microsoft.Cis.Services.Nephos.Common.RequestContext RequestContext
		{
			get;
			private set;
		}

		public string Signature
		{
			get;
			protected set;
		}

		public DateTime? SignedExpiry
		{
			get;
			protected set;
		}

		public SASExtraPermission? SignedExtraPermission
		{
			get;
			protected set;
		}

		public string SignedIdentifier
		{
			get;
			protected set;
		}

		public IPAddressRange SignedIP
		{
			get;
			private set;
		}

		public SASPermission? SignedPermission
		{
			get;
			protected set;
		}

		public SasProtocol SignedProtocol
		{
			get;
			private set;
		}

		public DateTime? SignedStart
		{
			get;
			protected set;
		}

		public string SignedVersion
		{
			get;
			protected set;
		}

		public NephosUriComponents UriComponents
		{
			get;
			private set;
		}

		static SignedAccessHelper()
		{
			using (HMACSHA256 hMACSHA256 = new HMACSHA256())
			{
				SignedAccessHelper.Sha256HashSize = hMACSHA256.HashSize;
			}
		}

		public SignedAccessHelper(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, NephosUriComponents uriComponents)
		{
			NephosAssertionException.Assert(requestContext != null);
			NephosAssertionException.Assert(uriComponents != null);
			NephosAssertionException.Assert(requestContext.QueryParameters != null);
			this.QueryParams = requestContext.QueryParameters;
			this.RequestContext = requestContext;
			this.UriComponents = uriComponents;
			this.SignedProtocol = SasProtocol.All;
		}

		public bool ComputeSignatureAndCompare(byte[] stringToSign, SecretKeyListV3 keys)
		{
			bool flag;
			NephosAssertionException.Assert(stringToSign != null);
			NephosAssertionException.Assert(keys != null);
			if (keys.Count == 0)
			{
				throw new ArgumentException("Invalid number of keys");
			}
			bool flag1 = string.IsNullOrEmpty(this.KeyName);
			List<SecretKeyV3>.Enumerator enumerator = keys.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SecretKeyV3 current = enumerator.Current;
					if ((!flag1 || !current.IsDefault()) && !string.Equals(current.Name, this.KeyName) || !SASUtilities.ComputeSignatureAndCompare(stringToSign, current.Value, this.Signature))
					{
						continue;
					}
					this.KeyUsedForSigning = current;
					flag = true;
					return flag;
				}
				return false;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return flag;
		}

		public abstract byte[] ComputeUrlDecodedUtf8EncodedStringToSign();

		public abstract AccountIdentifier CreateAccountIdentifier(IStorageAccount account);

		protected string ExtractSignedAuthorization(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext)
		{
			string authorizationScheme;
			string authorizationSchemeParameters;
			try
			{
				authorizationScheme = requestContext.AuthorizationScheme;
				authorizationSchemeParameters = requestContext.AuthorizationSchemeParameters;
			}
			catch (HttpRequestHeaderNotFoundException httpRequestHeaderNotFoundException)
			{
				throw new AuthenticationFailureException("Authorization header not found", httpRequestHeaderNotFoundException);
			}
			catch (HttpRequestInvalidHeaderException httpRequestInvalidHeaderException1)
			{
				HttpRequestInvalidHeaderException httpRequestInvalidHeaderException = httpRequestInvalidHeaderException1;
				throw new InvalidAuthenticationInfoException(httpRequestInvalidHeaderException.Message, httpRequestInvalidHeaderException);
			}
			catch (HttpRequestDuplicateHeaderException httpRequestDuplicateHeaderException)
			{
				throw new InvalidAuthenticationInfoException("Duplicate authorization headers found");
			}
			if (string.IsNullOrEmpty(authorizationSchemeParameters) || string.IsNullOrEmpty(authorizationScheme) || !authorizationScheme.Equals("SignedKey"))
			{
				throw new AuthenticationFailureException(string.Concat("SignedKey scheme expected but found ", authorizationScheme));
			}
			return authorizationSchemeParameters;
		}

		public SASIdentifier LocateSasIdentifier(List<SASIdentifier> sasIdentifiers)
		{
			SASIdentifier sASIdentifier;
			List<SASIdentifier>.Enumerator enumerator = sasIdentifiers.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SASIdentifier current = enumerator.Current;
					if (string.IsNullOrEmpty(current.Id) || !current.Id.Equals(this.SignedIdentifier))
					{
						continue;
					}
					sASIdentifier = current;
					return sASIdentifier;
				}
				throw new AuthenticationFailureException("SAS identifier cannot be found for specified signed identifier");
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return sASIdentifier;
		}

		public virtual void ParseAccessPolicyFields(bool isDoubleSigned)
		{
			string item = this.QueryParams["st"];
			this.ValidateOptionalField(item, "st");
			if (item != null)
			{
				this.SignedStart = new DateTime?(SASUtilities.ParseTime(item));
			}
			string str = this.QueryParams["se"];
			if (!this.IsRevocableAccess)
			{
				this.ValidateMandatoryField(str, "se");
				this.SignedExpiry = new DateTime?(SASUtilities.ParseTime(str));
			}
			else
			{
				this.ValidateOptionalField(str, "se");
				if (str != null)
				{
					this.SignedExpiry = new DateTime?(SASUtilities.ParseTime(str));
				}
			}
			if (!VersioningHelper.IsPreApril15OrInvalidVersion(this.SignedVersion))
			{
				string item1 = this.QueryParams["sip"];
				this.ValidateOptionalField(item1, "sip");
				if (item1 != null)
				{
					try
					{
						this.SignedIP = SASUtilities.ParseSip(item1);
					}
					catch (ArgumentOutOfRangeException argumentOutOfRangeException)
					{
						throw new FormatException("Invalid sip format", argumentOutOfRangeException);
					}
				}
				string str1 = this.QueryParams["spr"];
				this.ValidateOptionalField(str1, "spr");
				if (str1 != null)
				{
					this.SignedProtocol = SASUtilities.ParseSignedProtocol(str1);
				}
			}
			string item2 = this.QueryParams["sep"];
			this.ValidateOptionalField(item2, "sep");
			if (item2 != null)
			{
				this.SignedExtraPermission = SASUtilities.ParseExtraPermission(item2);
			}
		}

		public virtual void PerformSignedAccessAuthenticationFirstPhaseValidations()
		{
			if (!this.IsRevocableAccess)
			{
				if (!this.SignedExpiry.HasValue)
				{
					throw new ArgumentException("Signed expiry is missing");
				}
				this.ValidateAccessTimeframe((this.SignedStart.HasValue ? this.SignedStart.Value : DateTime.UtcNow), this.SignedExpiry.Value);
				return;
			}
			if (!this.SignedExpiry.HasValue)
			{
				if (this.SignedStart.HasValue && this.SignedStart.Value > DateTime.UtcNow)
				{
					throw new AuthenticationFailureException("Start time is after the current time");
				}
				return;
			}
			this.ValidateAccessTimeframe((this.SignedStart.HasValue ? this.SignedStart.Value : DateTime.UtcNow), this.SignedExpiry.Value);
		}

		public virtual void PerformSignedAccessAuthenticationSecondPhaseValidations()
		{
			bool flag;
			if (!this.SignedPermission.HasValue)
			{
				flag = false;
			}
			else
			{
				SASPermission? signedPermission = this.SignedPermission;
				flag = (signedPermission.GetValueOrDefault() != SASPermission.None ? true : !signedPermission.HasValue);
			}
			NephosAssertionException.Assert(flag);
			try
			{
				NephosAssertionException.Assert(this.SignedExpiry.HasValue);
				this.ValidateAccessTimeframe((this.SignedStart.HasValue ? this.SignedStart.Value : DateTime.UtcNow), this.SignedExpiry.Value);
			}
			catch (FormatException formatException)
			{
				throw new AuthenticationFailureException("Signature fields not well formed.", formatException);
			}
		}

		private void ValidateAccessTimeframe(DateTime startTime, DateTime expiryTime)
		{
			if (startTime >= expiryTime)
			{
				throw new AuthenticationFailureException(string.Format("Signed expiry time [{0}] has to be after signed start time [{1}]", expiryTime.ToString("R"), startTime.ToString("R")));
			}
			DateTime utcNow = DateTime.UtcNow;
			if (!(utcNow >= startTime) || !(utcNow < expiryTime))
			{
				throw new AuthenticationFailureException(string.Format("Signature not valid in the specified time frame: Start [{0}] - Expiry [{1}] - Current [{2}]", startTime.ToString("R"), expiryTime.ToString("R"), utcNow.ToString("R")));
			}
		}

		public virtual void ValidateAndDeriveEffectiveAccessPolicy(SASIdentifier sasIdentifier)
		{
			NephosAssertionException.Assert(sasIdentifier.Id.Equals(this.SignedIdentifier));
			if (!this.SignedExpiry.HasValue && !sasIdentifier.AccessPolicy.SignedExpiry.HasValue)
			{
				throw new AuthenticationFailureException("Signed expiry must be specified in signature or SAS identifier");
			}
			if (!this.SignedPermission.HasValue && !sasIdentifier.AccessPolicy.SignedPermission.HasValue)
			{
				throw new AuthenticationFailureException("Signed permission must be specified in signature or SAS identifier");
			}
			if (this.SignedStart.HasValue && sasIdentifier.AccessPolicy.SignedStart.HasValue || this.SignedExpiry.HasValue && sasIdentifier.AccessPolicy.SignedExpiry.HasValue || this.SignedPermission.HasValue && sasIdentifier.AccessPolicy.SignedPermission.HasValue)
			{
				throw new AuthenticationFailureException("Access policy fields can be associated with signature or SAS identifier but not both");
			}
			if (sasIdentifier.AccessPolicy.SignedStart.HasValue)
			{
				this.SignedStart = new DateTime?(sasIdentifier.AccessPolicy.SignedStart.Value);
			}
			if (sasIdentifier.AccessPolicy.SignedExpiry.HasValue)
			{
				NephosAssertionException.Assert(!this.SignedExpiry.HasValue);
				this.SignedExpiry = new DateTime?(sasIdentifier.AccessPolicy.SignedExpiry.Value);
			}
			if (sasIdentifier.AccessPolicy.SignedPermission.HasValue)
			{
				NephosAssertionException.Assert(!this.SignedPermission.HasValue);
				this.SignedPermission = new SASPermission?(sasIdentifier.AccessPolicy.SignedPermission.Value);
			}
		}

		protected void ValidateMandatoryField(string field, string fieldName)
		{
			if (string.IsNullOrWhiteSpace(field))
			{
				throw new AuthenticationFailureException(string.Concat(fieldName, " is mandatory. Cannot be empty"));
			}
		}

		protected void ValidateOptionalField(string field, string fieldName)
		{
			if (field != null && string.IsNullOrWhiteSpace(field))
			{
				throw new AuthenticationFailureException(string.Concat(fieldName, " is optional but cannot be empty"));
			}
		}

		protected void ValidateSignatureFormat(string signature)
		{
			NephosAssertionException.Assert(signature != null);
			if ((int)Convert.FromBase64String(signature).Length != SignedAccessHelper.Sha256HashSize / 8)
			{
				throw new AuthenticationFailureException("Signature size is invalid");
			}
		}

		protected void ValidateSignedAuthorizationFormat(string signedAuthorization)
		{
			this.ValidateSignatureFormat(signedAuthorization);
		}
	}
}