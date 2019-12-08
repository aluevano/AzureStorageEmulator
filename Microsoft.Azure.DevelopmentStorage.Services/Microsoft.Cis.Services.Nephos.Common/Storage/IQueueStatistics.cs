using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IQueueStatistics
	{
		long TotalMessages
		{
			get;
		}

		long TotalSize
		{
			get;
		}
	}
}