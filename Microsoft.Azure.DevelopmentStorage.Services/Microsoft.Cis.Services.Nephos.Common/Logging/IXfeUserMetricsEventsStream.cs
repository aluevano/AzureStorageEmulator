using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface IXfeUserMetricsEventsStream
	{
		void LogMetric(string account, string authenticationType, string requestVersion, string processingVersion, string userAgent, string protocol);
	}
}