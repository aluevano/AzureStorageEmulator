using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class QueueOperationMeasurementEvent<T> : NephosBaseOperationMeasurementEvent<T>, IQueueMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	where T : QueueOperationMeasurementEvent<T>
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

		protected QueueOperationMeasurementEvent(string operationName) : base(operationName)
		{
		}

		public override string GetObjectKey()
		{
			if (string.IsNullOrEmpty(this.QueueName))
			{
				return base.GenerateObjectKeyFrom(new string[] { base.AccountName });
			}
			string[] accountName = new string[] { base.AccountName, this.QueueName };
			return base.GenerateObjectKeyFrom(accountName);
		}
	}
}