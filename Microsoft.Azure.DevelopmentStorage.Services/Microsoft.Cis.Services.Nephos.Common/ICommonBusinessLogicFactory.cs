using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public interface ICommonBusinessLogicFactory
	{
		AuthenticationManager CreateAuthenticationManager(IStorageManager storageManager);

		AuthorizationManager CreateAuthorizationManager(IStorageManager storageManager);

		IStorageManager CreateStorageManager(TimeSpan accountCacheTtl);
	}
}