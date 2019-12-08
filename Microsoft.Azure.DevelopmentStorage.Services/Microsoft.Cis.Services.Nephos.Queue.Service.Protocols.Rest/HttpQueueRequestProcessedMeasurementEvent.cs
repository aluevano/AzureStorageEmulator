using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class HttpQueueRequestProcessedMeasurementEvent : NephosBaseMeasurementEvent<HttpQueueRequestProcessedMeasurementEvent>, IQueueMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		private string queueName;

		public override string OperationPartitionKey
		{
			get
			{
				return null;
			}
		}

		[MeasurementEventParameter]
		public string QueueName
		{
			get
			{
				return this.queueName;
			}
			set
			{
				this.queueName = value;
			}
		}

		public HttpQueueRequestProcessedMeasurementEvent() : base("HttpQueueRequestProcessed")
		{
		}
	}
}