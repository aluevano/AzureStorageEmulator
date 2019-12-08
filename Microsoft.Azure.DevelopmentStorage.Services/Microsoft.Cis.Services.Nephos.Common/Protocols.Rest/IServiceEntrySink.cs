using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public interface IServiceEntrySink
	{
		string GetConfigurationParameter(string name);

		bool IsTenantDevFabric();

		void RegisterRestHandler(ServiceRequestHandler handler);
	}
}