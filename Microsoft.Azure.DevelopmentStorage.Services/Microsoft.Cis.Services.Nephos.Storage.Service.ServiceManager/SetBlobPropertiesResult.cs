using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class SetBlobPropertiesResult : ISetBlobPropertiesResult
	{
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

		public long? SequenceNumber
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

		private long? JustDecompileGenerated_SequenceNumber_k__BackingField;

		public long? JustDecompileGenerated_get_SequenceNumber()
		{
			return this.JustDecompileGenerated_SequenceNumber_k__BackingField;
		}

		public void JustDecompileGenerated_set_SequenceNumber(long? value)
		{
			this.JustDecompileGenerated_SequenceNumber_k__BackingField = value;
		}

		public SetBlobPropertiesResult()
		{
		}
	}
}