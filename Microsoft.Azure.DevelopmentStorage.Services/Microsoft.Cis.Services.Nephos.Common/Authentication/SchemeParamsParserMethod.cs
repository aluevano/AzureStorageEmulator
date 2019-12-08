using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public delegate void SchemeParamsParserMethod(SupportedAuthScheme scheme, string parameter, NephosUriComponents uriComponents, out string account, out string signature);
}