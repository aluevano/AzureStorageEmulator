using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	internal static class XmlListEncoderHelpers
	{
		public static void WriteListOperationInfoToXml(XmlWriter xmlWriter, ListOperationContext loc)
		{
			if (!string.IsNullOrEmpty(loc.Prefix))
			{
				xmlWriter.WriteElementString("Prefix", loc.Prefix);
			}
			if (!string.IsNullOrEmpty(loc.Marker))
			{
				xmlWriter.WriteElementString("Marker", loc.Marker);
			}
			if (loc.MaxResults.HasValue)
			{
				int value = loc.MaxResults.Value;
				xmlWriter.WriteElementString("MaxResults", value.ToString(CultureInfo.InvariantCulture));
			}
			if (!string.IsNullOrEmpty(loc.Delimiter))
			{
				xmlWriter.WriteElementString("Delimiter", loc.Delimiter);
			}
		}
	}
}