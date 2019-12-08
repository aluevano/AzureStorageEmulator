using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class GetMessageMeasurementEvent : QueueOperationMeasurementEvent<GetMessageMeasurementEvent>
	{
		private long numMessagesRequested = (long)1;

		private Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest.OperationType operationType;

		[MeasurementEventParameter]
		public long NumMessagesRequested
		{
			get
			{
				return this.numMessagesRequested;
			}
			set
			{
				this.numMessagesRequested = value;
			}
		}

		[MeasurementEventParameter]
		public Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest.OperationType OperationType
		{
			get
			{
				return this.operationType;
			}
			protected set
			{
				this.operationType = value;
			}
		}

		protected GetMessageMeasurementEvent(string operationName) : base(operationName)
		{
		}

		public GetMessageMeasurementEvent() : base("GetMessage")
		{
			this.OperationType = Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest.OperationType.Get;
		}
	}
}