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
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public class BlobSignedAccessHelper : SignedAccessHelper
	{
		private List<SASAccessRestriction> accessRestrictions;

		public readonly static TimeSpan MaxAllowedNonRevocableAccessTimeWindowForPreFeb2012Version;

		public override List<SASAccessRestriction> AccessRestrictions
		{
			get
			{
				string remainingPart;
				if (this.accessRestrictions == null)
				{
					NephosAssertionException.Assert(this.SignedResource != SASAccessLevel.None);
					NephosUriComponents nephosUriComponent = new NephosUriComponents()
					{
						AccountName = base.UriComponents.AccountName,
						ContainerName = base.UriComponents.ContainerName
					};
					NephosUriComponents nephosUriComponent1 = nephosUriComponent;
					if (this.SignedResource == SASAccessLevel.Blob)
					{
						remainingPart = base.UriComponents.RemainingPart;
					}
					else
					{
						remainingPart = null;
					}
					nephosUriComponent1.RemainingPart = remainingPart;
					this.accessRestrictions = new List<SASAccessRestriction>()
					{
						new SASAccessRestriction(this.SignedResource, nephosUriComponent)
					};
				}
				return this.accessRestrictions;
			}
		}

		private bool IsAtLeastFeb2012SasVersion
		{
			get;
			set;
		}

		private SASAccessLevel SignedResource
		{
			get;
			set;
		}

		static BlobSignedAccessHelper()
		{
			BlobSignedAccessHelper.MaxAllowedNonRevocableAccessTimeWindowForPreFeb2012Version = TimeSpan.FromHours(1);
		}

		public BlobSignedAccessHelper(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, NephosUriComponents uriComponents) : base(requestContext, uriComponents)
		{
			this.SignedResource = SASAccessLevel.None;
		}

		private static string ComputeHMACSHA256(byte[] key, byte[] stringToSignBytes)
		{
			string base64String;
			if (key == null || stringToSignBytes == null)
			{
				return null;
			}
			using (HashAlgorithm hMACSHA256 = new HMACSHA256(key))
			{
				base64String = Convert.ToBase64String(hMACSHA256.ComputeHash(stringToSignBytes));
			}
			return base64String;
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
			stringBuilder.Append(BlobSignedAccessHelper.GetCanonicalizedResource(base.UriComponents, this.SignedResource, base.SignedVersion));
			stringBuilder.Append("\n");
			if (base.SignedIdentifier != null)
			{
				stringBuilder.Append(base.QueryParams["si"]);
			}
			if (base.SignedVersion != null)
			{
				if (VersioningHelper.CompareVersions(base.SignedVersion, "2015-04-05") >= 0)
				{
					stringBuilder.Append("\n");
					if (base.QueryParams["sip"] != null)
					{
						stringBuilder.Append(base.QueryParams["sip"]);
					}
					stringBuilder.Append("\n");
					string item = base.QueryParams["spr"];
					if (item != null)
					{
						stringBuilder.Append(item);
					}
				}
				stringBuilder.Append("\n");
				stringBuilder.Append(base.QueryParams["sv"]);
				if (VersioningHelper.CompareVersions(base.SignedVersion, "2013-08-15") >= 0)
				{
					stringBuilder.Append("\n");
					if (base.QueryParams["rscc"] != null)
					{
						stringBuilder.Append(base.QueryParams["rscc"]);
					}
					stringBuilder.Append("\n");
					if (base.QueryParams["rscd"] != null)
					{
						stringBuilder.Append(base.QueryParams["rscd"]);
					}
					stringBuilder.Append("\n");
					if (base.QueryParams["rsce"] != null)
					{
						stringBuilder.Append(base.QueryParams["rsce"]);
					}
					stringBuilder.Append("\n");
					if (base.QueryParams["rscl"] != null)
					{
						stringBuilder.Append(base.QueryParams["rscl"]);
					}
					stringBuilder.Append("\n");
					if (base.QueryParams["rsct"] != null)
					{
						stringBuilder.Append(base.QueryParams["rsct"]);
					}
				}
			}
			if (base.SignedExtraPermission.HasValue)
			{
				stringBuilder.Append("\n");
				stringBuilder.Append(base.QueryParams["sep"]);
			}
			return (new UTF8Encoding()).GetBytes(stringBuilder.ToString());
		}

		public static byte[] ComputeUrlDecodedUtf8EncodedStringToSign(NameValueCollection queryParams, NephosUriComponents uriComponents)
		{
			string item = queryParams["st"];
			string str = queryParams["se"];
			string item1 = queryParams["sp"];
			string str1 = queryParams["sr"];
			string item2 = queryParams["si"];
			string str2 = queryParams["sip"];
			string item3 = queryParams["spr"];
			string str3 = queryParams["sv"];
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(item1 ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(item ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(str ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(BlobSignedAccessHelper.GetCanonicalizedResource(uriComponents, SASUtilities.ParseSasAccessLevel(str1), str3));
			stringBuilder.Append("\n");
			stringBuilder.Append(item2 ?? string.Empty);
			if (str3 != null)
			{
				if (VersioningHelper.CompareVersions(str3, "2015-04-05") >= 0)
				{
					stringBuilder.Append("\n");
					stringBuilder.Append(str2 ?? string.Empty);
					stringBuilder.Append("\n");
					stringBuilder.Append(item3 ?? string.Empty);
				}
				stringBuilder.Append("\n");
				stringBuilder.Append(queryParams["sv"]);
				if (VersioningHelper.CompareVersions(str3, "2014-02-14") >= 0)
				{
					stringBuilder.Append("\n");
					if (queryParams["rscc"] != null)
					{
						stringBuilder.Append(queryParams["rscc"]);
					}
					stringBuilder.Append("\n");
					if (queryParams["rscd"] != null)
					{
						stringBuilder.Append(queryParams["rscd"]);
					}
					stringBuilder.Append("\n");
					if (queryParams["rsce"] != null)
					{
						stringBuilder.Append(queryParams["rsce"]);
					}
					stringBuilder.Append("\n");
					if (queryParams["rscl"] != null)
					{
						stringBuilder.Append(queryParams["rscl"]);
					}
					stringBuilder.Append("\n");
					if (queryParams["rsct"] != null)
					{
						stringBuilder.Append(queryParams["rsct"]);
					}
				}
			}
			if (queryParams["sep"] != null)
			{
				stringBuilder.Append("\n");
				stringBuilder.Append(queryParams["sep"]);
			}
			return (new UTF8Encoding()).GetBytes(stringBuilder.ToString());
		}

		public override AccountIdentifier CreateAccountIdentifier(IStorageAccount account)
		{
			return new SignedAccessAccountIdentifier(account, base.KeyUsedForSigning.Permissions);
		}

		public static string GenerateBlobSasUrl(string accountName, string containerName, string blobName, string blobSnapshot, IStorageAccount storageAccount, string requestUrlBase, bool includeWritePermission)
		{
			if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobName) || string.IsNullOrEmpty(requestUrlBase))
			{
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] objArray = new object[] { accountName, containerName, blobName, requestUrlBase };
				error.Log("Source blob account, container, blob name or requestUrl should not be empty. sourceUnversionedAccountName: {0} sourceUnversionedContainerName: {1} sourceBlobName: {2} requestUrlBase: {3}", objArray);
				return string.Empty;
			}
			NameValueCollection nameValueCollection = new NameValueCollection();
			if (!includeWritePermission)
			{
				nameValueCollection.Add("sp", "r");
			}
			else
			{
				nameValueCollection.Add("sp", "rw");
			}
			nameValueCollection.Add("se", SASUtilities.EncodeTime(DateTime.MaxValue));
			nameValueCollection.Add("sv", "2016-02-19");
			nameValueCollection.Add("sr", "b");
			byte[] sign = BlobSignedAccessHelper.ComputeUrlDecodedUtf8EncodedStringToSign(nameValueCollection, new NephosUriComponents(accountName, containerName, HttpUtility.UrlDecode(blobName)));
			string str = (new UTF8Encoding()).GetString(sign);
			str = str.Replace('\n', '.');
			SecretKeyV3 secretKeyV3 = null;
			if (secretKeyV3 == null)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("GenerateBlobSasUrl: could not find a system key for the account");
				return string.Empty;
			}
			string str1 = BlobSignedAccessHelper.ComputeHMACSHA256(secretKeyV3.Value, sign);
			if (string.IsNullOrEmpty(str1))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("GenerateBlobSasUrl: could not get HMACSHA256");
				return string.Empty;
			}
			nameValueCollection.Add("sig", str1);
			if (!string.IsNullOrEmpty(blobSnapshot))
			{
				nameValueCollection.Add("snapshot", blobSnapshot);
			}
			UriBuilder uriBuilder = new UriBuilder(requestUrlBase);
			StringBuilder stringBuilder = new StringBuilder();
			string[] allKeys = nameValueCollection.AllKeys;
			for (int i = 0; i < (int)allKeys.Length; i++)
			{
				string str2 = allKeys[i];
				stringBuilder.Append(HttpUtilities.PathEncode(str2));
				stringBuilder.Append("=");
				stringBuilder.Append(HttpUtilities.PathEncode(nameValueCollection[str2]));
				stringBuilder.Append("&");
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			uriBuilder.Query = stringBuilder.ToString();
			return uriBuilder.ToString();
		}

		private static string GetCanonicalizedResource(NephosUriComponents uriComponents, SASAccessLevel signedResource, string signedVersion)
		{
			NephosAssertionException.Assert(!string.IsNullOrEmpty(uriComponents.AccountName));
			NephosAssertionException.Assert(!string.IsNullOrEmpty(uriComponents.ContainerName));
			StringBuilder stringBuilder = new StringBuilder();
			if (signedVersion != null && VersioningHelper.CompareVersions(signedVersion, "2015-02-21") >= 0)
			{
				stringBuilder.Append("/blob");
			}
			stringBuilder.Append("/");
			stringBuilder.Append(uriComponents.AccountName);
			stringBuilder.Append("/");
			stringBuilder.Append(uriComponents.ContainerName);
			if (signedResource == SASAccessLevel.Blob)
			{
				NephosAssertionException.Assert(!string.IsNullOrEmpty(uriComponents.RemainingPart));
				stringBuilder.Append("/");
				stringBuilder.Append(uriComponents.RemainingPart);
			}
			return stringBuilder.ToString();
		}

		public override void ParseAccessPolicyFields(bool isDoubleSigned)
		{
			try
			{
				string item = base.QueryParams["sr"];
				base.ValidateMandatoryField(item, "sr");
				this.SignedResource = SASUtilities.ParseSasAccessLevel(item);
				if (isDoubleSigned)
				{
					string str = base.ExtractSignedAuthorization(base.RequestContext);
					base.ValidateMandatoryField(str, "SignedKey");
					base.ValidateSignedAuthorizationFormat(str);
				}
				else
				{
					string item1 = base.QueryParams["sig"];
					base.ValidateMandatoryField(item1, "sig");
					base.ValidateSignatureFormat(item1);
					base.Signature = item1;
				}
				string str1 = base.QueryParams["si"];
				base.ValidateOptionalField(str1, "si");
				base.SignedIdentifier = str1;
				this.ValidateAndSetSASVersionToUse(base.QueryParams["sv"]);
				base.ParseAccessPolicyFields(isDoubleSigned);
				string item2 = base.QueryParams["sp"];
				if (!base.IsRevocableAccess)
				{
					base.ValidateMandatoryField(item2, "sp");
				}
				else
				{
					base.ValidateOptionalField(item2, "sp");
				}
				if (item2 != null)
				{
					if (!VersioningHelper.IsPreApril15OrInvalidVersion(base.SignedVersion))
					{
						SASUtilities.ValidatePermissionOrdering(item2, SASPermission.BlobWithAddAndCreate);
					}
					else
					{
						SASUtilities.ValidatePermissionOrdering(item2, SASPermission.Blob);
					}
					base.SignedPermission = new SASPermission?(SASUtilities.ParseSASPermission(item2));
				}
				if (this.IsAtLeastFeb2012SasVersion)
				{
					string str2 = AuthenticationManagerHelper.ExtractKeyNameFromParamsWithConversion(base.QueryParams);
					base.ValidateOptionalField(str2, "sk");
					if (str2 != null)
					{
						base.KeyName = str2.Trim();
						Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Using secret key with KeyName '{0}' to authenticate SAS request.", new object[] { base.KeyName });
					}
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
				if (!base.IsRevocableAccess)
				{
					this.ValidateNonRevocableAccessWindow();
				}
				base.PerformSignedAccessAuthenticationFirstPhaseValidations();
				if (this.SignedResource == SASAccessLevel.Blob && base.UriComponents.RemainingPart == null || this.SignedResource == SASAccessLevel.Container && base.UriComponents.ContainerName == null || base.UriComponents.AccountName == null)
				{
					throw new AuthenticationFailureException("The specified signed resource is not allowed for the this resource level");
				}
			}
			catch (FormatException formatException)
			{
				throw new AuthenticationFailureException("Signature fields not well formed.", formatException);
			}
		}

		private void ValidateAndSetSASVersionToUse(string sasVersion)
		{
			this.IsAtLeastFeb2012SasVersion = false;
			if (sasVersion != null)
			{
				sasVersion = sasVersion.Trim();
				if (!VersioningConfigurationLookup.Instance.IsValidVersion(sasVersion) || VersioningHelper.CompareVersions(sasVersion, "2012-02-12") < 0)
				{
					FutureVersionProtocolException.ThrowIfFutureVersion(sasVersion);
					throw new AuthenticationFailureException(string.Format("{0} is either not in the correct format or is not equal or later than version {1}", "sv", "2012-02-12"));
				}
				this.IsAtLeastFeb2012SasVersion = true;
			}
			base.SignedVersion = sasVersion;
		}

		public void ValidateNonRevocableAccessWindow()
		{
			DateTime dateTime = (base.SignedStart.HasValue ? base.SignedStart.Value : DateTime.UtcNow);
			NephosAssertionException.Assert(true);
			DateTime value = base.SignedExpiry.Value;
			NephosAssertionException.Assert(true);
			if (dateTime >= base.SignedExpiry.Value)
			{
				DateTime value1 = base.SignedExpiry.Value;
				throw new AuthenticationFailureException(string.Format("Signed expiry time [{0}] must be after signed start time [{1}]", value1.ToString("R"), dateTime.ToString("R")));
			}
			if (!this.IsAtLeastFeb2012SasVersion && !((base.SignedExpiry.Value - dateTime) <= BlobSignedAccessHelper.MaxAllowedNonRevocableAccessTimeWindowForPreFeb2012Version))
			{
				string str = dateTime.ToString("R");
				DateTime dateTime1 = base.SignedExpiry.Value;
				throw new AuthenticationFailureException(string.Format("Access without signed identifier cannot have time window more than 1 hour: Start [{0}] - Expiry [{1}]", str, dateTime1.ToString("R")));
			}
		}
	}
}