using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class UpdateRowMeasurementEvent : TableOperationMeasurementEvent<UpdateRowMeasurementEvent>
	{
		public UpdateRowMeasurementEvent(bool isMerge) : base((isMerge ? "MergeEntity" : "UpdateEntity"))
		{
		}
	}
}