using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class AuthenticationFailureMeasurementEvent : RequestFailureMeasurementEvent<AuthenticationFailureMeasurementEvent>
	{
		public AuthenticationFailureMeasurementEvent() : base("AuthenticationFailure")
		{
		}
	}
}