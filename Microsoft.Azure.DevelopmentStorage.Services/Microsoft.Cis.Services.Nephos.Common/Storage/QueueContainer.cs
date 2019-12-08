using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class QueueContainer : Container, IQueueContainer, IContainer, IDisposable, IQueueOperations
	{
		public override Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType ContainerType
		{
			get
			{
				return Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.QueueContainer;
			}
		}

		private new IQueueContainer InternalContainer
		{
			get
			{
				return (IQueueContainer)base.InternalContainer;
			}
		}

		public TimeSpan IQueueOperationsTimeout
		{
			get
			{
				return base.Timeout;
			}
			set
			{
				try
				{
					base.Timeout = StorageStampHelpers.AdjustTimeoutRange(value);
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		internal QueueContainer(IQueueContainer queueContainer) : base(queueContainer)
		{
		}

		public IAsyncResult BeginClearQueue(AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("QueueContainer.ClearQueue", callback, state);
			asyncIteratorContext.Begin(this.ClearQueueImpl(asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginDeleteMessage(IQueueMessageReceipt receipt, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<bool> asyncIteratorContext = new AsyncIteratorContext<bool>("QueueContainer.DeleteMessage", callback, state);
			asyncIteratorContext.Begin(this.DeleteMessageImpl(receipt, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetMessage(int numberOfMessages, TimeSpan visibilityTimeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IEnumerable<IMessageData>> asyncIteratorContext = new AsyncIteratorContext<IEnumerable<IMessageData>>("QueueContainer.GetMesasage", callback, state);
			asyncIteratorContext.Begin(this.GetMessageImpl(numberOfMessages, visibilityTimeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetQueueStatistics(bool includeInvisibleMessages, bool includeExpiredMessages, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IQueueStatistics> asyncIteratorContext = new AsyncIteratorContext<IQueueStatistics>("QueueContainer.GetQueueStatistics", callback, state);
			asyncIteratorContext.Begin(this.GetQueueStatisticsImpl(includeInvisibleMessages, includeExpiredMessages, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginListMessages(string queueNameStart, DateTime? visibilityStart, Guid messageIdStart, int? subQueueId, bool includeInvisibleMessages, int numberOfMessages, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ListMessagesResult> asyncIteratorContext = new AsyncIteratorContext<ListMessagesResult>("QueueContainer.ListMesasages", callback, state);
			asyncIteratorContext.Begin(this.ListMessagesImpl(queueNameStart, visibilityStart, messageIdStart, subQueueId, includeInvisibleMessages, numberOfMessages, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginPeekMessage(int numberOfMessages, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IEnumerable<IMessageData>> asyncIteratorContext = new AsyncIteratorContext<IEnumerable<IMessageData>>("QueueContainer.PeekMesasage", callback, state);
			asyncIteratorContext.Begin(this.PeekMessageImpl(numberOfMessages, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginPutMessage(List<PushedMessage> messagesList, bool usePutMessageRowCommand, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<List<IMessageData>> asyncIteratorContext = new AsyncIteratorContext<List<IMessageData>>("QueueContainer.PutMesasage", callback, state);
			asyncIteratorContext.Begin(this.PutMessageImpl(messagesList, usePutMessageRowCommand, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginUpdateMessage(IQueueMessageReceipt receipt, byte[] body, TimeSpan visibilityTimeout, TimeSpan? timeToLive, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IQueueMessageReceipt> asyncIteratorContext = new AsyncIteratorContext<IQueueMessageReceipt>("QueueContainer.UpdateMessage", callback, state);
			asyncIteratorContext.Begin(this.UpdateMessageImpl(receipt, body, visibilityTimeout, timeToLive, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IEnumerator<IAsyncResult> ClearQueueImpl(AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			RemainingTime remainingTime = new RemainingTime(base.Timeout);
			try
			{
				this.InternalContainer.Timeout = StorageStampHelpers.AdjustTimeoutRange(remainingTime);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			try
			{
				asyncResult = this.InternalContainer.BeginGetProperties(ContainerPropertyNames.None, null, context.GetResumeCallback(), context.GetResumeState("QueueContainer.GetMessageImpl"));
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndGetProperties(asyncResult);
			}
			catch (Exception exception2)
			{
				StorageStamp.TranslateException(exception2);
				throw;
			}
			try
			{
				this.InternalContainer.Timeout = remainingTime;
				asyncResult = this.InternalContainer.BeginClearQueue(context.GetResumeCallback(), context.GetResumeState("QueueContainer.GetMessageImpl"));
			}
			catch (Exception exception3)
			{
				StorageStamp.TranslateException(exception3);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndClearQueue(asyncResult);
			}
			catch (Exception exception4)
			{
				StorageStamp.TranslateException(exception4);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> DeleteMessageImpl(IQueueMessageReceipt receipt, AsyncIteratorContext<bool> context)
		{
			IAsyncResult asyncResult;
			RemainingTime remainingTime = new RemainingTime(base.Timeout);
			try
			{
				this.InternalContainer.Timeout = StorageStampHelpers.AdjustTimeoutRange(remainingTime);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			try
			{
				asyncResult = this.InternalContainer.BeginGetProperties(ContainerPropertyNames.None, null, context.GetResumeCallback(), context.GetResumeState("QueueContainer.GetMessageImpl"));
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndGetProperties(asyncResult);
			}
			catch (Exception exception2)
			{
				StorageStamp.TranslateException(exception2);
				throw;
			}
			try
			{
				this.InternalContainer.Timeout = remainingTime;
				asyncResult = this.InternalContainer.BeginDeleteMessage(receipt, context.GetResumeCallback(), context.GetResumeState("QueueContainer.GetMessageImpl"));
			}
			catch (Exception exception3)
			{
				StorageStamp.TranslateException(exception3);
				throw;
			}
			yield return asyncResult;
			try
			{
				context.ResultData = this.InternalContainer.EndDeleteMessage(asyncResult);
			}
			catch (Exception exception4)
			{
				StorageStamp.TranslateException(exception4);
				throw;
			}
		}

		public void EndClearQueue(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public bool EndDeleteMessage(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<bool> asyncIteratorContext = (AsyncIteratorContext<bool>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public IEnumerable<IMessageData> EndGetMessage(IAsyncResult ar)
		{
			return this.EndPeekMessage(ar);
		}

		public IQueueStatistics EndGetQueueStatistics(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<IQueueStatistics> asyncIteratorContext = (AsyncIteratorContext<IQueueStatistics>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public ListMessagesResult EndListMessages(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<ListMessagesResult> asyncIteratorContext = (AsyncIteratorContext<ListMessagesResult>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public IEnumerable<IMessageData> EndPeekMessage(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<IEnumerable<IMessageData>> asyncIteratorContext = (AsyncIteratorContext<IEnumerable<IMessageData>>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public List<IMessageData> EndPutMessage(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<List<IMessageData>> asyncIteratorContext = (AsyncIteratorContext<List<IMessageData>>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public IQueueMessageReceipt EndUpdateMessage(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<IQueueMessageReceipt> asyncIteratorContext = (AsyncIteratorContext<IQueueMessageReceipt>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		private IEnumerator<IAsyncResult> GetMessageImpl(int numberOfMessages, TimeSpan visibilityTimeout, AsyncIteratorContext<IEnumerable<IMessageData>> context)
		{
			IAsyncResult asyncResult;
			RemainingTime remainingTime = new RemainingTime(base.Timeout);
			try
			{
				this.InternalContainer.Timeout = remainingTime;
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			try
			{
				asyncResult = this.InternalContainer.BeginGetProperties(ContainerPropertyNames.None, null, context.GetResumeCallback(), context.GetResumeState("QueueContainer.GetMessageImpl"));
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndGetProperties(asyncResult);
			}
			catch (Exception exception2)
			{
				StorageStamp.TranslateException(exception2);
				throw;
			}
			try
			{
				this.InternalContainer.Timeout = remainingTime;
				asyncResult = this.InternalContainer.BeginGetMessage(numberOfMessages, visibilityTimeout, context.GetResumeCallback(), context.GetResumeState("QueueContainer.GetMessageImpl"));
			}
			catch (Exception exception3)
			{
				StorageStamp.TranslateException(exception3);
				throw;
			}
			yield return asyncResult;
			try
			{
				context.ResultData = this.InternalContainer.EndGetMessage(asyncResult);
			}
			catch (Exception exception4)
			{
				StorageStamp.TranslateException(exception4);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> GetQueueStatisticsImpl(bool includeInvisibleMessages, bool includeExpiredMessages, AsyncIteratorContext<IQueueStatistics> context)
		{
			IAsyncResult asyncResult;
			RemainingTime remainingTime = new RemainingTime(base.Timeout);
			try
			{
				this.InternalContainer.Timeout = StorageStampHelpers.AdjustTimeoutRange(remainingTime);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			try
			{
				asyncResult = this.InternalContainer.BeginGetProperties(ContainerPropertyNames.None, null, context.GetResumeCallback(), context.GetResumeState("QueueContainer.GetMessageImpl"));
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndGetProperties(asyncResult);
			}
			catch (Exception exception2)
			{
				StorageStamp.TranslateException(exception2);
				throw;
			}
			try
			{
				this.InternalContainer.Timeout = remainingTime;
				asyncResult = this.InternalContainer.BeginGetQueueStatistics(includeInvisibleMessages, includeExpiredMessages, context.GetResumeCallback(), context.GetResumeState("QueueContainer.GetMessageImpl"));
			}
			catch (Exception exception3)
			{
				StorageStamp.TranslateException(exception3);
				throw;
			}
			yield return asyncResult;
			try
			{
				context.ResultData = this.InternalContainer.EndGetQueueStatistics(asyncResult);
			}
			catch (Exception exception4)
			{
				StorageStamp.TranslateException(exception4);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> ListMessagesImpl(string queueNameStart, DateTime? visibilityStart, Guid messageIdStart, int? subQueueId, bool includeInvisibleMessages, int numberOfMessages, AsyncIteratorContext<ListMessagesResult> context)
		{
			IAsyncResult asyncResult;
			RemainingTime remainingTime = new RemainingTime(base.Timeout);
			try
			{
				this.InternalContainer.Timeout = StorageStampHelpers.AdjustTimeoutRange(remainingTime);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			try
			{
				asyncResult = this.InternalContainer.BeginGetProperties(ContainerPropertyNames.None, null, context.GetResumeCallback(), context.GetResumeState("QueueContainer.ListMessagesImpl"));
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndGetProperties(asyncResult);
			}
			catch (Exception exception2)
			{
				StorageStamp.TranslateException(exception2);
				throw;
			}
			try
			{
				this.InternalContainer.Timeout = remainingTime;
				asyncResult = this.InternalContainer.BeginListMessages(queueNameStart, visibilityStart, messageIdStart, subQueueId, includeInvisibleMessages, numberOfMessages, context.GetResumeCallback(), context.GetResumeState("QueueContainer.ListMessagesImpl"));
			}
			catch (Exception exception3)
			{
				StorageStamp.TranslateException(exception3);
				throw;
			}
			yield return asyncResult;
			try
			{
				context.ResultData = this.InternalContainer.EndListMessages(asyncResult);
			}
			catch (Exception exception4)
			{
				StorageStamp.TranslateException(exception4);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> PeekMessageImpl(int numberOfMessages, AsyncIteratorContext<IEnumerable<IMessageData>> context)
		{
			IAsyncResult asyncResult;
			RemainingTime remainingTime = new RemainingTime(base.Timeout);
			try
			{
				this.InternalContainer.Timeout = StorageStampHelpers.AdjustTimeoutRange(remainingTime);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			try
			{
				asyncResult = this.InternalContainer.BeginGetProperties(ContainerPropertyNames.None, null, context.GetResumeCallback(), context.GetResumeState("QueueContainer.PeekMessageImpl"));
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndGetProperties(asyncResult);
			}
			catch (Exception exception2)
			{
				StorageStamp.TranslateException(exception2);
				throw;
			}
			try
			{
				this.InternalContainer.Timeout = remainingTime;
				asyncResult = this.InternalContainer.BeginPeekMessage(numberOfMessages, context.GetResumeCallback(), context.GetResumeState("QueueContainer.PeekMessageImpl"));
			}
			catch (Exception exception3)
			{
				StorageStamp.TranslateException(exception3);
				throw;
			}
			yield return asyncResult;
			try
			{
				context.ResultData = this.InternalContainer.EndPeekMessage(asyncResult);
			}
			catch (Exception exception4)
			{
				StorageStamp.TranslateException(exception4);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> PutMessageImpl(List<PushedMessage> messagesList, bool usePutMessageRowCommand, AsyncIteratorContext<List<IMessageData>> context)
		{
			IAsyncResult asyncResult;
			RemainingTime remainingTime = new RemainingTime(base.Timeout);
			try
			{
				this.InternalContainer.Timeout = StorageStampHelpers.AdjustTimeoutRange(remainingTime);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			try
			{
				asyncResult = this.InternalContainer.BeginGetProperties(ContainerPropertyNames.None, null, context.GetResumeCallback(), context.GetResumeState("QueueContainer.PutMessageImpl"));
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndGetProperties(asyncResult);
			}
			catch (Exception exception2)
			{
				StorageStamp.TranslateException(exception2);
				throw;
			}
			try
			{
				this.InternalContainer.Timeout = remainingTime;
				asyncResult = this.InternalContainer.BeginPutMessage(messagesList, usePutMessageRowCommand, context.GetResumeCallback(), context.GetResumeState("QueueContainer.PutMessageImpl"));
			}
			catch (Exception exception3)
			{
				StorageStamp.TranslateException(exception3);
				throw;
			}
			yield return asyncResult;
			try
			{
				context.ResultData = this.InternalContainer.EndPutMessage(asyncResult);
			}
			catch (Exception exception4)
			{
				StorageStamp.TranslateException(exception4);
				throw;
			}
		}

		private IEnumerator<IAsyncResult> UpdateMessageImpl(IQueueMessageReceipt receipt, byte[] body, TimeSpan visibilityTimeout, TimeSpan? timeToLive, AsyncIteratorContext<IQueueMessageReceipt> context)
		{
			IAsyncResult asyncResult;
			RemainingTime remainingTime = new RemainingTime(base.Timeout);
			try
			{
				this.InternalContainer.Timeout = StorageStampHelpers.AdjustTimeoutRange(remainingTime);
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			try
			{
				asyncResult = this.InternalContainer.BeginGetProperties(ContainerPropertyNames.None, null, context.GetResumeCallback(), context.GetResumeState("QueueContainer.GetMessageImpl"));
			}
			catch (Exception exception1)
			{
				StorageStamp.TranslateException(exception1);
				throw;
			}
			yield return asyncResult;
			try
			{
				this.InternalContainer.EndGetProperties(asyncResult);
			}
			catch (Exception exception2)
			{
				StorageStamp.TranslateException(exception2);
				throw;
			}
			try
			{
				this.InternalContainer.Timeout = remainingTime;
				asyncResult = this.InternalContainer.BeginUpdateMessage(receipt, body, visibilityTimeout, timeToLive, context.GetResumeCallback(), context.GetResumeState("QueueContainer.GetMessageImpl"));
			}
			catch (Exception exception3)
			{
				StorageStamp.TranslateException(exception3);
				throw;
			}
			yield return asyncResult;
			try
			{
				context.ResultData = this.InternalContainer.EndUpdateMessage(asyncResult);
			}
			catch (Exception exception4)
			{
				StorageStamp.TranslateException(exception4);
				throw;
			}
		}

		private class ContainerMessageData : IMessageData
		{
			public int DequeueCount
			{
				get
				{
					return get_DequeueCount();
				}
				set
				{
					set_DequeueCount(value);
				}
			}

			private int <DequeueCount>k__BackingField;

			public int get_DequeueCount()
			{
				return this.<DequeueCount>k__BackingField;
			}

			public void set_DequeueCount(int value)
			{
				this.<DequeueCount>k__BackingField = value;
			}

			public DateTime ExpiryTime
			{
				get
				{
					return get_ExpiryTime();
				}
				set
				{
					set_ExpiryTime(value);
				}
			}

			private DateTime <ExpiryTime>k__BackingField;

			public DateTime get_ExpiryTime()
			{
				return this.<ExpiryTime>k__BackingField;
			}

			public void set_ExpiryTime(DateTime value)
			{
				this.<ExpiryTime>k__BackingField = value;
			}

			public Guid Id
			{
				get
				{
					return get_Id();
				}
				set
				{
					set_Id(value);
				}
			}

			private Guid <Id>k__BackingField;

			public Guid get_Id()
			{
				return this.<Id>k__BackingField;
			}

			public void set_Id(Guid value)
			{
				this.<Id>k__BackingField = value;
			}

			public DateTime InsertionTime
			{
				get
				{
					return get_InsertionTime();
				}
				set
				{
					set_InsertionTime(value);
				}
			}

			private DateTime <InsertionTime>k__BackingField;

			public DateTime get_InsertionTime()
			{
				return this.<InsertionTime>k__BackingField;
			}

			public void set_InsertionTime(DateTime value)
			{
				this.<InsertionTime>k__BackingField = value;
			}

			public byte[] Message
			{
				get
				{
					return get_Message();
				}
				set
				{
					set_Message(value);
				}
			}

			private byte[] <Message>k__BackingField;

			public byte[] get_Message()
			{
				return this.<Message>k__BackingField;
			}

			public void set_Message(byte[] value)
			{
				this.<Message>k__BackingField = value;
			}

			public DateTime VisibilityStart
			{
				get
				{
					return get_VisibilityStart();
				}
				set
				{
					set_VisibilityStart(value);
				}
			}

			private DateTime <VisibilityStart>k__BackingField;

			public DateTime get_VisibilityStart()
			{
				return this.<VisibilityStart>k__BackingField;
			}

			public void set_VisibilityStart(DateTime value)
			{
				this.<VisibilityStart>k__BackingField = value;
			}

			public ContainerMessageData(DateTime expiryTime, byte[] message, Guid id, DateTime visibilityStart)
			{
				this.ExpiryTime = expiryTime;
				this.Message = message;
				this.Id = id;
				this.VisibilityStart = visibilityStart;
			}
		}
	}
}