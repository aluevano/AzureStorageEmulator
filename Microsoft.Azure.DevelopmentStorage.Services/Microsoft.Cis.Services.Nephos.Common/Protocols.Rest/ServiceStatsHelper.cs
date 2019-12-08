using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public static class ServiceStatsHelper
	{
		private const string RootPropertiesElementName = "StorageServiceStats";

		private const string GeoReplicationElementName = "GeoReplication";

		private const string StatusElementName = "Status";

		private const string LastSyncTimeElementName = "LastSyncTime";

		static ServiceStatsHelper()
		{
		}

		public static void SerializeServiceStatsToWriter(XmlWriter xmlWriter, GeoReplicationStats stats)
		{
			string lowerInvariant = GeoReplicationStatus.Unavailable.ToString().ToLowerInvariant();
			string empty = string.Empty;
			if (stats == null)
			{
				ServiceStatsHelper.SerializeServiceStatsToXmlWriter(xmlWriter, lowerInvariant, empty);
				return;
			}
			if (stats.Status.HasValue && stats.Status.Value != GeoReplicationStatus.Invalid && stats.Status.Value != GeoReplicationStatus.Repair && (stats.Status.Value != GeoReplicationStatus.Live || stats.LastSyncTime.HasValue) && (stats.Status.Value == GeoReplicationStatus.Unavailable || stats.Status.Value == GeoReplicationStatus.Live || stats.Status.Value == GeoReplicationStatus.Bootstrap))
			{
				lowerInvariant = stats.Status.ToString().ToLowerInvariant();
				if (stats.LastSyncTime.HasValue)
				{
					empty = HttpUtilities.ConvertDateTimeToHttpString(stats.LastSyncTime.Value);
				}
			}
			ServiceStatsHelper.SerializeServiceStatsToXmlWriter(xmlWriter, lowerInvariant, empty);
		}

		private static void SerializeServiceStatsToXmlWriter(XmlWriter xmlWriter, string geoReplicationStatus, string lastSyncTime)
		{
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("StorageServiceStats");
			xmlWriter.WriteStartElement("GeoReplication");
			xmlWriter.WriteStartElement("Status");
			xmlWriter.WriteValue(geoReplicationStatus);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("LastSyncTime");
			xmlWriter.WriteValue(lastSyncTime);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
		}
	}
}