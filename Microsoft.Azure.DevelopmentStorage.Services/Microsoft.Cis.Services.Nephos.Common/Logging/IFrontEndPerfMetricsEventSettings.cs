using System;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public interface IFrontEndPerfMetricsEventSettings
	{
		bool LogLateTransferEvent
		{
			get;
		}

		bool LogNormalEvent
		{
			get;
		}

		bool LogTrace
		{
			get;
		}
	}
}