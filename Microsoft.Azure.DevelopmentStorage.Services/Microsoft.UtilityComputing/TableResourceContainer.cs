using System;
using System.Data.Services.Providers;
using System.Diagnostics;

namespace Microsoft.UtilityComputing
{
	[DebuggerDisplay("{Name}: {ResourceType}")]
	public class TableResourceContainer : ResourceSet
	{
		private static ResourceProperty TableNameProperty;

		private static ResourceProperty RequestedIOPSProperty;

		private static ResourceProperty ProvisionedIOPSProperty;

		private static ResourceProperty TableStatusProperty;

		private static ResourceProperty PKProperty;

		private static ResourceProperty RKProperty;

		private static ResourceProperty TSProperty;

		static TableResourceContainer()
		{
			UtilityTableResourceProperty utilityTableResourceProperty = new UtilityTableResourceProperty("TableName", ResourcePropertyKind.Primitive | ResourcePropertyKind.Key, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(string)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			TableResourceContainer.TableNameProperty = utilityTableResourceProperty;
			UtilityTableResourceProperty utilityTableResourceProperty1 = new UtilityTableResourceProperty("RequestedIOPS", ResourcePropertyKind.Primitive, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(int)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			TableResourceContainer.RequestedIOPSProperty = utilityTableResourceProperty1;
			UtilityTableResourceProperty utilityTableResourceProperty2 = new UtilityTableResourceProperty("ProvisionedIOPS", ResourcePropertyKind.Primitive, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(int)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			TableResourceContainer.ProvisionedIOPSProperty = utilityTableResourceProperty2;
			UtilityTableResourceProperty utilityTableResourceProperty3 = new UtilityTableResourceProperty("TableStatus", ResourcePropertyKind.Primitive, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(string)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			TableResourceContainer.TableStatusProperty = utilityTableResourceProperty3;
			UtilityRowResourceProperty utilityRowResourceProperty = new UtilityRowResourceProperty("PartitionKey", ResourcePropertyKind.Primitive | ResourcePropertyKind.Key, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(string)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			TableResourceContainer.PKProperty = utilityRowResourceProperty;
			UtilityRowResourceProperty utilityRowResourceProperty1 = new UtilityRowResourceProperty("RowKey", ResourcePropertyKind.Primitive | ResourcePropertyKind.Key, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(string)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			TableResourceContainer.RKProperty = utilityRowResourceProperty1;
			UtilityRowResourceProperty utilityRowResourceProperty2 = new UtilityRowResourceProperty("Timestamp", ResourcePropertyKind.Primitive | ResourcePropertyKind.ETag, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(DateTime)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			TableResourceContainer.TSProperty = utilityRowResourceProperty2;
		}

		public TableResourceContainer(string name, System.Data.Services.Providers.ResourceType elementType) : base(name, elementType)
		{
		}

		public static TableResourceContainer GetResourceContainer(string accountName, string tableName, bool PremiumTableAccountRequest)
		{
			if (TableResourceContainer.IsUtilityTables(tableName))
			{
				return TableResourceContainer.GetUtilityTableResourceContainer(accountName, PremiumTableAccountRequest);
			}
			return TableResourceContainer.GetUtilityRowResourceContainer(accountName, tableName);
		}

		public static TableResourceContainer GetUtilityRowResourceContainer(string accountName, string tableName)
		{
			System.Data.Services.Providers.ResourceType resourceType = new System.Data.Services.Providers.ResourceType(typeof(UtilityRow), ResourceTypeKind.EntityType, null, accountName, tableName, false)
			{
				CanReflectOnInstanceType = false,
				IsOpenType = true
			};
			ResourceProperty utilityRowResourceProperty = new UtilityRowResourceProperty("PartitionKey", ResourcePropertyKind.Primitive | ResourcePropertyKind.Key, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(string)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			resourceType.AddProperty(utilityRowResourceProperty);
			ResourceProperty resourceProperty = new UtilityRowResourceProperty("RowKey", ResourcePropertyKind.Primitive | ResourcePropertyKind.Key, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(string)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			resourceType.AddProperty(resourceProperty);
			ResourceProperty utilityRowResourceProperty1 = new UtilityRowResourceProperty("Timestamp", ResourcePropertyKind.Primitive | ResourcePropertyKind.ETag, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(DateTime)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			resourceType.AddProperty(utilityRowResourceProperty1);
			TableResourceContainer tableResourceContainer = new TableResourceContainer(tableName, resourceType);
			tableResourceContainer.SetReadOnly();
			return tableResourceContainer;
		}

		public static TableResourceContainer GetUtilityTableResourceContainer(string accountName, bool PremiumTableAccountRequest)
		{
			System.Data.Services.Providers.ResourceType resourceType = new System.Data.Services.Providers.ResourceType(typeof(UtilityTable), ResourceTypeKind.EntityType, null, accountName, "Tables", false)
			{
				CanReflectOnInstanceType = false,
				IsOpenType = true
			};
			if (PremiumTableAccountRequest)
			{
				resourceType.AddProperty(TableResourceContainer.ProvisionedIOPSProperty);
				resourceType.AddProperty(TableResourceContainer.TableStatusProperty);
				resourceType.AddProperty(TableResourceContainer.RequestedIOPSProperty);
			}
			ResourceProperty utilityTableResourceProperty = new UtilityTableResourceProperty("TableName", ResourcePropertyKind.Primitive | ResourcePropertyKind.Key, System.Data.Services.Providers.ResourceType.GetPrimitiveResourceType(typeof(string)))
			{
				CanReflectOnInstanceTypeProperty = false
			};
			resourceType.AddProperty(utilityTableResourceProperty);
			TableResourceContainer tableResourceContainer = new TableResourceContainer("Tables", resourceType);
			tableResourceContainer.SetReadOnly();
			return tableResourceContainer;
		}

		public static bool IsUtilityTables(string name)
		{
			return string.Equals(name, "Tables", StringComparison.OrdinalIgnoreCase);
		}
	}
}