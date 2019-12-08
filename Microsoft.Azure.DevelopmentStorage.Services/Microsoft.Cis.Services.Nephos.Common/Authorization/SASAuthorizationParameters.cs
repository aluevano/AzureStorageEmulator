using Microsoft.Cis.Services.Nephos.Common.Authentication;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	public class SASAuthorizationParameters
	{
		public SASPermission SignedPermission
		{
			get;
			set;
		}

		public SasResourceType SignedResourceType
		{
			get;
			set;
		}

		public SasType SupportedSasTypes
		{
			get;
			set;
		}

		public SASAuthorizationParameters()
		{
		}
	}
}