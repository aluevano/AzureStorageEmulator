using System;
using System.Data.Services.Providers;

namespace Microsoft.UtilityComputing
{
	public class UtilityTableResourceProperty : ResourceProperty
	{
		public UtilityTableResourceProperty(string name, ResourcePropertyKind kind, System.Data.Services.Providers.ResourceType propertyResourceType) : this(name, kind, propertyResourceType, null)
		{
		}

		public UtilityTableResourceProperty(string name, ResourcePropertyKind kind, System.Data.Services.Providers.ResourceType propertyResourceType, ResourceSet targetContainer) : base(name, kind, propertyResourceType)
		{
		}
	}
}