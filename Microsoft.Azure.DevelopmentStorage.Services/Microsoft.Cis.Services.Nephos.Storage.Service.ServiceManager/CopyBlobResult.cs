using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class CopyBlobResult : ICopyBlobResult
	{
		public CopyBlobOperationInfo CopyInfo
		{
			get
			{
				return JustDecompileGenerated_get_CopyInfo();
			}
			set
			{
				JustDecompileGenerated_set_CopyInfo(value);
			}
		}

		private CopyBlobOperationInfo JustDecompileGenerated_CopyInfo_k__BackingField;

		public CopyBlobOperationInfo JustDecompileGenerated_get_CopyInfo()
		{
			return this.JustDecompileGenerated_CopyInfo_k__BackingField;
		}

		public void JustDecompileGenerated_set_CopyInfo(CopyBlobOperationInfo value)
		{
			this.JustDecompileGenerated_CopyInfo_k__BackingField = value;
		}

		public TimeSpan CopySourceVerificationRequestRoundTripLatency
		{
			get
			{
				return JustDecompileGenerated_get_CopySourceVerificationRequestRoundTripLatency();
			}
			set
			{
				JustDecompileGenerated_set_CopySourceVerificationRequestRoundTripLatency(value);
			}
		}

		private TimeSpan JustDecompileGenerated_CopySourceVerificationRequestRoundTripLatency_k__BackingField;

		public TimeSpan JustDecompileGenerated_get_CopySourceVerificationRequestRoundTripLatency()
		{
			return this.JustDecompileGenerated_CopySourceVerificationRequestRoundTripLatency_k__BackingField;
		}

		public void JustDecompileGenerated_set_CopySourceVerificationRequestRoundTripLatency(TimeSpan value)
		{
			this.JustDecompileGenerated_CopySourceVerificationRequestRoundTripLatency_k__BackingField = value;
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

		public CopyBlobResult()
		{
		}
	}
}