using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class UpsertRowMeasurementEvent : TableOperationMeasurementEvent<UpsertRowMeasurementEvent>
	{
		public UpsertRowMeasurementEvent(bool isMerge) : base((isMerge ? "InsertOrMergeEntity" : "InsertOrReplaceEntity"))
		{
		}
	}
}