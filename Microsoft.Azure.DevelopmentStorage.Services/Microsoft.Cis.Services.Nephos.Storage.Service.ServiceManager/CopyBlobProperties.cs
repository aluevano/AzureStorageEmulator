using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class CopyBlobProperties : ICopyBlobProperties
	{
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

		public CopyBlobProperties()
		{
		}
	}
}