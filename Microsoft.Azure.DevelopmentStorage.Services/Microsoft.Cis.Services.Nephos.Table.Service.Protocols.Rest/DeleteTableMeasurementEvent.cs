using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class DeleteTableMeasurementEvent : TableOperationMeasurementEvent<DeleteTableMeasurementEvent>
	{
		public DeleteTableMeasurementEvent() : base("DeleteTable")
		{
		}
	}
}