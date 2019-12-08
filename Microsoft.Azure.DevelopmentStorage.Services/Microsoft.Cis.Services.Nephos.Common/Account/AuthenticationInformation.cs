using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common.Account
{
	public class AuthenticationInformation : IAuthenticationInformation
	{
		private string authScheme;

		private AuthDataEntry namedKeyAuthData;

		private Collection<AuthDataEntry> authData;

		private Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext;

		private NephosUriComponents uriComponents;

		private string authKeyName;

		public IEnumerable<AuthDataEntry> AuthData
		{
			get
			{
				foreach (AuthDataEntry authDatum in this.authData)
				{
					yield return authDatum;
				}
			}
		}

		public Collection<AuthDataEntry> AuthDataConcrete
		{
			get
			{
				return this.authData;
			}
			set
			{
				this.authData = value;
			}
		}

		public string AuthKeyName
		{
			get
			{
				return JustDecompileGenerated_get_AuthKeyName();
			}
			set
			{
				JustDecompileGenerated_set_AuthKeyName(value);
			}
		}

		public string JustDecompileGenerated_get_AuthKeyName()
		{
			return this.authKeyName;
		}

		public void JustDecompileGenerated_set_AuthKeyName(string value)
		{
			this.authKeyName = value;
		}

		public string AuthScheme
		{
			get
			{
				return JustDecompileGenerated_get_AuthScheme();
			}
			set
			{
				JustDecompileGenerated_set_AuthScheme(value);
			}
		}

		public string JustDecompileGenerated_get_AuthScheme()
		{
			return this.authScheme;
		}

		public void JustDecompileGenerated_set_AuthScheme(string value)
		{
			this.authScheme = value;
		}

		public AuthDataEntry NamedKeyAuthData
		{
			get
			{
				if (this.namedKeyAuthData == null && !string.IsNullOrEmpty(this.AuthKeyName))
				{
					foreach (AuthDataEntry authDatum in this.AuthData)
					{
						if (!this.authKeyName.Equals(authDatum.KeyName))
						{
							continue;
						}
						this.namedKeyAuthData = authDatum;
						break;
					}
				}
				return this.namedKeyAuthData;
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.RequestContext RequestContext
		{
			get
			{
				return JustDecompileGenerated_get_RequestContext();
			}
			set
			{
				JustDecompileGenerated_set_RequestContext(value);
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.RequestContext JustDecompileGenerated_get_RequestContext()
		{
			return this.requestContext;
		}

		public void JustDecompileGenerated_set_RequestContext(Microsoft.Cis.Services.Nephos.Common.RequestContext value)
		{
			this.RequestContext = value;
		}

		public NephosUriComponents UriComponents
		{
			get
			{
				return this.uriComponents;
			}
		}

		public AuthenticationInformation(string authScheme, SupportedAuthScheme originalRequestAuthScheme, Collection<AuthDataEntry> authData, Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, NephosUriComponents uriComponents, bool isInterStampAuthentication)
		{
			this.authScheme = authScheme;
			this.authData = authData;
			this.requestContext = requestContext;
			this.uriComponents = uriComponents;
			this.authKeyName = null;
			if (originalRequestAuthScheme == SupportedAuthScheme.SignedKey && requestContext.QueryParameters["sv"] != null)
			{
				this.authKeyName = AuthenticationManagerHelper.ExtractKeyNameFromParamsWithConversion(requestContext.QueryParameters);
				if (this.authKeyName != null)
				{
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Using secret key with KeyName '{0}' to authenticate SAS/DSAS.", new object[] { this.authKeyName });
				}
			}
		}

		public AuthenticationInformation(string authScheme, Collection<AuthDataEntry> authData)
		{
			this.authScheme = authScheme;
			this.authData = authData;
		}

		public AuthenticationInformation()
		{
		}
	}
}