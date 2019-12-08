using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Versioning;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	internal class QueueSignedAccessHelper : SignedAccessHelper
	{
		private List<SASAccessRestriction> accessRestrictions;

		public override List<SASAccessRestriction> AccessRestrictions
		{
			get
			{
				if (this.accessRestrictions == null)
				{
					NephosUriComponents nephosUriComponent = new NephosUriComponents()
					{
						AccountName = base.UriComponents.AccountName,
						ContainerName = base.UriComponents.ContainerName,
						RemainingPart = null
					};
					this.accessRestrictions = new List<SASAccessRestriction>()
					{
						new SASAccessRestriction(SASAccessLevel.None, nephosUriComponent)
					};
				}
				return this.accessRestrictions;
			}
		}

		public QueueSignedAccessHelper(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, NephosUriComponents uriComponents, bool isDoubleSigned) : base(requestContext, uriComponents)
		{
		}

		public override byte[] ComputeUrlDecodedUtf8EncodedStringToSign()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (base.SignedPermission.HasValue)
			{
				stringBuilder.Append(base.QueryParams["sp"]);
			}
			stringBuilder.Append("\n");
			if (base.SignedStart.HasValue)
			{
				stringBuilder.Append(base.QueryParams["st"]);
			}
			stringBuilder.Append("\n");
			if (base.SignedExpiry.HasValue)
			{
				stringBuilder.Append(base.QueryParams["se"]);
			}
			stringBuilder.Append("\n");
			stringBuilder.Append(QueueSignedAccessHelper.GetCanonicalizedResource(base.UriComponents, base.SignedVersion));
			stringBuilder.Append("\n");
			if (base.SignedIdentifier != null)
			{
				stringBuilder.Append(base.QueryParams["si"]);
			}
			stringBuilder.Append("\n");
			if (!VersioningHelper.IsPreApril15OrInvalidVersion(base.SignedVersion))
			{
				string item = base.QueryParams["sip"];
				if (item != null)
				{
					stringBuilder.Append(item);
				}
				stringBuilder.Append("\n");
				string str = base.QueryParams["spr"];
				if (str != null)
				{
					stringBuilder.Append(str);
				}
				stringBuilder.Append("\n");
			}
			stringBuilder.Append(base.QueryParams["sv"]);
			if (base.SignedExtraPermission.HasValue)
			{
				stringBuilder.Append("\n");
				stringBuilder.Append(base.QueryParams["sep"]);
			}
			return (new UTF8Encoding()).GetBytes(stringBuilder.ToString());
		}

		public static byte[] ComputeUrlDecodedUtf8EncodedStringToSign(string st, string se, string sp, string si, string sip, string spr, string sv, string sep, NephosUriComponents uriComponents)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(sp ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(st ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(se ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(QueueSignedAccessHelper.GetCanonicalizedResource(uriComponents, sv));
			stringBuilder.Append("\n");
			stringBuilder.Append(si ?? string.Empty);
			stringBuilder.Append("\n");
			if (!VersioningHelper.IsPreApril15OrInvalidVersion(sv))
			{
				stringBuilder.Append(sip ?? string.Empty);
				stringBuilder.Append("\n");
				stringBuilder.Append(spr ?? string.Empty);
				stringBuilder.Append("\n");
			}
			stringBuilder.Append(sv ?? string.Empty);
			if (sep != null)
			{
				stringBuilder.Append("\n");
				stringBuilder.Append(sep);
			}
			return (new UTF8Encoding()).GetBytes(stringBuilder.ToString());
		}

		public override AccountIdentifier CreateAccountIdentifier(IStorageAccount account)
		{
			return new SignedAccessAccountIdentifier(account, base.KeyUsedForSigning.Permissions);
		}

		private static string GetCanonicalizedResource(NephosUriComponents uriComponents, string signedVersion)
		{
			NephosAssertionException.Assert(!string.IsNullOrEmpty(uriComponents.AccountName));
			NephosAssertionException.Assert(!string.IsNullOrEmpty(uriComponents.ContainerName));
			StringBuilder stringBuilder = new StringBuilder();
			if (signedVersion != null && VersioningHelper.CompareVersions(signedVersion, "2015-02-21") >= 0)
			{
				stringBuilder.Append("/queue");
			}
			stringBuilder.Append("/");
			stringBuilder.Append(uriComponents.AccountName);
			stringBuilder.Append("/");
			stringBuilder.Append(uriComponents.ContainerName);
			return stringBuilder.ToString();
		}

		public override void ParseAccessPolicyFields(bool isDoubleSigned)
		{
			try
			{
				if (isDoubleSigned)
				{
					string str = base.ExtractSignedAuthorization(base.RequestContext);
					base.ValidateMandatoryField(str, "SignedKey");
					base.ValidateSignedAuthorizationFormat(str);
				}
				else
				{
					string item = base.QueryParams["sig"];
					base.ValidateMandatoryField(item, "sig");
					base.ValidateSignatureFormat(item);
					base.Signature = item;
				}
				string item1 = base.QueryParams["si"];
				base.ValidateOptionalField(item1, "si");
				base.SignedIdentifier = item1;
				string str1 = base.QueryParams["sv"];
				base.ValidateMandatoryField(str1, "sv");
				this.ValidateSASVersion(str1);
				base.ParseAccessPolicyFields(isDoubleSigned);
				string item2 = base.QueryParams["sp"];
				if (!base.IsRevocableAccess)
				{
					base.ValidateMandatoryField(item2, "sp");
					SASUtilities.ValidatePermissionOrdering(item2, SASPermission.Queue);
					base.SignedPermission = new SASPermission?(SASUtilities.ParseSASPermission(item2));
				}
				else
				{
					base.ValidateOptionalField(item2, "sp");
					if (item2 != null)
					{
						SASUtilities.ValidatePermissionOrdering(item2, SASPermission.Queue);
						base.SignedPermission = new SASPermission?(SASUtilities.ParseSASPermission(item2));
					}
				}
				string str2 = AuthenticationManagerHelper.ExtractKeyNameFromParamsWithConversion(base.QueryParams);
				base.ValidateOptionalField(str2, "sk");
				if (str2 != null)
				{
					base.KeyName = str2.Trim();
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Using secret key with KeyName '{0}' to authenticate SAS request.", new object[] { base.KeyName });
				}
			}
			catch (FormatException formatException)
			{
				throw new AuthenticationFailureException("Signature fields not well formed.", formatException);
			}
		}

		public override void PerformSignedAccessAuthenticationFirstPhaseValidations()
		{
			try
			{
				base.PerformSignedAccessAuthenticationFirstPhaseValidations();
				if (base.UriComponents.ContainerName == null || base.UriComponents.AccountName == null)
				{
					throw new AuthenticationFailureException("The specified signed resource is not allowed for the this resource level");
				}
			}
			catch (FormatException formatException)
			{
				throw new AuthenticationFailureException("Signature fields not well formed.", formatException);
			}
		}

		private void ValidateSASVersion(string sasVersion)
		{
			sasVersion = sasVersion.Trim();
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(sasVersion) || VersioningHelper.CompareVersions(sasVersion, "2012-02-12") < 0)
			{
				FutureVersionProtocolException.ThrowIfFutureVersion(sasVersion);
				throw new AuthenticationFailureException(string.Format("{0} is either not in the correct format or is not equal or later than version {1}", "sv", "2012-02-12"));
			}
			base.SignedVersion = sasVersion;
		}
	}
}