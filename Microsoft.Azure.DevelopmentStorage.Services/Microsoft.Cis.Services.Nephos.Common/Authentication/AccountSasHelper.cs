using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public class AccountSasHelper : SignedAccessHelper
	{
		public override List<SASAccessRestriction> AccessRestrictions
		{
			get
			{
				return null;
			}
		}

		public SasResourceType SignedResourceType
		{
			get;
			private set;
		}

		public SasService SignedService
		{
			get;
			private set;
		}

		public AccountSasHelper(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, NephosUriComponents uriComponents) : base(requestContext, uriComponents)
		{
			this.SignedService = SasService.None;
			this.SignedResourceType = SasResourceType.None;
		}

		public static byte[] ComputeUrlDecodedUtf8EncodedStringToSign(NameValueCollection queryParams, NephosUriComponents uriComponents)
		{
			string item = queryParams["st"];
			string str = queryParams["se"];
			string item1 = queryParams["sp"];
			string str1 = queryParams["srt"];
			string item2 = queryParams["ss"];
			string str2 = queryParams["sip"];
			string item3 = queryParams["spr"];
			string str3 = queryParams["sv"];
			string item4 = queryParams["sep"];
			StringBuilder stringBuilder = new StringBuilder();
			string accountName = uriComponents.AccountName;
			stringBuilder.Append(accountName ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(item1 ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(item2 ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(str1 ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(item ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(str ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(str2 ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(item3 ?? string.Empty);
			stringBuilder.Append("\n");
			stringBuilder.Append(str3 ?? string.Empty);
			stringBuilder.Append("\n");
			if (!string.IsNullOrWhiteSpace(item4))
			{
				stringBuilder.Append(item4);
				stringBuilder.Append("\n");
			}
			return (new UTF8Encoding()).GetBytes(stringBuilder.ToString());
		}

		public override byte[] ComputeUrlDecodedUtf8EncodedStringToSign()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string accountName = base.UriComponents.AccountName;
			if (!string.IsNullOrEmpty(accountName))
			{
				stringBuilder.Append(accountName);
			}
			stringBuilder.Append("\n");
			stringBuilder.Append(base.QueryParams["sp"]);
			stringBuilder.Append("\n");
			stringBuilder.Append(base.QueryParams["ss"]);
			stringBuilder.Append("\n");
			stringBuilder.Append(base.QueryParams["srt"]);
			stringBuilder.Append("\n");
			if (base.SignedStart.HasValue)
			{
				stringBuilder.Append(base.QueryParams["st"]);
			}
			stringBuilder.Append("\n");
			stringBuilder.Append(base.QueryParams["se"]);
			stringBuilder.Append("\n");
			if (base.SignedIP != null)
			{
				stringBuilder.Append(base.QueryParams["sip"]);
			}
			stringBuilder.Append("\n");
			string item = base.QueryParams["spr"];
			if (item != null)
			{
				stringBuilder.Append(item);
			}
			stringBuilder.Append("\n");
			stringBuilder.Append(base.QueryParams["sv"]);
			stringBuilder.Append("\n");
			if (base.SignedExtraPermission.HasValue)
			{
				stringBuilder.Append(base.QueryParams["sep"]);
				stringBuilder.Append("\n");
			}
			return (new UTF8Encoding()).GetBytes(stringBuilder.ToString());
		}

		public override AccountIdentifier CreateAccountIdentifier(IStorageAccount account)
		{
			return new AccountSasAccessIdentifier(account, base.KeyUsedForSigning.Permissions);
		}

		public override void ParseAccessPolicyFields(bool isDoubleSigned)
		{
			try
			{
				this.ValidateAndSetSASVersionToUse(base.QueryParams["sv"]);
				base.ParseAccessPolicyFields(false);
				string str = AuthenticationManagerHelper.ExtractKeyNameFromParamsWithConversion(base.QueryParams);
				base.ValidateOptionalField(str, "sk");
				if (str != null)
				{
					base.KeyName = str.Trim();
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Using secret key with KeyName '{0}' to authenticate SAS request.", new object[] { base.KeyName });
				}
				string item = base.QueryParams["ss"];
				base.ValidateMandatoryField(item, "ss");
				this.SignedService = AccountSasHelper.ParseSasService(item);
				string item1 = base.QueryParams["srt"];
				base.ValidateMandatoryField(item1, "srt");
				this.SignedResourceType = AccountSasHelper.ParseSasResourceType(item1);
				string str1 = base.QueryParams["sig"];
				base.ValidateMandatoryField(str1, "sig");
				base.ValidateSignatureFormat(str1);
				base.Signature = str1;
				string item2 = base.QueryParams["sp"];
				base.ValidateMandatoryField(item2, "sp");
				base.SignedPermission = new SASPermission?(AccountSasHelper.ParsePermissionsForAccountSas(item2));
			}
			catch (FormatException formatException)
			{
				throw new AuthenticationFailureException("Signature fields not well formed.", formatException);
			}
		}

		public static SASPermission ParsePermissionsForAccountSas(string permission)
		{
			if (string.IsNullOrEmpty(permission))
			{
				throw new ArgumentException(permission);
			}
			SASPermission sASPermission = SASPermission.None;
			int num = 0;
			while (true)
			{
				if (num >= permission.Length)
				{
					return sASPermission;
				}
				char chr = permission[num];
				if (chr > 'l')
				{
					switch (chr)
					{
						case 'p':
						{
							sASPermission = AccountSasHelper.ValidateAndAddPermission(sASPermission, SASPermission.Process, num);
							break;
						}
						case 'q':
						{
							throw new FormatException(string.Concat("Unexpected character ", permission[num], " in permission"));
						}
						case 'r':
						{
							sASPermission = AccountSasHelper.ValidateAndAddPermission(sASPermission, SASPermission.Read, num);
							break;
						}
						default:
						{
							switch (chr)
							{
								case 'u':
								{
									sASPermission = AccountSasHelper.ValidateAndAddPermission(sASPermission, SASPermission.Update, num);
									break;
								}
								case 'v':
								{
									throw new FormatException(string.Concat("Unexpected character ", permission[num], " in permission"));
								}
								case 'w':
								{
									sASPermission = AccountSasHelper.ValidateAndAddPermission(sASPermission, SASPermission.Write, num);
									break;
								}
								default:
								{
									throw new FormatException(string.Concat("Unexpected character ", permission[num], " in permission"));
								}
							}
							break;
						}
					}
				}
				else
				{
					switch (chr)
					{
						case 'a':
						{
							sASPermission = AccountSasHelper.ValidateAndAddPermission(sASPermission, SASPermission.Add, num);
							break;
						}
						case 'b':
						{
							throw new FormatException(string.Concat("Unexpected character ", permission[num], " in permission"));
						}
						case 'c':
						{
							sASPermission = AccountSasHelper.ValidateAndAddPermission(sASPermission, SASPermission.Create, num);
							break;
						}
						case 'd':
						{
							sASPermission = AccountSasHelper.ValidateAndAddPermission(sASPermission, SASPermission.Delete, num);
							break;
						}
						default:
						{
							if (chr == 'l')
							{
								sASPermission = AccountSasHelper.ValidateAndAddPermission(sASPermission, SASPermission.List, num);
								break;
							}
							else
							{
								throw new FormatException(string.Concat("Unexpected character ", permission[num], " in permission"));
							}
						}
					}
				}
				num++;
			}
			throw new FormatException(string.Concat("Unexpected character ", permission[num], " in permission"));
		}

		public static SasResourceType ParseSasResourceType(string signedResourceType)
		{
			if (string.IsNullOrWhiteSpace(signedResourceType))
			{
				throw new ArgumentException("signedResourceType");
			}
			SasResourceType sasResourceType = SasResourceType.None;
			for (int i = 0; i < signedResourceType.Length; i++)
			{
				char chr = signedResourceType[i];
				if (chr == 'c')
				{
					sasResourceType = AccountSasHelper.ValidateAndAddResourceType(sasResourceType, SasResourceType.Container, i);
				}
				else if (chr == 'o')
				{
					sasResourceType = AccountSasHelper.ValidateAndAddResourceType(sasResourceType, SasResourceType.Object, i);
				}
				else
				{
					if (chr != 's')
					{
						throw new FormatException(string.Concat("Unexpected character ", signedResourceType[i], " in signed resource type"));
					}
					sasResourceType = AccountSasHelper.ValidateAndAddResourceType(sasResourceType, SasResourceType.Service, i);
				}
			}
			return sasResourceType;
		}

		public static SasService ParseSasService(string service)
		{
			if (string.IsNullOrEmpty(service))
			{
				throw new ArgumentException(service);
			}
			SasService sasService = SasService.None;
			int num = 0;
			while (true)
			{
				if (num >= service.Length)
				{
					return sasService;
				}
				char chr = service[num];
				if (chr <= 'f')
				{
					if (chr == 'b')
					{
						sasService = AccountSasHelper.ValidateAndAddService(sasService, SasService.Blob, num);
					}
					else if (chr == 'f')
					{
						sasService = AccountSasHelper.ValidateAndAddService(sasService, SasService.File, num);
					}
					else
					{
						break;
					}
				}
				else if (chr == 'q')
				{
					sasService = AccountSasHelper.ValidateAndAddService(sasService, SasService.Queue, num);
				}
				else if (chr == 't')
				{
					sasService = AccountSasHelper.ValidateAndAddService(sasService, SasService.Table, num);
				}
				else
				{
					break;
				}
				num++;
			}
			throw new FormatException(string.Concat("Unexpected character ", service[num], " in service"));
		}

		private static SASPermission ValidateAndAddPermission(SASPermission currPermissions, SASPermission newPermission, int position)
		{
			if ((currPermissions & newPermission) == newPermission)
			{
				object[] objArray = new object[] { "Invalid duplicate permission. Error at index ", position, " of ", currPermissions };
				throw new FormatException(string.Concat(objArray));
			}
			currPermissions |= newPermission;
			return currPermissions;
		}

		private static SasResourceType ValidateAndAddResourceType(SasResourceType currResourceTypes, SasResourceType newResourceType, int position)
		{
			if ((currResourceTypes & newResourceType) == newResourceType)
			{
				object[] objArray = new object[] { "Invalid duplicate service. Error at index ", position, " of ", currResourceTypes };
				throw new FormatException(string.Concat(objArray));
			}
			currResourceTypes |= newResourceType;
			return currResourceTypes;
		}

		private static SasService ValidateAndAddService(SasService currServices, SasService newService, int position)
		{
			if ((currServices & newService) == newService)
			{
				object[] objArray = new object[] { "Invalid duplicate service. Error at index ", position, " of ", currServices };
				throw new FormatException(string.Concat(objArray));
			}
			currServices |= newService;
			return currServices;
		}

		private void ValidateAndSetSASVersionToUse(string sasVersion)
		{
			if (string.IsNullOrWhiteSpace(sasVersion))
			{
				throw new AuthenticationFailureException("sv is mandatory. Cannot be empty");
			}
			if (VersioningHelper.IsPreApril15OrInvalidVersion(sasVersion))
			{
				throw new AuthenticationFailureException(string.Format("{0} is either not in the correct format or is not equal or later than version {1}", "sv", "2015-04-05"));
			}
			base.SignedVersion = sasVersion;
		}
	}
}