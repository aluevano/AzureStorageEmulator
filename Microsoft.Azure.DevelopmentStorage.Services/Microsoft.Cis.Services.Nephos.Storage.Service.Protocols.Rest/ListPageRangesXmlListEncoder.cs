using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListPageRangesXmlListEncoder : XmlListEncoder<IPageRangeCollection, IPageRange, GetPageRangeListOperationContext>
	{
		public ListPageRangesXmlListEncoder(bool isCompletingManually) : base(isCompletingManually)
		{
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
			if (!pageRange.IsClear)
			{
				xmlWriter.WriteStartElement("PageRange");
			}
			else
			{
				xmlWriter.WriteStartElement("ClearRange");
			}
			xmlWriter.WriteStartElement("Start");
			xmlWriter.WriteValue(pageRange.PageStart);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("End");
			xmlWriter.WriteValue(pageRange.PageEnd);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeInitialElements(Uri requestUrl, GetPageRangeListOperationContext loc, IPageRangeCollection result, XmlWriter xmlWriter)
		{
			if (!loc.IsContinuing)
			{
				xmlWriter.WriteStartElement("PageList");
			}
		}
	}
}