using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IQueueOperations
	{
		TimeSpan IQueueOperationsTimeout
		{
			get;
			set;
		}

		IAsyncResult BeginClearQueue(AsyncCallback callback, object state);

		IAsyncResult BeginDeleteMessage(IQueueMessageReceipt queueMessageReceipt, AsyncCallback callback, object state);

		IAsyncResult BeginGetMessage(int numberOfMessages, TimeSpan visibilityTimeout, AsyncCallback callback, object state);

		IAsyncResult BeginGetQueueStatistics(bool includeInvisibleMessages, bool includeExpiredMessages, AsyncCallback callback, object state);

		IAsyncResult BeginListMessages(string queueNameStart, DateTime? visibilityStart, Guid messageIdStart, int? subQueueId, bool includeInvisibleMessages, int numberOfMessages, AsyncCallback callback, object state);

		IAsyncResult BeginPeekMessage(int numberOfMessages, AsyncCallback callback, object state);

		IAsyncResult BeginPutMessage(List<PushedMessage> messagesList, bool usePutMessageRowCommand, AsyncCallback callback, object state);

		IAsyncResult BeginUpdateMessage(IQueueMessageReceipt receipt, byte[] body, TimeSpan visibilityTimeout, TimeSpan? timeToLive, AsyncCallback callback, object state);

		void EndClearQueue(IAsyncResult ar);

		bool EndDeleteMessage(IAsyncResult ar);

		IEnumerable<IMessageData> EndGetMessage(IAsyncResult ar);

		IQueueStatistics EndGetQueueStatistics(IAsyncResult ar);

		ListMessagesResult EndListMessages(IAsyncResult ar);

		IEnumerable<IMessageData> EndPeekMessage(IAsyncResult ar);

		List<IMessageData> EndPutMessage(IAsyncResult ar);

		IQueueMessageReceipt EndUpdateMessage(IAsyncResult ar);
	}
}