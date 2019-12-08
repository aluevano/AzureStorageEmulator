using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections;
using System.Globalization;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class ListContainerSummaryResultEncoder : SummaryResultEncoder
	{
		public ListContainerSummaryResultEncoder()
		{
		}

		protected override void EncodeEntry(Uri requestUrl, object loc, SummaryResult result, XmlWriter xmlWriter)
		{
			base.EncodeEntry(requestUrl, loc, result, xmlWriter);
			long totalMetadataSize = result.TotalMetadataSize;
			xmlWriter.WriteElementString("TotalMetadataSize", totalMetadataSize.ToString(CultureInfo.InvariantCulture));
		}

		protected override void EncodeInitialElements(Uri requestUrl, object loc, IEnumerable result, XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("ListContainerSummary");
		}
	}
}