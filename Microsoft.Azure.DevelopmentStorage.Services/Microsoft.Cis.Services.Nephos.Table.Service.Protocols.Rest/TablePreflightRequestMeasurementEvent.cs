using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class TablePreflightRequestMeasurementEvent : TableOperationMeasurementEvent<TablePreflightRequestMeasurementEvent>
	{
		public TablePreflightRequestMeasurementEvent() : base("TablePreflightRequest")
		{
		}
	}
}