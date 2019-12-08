using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class DeleteRowMeasurementEvent : TableOperationMeasurementEvent<DeleteRowMeasurementEvent>
	{
		public DeleteRowMeasurementEvent() : base("DeleteEntity")
		{
		}
	}
}