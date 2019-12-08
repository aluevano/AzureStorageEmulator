namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public interface IUserMetricsLogger
	{
		[EventStream]
		IXfeUserMetricsEventsStream Metrics
		{
			get;
		}
	}
}