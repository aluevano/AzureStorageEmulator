using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class HttpProcessorConfiguration
	{
		public static HttpProcessorConfiguration DefaultHttpProcessorConfiguration;

		public bool AllowPathStyleUris
		{
			get;
			set;
		}

		public bool EnableStorageDomainNames
		{
			get;
			set;
		}

		public bool IncludeInternalDetailsInErrorResponses
		{
			get;
			set;
		}

		public bool MaskClientIPAddressesInLog
		{
			get;
			set;
		}

		public string StampName
		{
			get;
			set;
		}

		public string[] ValidHostSuffixes
		{
			get;
			set;
		}

		static HttpProcessorConfiguration()
		{
		}

		internal HttpProcessorConfiguration()
		{
		}

		public static HttpProcessorConfiguration GetTestHttpProcessorConfiguration()
		{
			HttpProcessorConfiguration httpProcessorConfiguration = new HttpProcessorConfiguration()
			{
				AllowPathStyleUris = true,
				ValidHostSuffixes = new string[0],
				IncludeInternalDetailsInErrorResponses = true,
				EnableStorageDomainNames = false,
				MaskClientIPAddressesInLog = false
			};
			HttpProcessorConfiguration.DefaultHttpProcessorConfiguration = httpProcessorConfiguration;
			return httpProcessorConfiguration;
		}

		public static HttpProcessorConfiguration LoadDefaultAccountFEHttpProcessorConfiguration(IServiceEntrySink sink)
		{
			HttpProcessorConfiguration httpProcessorConfiguration = new HttpProcessorConfiguration()
			{
				AllowPathStyleUris = true,
				ValidHostSuffixes = new string[0],
				IncludeInternalDetailsInErrorResponses = ConfigurationHelper.GetConfigSetting<bool>(sink, "NephosIncludeInternalDetailsInErrorResponses", "Include internal details in error responses"),
				MaskClientIPAddressesInLog = false
			};
			HttpProcessorConfiguration.DefaultHttpProcessorConfiguration = httpProcessorConfiguration;
			return httpProcessorConfiguration;
		}

		public static HttpProcessorConfiguration LoadDefaultHttpProcessorConfiguration(IServiceEntrySink sink, string hostSuffixesConfigParamName, string enableStorageDomainNamesConfigParamName)
		{
			HttpProcessorConfiguration httpProcessorConfiguration = ConfigurationHelper.LoadHttpProcessorConfiguration(sink, hostSuffixesConfigParamName, enableStorageDomainNamesConfigParamName);
			HttpProcessorConfiguration.DefaultHttpProcessorConfiguration = httpProcessorConfiguration;
			return httpProcessorConfiguration;
		}

		public static HttpProcessorConfiguration LoadDefaultStampAccountFEHttpProcessorConfiguration(IServiceEntrySink sink, string hostSuffixesConfigParamName)
		{
			HttpProcessorConfiguration httpProcessorConfiguration = new HttpProcessorConfiguration();
			string[] hostSuffixes = ConfigurationHelper.GetHostSuffixes(sink, hostSuffixesConfigParamName);
			httpProcessorConfiguration.ValidHostSuffixes = (hostSuffixes != null ? hostSuffixes : new string[0]);
			string configurationParameter = sink.GetConfigurationParameter("NephosAllowPathStyleUris");
			bool flag = false;
			if (string.IsNullOrEmpty(configurationParameter))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Critical.Log("{0} setting was not found for role", new object[] { "NephosAllowPathStyleUris" });
			}
			else if (!bool.TryParse(configurationParameter, out flag))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Critical.Log("{0} setting with value {1} is not a boolean", new object[] { "NephosAllowPathStyleUris", configurationParameter });
			}
			httpProcessorConfiguration.AllowPathStyleUris = flag;
			httpProcessorConfiguration.IncludeInternalDetailsInErrorResponses = ConfigurationHelper.GetConfigSetting<bool>(sink, "NephosIncludeInternalDetailsInErrorResponses", "Include internal details in error responses");
			httpProcessorConfiguration.MaskClientIPAddressesInLog = false;
			HttpProcessorConfiguration.DefaultHttpProcessorConfiguration = httpProcessorConfiguration;
			return httpProcessorConfiguration;
		}
	}
}