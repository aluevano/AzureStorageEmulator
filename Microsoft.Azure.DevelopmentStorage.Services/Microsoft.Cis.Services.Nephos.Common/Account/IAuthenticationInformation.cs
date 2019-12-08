using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Account
{
	public interface IAuthenticationInformation
	{
		IEnumerable<AuthDataEntry> AuthData
		{
			get;
		}

		string AuthKeyName
		{
			get;
		}

		string AuthScheme
		{
			get;
		}

		AuthDataEntry NamedKeyAuthData
		{
			get;
		}

		Microsoft.Cis.Services.Nephos.Common.RequestContext RequestContext
		{
			get;
		}

		NephosUriComponents UriComponents
		{
			get;
		}
	}
}