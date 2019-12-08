using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class UpdateMessageMeasurementEvent : QueueOperationMeasurementEvent<UpdateMessageMeasurementEvent>
	{
		[MeasurementEventParameter]
		public long MessageBytesRead
		{
			get
			{
				return base.RequestBytesRead;
			}
		}

		public string MessageId
		{
			get;
			set;
		}

		public UpdateMessageMeasurementEvent() : base("UpdateMessage")
		{
		}

		public override string GetObjectKey()
		{
			string[] accountName = new string[] { base.AccountName, base.QueueName, this.MessageId };
			return base.GenerateObjectKeyFrom(accountName);
		}
	}
}