using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListBlocksXmlListEncoder : XmlListEncoder<IEnumerable<IBlock>, IBlock, GetBlockListOperationContext>
	{
		public ListBlocksXmlListEncoder()
		{
		}

		protected override void EncodeEndElements(XmlWriter xmlWriter, IEnumerable<IBlock> result, GetBlockListOperationContext loc)
		{
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeEntry(Uri requestUrl, GetBlockListOperationContext loc, IBlock block, XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("Block");
			xmlWriter.WriteStartElement("Name");
			xmlWriter.WriteValue(Convert.ToBase64String(block.Identifier));
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("Size");
			xmlWriter.WriteValue(block.Length);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeInitialElements(Uri requestUrl, GetBlockListOperationContext loc, IEnumerable<IBlock> result, XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement(loc.BlockListType);
		}
	}
}