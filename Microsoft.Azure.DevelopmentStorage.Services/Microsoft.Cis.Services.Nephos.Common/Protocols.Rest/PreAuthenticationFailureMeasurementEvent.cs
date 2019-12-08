using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class PreAuthenticationFailureMeasurementEvent : RequestFailureMeasurementEvent<PreAuthenticationFailureMeasurementEvent>
	{
		public PreAuthenticationFailureMeasurementEvent() : base("PreAuthenticationFailure")
		{
		}
	}
}