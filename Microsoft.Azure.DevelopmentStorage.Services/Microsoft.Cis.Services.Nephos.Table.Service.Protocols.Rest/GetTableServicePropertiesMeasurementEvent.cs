using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class GetTableServicePropertiesMeasurementEvent : TableOperationMeasurementEvent<GetTableServicePropertiesMeasurementEvent>
	{
		public GetTableServicePropertiesMeasurementEvent() : base("GetTableServiceProperties")
		{
		}
	}
}