using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class GetTableServiceStatsMeasurementEvent : TableOperationMeasurementEvent<GetTableServiceStatsMeasurementEvent>
	{
		public GetTableServiceStatsMeasurementEvent() : base("GetTableServiceStats")
		{
		}
	}
}