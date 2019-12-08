using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public enum UriCanonicalizeAsFilePathSettingType
	{
		NoOp,
		ClearUriCanonicalizeAsFilePathOnDotNet40,
		ClearUriCanonicalizeAsFilePath,
		SetUriCanonicalizeAsFilePath
	}
}