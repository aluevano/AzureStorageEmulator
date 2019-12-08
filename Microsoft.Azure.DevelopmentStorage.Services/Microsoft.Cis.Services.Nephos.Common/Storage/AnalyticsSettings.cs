using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.XLogging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[DataContract]
	[Serializable]
	public class AnalyticsSettings : ICloneable
	{
		private const char delimiter = '\u0001';

		private const char fieldDelimiter = '\u0002';

		private const string corsVersion1 = "V1";

		[DataMember]
		public static double Version;

		private string corsRulesSerializedString;

		private List<CorsRule> corsRules;

		public static string WildCard;

		public bool LoggingElementRead;

		public bool MetricsElementRead;

		public bool MinuteMetricsElementRead;

		public bool DefaultServiceVersionElementRead;

		public bool CorsRulesElementRead;

		private Microsoft.Cis.Services.Nephos.Common.Storage.MetricsType metricsType;

		private Microsoft.Cis.Services.Nephos.Common.Storage.MetricsType minuteMetricsType;

		public bool CorsEnabled
		{
			get
			{
				if (this.corsRulesSerializedString != null)
				{
					return true;
				}
				if (this.corsRules == null)
				{
					return false;
				}
				return this.corsRules.Count > 0;
			}
		}

		[DataMember]
		public List<CorsRule> CorsRules
		{
			get
			{
				int num;
				if (this.corsRules != null && this.corsRules.Count == 0 && !string.IsNullOrEmpty(this.corsRulesSerializedString))
				{
					List<CorsRule> corsRules = new List<CorsRule>();
					string[] strArrays = this.corsRulesSerializedString.Split(new char[] { '\u0002' });
					string str = strArrays[0];
					int num1 = 0;
					if (str == "V1")
					{
						int length = (int)strArrays.Length - 1;
						num1 = 7;
						if (length % num1 != 0)
						{
							Logger<IRestProtocolHeadLogger>.Instance.Error.Log("CORSError: Invalid number of ruleParts , the serialized string is corrupted, corsRuleSerializedString={0}", new object[] { this.corsRulesSerializedString });
							return this.corsRules;
						}
					}
					for (int i = 0; i < (int)strArrays.Length / num1; i++)
					{
						CorsRule corsRule = new CorsRule();
						int num2 = i * num1 + 1;
						corsRule.AllowedOrigins = new HashSet<string>();
						int num3 = num2;
						num2 = num3 + 1;
						this.FillCollectionFromString(corsRule.AllowedOrigins, strArrays[num3], '\u0001');
						corsRule.AllowedMethods = new HashSet<string>();
						int num4 = num2;
						num2 = num4 + 1;
						this.FillCollectionFromString(corsRule.AllowedMethods, strArrays[num4], '\u0001');
						corsRule.AllowedLiteralHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						int num5 = num2;
						num2 = num5 + 1;
						this.FillCollectionFromString(corsRule.AllowedLiteralHeaders, strArrays[num5], '\u0001');
						corsRule.AllowedPrefixedHeaders = new List<string>();
						int num6 = num2;
						num2 = num6 + 1;
						this.FillCollectionFromString(corsRule.AllowedPrefixedHeaders, strArrays[num6], '\u0001');
						corsRule.ExposedLiteralHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						int num7 = num2;
						num2 = num7 + 1;
						this.FillCollectionFromString(corsRule.ExposedLiteralHeaders, strArrays[num7], '\u0001');
						corsRule.ExposedPrefixedHeaders = new List<string>();
						int num8 = num2;
						num2 = num8 + 1;
						this.FillCollectionFromString(corsRule.ExposedPrefixedHeaders, strArrays[num8], '\u0001');
						if (!int.TryParse(strArrays[num2], out num))
						{
							IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
							object[] objArray = new object[] { strArrays[i], this.corsRulesSerializedString };
							error.Log("CORSError: Failed to parse max age for CORS rule, maxAge:{0} , CorsRulesSerializedString:{1}", objArray);
							return null;
						}
						corsRule.MaxAge = num;
						corsRules.Add(corsRule);
					}
					this.corsRules = corsRules;
				}
				return this.corsRules;
			}
			set
			{
				this.corsRules = value;
			}
		}

		[DataMember]
		public string CorsRulesSerializedString
		{
			get
			{
				if (this.corsRulesSerializedString == null && this.corsRules != null && this.corsRules.Count > 0)
				{
					StringBuilder stringBuilder = new StringBuilder("V1");
					stringBuilder.Append('\u0002');
					for (int i = 0; i < this.corsRules.Count; i++)
					{
						this.AppendSerializedString(this.corsRules[i].AllowedOrigins, stringBuilder, '\u0001', '\u0002');
						this.AppendSerializedString(this.corsRules[i].AllowedMethods, stringBuilder, '\u0001', '\u0002');
						this.AppendSerializedString(this.corsRules[i].AllowedLiteralHeaders, stringBuilder, '\u0001', '\u0002');
						this.AppendSerializedString(this.corsRules[i].AllowedPrefixedHeaders, stringBuilder, '\u0001', '\u0002');
						this.AppendSerializedString(this.corsRules[i].ExposedLiteralHeaders, stringBuilder, '\u0001', '\u0002');
						this.AppendSerializedString(this.corsRules[i].ExposedPrefixedHeaders, stringBuilder, '\u0001', '\u0002');
						int maxAge = this.corsRules[i].MaxAge;
						stringBuilder.Append(maxAge.ToString());
						if (i != this.corsRules.Count - 1)
						{
							stringBuilder.Append('\u0002');
						}
					}
					this.corsRulesSerializedString = stringBuilder.ToString();
				}
				return this.corsRulesSerializedString;
			}
			set
			{
				this.corsRulesSerializedString = value;
			}
		}

		[DataMember]
		public string DefaultRESTVersion
		{
			get;
			set;
		}

		[DataMember]
		public bool DeleteCorsRules
		{
			get;
			set;
		}

		[DataMember]
		public bool IsAnalyticsVersionAtLeastV3
		{
			get;
			set;
		}

		[DataMember]
		public bool IsAnalyticsVersionAtLeastV5
		{
			get;
			set;
		}

		[DataMember]
		public bool IsAnalyticsVersionAtLeastV6
		{
			get;
			set;
		}

		[DataMember]
		public bool IsLogRetentionPolicyEnabled
		{
			get;
			set;
		}

		[DataMember]
		public bool IsMetricsRetentionPolicyEnabled
		{
			get;
			set;
		}

		[DataMember]
		public bool? IsMinuteMetricsRetentionPolicyEnabled
		{
			get;
			set;
		}

		public bool LoggingEnabled
		{
			get
			{
				return this.LogType != LoggingLevel.None;
			}
		}

		[DataMember]
		public int LogRetentionInDays
		{
			get;
			set;
		}

		[DataMember]
		public LoggingLevel LogType
		{
			get;
			set;
		}

		[DataMember]
		public double LogVersion
		{
			get;
			set;
		}

		public bool MetricsEnabled
		{
			get
			{
				return this.MetricsType != Microsoft.Cis.Services.Nephos.Common.Storage.MetricsType.None;
			}
		}

		[DataMember]
		public int MetricsRetentionInDays
		{
			get;
			set;
		}

		[DataMember]
		public Microsoft.Cis.Services.Nephos.Common.Storage.MetricsType MetricsType
		{
			get
			{
				return this.metricsType;
			}
			set
			{
				if (value == Microsoft.Cis.Services.Nephos.Common.Storage.MetricsType.ApiSummary)
				{
					throw new ArgumentException("Including just ApiSummary is invalid.");
				}
				this.metricsType = value;
			}
		}

		[DataMember]
		public double MetricsVersion
		{
			get;
			set;
		}

		public bool MinuteMetricsEnabled
		{
			get
			{
				return this.MinuteMetricsType != Microsoft.Cis.Services.Nephos.Common.Storage.MetricsType.None;
			}
		}

		[DataMember]
		public int MinuteMetricsRetentionInDays
		{
			get;
			set;
		}

		[DataMember]
		public Microsoft.Cis.Services.Nephos.Common.Storage.MetricsType MinuteMetricsType
		{
			get
			{
				return this.minuteMetricsType;
			}
			set
			{
				if (value == Microsoft.Cis.Services.Nephos.Common.Storage.MetricsType.ApiSummary)
				{
					throw new ArgumentException("Including just ApiSummary is invalid.");
				}
				this.minuteMetricsType = value;
			}
		}

		[DataMember]
		public double MinuteMetricsVersion
		{
			get;
			set;
		}

		static AnalyticsSettings()
		{
			AnalyticsSettings.Version = 1;
			AnalyticsSettings.WildCard = "*";
		}

		public AnalyticsSettings()
		{
			this.LogType = LoggingLevel.None;
			this.LogVersion = AnalyticsSettings.Version;
			this.IsLogRetentionPolicyEnabled = false;
			this.LogRetentionInDays = 0;
			this.MetricsType = Microsoft.Cis.Services.Nephos.Common.Storage.MetricsType.None;
			this.MetricsVersion = AnalyticsSettings.Version;
			this.IsMetricsRetentionPolicyEnabled = false;
			this.MetricsRetentionInDays = 0;
			this.MinuteMetricsType = Microsoft.Cis.Services.Nephos.Common.Storage.MetricsType.None;
			this.MinuteMetricsVersion = AnalyticsSettings.Version;
			this.IsMinuteMetricsRetentionPolicyEnabled = null;
			this.MinuteMetricsRetentionInDays = 0;
			this.corsRules = new List<CorsRule>();
			this.DefaultRESTVersion = null;
		}

		private void AppendSerializedString(ICollection<string> collection, StringBuilder serializer, char firstDelimiter, char secondDelimiter)
		{
			int count = collection.Count;
			foreach (string str in collection)
			{
				serializer.Append(str);
				int num = count - 1;
				count = num;
				if (num <= 0)
				{
					continue;
				}
				serializer.Append(firstDelimiter);
			}
			serializer.Append(secondDelimiter);
		}

		public object Clone()
		{
			AnalyticsSettings analyticsSetting = new AnalyticsSettings()
			{
				LogType = this.LogType,
				LogVersion = this.LogVersion,
				IsLogRetentionPolicyEnabled = this.IsLogRetentionPolicyEnabled,
				LogRetentionInDays = this.LogRetentionInDays,
				MetricsType = this.MetricsType,
				MetricsVersion = this.MetricsVersion,
				IsMetricsRetentionPolicyEnabled = this.IsMetricsRetentionPolicyEnabled,
				MetricsRetentionInDays = this.MetricsRetentionInDays,
				MinuteMetricsType = this.MinuteMetricsType,
				MinuteMetricsVersion = this.MinuteMetricsVersion,
				IsMinuteMetricsRetentionPolicyEnabled = this.IsMinuteMetricsRetentionPolicyEnabled,
				MinuteMetricsRetentionInDays = this.MinuteMetricsRetentionInDays,
				corsRules = new List<CorsRule>()
			};
			foreach (CorsRule corsRule in this.corsRules)
			{
				analyticsSetting.corsRules.Add((CorsRule)corsRule.Clone());
			}
			analyticsSetting.corsRulesSerializedString = this.corsRulesSerializedString;
			analyticsSetting.DefaultRESTVersion = this.DefaultRESTVersion;
			return analyticsSetting;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			AnalyticsSettings analyticsSetting = (AnalyticsSettings)obj;
			if (this.LogVersion == analyticsSetting.LogVersion && this.LogType == analyticsSetting.LogType && this.IsLogRetentionPolicyEnabled == analyticsSetting.IsLogRetentionPolicyEnabled && (this.LogRetentionInDays == analyticsSetting.LogRetentionInDays || !this.IsLogRetentionPolicyEnabled) && this.MetricsVersion == analyticsSetting.MetricsVersion && this.MetricsType == analyticsSetting.MetricsType && this.IsMetricsRetentionPolicyEnabled == analyticsSetting.IsMetricsRetentionPolicyEnabled && (this.MetricsRetentionInDays == analyticsSetting.MetricsRetentionInDays || !this.IsMetricsRetentionPolicyEnabled) && this.MinuteMetricsVersion == analyticsSetting.MinuteMetricsVersion && this.MinuteMetricsType == analyticsSetting.MinuteMetricsType)
			{
				bool? isMinuteMetricsRetentionPolicyEnabled = this.IsMinuteMetricsRetentionPolicyEnabled;
				bool? nullable = analyticsSetting.IsMinuteMetricsRetentionPolicyEnabled;
				if ((isMinuteMetricsRetentionPolicyEnabled.GetValueOrDefault() != nullable.GetValueOrDefault() ? false : isMinuteMetricsRetentionPolicyEnabled.HasValue == nullable.HasValue))
				{
					if (this.MinuteMetricsRetentionInDays != analyticsSetting.MinuteMetricsRetentionInDays)
					{
						bool? isMinuteMetricsRetentionPolicyEnabled1 = this.IsMinuteMetricsRetentionPolicyEnabled;
						if ((!isMinuteMetricsRetentionPolicyEnabled1.GetValueOrDefault() ? false : isMinuteMetricsRetentionPolicyEnabled1.HasValue))
						{
							return false;
						}
					}
					if (this.DefaultRESTVersion == analyticsSetting.DefaultRESTVersion)
					{
						return this.CorsRulesSerializedString == analyticsSetting.CorsRulesSerializedString;
					}
				}
			}
			return false;
		}

		private void FillCollectionFromString(ICollection<string> collection, string originalString, char delimiter)
		{
			char[] chrArray = new char[] { delimiter };
			string[] strArrays = originalString.Split(chrArray, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				collection.Add(strArrays[i]);
			}
		}

		public override int GetHashCode()
		{
			return this.GetHashCode();
		}
	}
}