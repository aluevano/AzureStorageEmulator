using Microsoft.Cis.Services.Nephos.Common.Authentication;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class ListContainersAclXmlListEncoder : XmlListEncoder<IEnumerable<SASIdentifier>, SASIdentifier, object>
	{
		private bool encodeSASPermissionWithOrder;

		public ListContainersAclXmlListEncoder(bool encodeSASPermissionWithOrder)
		{
			this.encodeSASPermissionWithOrder = encodeSASPermissionWithOrder;
		}

		protected override void EncodeEndElements(XmlWriter xmlWriter, IEnumerable<SASIdentifier> result, object loc)
		{
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeEntry(Uri uri, object obj, SASIdentifier sasIdentifier, XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("SignedIdentifier");
			xmlWriter.WriteElementString("Id", sasIdentifier.Id);
			if (sasIdentifier.AccessPolicy.SignedStart.HasValue || sasIdentifier.AccessPolicy.SignedExpiry.HasValue || sasIdentifier.AccessPolicy.SignedPermission.HasValue)
			{
				xmlWriter.WriteStartElement("AccessPolicy");
				if (sasIdentifier.AccessPolicy.SignedStart.HasValue)
				{
					DateTime? signedStart = sasIdentifier.AccessPolicy.SignedStart;
					xmlWriter.WriteElementString("Start", SASUtilities.EncodeTime(signedStart.Value));
				}
				if (sasIdentifier.AccessPolicy.SignedExpiry.HasValue)
				{
					DateTime? signedExpiry = sasIdentifier.AccessPolicy.SignedExpiry;
					xmlWriter.WriteElementString("Expiry", SASUtilities.EncodeTime(signedExpiry.Value));
				}
				if (sasIdentifier.AccessPolicy.SignedPermission.HasValue)
				{
					xmlWriter.WriteElementString("Permission", (this.encodeSASPermissionWithOrder ? SASUtilities.EncodeSASPermissionWithOrder(sasIdentifier.AccessPolicy.SignedPermission.Value) : SASUtilities.EncodeSASPermission(sasIdentifier.AccessPolicy.SignedPermission.Value)));
				}
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeInitialElements(Uri requestUrl, object obj, IEnumerable<SASIdentifier> result, XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("SignedIdentifiers");
		}
	}
}