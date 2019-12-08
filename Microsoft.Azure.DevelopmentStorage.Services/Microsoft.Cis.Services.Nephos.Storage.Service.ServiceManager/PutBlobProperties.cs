using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class PutBlobProperties : IPutBlobProperties
	{
		public string BlobLinkSource
		{
			get
			{
				return JustDecompileGenerated_get_BlobLinkSource();
			}
			set
			{
				JustDecompileGenerated_set_BlobLinkSource(value);
			}
		}

		private string JustDecompileGenerated_BlobLinkSource_k__BackingField;

		public string JustDecompileGenerated_get_BlobLinkSource()
		{
			return this.JustDecompileGenerated_BlobLinkSource_k__BackingField;
		}

		public void JustDecompileGenerated_set_BlobLinkSource(string value)
		{
			this.JustDecompileGenerated_BlobLinkSource_k__BackingField = value;
		}

		public NameValueCollection BlobMetadata
		{
			get
			{
				return JustDecompileGenerated_get_BlobMetadata();
			}
			set
			{
				JustDecompileGenerated_set_BlobMetadata(value);
			}
		}

		private NameValueCollection JustDecompileGenerated_BlobMetadata_k__BackingField;

		public NameValueCollection JustDecompileGenerated_get_BlobMetadata()
		{
			return this.JustDecompileGenerated_BlobMetadata_k__BackingField;
		}

		public void JustDecompileGenerated_set_BlobMetadata(NameValueCollection value)
		{
			this.JustDecompileGenerated_BlobMetadata_k__BackingField = value;
		}

		public Microsoft.Cis.Services.Nephos.Common.Storage.BlobType BlobType
		{
			get
			{
				return JustDecompileGenerated_get_BlobType();
			}
			set
			{
				JustDecompileGenerated_set_BlobType(value);
			}
		}

		private Microsoft.Cis.Services.Nephos.Common.Storage.BlobType JustDecompileGenerated_BlobType_k__BackingField;

		public Microsoft.Cis.Services.Nephos.Common.Storage.BlobType JustDecompileGenerated_get_BlobType()
		{
			return this.JustDecompileGenerated_BlobType_k__BackingField;
		}

		public void JustDecompileGenerated_set_BlobType(Microsoft.Cis.Services.Nephos.Common.Storage.BlobType value)
		{
			this.JustDecompileGenerated_BlobType_k__BackingField = value;
		}

		public string CacheControl
		{
			get
			{
				return JustDecompileGenerated_get_CacheControl();
			}
			set
			{
				JustDecompileGenerated_set_CacheControl(value);
			}
		}

		private string JustDecompileGenerated_CacheControl_k__BackingField;

		public string JustDecompileGenerated_get_CacheControl()
		{
			return this.JustDecompileGenerated_CacheControl_k__BackingField;
		}

		public void JustDecompileGenerated_set_CacheControl(string value)
		{
			this.JustDecompileGenerated_CacheControl_k__BackingField = value;
		}

		public long? ContentCrc64
		{
			get;
			set;
		}

		public string ContentDisposition
		{
			get
			{
				return JustDecompileGenerated_get_ContentDisposition();
			}
			set
			{
				JustDecompileGenerated_set_ContentDisposition(value);
			}
		}

		private string JustDecompileGenerated_ContentDisposition_k__BackingField;

		public string JustDecompileGenerated_get_ContentDisposition()
		{
			return this.JustDecompileGenerated_ContentDisposition_k__BackingField;
		}

		public void JustDecompileGenerated_set_ContentDisposition(string value)
		{
			this.JustDecompileGenerated_ContentDisposition_k__BackingField = value;
		}

		public string ContentEncoding
		{
			get
			{
				return JustDecompileGenerated_get_ContentEncoding();
			}
			set
			{
				JustDecompileGenerated_set_ContentEncoding(value);
			}
		}

		private string JustDecompileGenerated_ContentEncoding_k__BackingField;

		public string JustDecompileGenerated_get_ContentEncoding()
		{
			return this.JustDecompileGenerated_ContentEncoding_k__BackingField;
		}

		public void JustDecompileGenerated_set_ContentEncoding(string value)
		{
			this.JustDecompileGenerated_ContentEncoding_k__BackingField = value;
		}

		public string ContentLanguage
		{
			get
			{
				return JustDecompileGenerated_get_ContentLanguage();
			}
			set
			{
				JustDecompileGenerated_set_ContentLanguage(value);
			}
		}

		private string JustDecompileGenerated_ContentLanguage_k__BackingField;

		public string JustDecompileGenerated_get_ContentLanguage()
		{
			return this.JustDecompileGenerated_ContentLanguage_k__BackingField;
		}

		public void JustDecompileGenerated_set_ContentLanguage(string value)
		{
			this.JustDecompileGenerated_ContentLanguage_k__BackingField = value;
		}

		public byte[] ContentMD5
		{
			get;
			set;
		}

		public string ContentType
		{
			get
			{
				return JustDecompileGenerated_get_ContentType();
			}
			set
			{
				JustDecompileGenerated_set_ContentType(value);
			}
		}

		private string JustDecompileGenerated_ContentType_k__BackingField;

		public string JustDecompileGenerated_get_ContentType()
		{
			return this.JustDecompileGenerated_ContentType_k__BackingField;
		}

		public void JustDecompileGenerated_set_ContentType(string value)
		{
			this.JustDecompileGenerated_ContentType_k__BackingField = value;
		}

		public DateTime? CreationTime
		{
			get
			{
				return JustDecompileGenerated_get_CreationTime();
			}
			set
			{
				JustDecompileGenerated_set_CreationTime(value);
			}
		}

		private DateTime? JustDecompileGenerated_CreationTime_k__BackingField;

		public DateTime? JustDecompileGenerated_get_CreationTime()
		{
			return this.JustDecompileGenerated_CreationTime_k__BackingField;
		}

		public void JustDecompileGenerated_set_CreationTime(DateTime? value)
		{
			this.JustDecompileGenerated_CreationTime_k__BackingField = value;
		}

		public Guid GenerationId
		{
			get;
			set;
		}

		public DateTime? LastModifiedTime
		{
			get
			{
				return JustDecompileGenerated_get_LastModifiedTime();
			}
			set
			{
				JustDecompileGenerated_set_LastModifiedTime(value);
			}
		}

		private DateTime? JustDecompileGenerated_LastModifiedTime_k__BackingField;

		public DateTime? JustDecompileGenerated_get_LastModifiedTime()
		{
			return this.JustDecompileGenerated_LastModifiedTime_k__BackingField;
		}

		public void JustDecompileGenerated_set_LastModifiedTime(DateTime? value)
		{
			this.JustDecompileGenerated_LastModifiedTime_k__BackingField = value;
		}

		public ILeaseInfo LeaseInfo
		{
			get
			{
				return JustDecompileGenerated_get_LeaseInfo();
			}
			set
			{
				JustDecompileGenerated_set_LeaseInfo(value);
			}
		}

		private ILeaseInfo JustDecompileGenerated_LeaseInfo_k__BackingField;

		public ILeaseInfo JustDecompileGenerated_get_LeaseInfo()
		{
			return this.JustDecompileGenerated_LeaseInfo_k__BackingField;
		}

		public void JustDecompileGenerated_set_LeaseInfo(ILeaseInfo value)
		{
			this.JustDecompileGenerated_LeaseInfo_k__BackingField = value;
		}

		public long? MaxBlobSize
		{
			get
			{
				return JustDecompileGenerated_get_MaxBlobSize();
			}
			set
			{
				JustDecompileGenerated_set_MaxBlobSize(value);
			}
		}

		private long? JustDecompileGenerated_MaxBlobSize_k__BackingField;

		public long? JustDecompileGenerated_get_MaxBlobSize()
		{
			return this.JustDecompileGenerated_MaxBlobSize_k__BackingField;
		}

		public void JustDecompileGenerated_set_MaxBlobSize(long? value)
		{
			this.JustDecompileGenerated_MaxBlobSize_k__BackingField = value;
		}

		public bool? PutBlobComputeMD5
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Common.Storage.SequenceNumberUpdate SequenceNumberUpdate
		{
			get
			{
				return JustDecompileGenerated_get_SequenceNumberUpdate();
			}
			set
			{
				JustDecompileGenerated_set_SequenceNumberUpdate(value);
			}
		}

		private Microsoft.Cis.Services.Nephos.Common.Storage.SequenceNumberUpdate JustDecompileGenerated_SequenceNumberUpdate_k__BackingField;

		public Microsoft.Cis.Services.Nephos.Common.Storage.SequenceNumberUpdate JustDecompileGenerated_get_SequenceNumberUpdate()
		{
			return this.JustDecompileGenerated_SequenceNumberUpdate_k__BackingField;
		}

		public void JustDecompileGenerated_set_SequenceNumberUpdate(Microsoft.Cis.Services.Nephos.Common.Storage.SequenceNumberUpdate value)
		{
			this.JustDecompileGenerated_SequenceNumberUpdate_k__BackingField = value;
		}

		public PutBlobProperties()
		{
			this.BlobMetadata = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
		}

		public PutBlobProperties(NameValueCollection blobMetadata, string cacheControl, string contentType, string contentEncoding, string contentLanguage, long? contentCrc64, bool? putBlobComputeMD5, byte[] contentMD5, string contentDisposition)
		{
			this.BlobMetadata = blobMetadata;
			this.CacheControl = cacheControl;
			this.ContentType = contentType;
			this.ContentEncoding = contentEncoding;
			this.ContentLanguage = contentLanguage;
			this.ContentCrc64 = contentCrc64;
			this.PutBlobComputeMD5 = putBlobComputeMD5;
			this.ContentMD5 = contentMD5;
			this.ContentDisposition = contentDisposition;
		}
	}
}