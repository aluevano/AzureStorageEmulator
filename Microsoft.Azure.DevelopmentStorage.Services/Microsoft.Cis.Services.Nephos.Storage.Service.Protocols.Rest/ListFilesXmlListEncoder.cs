using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListFilesXmlListEncoder : XmlListEncoder<IListFilesResultCollection, IListFileResultsFileProperties, ListFilesOperationContext>
	{
		private NephosUriComponents uriComponents;

		private DateTime shareSnapshot = DateTime.MaxValue;

		public ListFilesXmlListEncoder(NephosUriComponents uriComponents, DateTime? shareSnapshot)
		{
			this.uriComponents = uriComponents;
			this.shareSnapshot = (shareSnapshot.HasValue ? shareSnapshot.Value : DateTime.MaxValue);
		}

		protected override void EncodeEndElements(XmlWriter xmlWriter, IListFilesResultCollection result, ListFilesOperationContext operationContext)
		{
			xmlWriter.WriteEndElement();
			xmlWriter.WriteElementString("NextMarker", result.NextMarker);
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeEntry(Uri requestUrl, ListFilesOperationContext operationContext, IListFileResultsFileProperties fileProps, XmlWriter xmlWriter)
		{
			string str;
			xmlWriter.WriteStartElement((fileProps.IsFile ? "File" : "Directory"));
			xmlWriter.WriteElementString("Name", fileProps.FileOrDirectoryName);
			xmlWriter.WriteStartElement("Properties");
			if (fileProps.IsFile)
			{
				XmlWriter xmlWriter1 = xmlWriter;
				if (fileProps.ContentLength.HasValue)
				{
					str = fileProps.ContentLength.Value.ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					str = null;
				}
				xmlWriter1.WriteElementString("Content-Length", str);
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeInitialElements(Uri requestUrl, ListFilesOperationContext operationContext, IListFilesResultCollection result, XmlWriter xmlWriter)
		{
			string serviceEndpointFromUri = this.GetServiceEndpointFromUri(requestUrl, this.uriComponents.IsUriPathStyle);
			xmlWriter.WriteStartElement("EnumerationResults");
			xmlWriter.WriteAttributeString("ServiceEndpoint", serviceEndpointFromUri);
			xmlWriter.WriteAttributeString("ShareName", this.uriComponents.ContainerName);
			if (this.shareSnapshot != DateTime.MaxValue)
			{
				xmlWriter.WriteAttributeString("ShareSnapshot", HttpUtilities.ConvertSnapshotDateTimeToHttpString(this.shareSnapshot));
			}
			xmlWriter.WriteAttributeString("DirectoryPath", this.uriComponents.RemainingPart);
			XmlListEncoderHelpers.WriteListOperationInfoToXml(xmlWriter, operationContext);
			xmlWriter.WriteStartElement("Entries");
		}

		private string GetServiceEndpointFromUri(Uri requestUrl, bool isPathStyle)
		{
			int i;
			string absoluteUri = requestUrl.AbsoluteUri;
			int num = (isPathStyle ? 4 : 3);
			for (i = 0; i < absoluteUri.Length && num > 0; i++)
			{
				if (absoluteUri[i] == "/"[0])
				{
					num--;
				}
			}
			if (num > 0)
			{
				throw new ArgumentException(string.Format("AbsoluteUrl '{0}' has unexpected number of slashes", absoluteUri));
			}
			return absoluteUri.Substring(0, i);
		}
	}
}