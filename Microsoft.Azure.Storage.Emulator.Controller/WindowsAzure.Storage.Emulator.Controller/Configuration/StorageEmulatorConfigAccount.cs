using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration
{
	[XmlRoot("account")]
	public class StorageEmulatorConfigAccount
	{
		[XmlAttribute("authKey")]
		public string AuthKey
		{
			get;
			set;
		}

		[XmlAttribute("name")]
		public string Name
		{
			get;
			set;
		}

		public StorageEmulatorConfigAccount()
		{
		}
	}
}