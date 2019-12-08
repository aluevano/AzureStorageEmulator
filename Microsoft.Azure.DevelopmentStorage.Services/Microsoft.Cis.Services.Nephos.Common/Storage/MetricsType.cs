using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Flags]
	public enum MetricsType
	{
		None,
		ServiceSummary,
		ApiSummary,
		All
	}
}