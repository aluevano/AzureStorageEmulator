using System;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration
{
	internal class StorageEmulatorConfigurationHandler : IConfigurationSectionHandler
	{
		private readonly static XmlSerializer Serializer;

		static StorageEmulatorConfigurationHandler()
		{
			StorageEmulatorConfigurationHandler.Serializer = new XmlSerializer(typeof(StorageEmulatorConfig));
		}

		public StorageEmulatorConfigurationHandler()
		{
		}

		public object Create(object parent, object configContext, XmlNode section)
		{
			return StorageEmulatorConfigurationHandler.FromXML(section);
		}

		internal static StorageEmulatorConfig FromString(string s)
		{
			return (StorageEmulatorConfig)StorageEmulatorConfigurationHandler.Serializer.Deserialize(new StringReader(s));
		}

		internal static StorageEmulatorConfig FromXML(XmlNode section)
		{
			return (StorageEmulatorConfig)StorageEmulatorConfigurationHandler.Serializer.Deserialize(new XmlNodeReader(section));
		}
	}
}