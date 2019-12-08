using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class PutMessageMeasurementEvent : QueueOperationMeasurementEvent<PutMessageMeasurementEvent>
	{
		[MeasurementEventParameter]
		public long MessageBytesRead
		{
			get
			{
				return base.RequestBytesRead;
			}
		}

		public IEnumerable<PoppedMessage> MessageList
		{
			get;
			set;
		}

		public PutMessageMeasurementEvent() : base("PutMessage")
		{
		}

		public override string GetObjectKey()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (PoppedMessage messageList in this.MessageList ?? Enumerable.Empty<PoppedMessage>())
			{
				stringBuilder.AppendFormat("{0}", messageList.Id);
			}
			string[] accountName = new string[] { base.AccountName, base.QueueName, stringBuilder.ToString() };
			return base.GenerateObjectKeyFrom(accountName);
		}
	}
}