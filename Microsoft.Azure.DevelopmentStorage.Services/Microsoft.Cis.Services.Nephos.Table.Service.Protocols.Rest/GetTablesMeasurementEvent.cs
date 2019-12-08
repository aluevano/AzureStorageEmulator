using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class GetTablesMeasurementEvent : TableOperationMeasurementEvent<GetTablesMeasurementEvent>
	{
		public GetTablesMeasurementEvent() : base("QueryTables")
		{
		}
	}
}