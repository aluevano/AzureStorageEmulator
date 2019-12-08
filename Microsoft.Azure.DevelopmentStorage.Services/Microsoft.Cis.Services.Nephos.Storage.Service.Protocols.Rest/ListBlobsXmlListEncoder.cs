using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Globalization;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListBlobsXmlListEncoder : XmlListEncoder<IListBlobsResultCollection, IListBlobResultsBlobProperties, ListBlobsOperationContext>
	{
		private bool shouldEncloseEtagsInQuotes;

		private bool obfuscateSourceUri;

		public ListBlobsXmlListEncoder(bool shouldEncloseEtagsInQuotes, bool obfuscateSourceUri = false)
		{
			this.shouldEncloseEtagsInQuotes = shouldEncloseEtagsInQuotes;
			this.obfuscateSourceUri = obfuscateSourceUri;
		}

		protected override void EncodeEndElements(XmlWriter xmlWriter, IListBlobsResultCollection result, ListBlobsOperationContext lboc)
		{
			xmlWriter.WriteEndElement();
			xmlWriter.WriteElementString("NextMarker", result.NextMarker);
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeEntry(Uri requestUrl, ListBlobsOperationContext lboc, IListBlobResultsBlobProperties blobProps, XmlWriter xmlWriter)
		{
			string str;
			string httpString;
			string eTag;
			if (!blobProps.IsActualBlob)
			{
				xmlWriter.WriteStartElement("BlobPrefix");
				this.WriteElementString(xmlWriter, "Name", blobProps.BlobName);
			}
			else
			{
				xmlWriter.WriteStartElement("Blob");
				this.WriteElementString(xmlWriter, "Name", blobProps.BlobName);
				if (lboc.IsIncludingSnapshots && blobProps.Snapshot != DateTime.MaxValue)
				{
					this.WriteElementString(xmlWriter, "Snapshot", HttpUtilities.ConvertSnapshotDateTimeToHttpString(blobProps.Snapshot));
				}
				if (lboc.ListingAcrossContainers)
				{
					NephosAssertionException.Assert(!string.IsNullOrEmpty(blobProps.ContainerName), string.Concat("XStore didn't return us valid ContainerName for this blob ", blobProps.BlobName, " when we are listing blobs across the account"));
					this.WriteElementString(xmlWriter, "ContainerName", blobProps.ContainerName);
				}
				if (lboc.IsIncludingUrlInResponse)
				{
					string empty = string.Empty;
					if (lboc.ListingAcrossContainers)
					{
						empty = string.Concat("/", blobProps.ContainerName);
					}
					string str1 = string.Concat(HttpRequestAccessorCommon.TrimEndSlash(requestUrl.GetLeftPart(UriPartial.Path)), empty);
					string str2 = HttpRequestAccessorCommon.TrimRootContainerNameFromEnd(str1, true);
					string str3 = string.Concat(str2, "/", blobProps.BlobName);
					if (lboc.IsIncludingSnapshots && blobProps.Snapshot != DateTime.MaxValue)
					{
						str3 = string.Concat(str3, HttpUtilities.GetSnapshotQueryParameterStringForUrl(blobProps.Snapshot));
					}
					this.WriteElementString(xmlWriter, "Url", str3);
				}
				if (lboc.IsUsingPropertiesElement)
				{
					xmlWriter.WriteStartElement("Properties");
				}
				string str4 = (lboc.IsUsingPropertiesElement ? "Last-Modified" : "LastModified");
				string str5 = (lboc.IsUsingPropertiesElement ? "Content-Length" : "Size");
				string str6 = (lboc.IsUsingPropertiesElement ? "Content-Type" : "ContentType");
				string str7 = (lboc.IsUsingPropertiesElement ? "Content-Encoding" : "ContentEncoding");
				string str8 = (lboc.IsUsingPropertiesElement ? "Content-Language" : "ContentLanguage");
				string str9 = (lboc.IsUsingPropertiesElement ? "Cache-Control" : "CacheControl");
				string str10 = (lboc.IsUsingPropertiesElement ? "Content-MD5" : "ContentMD5");
				bool value = blobProps.LastModifiedTime.Value > DateTimeConstants.MinimumBlobLastModificationTime;
				if (value)
				{
					XmlWriter xmlWriter1 = xmlWriter;
					string str11 = str4;
					if (blobProps.LastModifiedTime.HasValue)
					{
						httpString = HttpUtilities.ConvertDateTimeToHttpString(blobProps.LastModifiedTime.Value);
					}
					else
					{
						httpString = null;
					}
					this.WriteElementString(xmlWriter1, str11, httpString);
					XmlWriter xmlWriter2 = xmlWriter;
					if (blobProps.LastModifiedTime.HasValue)
					{
						DateTime? lastModifiedTime = blobProps.LastModifiedTime;
						eTag = BasicHttpProcessor.GetETag(lastModifiedTime.Value, this.shouldEncloseEtagsInQuotes);
					}
					else
					{
						eTag = null;
					}
					this.WriteElementString(xmlWriter2, "Etag", eTag);
				}
				XmlWriter xmlWriter3 = xmlWriter;
				string str12 = str5;
				if (blobProps.ContentLength.HasValue)
				{
					str = blobProps.ContentLength.Value.ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					str = null;
				}
				this.WriteElementString(xmlWriter3, str12, str);
				if (value)
				{
					this.WriteElementString(xmlWriter, str6, blobProps.ContentType);
					this.WriteElementString(xmlWriter, str7, blobProps.ContentEncoding);
					this.WriteElementString(xmlWriter, str8, blobProps.ContentLanguage);
					if (lboc.IsIncludingCrc64InResponse)
					{
						this.WriteElementString(xmlWriter, "Content-CRC64", blobProps.ContentCrc64);
					}
					if (lboc.IsUsingPropertiesElement)
					{
						this.WriteElementString(xmlWriter, str10, blobProps.ContentMD5);
					}
					if (lboc.IsIncludingCacheControlInResponse)
					{
						this.WriteElementString(xmlWriter, str9, blobProps.CacheControl);
					}
					if (lboc.IsIncludingContentDispositionInResponse)
					{
						this.WriteElementString(xmlWriter, "Content-Disposition", blobProps.ContentDisposition);
					}
				}
				if (lboc.IsIncludingBlobTypeInResponse)
				{
					if (blobProps.SequenceNumber.HasValue)
					{
						long num = blobProps.SequenceNumber.Value;
						this.WriteElementString(xmlWriter, "x-ms-blob-sequence-number", num.ToString());
					}
					this.WriteElementString(xmlWriter, "BlobType", blobProps.BlobType);
				}
				if (lboc.IsIncludingLeaseStatusInResponse && blobProps.Snapshot == DateTime.MaxValue)
				{
					this.WriteElementString(xmlWriter, "LeaseStatus", blobProps.LeaseStatus);
					if (lboc.IsIncludingLeaseStateAndDurationInResponse)
					{
						if (!string.IsNullOrEmpty(blobProps.LeaseState))
						{
							this.WriteElementString(xmlWriter, "LeaseState", blobProps.LeaseState);
						}
						if (!string.IsNullOrEmpty(blobProps.LeaseDuration))
						{
							this.WriteElementString(xmlWriter, "LeaseDuration", blobProps.LeaseDuration);
						}
					}
				}
				if (lboc.IsIncludingCopyPropertiesInResponse)
				{
					if (!string.IsNullOrEmpty(blobProps.CopyId))
					{
						this.WriteElementString(xmlWriter, "CopyId", blobProps.CopyId);
					}
					if (!string.IsNullOrEmpty(blobProps.CopySource))
					{
						this.WriteElementString(xmlWriter, "CopySource", (this.obfuscateSourceUri ? HttpUtilities.ObfuscateSourceUri(blobProps.CopySource) : blobProps.CopySource));
					}
					if (!string.IsNullOrEmpty(blobProps.CopyStatus))
					{
						this.WriteElementString(xmlWriter, "CopyStatus", blobProps.CopyStatus);
					}
					if (!string.IsNullOrEmpty(blobProps.CopyStatusDescription))
					{
						this.WriteElementString(xmlWriter, "CopyStatusDescription", blobProps.CopyStatusDescription);
					}
					if (!string.IsNullOrEmpty(blobProps.CopyProgress))
					{
						this.WriteElementString(xmlWriter, "CopyProgress", blobProps.CopyProgress);
					}
					if (blobProps.CopyCompletionTime.HasValue && !string.IsNullOrEmpty(blobProps.CopyStatus) && !blobProps.CopyStatus.Equals("pending", StringComparison.OrdinalIgnoreCase))
					{
						DateTime? copyCompletionTime = blobProps.CopyCompletionTime;
						this.WriteElementString(xmlWriter, "CopyCompletionTime", HttpUtilities.ConvertDateTimeToHttpString(copyCompletionTime.Value));
					}
				}
				if (lboc.IsIncludingIncrementalCopy && blobProps.IsIncrementalCopy)
				{
					this.WriteElementString(xmlWriter, "IncrementalCopy", "true");
					if (blobProps.LastCopySnapshot.HasValue && !string.IsNullOrEmpty(blobProps.CopyStatus) && blobProps.CopyStatus.Equals("success", StringComparison.OrdinalIgnoreCase))
					{
						if (blobProps.LastCopySnapshot.Value <= DateTimeConstants.MinimumIncrementalCopySnapshotTime)
						{
							AlertsManager.AlertOrLogException("LastCopySnapshot set to Minimum value, while a valid timestamp was expected.", null, null);
						}
						DateTime? lastCopySnapshot = blobProps.LastCopySnapshot;
						this.WriteElementString(xmlWriter, "CopyDestinationSnapshot", HttpUtilities.ConvertSnapshotDateTimeToHttpString(lastCopySnapshot.Value));
					}
				}
				if (lboc.IsIncludingEncryption)
				{
					this.WriteElementString(xmlWriter, "ServerEncrypted", (!blobProps.IsBlobEncrypted.HasValue || !blobProps.IsBlobEncrypted.Value ? "false" : "true"));
				}
				if (lboc.IsUsingPropertiesElement)
				{
					xmlWriter.WriteEndElement();
				}
				if (value && lboc.IsFetchingMetadata)
				{
					MetadataEncoding.WriteMetadataToXml(xmlWriter, blobProps.Metadata, true, lboc.RequestVersion);
				}
			}
			xmlWriter.WriteEndElement();
		}

		protected override void EncodeInitialElements(Uri requestUrl, ListBlobsOperationContext lboc, IListBlobsResultCollection result, XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("EnumerationResults");
			if (!VersioningHelper.IsPreAugust13OrInvalidVersion(lboc.RequestVersion))
			{
				string str = HttpRequestAccessorCommon.TrimEndSlash(requestUrl.GetLeftPart(UriPartial.Path));
				int num = str.LastIndexOf("/");
				xmlWriter.WriteAttributeString("ServiceEndpoint", str.Remove(num + 1));
				xmlWriter.WriteAttributeString("ContainerName", str.Substring(num + 1));
			}
			else
			{
				xmlWriter.WriteAttributeString("ContainerName", requestUrl.GetLeftPart(UriPartial.Path));
			}
			XmlListEncoderHelpers.WriteListOperationInfoToXml(xmlWriter, lboc);
			xmlWriter.WriteStartElement("Blobs");
		}

		protected void WriteElementString(XmlWriter xmlWriter, string elementName, string elementValue)
		{
			try
			{
				xmlWriter.WriteElementString(elementName, elementValue);
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				throw new InvalidCharacterProtocolException(elementName, XmlConvert.EncodeName(elementValue), argumentException.Message);
			}
		}
	}
}