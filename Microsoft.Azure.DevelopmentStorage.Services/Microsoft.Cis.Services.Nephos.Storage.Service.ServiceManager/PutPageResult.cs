using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class PutPageResult : IPutPageResult, ICrc64Md5Result
	{
		private long? contentCrc64;

		private byte[] contentMD5;

		public long? ContentCrc64
		{
			get
			{
				return JustDecompileGenerated_get_ContentCrc64();
			}
			set
			{
				JustDecompileGenerated_set_ContentCrc64(value);
			}
		}

		public long? JustDecompileGenerated_get_ContentCrc64()
		{
			return this.contentCrc64;
		}

		public void JustDecompileGenerated_set_ContentCrc64(long? value)
		{
			this.contentCrc64 = value;
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

		private DateTime JustDecompileGenerated_LastModifiedTime_k__BackingField;

		public DateTime JustDecompileGenerated_get_LastModifiedTime()
		{
			return this.JustDecompileGenerated_LastModifiedTime_k__BackingField;
		}

		public void JustDecompileGenerated_set_LastModifiedTime(DateTime value)
		{
			this.JustDecompileGenerated_LastModifiedTime_k__BackingField = value;
		}

		public long SequenceNumber
		{
			get
			{
				return JustDecompileGenerated_get_SequenceNumber();
			}
			set
			{
				JustDecompileGenerated_set_SequenceNumber(value);
			}
		}

		private long JustDecompileGenerated_SequenceNumber_k__BackingField;

		public long JustDecompileGenerated_get_SequenceNumber()
		{
			return this.JustDecompileGenerated_SequenceNumber_k__BackingField;
		}

		public void JustDecompileGenerated_set_SequenceNumber(long value)
		{
			this.JustDecompileGenerated_SequenceNumber_k__BackingField = value;
		}

		public PutPageResult()
		{
		}
	}
}