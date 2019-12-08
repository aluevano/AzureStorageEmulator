using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class InsertRowMeasurementEvent : TableOperationMeasurementEvent<InsertRowMeasurementEvent>
	{
		public InsertRowMeasurementEvent() : base("InsertEntity")
		{
		}
	}
}