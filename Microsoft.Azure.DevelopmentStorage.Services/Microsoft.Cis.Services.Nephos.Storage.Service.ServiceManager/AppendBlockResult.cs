using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class AppendBlockResult : IAppendBlockResult, ICrc64Md5Result
	{
		public long AppendOffset
		{
			get
			{
				return JustDecompileGenerated_get_AppendOffset();
			}
			set
			{
				JustDecompileGenerated_set_AppendOffset(value);
			}
		}

		private long JustDecompileGenerated_AppendOffset_k__BackingField;

		public long JustDecompileGenerated_get_AppendOffset()
		{
			return this.JustDecompileGenerated_AppendOffset_k__BackingField;
		}

		public void JustDecompileGenerated_set_AppendOffset(long value)
		{
			this.JustDecompileGenerated_AppendOffset_k__BackingField = value;
		}

		public int CommittedBlockCount
		{
			get
			{
				return JustDecompileGenerated_get_CommittedBlockCount();
			}
			set
			{
				JustDecompileGenerated_set_CommittedBlockCount(value);
			}
		}

		private int JustDecompileGenerated_CommittedBlockCount_k__BackingField;

		public int JustDecompileGenerated_get_CommittedBlockCount()
		{
			return this.JustDecompileGenerated_CommittedBlockCount_k__BackingField;
		}

		public void JustDecompileGenerated_set_CommittedBlockCount(int value)
		{
			this.JustDecompileGenerated_CommittedBlockCount_k__BackingField = value;
		}

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

		private long? JustDecompileGenerated_ContentCrc64_k__BackingField;

		public long? JustDecompileGenerated_get_ContentCrc64()
		{
			return this.JustDecompileGenerated_ContentCrc64_k__BackingField;
		}

		public void JustDecompileGenerated_set_ContentCrc64(long? value)
		{
			this.JustDecompileGenerated_ContentCrc64_k__BackingField = value;
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

		private byte[] JustDecompileGenerated_ContentMD5_k__BackingField;

		public byte[] JustDecompileGenerated_get_ContentMD5()
		{
			return this.JustDecompileGenerated_ContentMD5_k__BackingField;
		}

		public void JustDecompileGenerated_set_ContentMD5(byte[] value)
		{
			this.JustDecompileGenerated_ContentMD5_k__BackingField = value;
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

		public AppendBlockResult()
		{
		}
	}
}