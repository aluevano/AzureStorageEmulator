using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class PreOperationFailureMeasurementEvent : RequestFailureMeasurementEvent<PreOperationFailureMeasurementEvent>
	{
		public PreOperationFailureMeasurementEvent() : base("Unknown")
		{
		}
	}
}