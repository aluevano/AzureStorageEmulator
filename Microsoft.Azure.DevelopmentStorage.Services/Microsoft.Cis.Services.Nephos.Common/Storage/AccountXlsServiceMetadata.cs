using Newtonsoft.Json;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class AccountXlsServiceMetadata : ICloneable
	{
		private string originalJson;

		public AccountXlsServiceMetadata()
		{
			this.originalJson = string.Empty;
		}

		public object Clone()
		{
			return AccountXlsServiceMetadata.FromJsonString(this.originalJson);
		}

		public static AccountXlsServiceMetadata FromJsonString(string xlsServiceMetadataString)
		{
			AccountXlsServiceMetadata accountXlsServiceMetadatum;
			if (string.IsNullOrEmpty(xlsServiceMetadataString))
			{
				return null;
			}
			try
			{
				accountXlsServiceMetadatum = JsonConvert.DeserializeObject<AccountXlsServiceMetadata>(xlsServiceMetadataString);
			}
			catch
			{
				accountXlsServiceMetadatum = new AccountXlsServiceMetadata();
			}
			accountXlsServiceMetadatum.originalJson = xlsServiceMetadataString;
			return accountXlsServiceMetadatum;
		}
	}
}