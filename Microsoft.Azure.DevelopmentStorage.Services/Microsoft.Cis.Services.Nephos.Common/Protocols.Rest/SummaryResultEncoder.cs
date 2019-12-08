using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public abstract class SummaryResultEncoder : XmlListEncoder<IEnumerable, SummaryResult, object>
	{
		protected SummaryResultEncoder()
		{
		}

		protected override void EncodeEndElements(XmlWriter xmlWriter, IEnumerable result, object loc)
		{
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeEntry(Uri requestUrl, object loc, SummaryResult result, XmlWriter xmlWriter)
		{
			xmlWriter.WriteElementString("Count", result.Count.ToString());
			xmlWriter.WriteElementString("NextMarker", result.NextMarker);
		}
	}
}