using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class GetRowsMeasurementEvent : TableOperationMeasurementEvent<GetRowsMeasurementEvent>
	{
		public GetRowsMeasurementEvent() : base("QueryEntities")
		{
		}
	}
}