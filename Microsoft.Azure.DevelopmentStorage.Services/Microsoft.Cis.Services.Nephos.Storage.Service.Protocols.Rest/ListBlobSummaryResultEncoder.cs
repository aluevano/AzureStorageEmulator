using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.Collections;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListBlobSummaryResultEncoder : SummaryResultEncoder
	{
		public ListBlobSummaryResultEncoder()
		{
		}

		protected override void EncodeEntry(Uri requestUrl, object loc, SummaryResult result, XmlWriter xmlWriter)
		{
			ListBlobSummaryResult listBlobSummaryResult = result as ListBlobSummaryResult;
			xmlWriter.WriteElementString("Count", listBlobSummaryResult.Count.ToString());
			xmlWriter.WriteElementString("TotalContentSize", listBlobSummaryResult.TotalContentSize.ToString());
			xmlWriter.WriteElementString("NextContainerName", listBlobSummaryResult.NextContainerName);
			xmlWriter.WriteElementString("NextBlobName", listBlobSummaryResult.NextBlobName);
		}

		protected override void EncodeInitialElements(Uri requestUrl, object loc, IEnumerable result, XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("ListBlobSummary");
		}
	}
}