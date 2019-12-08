using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IPThrottlingFailureMeasurementEvent : RequestFailureMeasurementEvent<IPThrottlingFailureMeasurementEvent>
	{
		private string clientIPAddress;

		[MeasurementEventParameter]
		public string ClientIPAddress
		{
			get
			{
				return this.clientIPAddress;
			}
			set
			{
				this.clientIPAddress = value;
			}
		}

		public IPThrottlingFailureMeasurementEvent() : base("IPThrottlingFailure")
		{
		}
	}
}