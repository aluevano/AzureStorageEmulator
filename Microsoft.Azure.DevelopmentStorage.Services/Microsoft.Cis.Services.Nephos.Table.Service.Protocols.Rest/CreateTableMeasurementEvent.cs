using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class CreateTableMeasurementEvent : TableOperationMeasurementEvent<CreateTableMeasurementEvent>
	{
		public CreateTableMeasurementEvent() : base("CreateTable")
		{
		}
	}
}