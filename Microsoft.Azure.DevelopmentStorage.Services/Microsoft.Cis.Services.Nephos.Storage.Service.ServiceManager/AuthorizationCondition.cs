using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class AuthorizationCondition
	{
		public bool MustNotExist
		{
			get;
			set;
		}

		public AuthorizationCondition()
		{
		}
	}
}