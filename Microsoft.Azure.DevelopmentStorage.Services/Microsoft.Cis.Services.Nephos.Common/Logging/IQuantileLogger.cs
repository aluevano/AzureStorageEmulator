namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public interface IQuantileLogger
	{
		[EventStream("RdQuantile::ActionDataPointEvent")]
		IQuantileDataEventStream Quantile
		{
			get;
		}

		[EventStream]
		IStringDataEventStream QuantileAggregationError
		{
			get;
		}

		[EventStream]
		IStringDataEventStream QuantileLogged
		{
			get;
		}
	}
}