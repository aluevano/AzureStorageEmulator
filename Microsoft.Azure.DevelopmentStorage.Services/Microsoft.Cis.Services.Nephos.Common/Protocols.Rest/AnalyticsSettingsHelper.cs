using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Versioning;
using Microsoft.Cis.Services.Nephos.Common.XLogging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public static class AnalyticsSettingsHelper
	{
		private const string RootPropertiesElementName = "StorageServiceProperties";

		private const string VersionElementName = "Version";

		private const string RetentionPolicyElementName = "RetentionPolicy";

		private const string RetentionPolicyEnabledElementName = "Enabled";

		private const string RetentionPolicyDaysElementName = "Days";

		private const string LoggingElementName = "Logging";

		private const string ApiTypeDeleteElementName = "Delete";

		private const string ApiTypeReadElementName = "Read";

		private const string ApiTypeWriteElementName = "Write";

		private const string MetricsElementName = "Metrics";

		private const string HourMetricsElementName = "HourMetrics";

		private const string MinuteMetricsElementName = "MinuteMetrics";

		private const string IncludeApiSummaryElementName = "IncludeAPIs";

		private const string MetricsEnabledElementName = "Enabled";

		private const string DefaultServiceVersionElementName = "DefaultServiceVersion";

		private const string CorsElementName = "Cors";

		private const string CorsRuleElementName = "CorsRule";

		private const string CorsAllowedMethods = "AllowedMethods";

		private const string CorsAllowedOrigins = "AllowedOrigins";

		private const string CorsAllowedHeaders = "AllowedHeaders";

		private const string CorsExposedHeaders = "ExposedHeaders";

		private const string CorsMaxAge = "MaxAgeInSeconds";

		private const string ElementNotSupportedForFileServiceFormatString = "Element {0} not supported for File service.";

		private const string DuplicateElementExceptionFormatString = "Found duplicate element: {0}.";

		private const string MissingElementExceptionFormatString = "Element is missing: {0}.";

		private const string RedundantElementExceptionFormatString = "Element {0} is only expected when {1} is enabled.";

		private const string InvalidXmlProtocolExceptionFormatString = "Error parsing Xml content: {0}";

		private const string InvalidRetentionDaysErrorFormatString = "Retention days must be an integer with value greater than 0 and less than or equal to {0} days.";

		private const string RetentionDaysRangeErrorFormatString = "Retention days must be greater than 0 and less than or equal to {0} days.";

		private const string MaxRetentionVersionsRangeErrorFormatString = "Max retention versions per blob must be greater than 0 and less than or equal to {0}.";

		private const string InvalidBooleanValueErrorFormatString = "Valid values for {0} are true|false.";

		private const string InvalidIntegerValueErrorFormatString = "{0} is not a valid integer value";

		private const string UnexpectedVersionValueErrorString = "Unexpected value for Version.";

		private const string DefaultVersionElementNotAllowedErrorString = "DefaultServiceVersion element is allowed only for Blob service with REST versions starting from 2011-08-18";

		private const string CorsElementNotAllowedErrorString = "Cors element is allowed only for service with REST versions starting from 2013-08-15";

		private const string MinuteMetricsElementNotAllowedErrorString = "MinuteMetrics element is allowed only for service with REST versions starting from 2013-08-15";

		private const string MetricsElementNotAllowedErrorString = "Metrics element is not allowed starting version 2013-08-15 , Use HourMetrics instead";

		private const string HourMetricsElementNotAllowedErrorString = "HourMetrics element is not allowed for versions before 2013-08-15 , Use Metrics instead";

		private const string InvalidServiceVersionErrorString = "Version '{0}' is an invalid REST version. It should be equal to or later than {1}";

		private const string InvalidElementInXml = "Element '{0}' is not recognized.";

		private const int MaximumRetentionDays = 365;

		private const int MaxRetentionVersionsPerBlob = 10;

		private const int MaximumCorsPrefixedHeaders = 2;

		private const int MaximumCorsRuleLiterals = 64;

		private const int MaximumCorsHeaders = 66;

		private const int MaximumCorsElementLength = 256;

		private const int MaxCorsRulesNumber = 5;

		private const int MaxCorsRulesSize = 2048;

		public static HashSet<double> ExpectedVersions;

		static AnalyticsSettingsHelper()
		{
			AnalyticsSettingsHelper.ExpectedVersions = new HashSet<double>();
			AnalyticsSettingsHelper.ExpectedVersions.Add(1);
		}

		private static void AddStringComponentsToCorsRule(string nodeName, string commaSeparatedString, HashSet<string> literalHeaders, List<string> prefixedHeaders, ref int corsRulesSize)
		{
			string[] strArrays = commaSeparatedString.Split(new char[] { ',' });
			if ((int)strArrays.Length > 66)
			{
				throw new InvalidXmlNodeProtocolException(nodeName, commaSeparatedString, string.Format("Headers can't exceed maximum header count of {0} , current header length is {1}", 66, (int)strArrays.Length));
			}
			int num = 0;
			int num1 = 0;
			string[] strArrays1 = strArrays;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string str = strArrays1[i].Trim();
				corsRulesSize += str.Length;
				if (str == string.Empty)
				{
					throw new InvalidXmlNodeProtocolException(nodeName, commaSeparatedString, "CORS headers can't include empty headers");
				}
				if (str.Length > 256)
				{
					throw new InvalidXmlNodeProtocolException(nodeName, commaSeparatedString, string.Format("CORS headers can't exceed maximum header count ={0}", 256));
				}
				int num2 = str.IndexOf(AnalyticsSettings.WildCard);
				if (num2 != -1)
				{
					if (num2 != str.Length - 1)
					{
						throw new InvalidXmlNodeProtocolException(nodeName, commaSeparatedString, string.Format("Wildcard can only exist at the end of headers", new object[0]));
					}
					num++;
					if (num > 2)
					{
						throw new InvalidXmlNodeProtocolException(nodeName, commaSeparatedString, string.Format("Prefixed headers can't exceed maximum header count = {0}", 2));
					}
					prefixedHeaders.Add((str.Length == 1 ? str : str.Substring(0, str.Length - 1)));
				}
				else
				{
					literalHeaders.Add(str);
					num1++;
					if (num1 > 64)
					{
						throw new InvalidXmlNodeProtocolException(nodeName, commaSeparatedString, string.Format("Literal headers can't exceed maximum header count = {0}", 64));
					}
				}
			}
		}

		private static string ConvertListToCommaDelimitedString(List<string> stringList, bool appendWildCard = false)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < stringList.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(stringList[i]);
				if (appendWildCard && stringList[i] != AnalyticsSettings.WildCard)
				{
					stringBuilder.Append(AnalyticsSettings.WildCard);
				}
			}
			return stringBuilder.ToString();
		}

		public static AnalyticsSettings DeserializeAnalyticsSettingsFromReader(XmlReader xmlReader, AnalyticsSettingsVersion settingsVersion, ServiceType service = 2, bool enableMetricsOverride = false, bool allowArchive = false)
		{
			AnalyticsSettings analyticsSetting;
			try
			{
				bool flag = true;
				IXmlLineInfo xmlLineInfo = xmlReader as IXmlLineInfo;
				xmlReader.Read();
				xmlReader.ReadStartElement("StorageServiceProperties");
				AnalyticsSettings analyticsSetting1 = new AnalyticsSettings()
				{
					IsAnalyticsVersionAtLeastV3 = settingsVersion >= AnalyticsSettingsVersion.V3,
					IsAnalyticsVersionAtLeastV5 = settingsVersion >= AnalyticsSettingsVersion.V5,
					IsAnalyticsVersionAtLeastV6 = settingsVersion >= AnalyticsSettingsVersion.V6
				};
				while (true)
				{
					if (xmlReader.IsStartElement("Logging"))
					{
						if (analyticsSetting1.LoggingElementRead)
						{
							AnalyticsSettingsHelper.ThrowDuplicateElementException("Logging", xmlLineInfo);
						}
						if (service == ServiceType.FileService)
						{
							AnalyticsSettingsHelper.ThrowUnsupportedElementOnFileServiceException("Logging", xmlLineInfo);
						}
						AnalyticsSettingsHelper.DeserializeLoggingElement(xmlReader, xmlLineInfo, analyticsSetting1);
						analyticsSetting1.LoggingElementRead = true;
					}
					else if (xmlReader.IsStartElement("Metrics"))
					{
						if (settingsVersion >= AnalyticsSettingsVersion.V3)
						{
							throw new InvalidXmlProtocolException("Metrics element is not allowed starting version 2013-08-15 , Use HourMetrics instead", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
						}
						if (analyticsSetting1.MetricsElementRead)
						{
							AnalyticsSettingsHelper.ThrowDuplicateElementException("Metrics", xmlLineInfo);
						}
						if (service == ServiceType.FileService)
						{
							AnalyticsSettingsHelper.ThrowUnsupportedElementOnFileServiceException("Metrics", xmlLineInfo);
						}
						xmlReader.ReadStartElement("Metrics");
						AnalyticsSettingsHelper.DeserializeMetricsElement(xmlReader, xmlLineInfo, analyticsSetting1, false);
						xmlReader.ReadEndElement();
						analyticsSetting1.MetricsElementRead = true;
					}
					else if (xmlReader.IsStartElement("MinuteMetrics"))
					{
						if (settingsVersion < AnalyticsSettingsVersion.V3)
						{
							throw new InvalidXmlProtocolException("MinuteMetrics element is allowed only for service with REST versions starting from 2013-08-15", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
						}
						if (analyticsSetting1.MinuteMetricsElementRead)
						{
							AnalyticsSettingsHelper.ThrowDuplicateElementException("MinuteMetrics", xmlLineInfo);
						}
						if (service == ServiceType.FileService && (!flag || settingsVersion < AnalyticsSettingsVersion.V4))
						{
							AnalyticsSettingsHelper.ThrowUnsupportedElementOnFileServiceException("MinuteMetrics", xmlLineInfo);
						}
						xmlReader.ReadStartElement("MinuteMetrics");
						AnalyticsSettingsHelper.DeserializeMetricsElement(xmlReader, xmlLineInfo, analyticsSetting1, true);
						xmlReader.ReadEndElement();
						analyticsSetting1.MinuteMetricsElementRead = true;
					}
					else if (xmlReader.IsStartElement("HourMetrics"))
					{
						if (settingsVersion < AnalyticsSettingsVersion.V3)
						{
							throw new InvalidXmlProtocolException("HourMetrics element is not allowed for versions before 2013-08-15 , Use Metrics instead", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
						}
						if (analyticsSetting1.MetricsElementRead)
						{
							AnalyticsSettingsHelper.ThrowDuplicateElementException("HourMetrics", xmlLineInfo);
						}
						if (service == ServiceType.FileService && (!flag || settingsVersion < AnalyticsSettingsVersion.V4))
						{
							AnalyticsSettingsHelper.ThrowUnsupportedElementOnFileServiceException("HourMetrics", xmlLineInfo);
						}
						xmlReader.ReadStartElement("HourMetrics");
						AnalyticsSettingsHelper.DeserializeMetricsElement(xmlReader, xmlLineInfo, analyticsSetting1, false);
						xmlReader.ReadEndElement();
						analyticsSetting1.MetricsElementRead = true;
					}
					else if (!xmlReader.IsStartElement("DefaultServiceVersion"))
					{
						if (!xmlReader.IsStartElement("Cors"))
						{
							if (service == ServiceType.BlobService && settingsVersion >= AnalyticsSettingsVersion.V2 && !string.Equals(xmlReader.Name, "StorageServiceProperties"))
							{
								CultureInfo invariantCulture = CultureInfo.InvariantCulture;
								object[] name = new object[] { xmlReader.Name };
								throw new InvalidXmlProtocolException(string.Format(invariantCulture, "Element '{0}' is not recognized.", name), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
							}
							if (!analyticsSetting1.LoggingElementRead && settingsVersion < AnalyticsSettingsVersion.V3 && service != ServiceType.FileService)
							{
								AnalyticsSettingsHelper.ThrowMissingElementException("Logging", xmlLineInfo);
							}
							if (!analyticsSetting1.MetricsElementRead && settingsVersion < AnalyticsSettingsVersion.V3 && service != ServiceType.FileService)
							{
								AnalyticsSettingsHelper.ThrowMissingElementException("Metrics", xmlLineInfo);
							}
							xmlReader.ReadEndElement();
							analyticsSetting = analyticsSetting1;
							return analyticsSetting;
						}
						if (analyticsSetting1.CorsRulesElementRead)
						{
							AnalyticsSettingsHelper.ThrowDuplicateElementException("Cors", xmlLineInfo);
						}
						if (!analyticsSetting1.IsAnalyticsVersionAtLeastV3)
						{
							throw new InvalidXmlProtocolException("Cors element is allowed only for service with REST versions starting from 2013-08-15", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
						}
						AnalyticsSettingsHelper.DeserializeCorsRules(xmlReader, xmlLineInfo, analyticsSetting1);
						analyticsSetting1.CorsRulesElementRead = true;
					}
					else
					{
						if (service != ServiceType.BlobService || settingsVersion == AnalyticsSettingsVersion.V1)
						{
							break;
						}
						if (analyticsSetting1.DefaultServiceVersionElementRead)
						{
							AnalyticsSettingsHelper.ThrowDuplicateElementException("DefaultServiceVersion", xmlLineInfo);
						}
						string str = xmlReader.ReadElementString("DefaultServiceVersion");
						if (!VersioningConfigurationLookup.Instance.IsValidVersion(str))
						{
							CultureInfo cultureInfo = CultureInfo.InvariantCulture;
							object[] objArray = new object[] { str, "2009-09-19" };
							throw new InvalidXmlProtocolException(string.Format(cultureInfo, "Version '{0}' is an invalid REST version. It should be equal to or later than {1}", objArray), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
						}
						analyticsSetting1.DefaultRESTVersion = str;
						analyticsSetting1.DefaultServiceVersionElementRead = true;
					}
				}
				throw new InvalidXmlProtocolException("DefaultServiceVersion element is allowed only for Blob service with REST versions starting from 2011-08-18", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
			}
			catch (FormatException formatException1)
			{
				FormatException formatException = formatException1;
				CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
				object[] message = new object[] { formatException.Message };
				throw new InvalidXmlProtocolException(string.Format(invariantCulture1, "Error parsing Xml content: {0}", message));
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				CultureInfo cultureInfo1 = CultureInfo.InvariantCulture;
				object[] message1 = new object[] { invalidOperationException.Message };
				throw new InvalidXmlProtocolException(string.Format(cultureInfo1, "Error parsing Xml content: {0}", message1));
			}
			catch (XmlException xmlException1)
			{
				XmlException xmlException = xmlException1;
				CultureInfo invariantCulture2 = CultureInfo.InvariantCulture;
				object[] objArray1 = new object[] { xmlException.Message };
				throw new InvalidXmlProtocolException(string.Format(invariantCulture2, "Error parsing Xml content: {0}", objArray1), xmlException.LineNumber, xmlException.LinePosition);
			}
			return analyticsSetting;
		}

		private static bool DeserializeBooleanElementValue(XmlReader xmlReader, IXmlLineInfo xmlLineInfo, string elementToRead)
		{
			string str = xmlReader.ReadElementString(elementToRead);
			bool flag = false;
			if (string.IsNullOrEmpty(str) || !bool.TryParse(str, out flag))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { elementToRead };
				throw new InvalidXmlProtocolException(string.Format(invariantCulture, "Valid values for {0} are true|false.", objArray), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
			}
			return flag;
		}

		private static void DeserializeCorsRules(XmlReader xmlReader, IXmlLineInfo xmlLineInfo, AnalyticsSettings settings)
		{
			string str;
			string str1;
			if (xmlReader.IsEmptyElement)
			{
				xmlReader.ReadStartElement("Cors");
				settings.DeleteCorsRules = true;
				return;
			}
			xmlReader.ReadStartElement("Cors");
			int num = 0;
			int length = 0;
		Label0:
			while (xmlReader.IsStartElement("CorsRule"))
			{
				CorsRule corsRule = new CorsRule();
				bool flag = false;
				num++;
				if (num > 5)
				{
					throw new InvalidXmlProtocolException(string.Format("CORS rules can't exceed maximum of {0} rules", 5));
				}
				xmlReader.ReadStartElement("CorsRule");
				do
				{
					if (xmlReader.IsStartElement("AllowedHeaders"))
					{
						if (corsRule.AllowedLiteralHeaders != null || corsRule.AllowedPrefixedHeaders != null)
						{
							throw new InvalidXmlProtocolException(string.Format("Element {0} is not allowed to be duplicate for CORS rules", "AllowedHeaders"));
						}
						corsRule.AllowedLiteralHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						corsRule.AllowedPrefixedHeaders = new List<string>();
						string str2 = xmlReader.ReadElementString("AllowedHeaders");
						if (str2 != string.Empty)
						{
							AnalyticsSettingsHelper.AddStringComponentsToCorsRule("AllowedHeaders", str2, corsRule.AllowedLiteralHeaders, corsRule.AllowedPrefixedHeaders, ref length);
							goto Label1;
						}
						else
						{
							goto Label1;
						}
					}
					else if (xmlReader.IsStartElement("AllowedOrigins"))
					{
						if (corsRule.AllowedOrigins != null)
						{
							throw new InvalidXmlProtocolException(string.Format("Element {0} is not allowed to be duplicate for CORS rules", "AllowedOrigins"));
						}
						str = xmlReader.ReadElementString("AllowedOrigins");
						corsRule.AllowedOrigins = new HashSet<string>();
						string[] strArrays = str.Split(new char[] { ',' });
						if ((int)strArrays.Length > 64)
						{
							throw new InvalidXmlNodeProtocolException("AllowedOrigins", str, string.Format("Number of origins can not exceed {0}", 64));
						}
						string[] strArrays1 = strArrays;
						for (int i = 0; i < (int)strArrays1.Length; i++)
						{
							string str3 = strArrays1[i].Trim();
							if (str3 == string.Empty)
							{
								throw new InvalidXmlNodeProtocolException("AllowedOrigins", str, "Allowed origins can't contain empty origin");
							}
							length += str3.Length;
							if (str3.Length > 256)
							{
								throw new InvalidXmlNodeProtocolException("AllowedOrigins", str, string.Format("Origin characters can't exceed {0}", 256));
							}
							corsRule.AllowedOrigins.Add(str3);
						}
					}
					else if (xmlReader.IsStartElement("ExposedHeaders"))
					{
						if (corsRule.ExposedLiteralHeaders != null || corsRule.ExposedPrefixedHeaders != null)
						{
							throw new InvalidXmlProtocolException(string.Format("Element {0} is not allowed to be duplicate for CORS rules", "ExposedHeaders"));
						}
						corsRule.ExposedLiteralHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						corsRule.ExposedPrefixedHeaders = new List<string>();
						string str4 = xmlReader.ReadElementString("ExposedHeaders");
						if (str4 != string.Empty)
						{
							AnalyticsSettingsHelper.AddStringComponentsToCorsRule("ExposedHeaders", str4, corsRule.ExposedLiteralHeaders, corsRule.ExposedPrefixedHeaders, ref length);
							goto Label1;
						}
						else
						{
							goto Label1;
						}
					}
					else if (xmlReader.IsStartElement("MaxAgeInSeconds"))
					{
						if (flag)
						{
							throw new InvalidXmlProtocolException(string.Format("Element {0} is not allowed to be duplicate for CORS rules", "MaxAgeInSeconds"));
						}
						flag = true;
						corsRule.MaxAge = AnalyticsSettingsHelper.DeserializeIntegerElementValue(xmlReader, xmlLineInfo, "MaxAgeInSeconds", out str1);
						length += str1.Length;
						goto Label1;
					}
					else if (!xmlReader.IsStartElement("AllowedMethods"))
					{
						xmlReader.ReadEndElement();
						if (corsRule.AllowedOrigins == null || corsRule.AllowedMethods == null || !flag || corsRule.AllowedLiteralHeaders == null && corsRule.AllowedPrefixedHeaders == null || corsRule.ExposedLiteralHeaders == null && corsRule.ExposedPrefixedHeaders == null)
						{
							throw new InvalidXmlProtocolException("CORS required fields are not present");
						}
						settings.CorsRules.Add(corsRule);
						if (length <= 2048)
						{
							goto Label0;
						}
						throw new InvalidXmlProtocolException("CORS rules can't exceed 4KB of data");
					}
					else
					{
						if (corsRule.AllowedMethods != null)
						{
							throw new InvalidXmlProtocolException(string.Format("Element {0} is not allowed to be duplicate for CORS rules", "AllowedMethods"));
						}
						string str5 = xmlReader.ReadElementString("AllowedMethods");
						corsRule.AllowedMethods = new HashSet<string>();
						string[] strArrays2 = str5.Split(new char[] { ',' });
						for (int j = 0; j < (int)strArrays2.Length; j++)
						{
							string str6 = strArrays2[j];
							try
							{
								Enum.Parse(typeof(RestMethod), str6);
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								throw new InvalidXmlNodeProtocolException("AllowedMethods", str6, string.Format("\"{0}\" is not an allowed method", str6), exception);
							}
							length += str6.Length;
							corsRule.AllowedMethods.Add(str6.Trim());
						}
						goto Label1;
					}
				}
				while (!corsRule.AllowedOrigins.Contains(AnalyticsSettings.WildCard) || corsRule.AllowedOrigins.Count <= 1);
				throw new InvalidXmlNodeProtocolException("AllowedOrigins", str, "Allowed origins can't have '*' and other origins");
			}
			xmlReader.ReadEndElement();
			if (settings.CorsRules.Count == 0)
			{
				settings.DeleteCorsRules = true;
			}
		}

		private static int DeserializeIntegerElementValue(XmlReader xmlReader, IXmlLineInfo xmlLineInfo, string elementToRead, out string integerString)
		{
			string str = xmlReader.ReadElementString(elementToRead);
			int num = -1;
			if (string.IsNullOrEmpty(str) || !int.TryParse(str, out num))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { elementToRead };
				throw new InvalidXmlNodeProtocolException(elementToRead, str, string.Format(invariantCulture, "{0} is not a valid integer value", objArray));
			}
			integerString = str;
			return num;
		}

		private static void DeserializeLoggingElement(XmlReader xmlReader, IXmlLineInfo xmlLineInfo, AnalyticsSettings settings)
		{
			bool flag = false;
			bool flag1 = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			xmlReader.ReadStartElement("Logging");
			while (true)
			{
				if (xmlReader.IsStartElement("Version"))
				{
					if (flag)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("Version", xmlLineInfo);
					}
					settings.LogVersion = AnalyticsSettingsHelper.DeserializeVersion(xmlReader, xmlLineInfo);
					flag = true;
				}
				else if (xmlReader.IsStartElement("Read"))
				{
					if (flag1)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("Read", xmlLineInfo);
					}
					if (AnalyticsSettingsHelper.DeserializeBooleanElementValue(xmlReader, xmlLineInfo, "Read"))
					{
						settings.LogType = settings.LogType | LoggingLevel.Read;
					}
					flag1 = true;
				}
				else if (xmlReader.IsStartElement("Write"))
				{
					if (flag2)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("Write", xmlLineInfo);
					}
					if (AnalyticsSettingsHelper.DeserializeBooleanElementValue(xmlReader, xmlLineInfo, "Write"))
					{
						settings.LogType = settings.LogType | LoggingLevel.Write;
					}
					flag2 = true;
				}
				else if (!xmlReader.IsStartElement("Delete"))
				{
					if (!xmlReader.IsStartElement("RetentionPolicy"))
					{
						break;
					}
					if (flag4)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("RetentionPolicy", xmlLineInfo);
					}
					bool flag5 = false;
					int num = 0;
					AnalyticsSettingsHelper.DeserializeRetentionPolicy(xmlReader, xmlLineInfo, ref flag5, ref num);
					settings.IsLogRetentionPolicyEnabled = flag5;
					settings.LogRetentionInDays = num;
					flag4 = true;
				}
				else
				{
					if (flag3)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("Delete", xmlLineInfo);
					}
					if (AnalyticsSettingsHelper.DeserializeBooleanElementValue(xmlReader, xmlLineInfo, "Delete"))
					{
						settings.LogType = settings.LogType | LoggingLevel.Delete;
					}
					flag3 = true;
				}
			}
			if (!flag)
			{
				AnalyticsSettingsHelper.ThrowMissingElementException("Version", xmlLineInfo);
			}
			if (!flag1)
			{
				AnalyticsSettingsHelper.ThrowMissingElementException("Read", xmlLineInfo);
			}
			if (!flag2)
			{
				AnalyticsSettingsHelper.ThrowMissingElementException("Write", xmlLineInfo);
			}
			if (!flag3)
			{
				AnalyticsSettingsHelper.ThrowMissingElementException("Delete", xmlLineInfo);
			}
			if (!flag4)
			{
				AnalyticsSettingsHelper.ThrowMissingElementException("RetentionPolicy", xmlLineInfo);
			}
			xmlReader.ReadEndElement();
		}

		private static void DeserializeMetricsElement(XmlReader xmlReader, IXmlLineInfo xmlLineInfo, AnalyticsSettings settings, bool isMinuteMetrics = false)
		{
			bool flag = false;
			bool flag1 = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			while (true)
			{
				if (xmlReader.IsStartElement("Version"))
				{
					if (flag)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("Version", xmlLineInfo);
					}
					if (!isMinuteMetrics)
					{
						settings.MetricsVersion = AnalyticsSettingsHelper.DeserializeVersion(xmlReader, xmlLineInfo);
					}
					else
					{
						settings.MinuteMetricsVersion = AnalyticsSettingsHelper.DeserializeVersion(xmlReader, xmlLineInfo);
					}
					flag = true;
				}
				else if (xmlReader.IsStartElement("Enabled"))
				{
					if (flag1)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("Enabled", xmlLineInfo);
					}
					if (AnalyticsSettingsHelper.DeserializeBooleanElementValue(xmlReader, xmlLineInfo, "Enabled"))
					{
						if (!isMinuteMetrics)
						{
							settings.MetricsType = settings.MetricsType | MetricsType.ServiceSummary;
						}
						else
						{
							settings.MinuteMetricsType = settings.MinuteMetricsType | MetricsType.ServiceSummary;
						}
					}
					flag1 = true;
				}
				else if (!xmlReader.IsStartElement("IncludeAPIs"))
				{
					if (!xmlReader.IsStartElement("RetentionPolicy"))
					{
						break;
					}
					if (flag3)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("RetentionPolicy", xmlLineInfo);
					}
					bool flag5 = false;
					int num = 0;
					AnalyticsSettingsHelper.DeserializeRetentionPolicy(xmlReader, xmlLineInfo, ref flag5, ref num);
					if (!isMinuteMetrics)
					{
						settings.IsMetricsRetentionPolicyEnabled = flag5;
						settings.MetricsRetentionInDays = num;
					}
					else
					{
						settings.IsMinuteMetricsRetentionPolicyEnabled = new bool?(flag5);
						settings.MinuteMetricsRetentionInDays = num;
					}
					flag3 = true;
				}
				else
				{
					if (flag2)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("IncludeAPIs", xmlLineInfo);
					}
					if (AnalyticsSettingsHelper.DeserializeBooleanElementValue(xmlReader, xmlLineInfo, "IncludeAPIs"))
					{
						flag4 = true;
					}
					flag2 = true;
				}
			}
			if (!flag)
			{
				AnalyticsSettingsHelper.ThrowMissingElementException("Version", xmlLineInfo);
			}
			if (!flag1)
			{
				AnalyticsSettingsHelper.ThrowMissingElementException("Enabled", xmlLineInfo);
			}
			if (isMinuteMetrics && (settings.MinuteMetricsType & MetricsType.ServiceSummary) != MetricsType.None || !isMinuteMetrics && (settings.MetricsType & MetricsType.ServiceSummary) != MetricsType.None)
			{
				if (!flag2)
				{
					AnalyticsSettingsHelper.ThrowMissingElementException("IncludeAPIs", xmlLineInfo);
				}
				if (flag4)
				{
					if (!isMinuteMetrics)
					{
						settings.MetricsType = settings.MetricsType | MetricsType.ApiSummary;
					}
					else
					{
						settings.MinuteMetricsType = settings.MinuteMetricsType | MetricsType.ApiSummary;
					}
				}
			}
			else if (flag2)
			{
				AnalyticsSettingsHelper.ThrowRedundantElementException("IncludeAPIs", "Metrics", xmlLineInfo);
			}
			if (!flag3)
			{
				AnalyticsSettingsHelper.ThrowMissingElementException("RetentionPolicy", xmlLineInfo);
			}
		}

		private static void DeserializeRetentionPolicy(XmlReader xmlReader, IXmlLineInfo xmlLineInfo, ref bool isRetentionEnabled, ref int retentionDays)
		{
			int num;
			bool flag = false;
			bool flag1 = false;
			xmlReader.ReadStartElement("RetentionPolicy");
			while (true)
			{
				if (!xmlReader.IsStartElement("Enabled"))
				{
					if (!xmlReader.IsStartElement("Days"))
					{
						if (!flag)
						{
							AnalyticsSettingsHelper.ThrowMissingElementException("Enabled", xmlLineInfo);
						}
						if (isRetentionEnabled && !flag1)
						{
							AnalyticsSettingsHelper.ThrowMissingElementException("Days", xmlLineInfo);
						}
						if (!isRetentionEnabled && flag1)
						{
							AnalyticsSettingsHelper.ThrowRedundantElementException("Days", "RetentionPolicy", xmlLineInfo);
						}
						xmlReader.ReadEndElement();
						return;
					}
					if (flag1)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("Days", xmlLineInfo);
					}
					if (!int.TryParse(xmlReader.ReadElementString("Days"), out num))
					{
						CultureInfo invariantCulture = CultureInfo.InvariantCulture;
						object[] objArray = new object[] { 365 };
						throw new InvalidXmlProtocolException(string.Format(invariantCulture, "Retention days must be an integer with value greater than 0 and less than or equal to {0} days.", objArray), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
					}
					if (num <= 0 || num > 365)
					{
						break;
					}
					retentionDays = num;
					flag1 = true;
				}
				else
				{
					if (flag)
					{
						AnalyticsSettingsHelper.ThrowDuplicateElementException("Enabled", xmlLineInfo);
					}
					isRetentionEnabled = AnalyticsSettingsHelper.DeserializeBooleanElementValue(xmlReader, xmlLineInfo, "Enabled");
					flag = true;
				}
			}
			CultureInfo cultureInfo = CultureInfo.InvariantCulture;
			object[] objArray1 = new object[] { 365 };
			throw new InvalidXmlProtocolException(string.Format(cultureInfo, "Retention days must be greater than 0 and less than or equal to {0} days.", objArray1), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
		}

		private static double DeserializeVersion(XmlReader xmlReader, IXmlLineInfo xmlLineInfo)
		{
			double num;
			if (!double.TryParse(xmlReader.ReadElementString("Version"), out num))
			{
				throw new InvalidXmlProtocolException("Unexpected value for Version.", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
			}
			if (!AnalyticsSettingsHelper.ExpectedVersions.Contains(num))
			{
				throw new InvalidXmlProtocolException("Unexpected value for Version.", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
			}
			return num;
		}

		public static AnalyticsSettingsVersion GetSettingVersion(RequestContext requestContext)
		{
			if (requestContext.IsRequestVersionAtLeastApril17)
			{
				return AnalyticsSettingsVersion.V5;
			}
			if (requestContext.IsRequestVersionAtLeastApril15)
			{
				return AnalyticsSettingsVersion.V4;
			}
			if (requestContext.IsRequestVersionAtLeastAugust13)
			{
				return AnalyticsSettingsVersion.V3;
			}
			if (requestContext.IsRequestVersionAtLeastAugust11)
			{
				return AnalyticsSettingsVersion.V2;
			}
			return AnalyticsSettingsVersion.V1;
		}

		public static void SerializeAnalyticsSettingsToWriter(XmlWriter xmlWriter, AnalyticsSettings settings, AnalyticsSettingsVersion settingsVersion, ServiceType service = 2, bool enableMetricsOverride = false)
		{
			if (service == ServiceType.FileService)
			{
				AnalyticsSettingsHelper.SerializeFileAnalyticsSettingsToWriter(xmlWriter, settings, settingsVersion, enableMetricsOverride);
				return;
			}
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("StorageServiceProperties");
			xmlWriter.WriteStartElement("Logging");
			xmlWriter.WriteStartElement("Version");
			xmlWriter.WriteValue(string.Format("{0:0.0}", settings.LogVersion));
			xmlWriter.WriteEndElement();
			bool logType = (settings.LogType & LoggingLevel.Read) != LoggingLevel.None;
			xmlWriter.WriteStartElement("Read");
			xmlWriter.WriteValue(logType);
			xmlWriter.WriteEndElement();
			bool flag = (settings.LogType & LoggingLevel.Write) != LoggingLevel.None;
			xmlWriter.WriteStartElement("Write");
			xmlWriter.WriteValue(flag);
			xmlWriter.WriteEndElement();
			bool logType1 = (settings.LogType & LoggingLevel.Delete) != LoggingLevel.None;
			xmlWriter.WriteStartElement("Delete");
			xmlWriter.WriteValue(logType1);
			xmlWriter.WriteEndElement();
			AnalyticsSettingsHelper.SerializeRetentionPolicy(xmlWriter, settings.IsLogRetentionPolicyEnabled, settings.LogRetentionInDays);
			xmlWriter.WriteEndElement();
			if (settingsVersion < AnalyticsSettingsVersion.V3)
			{
				xmlWriter.WriteStartElement("Metrics");
			}
			else
			{
				xmlWriter.WriteStartElement("HourMetrics");
			}
			xmlWriter.WriteStartElement("Version");
			xmlWriter.WriteValue(string.Format("{0:0.0}", settings.MetricsVersion));
			xmlWriter.WriteEndElement();
			bool metricsType = (settings.MetricsType & MetricsType.ServiceSummary) != MetricsType.None;
			xmlWriter.WriteStartElement("Enabled");
			xmlWriter.WriteValue(metricsType);
			xmlWriter.WriteEndElement();
			if (metricsType)
			{
				bool metricsType1 = (settings.MetricsType & MetricsType.ApiSummary) != MetricsType.None;
				xmlWriter.WriteStartElement("IncludeAPIs");
				xmlWriter.WriteValue(metricsType1);
				xmlWriter.WriteEndElement();
			}
			AnalyticsSettingsHelper.SerializeRetentionPolicy(xmlWriter, settings.IsMetricsRetentionPolicyEnabled, settings.MetricsRetentionInDays);
			xmlWriter.WriteEndElement();
			if (settingsVersion >= AnalyticsSettingsVersion.V3)
			{
				xmlWriter.WriteStartElement("MinuteMetrics");
				xmlWriter.WriteStartElement("Version");
				xmlWriter.WriteValue(string.Format("{0:0.0}", settings.MinuteMetricsVersion));
				xmlWriter.WriteEndElement();
				metricsType = (settings.MinuteMetricsType & MetricsType.ServiceSummary) != MetricsType.None;
				xmlWriter.WriteStartElement("Enabled");
				xmlWriter.WriteValue(metricsType);
				xmlWriter.WriteEndElement();
				if (metricsType)
				{
					bool minuteMetricsType = (settings.MinuteMetricsType & MetricsType.ApiSummary) != MetricsType.None;
					xmlWriter.WriteStartElement("IncludeAPIs");
					xmlWriter.WriteValue(minuteMetricsType);
					xmlWriter.WriteEndElement();
				}
				XmlWriter xmlWriter1 = xmlWriter;
				bool? isMinuteMetricsRetentionPolicyEnabled = settings.IsMinuteMetricsRetentionPolicyEnabled;
				AnalyticsSettingsHelper.SerializeRetentionPolicy(xmlWriter1, (!isMinuteMetricsRetentionPolicyEnabled.GetValueOrDefault() ? false : isMinuteMetricsRetentionPolicyEnabled.HasValue), settings.MinuteMetricsRetentionInDays);
				xmlWriter.WriteEndElement();
			}
			if (settingsVersion >= AnalyticsSettingsVersion.V3)
			{
				AnalyticsSettingsHelper.serializeCorsRules(xmlWriter, settings.CorsRules);
			}
			if (service == ServiceType.BlobService && settingsVersion >= AnalyticsSettingsVersion.V2 && !string.IsNullOrEmpty(settings.DefaultRESTVersion))
			{
				xmlWriter.WriteStartElement("DefaultServiceVersion");
				xmlWriter.WriteValue(settings.DefaultRESTVersion);
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
		}

		private static void serializeCorsRules(XmlWriter xmlWriter, List<CorsRule> rules)
		{
			string str;
			string str1;
			xmlWriter.WriteStartElement("Cors");
			if (rules != null && rules.Count > 0)
			{
				foreach (CorsRule rule in rules)
				{
					xmlWriter.WriteStartElement("CorsRule");
					xmlWriter.WriteElementString("AllowedMethods", AnalyticsSettingsHelper.ConvertListToCommaDelimitedString(rule.AllowedMethods.ToList<string>(), false));
					xmlWriter.WriteElementString("AllowedOrigins", AnalyticsSettingsHelper.ConvertListToCommaDelimitedString(rule.AllowedOrigins.ToList<string>(), false));
					string commaDelimitedString = AnalyticsSettingsHelper.ConvertListToCommaDelimitedString(rule.AllowedLiteralHeaders.ToList<string>(), false);
					string commaDelimitedString1 = AnalyticsSettingsHelper.ConvertListToCommaDelimitedString(rule.AllowedPrefixedHeaders, true);
					if (commaDelimitedString == string.Empty)
					{
						str = commaDelimitedString1;
					}
					else
					{
						str = (commaDelimitedString1 == string.Empty ? commaDelimitedString : string.Concat(commaDelimitedString, ",", commaDelimitedString1));
					}
					xmlWriter.WriteElementString("AllowedHeaders", str);
					commaDelimitedString = AnalyticsSettingsHelper.ConvertListToCommaDelimitedString(rule.ExposedLiteralHeaders.ToList<string>(), false);
					commaDelimitedString1 = AnalyticsSettingsHelper.ConvertListToCommaDelimitedString(rule.ExposedPrefixedHeaders, true);
					if (commaDelimitedString == string.Empty)
					{
						str1 = commaDelimitedString1;
					}
					else
					{
						str1 = (commaDelimitedString1 == string.Empty ? commaDelimitedString : string.Concat(commaDelimitedString, ",", commaDelimitedString1));
					}
					xmlWriter.WriteElementString("ExposedHeaders", str1);
					xmlWriter.WriteElementString("MaxAgeInSeconds", rule.MaxAge.ToString());
					xmlWriter.WriteEndElement();
				}
			}
			xmlWriter.WriteEndElement();
		}

		private static void SerializeFileAnalyticsSettingsToWriter(XmlWriter xmlWriter, AnalyticsSettings settings, AnalyticsSettingsVersion settingsVersion, bool enableMetricsOverride = false)
		{
			bool flag = true;
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("StorageServiceProperties");
			if (flag && settingsVersion >= AnalyticsSettingsVersion.V4)
			{
				xmlWriter.WriteStartElement("HourMetrics");
				xmlWriter.WriteStartElement("Version");
				xmlWriter.WriteValue(string.Format("{0:0.0}", settings.MetricsVersion));
				xmlWriter.WriteEndElement();
				bool metricsType = (settings.MetricsType & MetricsType.ServiceSummary) != MetricsType.None;
				xmlWriter.WriteStartElement("Enabled");
				xmlWriter.WriteValue(metricsType);
				xmlWriter.WriteEndElement();
				if (metricsType)
				{
					bool metricsType1 = (settings.MetricsType & MetricsType.ApiSummary) != MetricsType.None;
					xmlWriter.WriteStartElement("IncludeAPIs");
					xmlWriter.WriteValue(metricsType1);
					xmlWriter.WriteEndElement();
				}
				AnalyticsSettingsHelper.SerializeRetentionPolicy(xmlWriter, settings.IsMetricsRetentionPolicyEnabled, settings.MetricsRetentionInDays);
				xmlWriter.WriteEndElement();
				xmlWriter.WriteStartElement("MinuteMetrics");
				xmlWriter.WriteStartElement("Version");
				xmlWriter.WriteValue(string.Format("{0:0.0}", settings.MinuteMetricsVersion));
				xmlWriter.WriteEndElement();
				metricsType = (settings.MinuteMetricsType & MetricsType.ServiceSummary) != MetricsType.None;
				xmlWriter.WriteStartElement("Enabled");
				xmlWriter.WriteValue(metricsType);
				xmlWriter.WriteEndElement();
				if (metricsType)
				{
					bool minuteMetricsType = (settings.MinuteMetricsType & MetricsType.ApiSummary) != MetricsType.None;
					xmlWriter.WriteStartElement("IncludeAPIs");
					xmlWriter.WriteValue(minuteMetricsType);
					xmlWriter.WriteEndElement();
				}
				XmlWriter xmlWriter1 = xmlWriter;
				bool? isMinuteMetricsRetentionPolicyEnabled = settings.IsMinuteMetricsRetentionPolicyEnabled;
				AnalyticsSettingsHelper.SerializeRetentionPolicy(xmlWriter1, (!isMinuteMetricsRetentionPolicyEnabled.GetValueOrDefault() ? false : isMinuteMetricsRetentionPolicyEnabled.HasValue), settings.MinuteMetricsRetentionInDays);
				xmlWriter.WriteEndElement();
			}
			if (settingsVersion >= AnalyticsSettingsVersion.V3)
			{
				AnalyticsSettingsHelper.serializeCorsRules(xmlWriter, settings.CorsRules);
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
		}

		private static void SerializeRetentionPolicy(XmlWriter xmlWriter, bool isRetentionEnabled, int days)
		{
			xmlWriter.WriteStartElement("RetentionPolicy");
			xmlWriter.WriteStartElement("Enabled");
			xmlWriter.WriteValue(isRetentionEnabled);
			xmlWriter.WriteEndElement();
			if (isRetentionEnabled)
			{
				xmlWriter.WriteStartElement("Days");
				xmlWriter.WriteValue(days);
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
		}

		private static void ThrowDuplicateElementException(string elementName, IXmlLineInfo xmlLineInfo)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { elementName };
			throw new InvalidXmlProtocolException(string.Format(invariantCulture, "Found duplicate element: {0}.", objArray), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
		}

		private static void ThrowMissingElementException(string elementName, IXmlLineInfo xmlLineInfo)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { elementName };
			throw new InvalidXmlProtocolException(string.Format(invariantCulture, "Element is missing: {0}.", objArray), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
		}

		private static void ThrowRedundantElementException(string elementName, string elementShouldBeEnabled, IXmlLineInfo xmlLineInfo)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { elementName, elementShouldBeEnabled };
			throw new InvalidXmlProtocolException(string.Format(invariantCulture, "Element {0} is only expected when {1} is enabled.", objArray), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
		}

		private static void ThrowUnsupportedElementOnFileServiceException(string elementName, IXmlLineInfo xmlLineInfo)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { elementName };
			throw new InvalidXmlProtocolException(string.Format(invariantCulture, "Element {0} not supported for File service.", objArray), xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
		}
	}
}