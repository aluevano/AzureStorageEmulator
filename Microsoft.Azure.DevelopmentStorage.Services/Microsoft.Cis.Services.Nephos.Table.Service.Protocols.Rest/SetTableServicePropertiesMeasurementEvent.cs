using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class SetTableServicePropertiesMeasurementEvent : TableOperationMeasurementEvent<SetTableServicePropertiesMeasurementEvent>
	{
		public SetTableServicePropertiesMeasurementEvent() : base("SetTableServiceProperties")
		{
		}
	}
}