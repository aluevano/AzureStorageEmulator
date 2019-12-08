using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class DeleteMessageMeasurementEvent : QueueOperationMeasurementEvent<DeleteMessageMeasurementEvent>
	{
		public string MessageId
		{
			get;
			set;
		}

		public DeleteMessageMeasurementEvent() : base("DeleteMessage")
		{
		}

		public override string GetObjectKey()
		{
			string[] accountName = new string[] { base.AccountName, base.QueueName, this.MessageId };
			return base.GenerateObjectKeyFrom(accountName);
		}
	}
}