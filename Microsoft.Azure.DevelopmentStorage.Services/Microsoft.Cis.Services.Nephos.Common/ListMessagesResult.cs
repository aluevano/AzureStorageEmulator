using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class ListMessagesResult : SummaryResult, IEnumerable<PoppedMessage>, IEnumerable
	{
		public List<IMessageData> Messages
		{
			get;
			set;
		}

		public IEnumerable<PoppedMessage> PoppedMessages
		{
			get;
			set;
		}

		public ListMessagesResult()
		{
		}

		public ListMessagesResult(List<IMessageData> messages, string nextMarker) : base((long)0, (long)0, nextMarker)
		{
			this.Messages = messages;
		}

		[SuppressMessage("Microsoft.Design", "CA1033")]
		IEnumerator<PoppedMessage> System.Collections.Generic.IEnumerable<Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC.PoppedMessage>.GetEnumerator()
		{
			throw new NotImplementedException("IEnumerable<PoppedMessage>.GetEnumerator()");
		}

		[SuppressMessage("Microsoft.Design", "CA1033")]
		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.PoppedMessages.GetEnumerator();
		}
	}
}