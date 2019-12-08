namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public interface IFrontEndPerfMetricsLogger
	{
		[EventStream]
		IFrontEndPerfMetricsEventsStream Metrics
		{
			get;
		}
	}
}