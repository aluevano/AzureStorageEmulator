using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface ICopyBlobResult
	{
		CopyBlobOperationInfo CopyInfo
		{
			get;
		}

		TimeSpan CopySourceVerificationRequestRoundTripLatency
		{
			get;
		}

		DateTime LastModifiedTime
		{
			get;
		}
	}
}