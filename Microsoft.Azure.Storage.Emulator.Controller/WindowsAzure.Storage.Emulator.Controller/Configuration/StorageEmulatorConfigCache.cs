using System;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration
{
	public static class StorageEmulatorConfigCache
	{
		public static StorageEmulatorConfig Configuration
		{
			get;
			private set;
		}

		static StorageEmulatorConfigCache()
		{
			object configurationSection = StorageEmulatorUpdatableConfiguration.GetConfigurationSection("StorageEmulatorConfig");
			StorageEmulatorConfigCache.Configuration = configurationSection as StorageEmulatorConfig;
			if (StorageEmulatorConfigCache.Configuration == null)
			{
				string rawXml = ((DefaultSection)configurationSection).SectionInformation.GetRawXml();
				StorageEmulatorConfigCache.Configuration = StorageEmulatorConfigurationHandler.FromString(rawXml);
			}
		}
	}
}