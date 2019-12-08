using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration
{
	[XmlRoot("StorageEmulatorConfig")]
	public class StorageEmulatorConfig
	{
		[XmlElement("accounts")]
		public XmlSerializableDictionary<string, StorageEmulatorConfigAccount> Accounts
		{
			get;
			set;
		}

		[XmlElement("services")]
		public XmlSerializableDictionary<string, StorageEmulatorConfigService> Services
		{
			get;
			set;
		}

		public StorageEmulatorConfig()
		{
		}
	}
}