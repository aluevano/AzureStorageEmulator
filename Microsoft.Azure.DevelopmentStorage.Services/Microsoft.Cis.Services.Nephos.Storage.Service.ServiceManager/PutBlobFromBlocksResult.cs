using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class PutBlobFromBlocksResult : IPutBlobFromBlocksResult
	{
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

		public PutBlobFromBlocksResult()
		{
		}
	}
}