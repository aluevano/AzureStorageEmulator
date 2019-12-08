using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class GetTableMeasurementEvent : TableOperationMeasurementEvent<GetTableMeasurementEvent>
	{
		public GetTableMeasurementEvent() : base("QueryTable")
		{
		}
	}
}