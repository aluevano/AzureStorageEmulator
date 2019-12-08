using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Versioning;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	internal class TableSignedAccessHelper : SignedAccessHelper
	{
		private List<SASAccessRestriction> accessRestrictions;

		public override List<SASAccessRestriction> AccessRestrictions
		{
			get
			{
				if (this.accessRestrictions == null)
				{
					this.accessRestrictions = new List<SASAccessRestriction>();
				}
				return this.accessRestrictions;
			}
		}

		internal string EndingPartitionKey
		{
			get;
			set;
		}

		internal string EndingRowKey
		{
			get;
			set;
		}

		internal string StartingPartitionKey
		{
			get;
			set;
		}

		internal string StartingRowKey
		{
			get;
			set;
		}

		internal string TableName
		{
			get;
			set;
		}

		public TableSignedAccessHelper(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, NephosUriComponents uriComponents) : base(requestContext, uriComponents)
		{
		}

		public override byte[] ComputeUrlDecodedUtf8EncodedStringToSign()
		{
			string item;
			string str = base.QueryParams["st"];
			string item1 = base.QueryParams["se"];
			string str1 = base.QueryParams["sp"];
			string signedIdentifier = base.SignedIdentifier ?? string.Empty;
			string item2 = base.QueryParams["sip"];
			string str2 = base.QueryParams["spr"];
			string item3 = base.QueryParams["sv"];
			string tableName = this.TableName ?? string.Empty;
			string startingPartitionKey = this.StartingPartitionKey ?? string.Empty;
			string startingRowKey = this.StartingRowKey ?? string.Empty;
			string endingPartitionKey = this.EndingPartitionKey ?? string.Empty;
			string endingRowKey = this.EndingRowKey ?? string.Empty;
			if (base.SignedExtraPermission.HasValue)
			{
				item = base.QueryParams["sep"];
			}
			else
			{
				item = null;
			}
			return TableSignedAccessHelper.ComputeUrlDecodedUtf8EncodedStringToSign(str, item1, str1, signedIdentifier, item2, str2, item3, tableName, startingPartitionKey, startingRowKey, endingPartitionKey, endingRowKey, item, base.UriComponents);
		}

		public static byte[] ComputeUrlDecodedUtf8EncodedStringToSign(string st, string se, string sp, string si, string sip, string spr, string sv, string tn, string spk, string srk, string epk, string erk, string sep, NephosUriComponents uriComponents)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(sp ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(st ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(se ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(TableSignedAccessHelper.GetCanonicalizedResource(uriComponents, tn, sv));
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
			stringBuilder.Append("\n");
			stringBuilder.Append(spk ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(srk ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(epk ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(erk ?? string.Empty);
			if (sep != null)
			{
				stringBuilder.Append("\n");
				stringBuilder.Append(sep);
			}
			return (new UTF8Encoding()).GetBytes(stringBuilder.ToString());
		}

		public override AccountIdentifier CreateAccountIdentifier(IStorageAccount account)
		{
			return new TableSignedAccessAccountIdentifier(account, this.TableName, this.StartingPartitionKey, this.StartingRowKey, this.EndingPartitionKey, this.EndingRowKey, base.KeyUsedForSigning.Permissions);
		}

		private static string GetCanonicalizedResource(NephosUriComponents uriComponents, string tableName, string signedVersion)
		{
			NephosAssertionException.Assert(!string.IsNullOrEmpty(uriComponents.AccountName));
			NephosAssertionException.Assert(!string.IsNullOrEmpty(tableName));
			StringBuilder stringBuilder = new StringBuilder();
			if (signedVersion != null && VersioningHelper.CompareVersions(signedVersion, "2015-02-21") >= 0)
			{
				stringBuilder.Append("/table");
			}
			stringBuilder.Append("/");
			stringBuilder.Append(uriComponents.AccountName);
			stringBuilder.Append("/");
			stringBuilder.Append(tableName.ToLower());
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
				string str1 = base.QueryParams["tn"];
				base.ValidateMandatoryField(str1, "tn");
				this.TableName = str1.ToLower();
				string item2 = base.QueryParams["spk"];
				this.StartingPartitionKey = item2;
				string str2 = base.QueryParams["srk"];
				if (item2 == null && str2 != null)
				{
					throw new AuthenticationFailureException(string.Format("{0} is required when {1} is provided.", "srk", "spk"));
				}
				this.StartingRowKey = str2;
				string item3 = base.QueryParams["epk"];
				this.EndingPartitionKey = item3;
				string str3 = base.QueryParams["erk"];
				if (item3 == null && str3 != null)
				{
					throw new AuthenticationFailureException(string.Format("{0} is required when {1} is provided.", "erk", "epk"));
				}
				this.EndingRowKey = str3;
				string item4 = base.QueryParams["sv"];
				base.ValidateMandatoryField(item4, "sv");
				this.ValidateSASVersion(item4);
				base.ParseAccessPolicyFields(isDoubleSigned);
				string str4 = base.QueryParams["sp"];
				if (!base.IsRevocableAccess)
				{
					base.ValidateMandatoryField(str4, "sp");
					SASUtilities.ValidatePermissionOrdering(str4, SASPermission.Table);
					base.SignedPermission = new SASPermission?(SASUtilities.ParseSASPermission(str4));
				}
				else
				{
					base.ValidateOptionalField(str4, "sp");
					if (str4 != null)
					{
						SASUtilities.ValidatePermissionOrdering(str4, SASPermission.Table);
						base.SignedPermission = new SASPermission?(SASUtilities.ParseSASPermission(str4));
					}
				}
				string str5 = AuthenticationManagerHelper.ExtractKeyNameFromParamsWithConversion(base.QueryParams);
				base.ValidateOptionalField(str5, "sk");
				if (str5 != null)
				{
					base.KeyName = str5.Trim();
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
				if (this.TableName == null || base.UriComponents.AccountName == null)
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