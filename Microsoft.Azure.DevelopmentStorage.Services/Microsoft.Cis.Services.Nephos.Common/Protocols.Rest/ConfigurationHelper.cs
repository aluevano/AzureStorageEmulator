using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public static class ConfigurationHelper
	{
		public const string AllowPathStyleUrisConfigParameterName = "NephosAllowPathStyleUris";

		public const string IncludeInternalDetailsInErrorResponsesConfigParameterName = "NephosIncludeInternalDetailsInErrorResponses";

		public const string StampNameParameterName = "StampName";

		public const string MaskClientIPAddressesInLogParameterName = "Nephos.MaskClientIPAddressesInLogs";

		private const string ProvisionedParameterName = "Provisioned";

		private const string IsGeoReceiverModeParameterName = "IsGeoReceiverMode";

		private readonly static char[] HostSuffixesSeparator;

		static ConfigurationHelper()
		{
			ConfigurationHelper.HostSuffixesSeparator = new char[] { ';' };
		}

		public static T GetConfigSetting<T>(IServiceEntrySink serviceEntrySink, string configParam, string configDescription)
		{
			string configurationParameter = serviceEntrySink.GetConfigurationParameter(configParam);
			if (configurationParameter == null)
			{
				throw new ArgumentNullException(string.Concat("The value for service model config parameter named ", configParam, " is missing. Check the config files."));
			}
			IStringDataEventStream infoDebug = Logger<IRestProtocolHeadLogger>.Instance.InfoDebug;
			object[] objArray = new object[] { configParam, configDescription, configurationParameter };
			infoDebug.Log("Loading config Param {0} ({1}) read: {2}", objArray);
			if (typeof(T) == typeof(string))
			{
				return (T)configurationParameter;
			}
			Type type = typeof(T);
			Type[] typeArray = new Type[] { typeof(string), type.MakeByRefType() };
			MethodInfo method = type.GetMethod("TryParse", typeArray);
			if (method == null)
			{
				throw new ArgumentException("The passed in type parameter doesn't contain TryParse", "T");
			}
			object[] objArray1 = new object[] { configurationParameter, null };
			if (!(bool)method.Invoke(null, objArray1))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray2 = new object[] { configParam };
				throw new ArgumentException(string.Format(invariantCulture, "Failed to convert {0} configuration option to a proper type", objArray2));
			}
			T t = (T)objArray1[1];
			IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.InfoDebug;
			object[] objArray3 = new object[] { configParam, t };
			stringDataEventStream.Log("Successfully loaded {0}: {1}", objArray3);
			return t;
		}

		internal static string[] GetHostSuffixes(IServiceEntrySink sink, string hostSuffixesConfigParamName)
		{
			string configurationParameter = sink.GetConfigurationParameter(hostSuffixesConfigParamName);
			if (configurationParameter == null)
			{
				return null;
			}
			return configurationParameter.Split(ConfigurationHelper.HostSuffixesSeparator, StringSplitOptions.RemoveEmptyEntries);
		}

		public static bool GetIPAddressMaskSetting(IServiceEntrySink sink)
		{
			bool configSetting;
			try
			{
				configSetting = ConfigurationHelper.GetConfigSetting<bool>(sink, "Nephos.MaskClientIPAddressesInLogs", "Indicates whether client IP addresses should be masked in logs.");
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] str = new object[] { exception.ToString() };
				error.Log("Hit Exception while reading Nephos.MaskClientIPAddressesInLogs setting from ACS: {0}", str);
				configSetting = false;
			}
			return configSetting;
		}

		internal static HttpProcessorConfiguration LoadHttpProcessorConfiguration(IServiceEntrySink sink, string hostSuffixesConfigParamName, string enableStorageDomainNamesConfigParamName)
		{
			HttpProcessorConfiguration httpProcessorConfiguration = new HttpProcessorConfiguration()
			{
				AllowPathStyleUris = ConfigurationHelper.GetConfigSetting<bool>(sink, "NephosAllowPathStyleUris", "Allow path-style URIs"),
				ValidHostSuffixes = ConfigurationHelper.GetHostSuffixes(sink, hostSuffixesConfigParamName),
				MaskClientIPAddressesInLog = ConfigurationHelper.GetIPAddressMaskSetting(sink)
			};
			if (!httpProcessorConfiguration.AllowPathStyleUris)
			{
				NephosAssertionException.Assert(httpProcessorConfiguration.ValidHostSuffixes != null, "Host suffixes can't be null when AllowPathStyleUris is false. Check service model configuaration.");
				NephosAssertionException.Assert((int)httpProcessorConfiguration.ValidHostSuffixes.Length > 0, "Host suffixes can't be empty when AllowPathStyleUris is false. Check service model configuaration.");
			}
			httpProcessorConfiguration.ValidHostSuffixes = httpProcessorConfiguration.ValidHostSuffixes ?? new string[0];
			httpProcessorConfiguration.IncludeInternalDetailsInErrorResponses = ConfigurationHelper.GetConfigSetting<bool>(sink, "NephosIncludeInternalDetailsInErrorResponses", "Include internal details in error responses");
			if (!string.IsNullOrEmpty(enableStorageDomainNamesConfigParamName))
			{
				httpProcessorConfiguration.EnableStorageDomainNames = ConfigurationHelper.GetConfigSetting<bool>(sink, enableStorageDomainNamesConfigParamName, "Enable storage domain names");
			}
			try
			{
				httpProcessorConfiguration.StampName = ConfigurationHelper.GetConfigSetting<string>(sink, "StampName", "Stamp Name");
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] str = new object[] { exception.ToString() };
				error.Log("Hit Exception while reading StampName setting from ACS: {0}", str);
			}
			return httpProcessorConfiguration;
		}
	}
}