using System;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public interface IFrontEndGeneralEventsStream
	{
		void LogTransferEvent(string message, Guid relatedActivityId);
	}
}