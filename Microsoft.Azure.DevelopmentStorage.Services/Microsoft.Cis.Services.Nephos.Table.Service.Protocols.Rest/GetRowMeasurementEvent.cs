using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class GetRowMeasurementEvent : TableOperationMeasurementEvent<GetRowMeasurementEvent>
	{
		public GetRowMeasurementEvent() : base("QueryEntity")
		{
		}
	}
}