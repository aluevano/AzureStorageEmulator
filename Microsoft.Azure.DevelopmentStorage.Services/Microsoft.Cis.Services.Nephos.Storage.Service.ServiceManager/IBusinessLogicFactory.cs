using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Storage;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IBusinessLogicFactory : ICommonBusinessLogicFactory
	{
		Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager CreateServiceManager(AuthorizationManager authorizationManager, IStorageManager storageManager, ServiceManagerConfiguration config);
	}
}