using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Service
{
	public class BaseServiceManager
	{
		protected AuthorizationManager authorizationManager;

		protected IStorageManager storageManager;

		protected BaseServiceManager()
		{
		}
	}
}