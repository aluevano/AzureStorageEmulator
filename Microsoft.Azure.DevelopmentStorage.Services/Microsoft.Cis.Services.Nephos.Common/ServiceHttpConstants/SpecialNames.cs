using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class SpecialNames
	{
		public const string RootContainer = "$root";

		public const string LogContainer = "$logs";

		public const string SystemContainer = "$system";

		public const string CoRContainer = "$cor";

		public const string MetricCapacityBlobTable = "$MetricsCapacityBlob";

		public const string MetricCapacityTableTable = "$MetricsCapacityTable";

		public const string MetricCapacityQueueTable = "$MetricsCapacityQueue";

		public const string MetricTransactionBlobTable = "$MetricsTransactionsBlob";

		public const string MetricTransactionTableTable = "$MetricsTransactionsTable";

		public const string MetricTransactionQueueTable = "$MetricsTransactionsQueue";

		public const string MetricRealtimeTransactionBlobTable = "$MetricsRealtimeTransactionsBlob";

		public const string MetricRealtimeTransactionTableTable = "$MetricsRealtimeTransactionsTable";

		public const string MetricRealtimeTransactionQueueTable = "$MetricsRealtimeTransactionsQueue";

		public const string MetricHourPrimaryTransactionBlobTable = "$MetricsHourPrimaryTransactionsBlob";

		public const string MetricHourPrimaryTransactionTableTable = "$MetricsHourPrimaryTransactionsTable";

		public const string MetricHourPrimaryTransactionQueueTable = "$MetricsHourPrimaryTransactionsQueue";

		public const string MetricHourPrimaryTransactionFileTable = "$MetricsHourPrimaryTransactionsFile";

		public const string MetricMinutePrimaryTransactionBlobTable = "$MetricsMinutePrimaryTransactionsBlob";

		public const string MetricMinutePrimaryTransactionTableTable = "$MetricsMinutePrimaryTransactionsTable";

		public const string MetricMinutePrimaryTransactionQueueTable = "$MetricsMinutePrimaryTransactionsQueue";

		public const string MetricMinutePrimaryTransactionFileTable = "$MetricsMinutePrimaryTransactionsFile";

		public const string MetricHourSecondaryTransactionBlobTable = "$MetricsHourSecondaryTransactionsBlob";

		public const string MetricHourSecondaryTransactionTableTable = "$MetricsHourSecondaryTransactionsTable";

		public const string MetricHourSecondaryTransactionQueueTable = "$MetricsHourSecondaryTransactionsQueue";

		public const string MetricHourSecondaryTransactionFileTable = "$MetricsHourSecondaryTransactionsFile";

		public const string MetricMinuteSecondaryTransactionBlobTable = "$MetricsMinuteSecondaryTransactionsBlob";

		public const string MetricMinuteSecondaryTransactionTableTable = "$MetricsMinuteSecondaryTransactionsTable";

		public const string MetricMinuteSecondaryTransactionQueueTable = "$MetricsMinuteSecondaryTransactionsQueue";

		public const string MetricMinuteSecondaryTransactionFileTable = "$MetricsMinuteSecondaryTransactionsFile";

		public const string CustomProbeUrlPathAndQuery = "/$ping";

		public const string SecondaryStampAccountSuffix = "-secondary";

		private const string MaxRestVersion = "9999-99-99";

		private static HashSet<string> SpecialBlobContainerNames;

		public static Dictionary<string, string> DeprecatedTableNamesMapping;

		private static Dictionary<string, string> SpecialTableContainerNames;

		static SpecialNames()
		{
			SpecialNames.SpecialBlobContainerNames = new HashSet<string>();
			SpecialNames.DeprecatedTableNamesMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			SpecialNames.SpecialTableContainerNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			SpecialNames.SpecialBlobContainerNames.Add("$root");
			SpecialNames.SpecialBlobContainerNames.Add("$logs");
			SpecialNames.SpecialBlobContainerNames.Add("$system");
			SpecialNames.SpecialBlobContainerNames.Add("$cor");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsCapacityBlob", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsTransactionsBlob", "2012-02-12");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsTransactionsQueue", "2012-02-12");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsTransactionsTable", "2012-02-12");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsHourPrimaryTransactionsBlob", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsHourPrimaryTransactionsQueue", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsHourPrimaryTransactionsTable", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsHourPrimaryTransactionsFile", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsHourSecondaryTransactionsBlob", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsHourSecondaryTransactionsQueue", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsHourSecondaryTransactionsTable", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsHourSecondaryTransactionsFile", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsRealtimeTransactionsBlob", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsRealtimeTransactionsQueue", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsRealtimeTransactionsTable", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsMinutePrimaryTransactionsBlob", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsMinutePrimaryTransactionsQueue", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsMinutePrimaryTransactionsTable", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsMinutePrimaryTransactionsFile", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsMinuteSecondaryTransactionsBlob", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsMinuteSecondaryTransactionsQueue", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsMinuteSecondaryTransactionsTable", "9999-99-99");
			SpecialNames.SpecialTableContainerNames.Add("$MetricsMinuteSecondaryTransactionsFile", "9999-99-99");
			SpecialNames.DeprecatedTableNamesMapping.Add("$MetricsTransactionsBlob", "$MetricsHourPrimaryTransactionsBlob");
			SpecialNames.DeprecatedTableNamesMapping.Add("$MetricsTransactionsQueue", "$MetricsHourPrimaryTransactionsQueue");
			SpecialNames.DeprecatedTableNamesMapping.Add("$MetricsTransactionsTable", "$MetricsHourPrimaryTransactionsTable");
		}

		public static bool CheckTableContainerNameAllowedForVersion(string containerName, string version, out string newResourceName)
		{
			newResourceName = null;
			if (string.IsNullOrWhiteSpace(containerName))
			{
				return true;
			}
			string str = null;
			SpecialNames.SpecialTableContainerNames.TryGetValue(containerName, out str);
			if (str == null || string.Compare(str, version, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
			string str1 = null;
			SpecialNames.DeprecatedTableNamesMapping.TryGetValue(str, out str1);
			newResourceName = (str1 == null ? string.Empty : str1);
			return false;
		}

		public static string GetCasePreservedMetricsTableName(string tableName)
		{
			return SpecialNames.SpecialTableContainerNames.Keys.FirstOrDefault<string>((string key) => string.Compare(key, tableName, StringComparison.OrdinalIgnoreCase) == 0);
		}

		public static bool IsAnalyticsBlobContainer(string containerName)
		{
			return string.Equals(containerName, "$logs", StringComparison.Ordinal);
		}

		public static bool IsBlobContainerSpecialName(string containerName)
		{
			return SpecialNames.SpecialBlobContainerNames.Contains(containerName);
		}

		public static bool IsSystemBlobContainer(string containerName)
		{
			return string.Equals(containerName, "$system", StringComparison.Ordinal);
		}

		public static bool IsTableContainerSpecialName(string containerName)
		{
			if (string.IsNullOrWhiteSpace(containerName))
			{
				return false;
			}
			return SpecialNames.SpecialTableContainerNames.ContainsKey(containerName);
		}
	}
}