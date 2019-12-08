using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration
{
	[XmlRoot("service")]
	public class StorageEmulatorConfigService
	{
		[XmlAttribute("accountStoreCacheTTLInMinutes")]
		public string AccountStoreCacheTTLInMinutes
		{
			get;
			set;
		}

		[XmlAttribute("allowPathStyleUris")]
		public string AllowPathStyleUris
		{
			get;
			set;
		}

		[XmlAttribute("dbServer")]
		public string DBServer
		{
			get;
			set;
		}

		[XmlAttribute("enableStorageDomainNames")]
		public string EnableStorageDomainNames
		{
			get;
			set;
		}

		[XmlAttribute("enableUsageCollection")]
		public string EnableUsageCollection
		{
			get;
			set;
		}

		[XmlAttribute("includeInternalDetailsInErrorResponses")]
		public string IncludeInternalDetailsInErrorResponses
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

		[XmlAttribute("serviceEntryType")]
		public string ServiceEntryType
		{
			get;
			set;
		}

		[XmlAttribute("serviceEntryTypeAssembly")]
		public string ServiceEntryTypeAssembly
		{
			get;
			set;
		}

		[XmlAttribute("stampName")]
		public string StampName
		{
			get;
			set;
		}

		[XmlAttribute("storageBackendSetting")]
		public string StorageBackendSetting
		{
			get;
			set;
		}

		[XmlAttribute("streamCopyBufferSize")]
		public string StreamCopyBufferSize
		{
			get;
			set;
		}

		[XmlAttribute("uriHostSuffixes")]
		public string UriHostSuffixes
		{
			get;
			set;
		}

		[XmlAttribute("url")]
		public string Url
		{
			get;
			set;
		}

		public StorageEmulatorConfigService()
		{
			this.StorageBackendSetting = "NoCachingDevelopmentStorage";
			this.StreamCopyBufferSize = "4000";
			this.EnableUsageCollection = "false";
			this.AccountStoreCacheTTLInMinutes = "10";
			this.AllowPathStyleUris = "true";
			this.UriHostSuffixes = string.Empty;
			this.IncludeInternalDetailsInErrorResponses = "false";
			this.EnableStorageDomainNames = "false";
			this.StampName = "StorageEmulator";
		}
	}
}