using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class PutBlobResult : IPutBlobResult
	{
		private long? contentCrc64;

		private byte[] contentMD5;

		private DateTime lastModifiedTime;

		public long? ContentCrc64
		{
			get
			{
				return this.contentCrc64;
			}
			set
			{
				this.contentCrc64 = value;
			}
		}

		public byte[] ContentMD5
		{
			get
			{
				return JustDecompileGenerated_get_ContentMD5();
			}
			set
			{
				JustDecompileGenerated_set_ContentMD5(value);
			}
		}

		public byte[] JustDecompileGenerated_get_ContentMD5()
		{
			return this.contentMD5;
		}

		public void JustDecompileGenerated_set_ContentMD5(byte[] value)
		{
			this.contentMD5 = value;
		}

		public bool IsWriteEncrypted
		{
			get
			{
				return JustDecompileGenerated_get_IsWriteEncrypted();
			}
			set
			{
				JustDecompileGenerated_set_IsWriteEncrypted(value);
			}
		}

		private bool JustDecompileGenerated_IsWriteEncrypted_k__BackingField;

		public bool JustDecompileGenerated_get_IsWriteEncrypted()
		{
			return this.JustDecompileGenerated_IsWriteEncrypted_k__BackingField;
		}

		public void JustDecompileGenerated_set_IsWriteEncrypted(bool value)
		{
			this.JustDecompileGenerated_IsWriteEncrypted_k__BackingField = value;
		}

		public DateTime LastModifiedTime
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

		public DateTime JustDecompileGenerated_get_LastModifiedTime()
		{
			return this.lastModifiedTime;
		}

		public void JustDecompileGenerated_set_LastModifiedTime(DateTime value)
		{
			this.lastModifiedTime = value;
		}

		public PutBlobResult()
		{
		}
	}
}