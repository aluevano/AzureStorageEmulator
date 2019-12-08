using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class BatchMeasurementEvent : TableOperationMeasurementEvent<BatchMeasurementEvent>
	{
		public int OperationCount
		{
			get;
			set;
		}

		public BatchMeasurementEvent() : base("EntityGroupTransaction")
		{
		}
	}
}