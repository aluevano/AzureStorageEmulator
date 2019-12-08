using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListFileRangesXmlListEncoder : XmlListEncoder<IPageRangeCollection, IPageRange, GetPageRangeListOperationContext>
	{
		private long startOffset;

		private long endOffset;

		public ListFileRangesXmlListEncoder(bool isCompletingManually, long startOffset, long endOffset) : base(isCompletingManually)
		{
			this.startOffset = startOffset;
			this.endOffset = endOffset;
		}

		protected override void EncodeEndElements(XmlWriter xmlWriter, IPageRangeCollection result, GetPageRangeListOperationContext loc)
		{
			if (!result.HasMoreRows || result.NextPageStart >= loc.EndOffset)
			{
				xmlWriter.WriteEndElement();
			}
		}

		protected override void EncodeEntry(Uri requestUrl, GetPageRangeListOperationContext loc, IPageRange pageRange, XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("Range");
			xmlWriter.WriteStartElement("Start");
			xmlWriter.WriteValue((pageRange.PageStart < this.startOffset ? this.startOffset : pageRange.PageStart));
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("End");
			xmlWriter.WriteValue((pageRange.PageEnd > this.endOffset ? this.endOffset : pageRange.PageEnd));
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeInitialElements(Uri requestUrl, GetPageRangeListOperationContext loc, IPageRangeCollection result, XmlWriter xmlWriter)
		{
			if (!loc.IsContinuing)
			{
				xmlWriter.WriteStartElement("Ranges");
			}
		}
	}
}