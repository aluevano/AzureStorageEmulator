using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Collections.Specialized;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class BlobServiceMetaData
	{
		public string CopyId;

		public string CopySource;

		public string CopyStatus;

		public string CopyStatusDescription;

		public string CopyProgressOffset;

		public string CopyProgressTotal;

		public long? CopyCompletionTime;

		private byte[] metadata;

		private NameValueCollection metadataNameValuePairs = new NameValueCollection();

		private BlobServiceMetaData()
		{
		}

		public static BlobServiceMetaData GetInstance()
		{
			return new BlobServiceMetaData();
		}

		public static BlobServiceMetaData GetInstance(byte[] metadata)
		{
			BlobServiceMetaData blobServiceMetaDatum = new BlobServiceMetaData();
			if (metadata != null)
			{
				blobServiceMetaDatum.metadata = metadata;
				MetadataEncoding.Decode(metadata, blobServiceMetaDatum.metadataNameValuePairs);
				blobServiceMetaDatum.PopulateCopyProperties();
			}
			return blobServiceMetaDatum;
		}

		public static BlobServiceMetaData GetInstance(NameValueCollection metadata)
		{
			BlobServiceMetaData blobServiceMetaDatum = new BlobServiceMetaData();
			if (metadata != null || metadata.Count < 1)
			{
				blobServiceMetaDatum.metadataNameValuePairs = metadata;
				blobServiceMetaDatum.metadata = MetadataEncoding.Encode(blobServiceMetaDatum.metadataNameValuePairs);
				blobServiceMetaDatum.PopulateCopyProperties();
			}
			return blobServiceMetaDatum;
		}

		public byte[] GetMetadata()
		{
			string str;
			this.metadataNameValuePairs[RealServiceManager.CopyIdTag] = this.CopyId;
			this.metadataNameValuePairs[RealServiceManager.CopySourceTag] = this.CopySource;
			this.metadataNameValuePairs[RealServiceManager.CopyStatusTag] = this.CopyStatus;
			this.metadataNameValuePairs[RealServiceManager.CopyStatusDescriptionTag] = this.CopyStatusDescription;
			this.metadataNameValuePairs[RealServiceManager.CopyProgressOffsetTag] = this.CopyProgressOffset;
			this.metadataNameValuePairs[RealServiceManager.CopyProgressTotalTag] = this.CopyProgressTotal;
			NameValueCollection nameValueCollection = this.metadataNameValuePairs;
			string copyCompletionTimeTag = RealServiceManager.CopyCompletionTimeTag;
			if (this.CopyCompletionTime.HasValue)
			{
				str = this.CopyCompletionTime.Value.ToString();
			}
			else
			{
				str = null;
			}
			nameValueCollection[copyCompletionTimeTag] = str;
			string[] allKeys = this.metadataNameValuePairs.AllKeys;
			for (int i = 0; i < (int)allKeys.Length; i++)
			{
				string str1 = allKeys[i];
				if (this.metadataNameValuePairs[str1] == null)
				{
					this.metadataNameValuePairs.Remove(str1);
				}
			}
			if (this.metadataNameValuePairs.Count <= 0)
			{
				this.metadata = null;
			}
			else
			{
				this.metadata = MetadataEncoding.Encode(this.metadataNameValuePairs);
			}
			return this.metadata;
		}

		private void PopulateCopyProperties()
		{
			this.CopyId = this.metadataNameValuePairs[RealServiceManager.CopyIdTag];
			this.CopySource = this.metadataNameValuePairs[RealServiceManager.CopySourceTag];
			this.CopyStatus = this.metadataNameValuePairs[RealServiceManager.CopyStatusTag];
			this.CopyStatusDescription = this.metadataNameValuePairs[RealServiceManager.CopyStatusDescriptionTag];
			this.CopyProgressOffset = this.metadataNameValuePairs[RealServiceManager.CopyProgressOffsetTag];
			this.CopyProgressTotal = this.metadataNameValuePairs[RealServiceManager.CopyProgressTotalTag];
			if (this.metadataNameValuePairs[RealServiceManager.CopyCompletionTimeTag] != null)
			{
				this.CopyCompletionTime = new long?(long.Parse(this.metadataNameValuePairs[RealServiceManager.CopyCompletionTimeTag]));
			}
		}
	}
}