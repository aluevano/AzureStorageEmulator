using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListContainersXmlListEncoder : XmlListEncoder<IListContainersResultCollection, IListContainersResultContainerProperties, ListContainersOperationContext>
	{
		protected bool shouldEncloseEtagsInQuotes;

		protected string XmlContainerElementName;

		protected string XmlContainersElementName;

		protected ListContainersXmlListEncoder()
		{
		}

		public ListContainersXmlListEncoder(bool shouldEncloseEtagsInQuotes)
		{
			this.shouldEncloseEtagsInQuotes = shouldEncloseEtagsInQuotes;
			this.XmlContainerElementName = "Container";
			this.XmlContainersElementName = "Containers";
		}

		protected override void EncodeEndElements(XmlWriter xmlWriter, IListContainersResultCollection result, ListContainersOperationContext loc)
		{
			xmlWriter.WriteEndElement();
			xmlWriter.WriteElementString("NextMarker", result.NextMarker);
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeEntry(Uri requestUrl, ListContainersOperationContext loc, IListContainersResultContainerProperties collProps, XmlWriter xmlWriter)
		{
			string httpString;
			string eTag;
			xmlWriter.WriteStartElement(this.XmlContainerElementName);
			xmlWriter.WriteElementString("Name", collProps.ContainerName);
			if (loc.IsIncludingUrlInResponse)
			{
				string str = string.Concat(HttpRequestAccessorCommon.TrimEndSlash(requestUrl.GetLeftPart(UriPartial.Path)), "/", collProps.ContainerName);
				xmlWriter.WriteElementString("Url", str);
			}
			if (loc.IsUsingPropertiesElement)
			{
				xmlWriter.WriteStartElement("Properties");
			}
			string str1 = (loc.IsUsingPropertiesElement ? "Last-Modified" : "LastModified");
			XmlWriter xmlWriter1 = xmlWriter;
			string str2 = str1;
			if (collProps.LastModifiedTime.HasValue)
			{
				httpString = HttpUtilities.ConvertDateTimeToHttpString(collProps.LastModifiedTime.Value);
			}
			else
			{
				httpString = null;
			}
			xmlWriter1.WriteElementString(str2, httpString);
			XmlWriter xmlWriter2 = xmlWriter;
			if (collProps.LastModifiedTime.HasValue)
			{
				DateTime? lastModifiedTime = collProps.LastModifiedTime;
				eTag = BasicHttpProcessor.GetETag(lastModifiedTime.Value, this.shouldEncloseEtagsInQuotes);
			}
			else
			{
				eTag = null;
			}
			xmlWriter2.WriteElementString("Etag", eTag);
			if (loc.IsIncludingLeaseStateAndDurationInResponse)
			{
				if (!string.IsNullOrEmpty(collProps.LeaseStatus))
				{
					xmlWriter.WriteElementString("LeaseStatus", collProps.LeaseStatus);
				}
				if (!string.IsNullOrEmpty(collProps.LeaseState))
				{
					xmlWriter.WriteElementString("LeaseState", collProps.LeaseState);
				}
				if (!string.IsNullOrEmpty(collProps.LeaseDuration))
				{
					xmlWriter.WriteElementString("LeaseDuration", collProps.LeaseDuration);
				}
			}
			if (loc.IsIncludingShareQuotaInResponse)
			{
				long containerQuotaInGB = collProps.ContainerQuotaInGB;
				xmlWriter.WriteElementString("Quota", containerQuotaInGB.ToString(CultureInfo.InvariantCulture));
			}
			if (loc.IsIncludingPublicAccessInResponse && !string.IsNullOrEmpty(collProps.PublicAccessLevel))
			{
				if (Comparison.StringEqualsIgnoreCase(collProps.PublicAccessLevel, "container") || Comparison.StringEqualsIgnoreCase(collProps.PublicAccessLevel, bool.TrueString))
				{
					xmlWriter.WriteElementString("PublicAccess", "container");
				}
				else if (Comparison.StringEqualsIgnoreCase(collProps.PublicAccessLevel, "blob"))
				{
					xmlWriter.WriteElementString("PublicAccess", "blob");
				}
			}
			if (loc.IsUsingPropertiesElement)
			{
				xmlWriter.WriteEndElement();
			}
			if (loc.IsFetchingMetadata)
			{
				MetadataEncoding.WriteMetadataToXml(xmlWriter, collProps.Metadata, true, loc.RequestVersion);
			}
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeInitialElements(Uri requestUrl, ListContainersOperationContext loc, IListContainersResultCollection result, XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("EnumerationResults");
			if (!VersioningHelper.IsPreAugust13OrInvalidVersion(loc.RequestVersion))
			{
				string str = string.Concat(HttpRequestAccessorCommon.TrimEndSlash(requestUrl.GetLeftPart(UriPartial.Path)), "/");
				xmlWriter.WriteAttributeString("ServiceEndpoint", str);
			}
			else
			{
				xmlWriter.WriteAttributeString("AccountName", requestUrl.GetLeftPart(UriPartial.Path));
			}
			XmlListEncoderHelpers.WriteListOperationInfoToXml(xmlWriter, loc);
			xmlWriter.WriteStartElement(this.XmlContainersElementName);
		}
	}
}