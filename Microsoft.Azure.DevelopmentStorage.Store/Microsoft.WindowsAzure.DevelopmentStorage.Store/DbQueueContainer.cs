using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbQueueContainer : IQueueContainer, IContainer, IDisposable, IQueueOperations
	{
		private Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer _queue;

		public IStorageAccount Account
		{
			get
			{
				return JustDecompileGenerated_get_Account();
			}
			set
			{
				JustDecompileGenerated_set_Account(value);
			}
		}

		private IStorageAccount JustDecompileGenerated_Account_k__BackingField;

		public IStorageAccount JustDecompileGenerated_get_Account()
		{
			return this.JustDecompileGenerated_Account_k__BackingField;
		}

		private void JustDecompileGenerated_set_Account(IStorageAccount value)
		{
			this.JustDecompileGenerated_Account_k__BackingField = value;
		}

		public byte[] ApplicationMetadata
		{
			get
			{
				return this._queue.Metadata;
			}
			set
			{
				this._queue.Metadata = value;
			}
		}

		public string ContainerName
		{
			get
			{
				return this._queue.QueueName;
			}
		}

		public ILeaseInfo LeaseInfo
		{
			get
			{
				return JustDecompileGenerated_get_LeaseInfo();
			}
			set
			{
				JustDecompileGenerated_set_LeaseInfo(value);
			}
		}

		private ILeaseInfo JustDecompileGenerated_LeaseInfo_k__BackingField;

		public ILeaseInfo JustDecompileGenerated_get_LeaseInfo()
		{
			return this.JustDecompileGenerated_LeaseInfo_k__BackingField;
		}

		private void JustDecompileGenerated_set_LeaseInfo(ILeaseInfo value)
		{
			this.JustDecompileGenerated_LeaseInfo_k__BackingField = value;
		}

		Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.ContainerType
		{
			get
			{
				return Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.QueueContainer;
			}
		}

		DateTime? Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.LastModificationTime
		{
			get
			{
				return new DateTime?(this._queue.LastModificationTime);
			}
		}

		byte[] Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.ServiceMetadata
		{
			get
			{
				return this._queue.ServiceMetadata;
			}
			set
			{
				this._queue.ServiceMetadata = value;
			}
		}

		TimeSpan Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.IQueueOperationsTimeout
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get;
			set;
		}

		internal DbStorageManager StorageManager
		{
			get;
			private set;
		}

		public TimeSpan Timeout
		{
			get;
			set;
		}

		internal DbQueueContainer(DbStorageAccount account, string queueName) : this(account, new Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer()
		{
			AccountName = account.Name,
			QueueName = queueName
		})
		{
		}

		internal DbQueueContainer(DbStorageAccount account, Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer container)
		{
			StorageStampHelpers.CheckContainerName(container.QueueName, Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.QueueContainer, false);
			this.StorageManager = account.StorageManager;
			this._queue = container;
			this.OperationStatus = account.OperationStatus;
			this.Account = account;
		}

		internal static void CheckQueueContainerCondition(Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer, IContainerCondition condition)
		{
			if (condition != null && condition.IfModifiedSinceTime.HasValue && condition.IfModifiedSinceTime.Value >= queueContainer.LastModificationTime)
			{
				throw new ConditionNotMetException(null, null, null);
			}
			if (condition != null && condition.IfNotModifiedSinceTime.HasValue && condition.IfNotModifiedSinceTime.Value < queueContainer.LastModificationTime)
			{
				throw new ConditionNotMetException(null, null, null);
			}
		}

		private IEnumerator<IAsyncResult> ClearQueueImpl(AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan remaining) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = this.LoadQueueContainer(dbContext);
					dbContext.ClearQueue(queueContainer.AccountName, queueContainer.QueueName);
					this._queue = queueContainer;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbQueueContainer.ClearQueue"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> DeleteMessageImpl(IQueueMessageReceipt queueMessageReceipt, AsyncIteratorContext<bool> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<bool>((TimeSpan remaining) => {
				bool flag;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = this.LoadQueueContainer(dbContext);
					context.ResultData = false;
					Receipt receipt = (Receipt)queueMessageReceipt;
					Guid messageId = receipt.MessageId;
					DateTime visibilityStart = receipt.VisibilityStart;
					QueueMessage queueMessage = (
						from m in dbContext.QueueMessages
						where (m.AccountName == this._queue.AccountName) && (m.QueueName == this._queue.QueueName) && (m.VisibilityStartTime == visibilityStart) && (m.MessageId == messageId)
						select m).FirstOrDefault<QueueMessage>();
					if (queueMessage == null)
					{
						throw new MessageNotFoundException();
					}
					dbContext.QueueMessages.DeleteOnSubmit(queueMessage);
					dbContext.SubmitChanges();
					this._queue = queueContainer;
					flag = true;
				}
				return flag;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbQueueContainer.DeleteMessageImpl"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<bool>(asyncResult);
		}

		public void Dispose()
		{
		}

		private IEnumerator<IAsyncResult> GetMessageImpl(int numberOfMessages, TimeSpan visibilityTimeout, AsyncIteratorContext<IEnumerable<IMessageData>> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<List<IMessageData>>((TimeSpan remaining) => {
				List<IMessageData> messageDatas;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = this.LoadQueueContainer(dbContext);
					ISingleResult<QueueMessage> queueMessages = dbContext.DequeueMessages(this._queue.AccountName, this._queue.QueueName, new int?((int)visibilityTimeout.TotalSeconds), new int?(numberOfMessages));
					List<IMessageData> messageDatas1 = new List<IMessageData>();
					foreach (QueueMessage queueMessage in queueMessages)
					{
						messageDatas1.Add(new DbMessageData(queueMessage, false));
					}
					this._queue = queueContainer;
					messageDatas = messageDatas1;
				}
				return messageDatas;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbQueueContainer.GetMessage"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<List<IMessageData>>(asyncResult);
		}

		private IEnumerator<IAsyncResult> GetPropertiesImpl(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan remaining) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = this.LoadQueueContainer(dbContext);
					DbQueueContainer.CheckQueueContainerCondition(queueContainer, condition);
					this._queue = queueContainer;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbQueueContainer.GetPropertiesImpl"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> GetQueueStatisticsImpl(bool includeInvisibleMessages, bool includeExpiredMessages, AsyncIteratorContext<IQueueStatistics> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<DbQueueStatistics>((TimeSpan remaining) => {
				DbQueueStatistics dbQueueStatistic;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = this.LoadQueueContainer(dbContext);
					IQueryable<QueueMessage> queueMessages = 
						from m in dbContext.QueueMessages
						where (m.AccountName == this._queue.AccountName) && (m.QueueName == this._queue.QueueName)
						select m;
					if (!includeExpiredMessages)
					{
						queueMessages = 
							from m in queueMessages
							where m.VisibilityStartTime >= DateTime.UtcNow
							select m;
					}
					if (!includeExpiredMessages)
					{
						queueMessages = 
							from m in queueMessages
							where m.ExpiryTime <= DateTime.UtcNow
							select m;
					}
					var variable = (
						from m in queueMessages
						group m by m.AccountName into s
						select new { TotalMessages = s.Count<QueueMessage>(), TotalSize = s.Sum<QueueMessage>((QueueMessage m) => m.Data.Length) }).FirstOrDefault();
					this._queue = queueContainer;
					dbQueueStatistic = (variable == null ? new DbQueueStatistics((long)0, (long)0) : new DbQueueStatistics((long)variable.TotalMessages, (long)variable.TotalSize));
				}
				return dbQueueStatistic;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbQueueContainer.GetQueueStatistics"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<DbQueueStatistics>(asyncResult);
		}

		private IEnumerator<IAsyncResult> ListMessagesImpl(string queueNameStart, DateTime? visibilityStart, Guid messageIdStart, int? subQueueId, bool includeInvisibleMessages, int numberOfMessages, AsyncIteratorContext<ListMessagesResult> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<ListMessagesResult>((TimeSpan remaining) => {
				ListMessagesResult listMessagesResults;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = this.LoadQueueContainer(dbContext);
					this._queue = queueContainer;
					queueNameStart = (string.IsNullOrEmpty(queueNameStart) ? this._queue.QueueName : queueNameStart);
					DateTime? nullable = visibilityStart;
					visibilityStart = new DateTime?((nullable.HasValue ? nullable.GetValueOrDefault() : DateTime.UtcNow));
					IQueryable<QueueMessage> queueMessages = 
						from m in dbContext.QueueMessages
						select m;
					queueMessages = 
						from m in queueMessages
						where (m.AccountName == this._queue.AccountName) && (m.QueueName == queueNameStart) && (m.ExpiryTime >= DateTime.UtcNow)
						select m;
					if (messageIdStart != Guid.Empty)
					{
						queueMessages = (!includeInvisibleMessages ? 
							from m in queueMessages
							where ((DateTime?)m.VisibilityStartTime > visibilityStart) && (m.VisibilityStartTime <= DateTime.UtcNow) || ((DateTime?)m.VisibilityStartTime == visibilityStart) && (m.VisibilityStartTime <= DateTime.UtcNow) && string.Compare(m.MessageId.ToString(), messageIdStart.ToString(), true) >= 0
							select m : 
							from m in queueMessages
							where ((DateTime?)m.VisibilityStartTime > visibilityStart) || ((DateTime?)m.VisibilityStartTime == visibilityStart) && string.Compare(m.MessageId.ToString(), messageIdStart.ToString(), true) >= 0
							select m);
					}
					else if (!includeInvisibleMessages)
					{
						queueMessages = 
							from m in queueMessages
							where (DateTime?)m.VisibilityStartTime <= visibilityStart
							select m;
					}
					queueMessages = (
						from m in queueMessages
						orderby m.VisibilityStartTime
						select m).Take<QueueMessage>(numberOfMessages);
					ListMessagesResult listMessagesResults1 = new ListMessagesResult();
					List<IMessageData> messageDatas = new List<IMessageData>();
					int num = 1;
					QueueMessage queueMessage = null;
					foreach (QueueMessage queueMessage1 in queueMessages)
					{
						queueMessage = queueMessage1;
						messageDatas.Add(new DbMessageData(queueMessage1, true));
						num++;
					}
					if (num > numberOfMessages && queueMessage != null)
					{
						IQueryable<QueueMessage> queueMessages1 = (
							from msg in dbContext.QueueMessages
							where (msg.AccountName == queueMessage.AccountName) && (msg.QueueName == queueMessage.QueueName) && ((msg.VisibilityStartTime > queueMessage.VisibilityStartTime) || (msg.VisibilityStartTime == queueMessage.VisibilityStartTime) && string.Compare(msg.MessageId.ToString(), queueMessage.MessageId.ToString(), true) > 0)
							orderby msg.VisibilityStartTime, msg.MessageId
							select msg).Take<QueueMessage>(1);
						if (queueMessages1 != null && queueMessages1.Count<QueueMessage>() > 0)
						{
							QueueMessage queueMessage2 = queueMessages1.First<QueueMessage>();
							listMessagesResults1.NextMarker = SummaryResult.EncodeMarker<ListMessagesMarker>(new ListMessagesMarker(queueMessage2.QueueName, 0, new DateTime?(queueMessage2.VisibilityStartTime), queueMessage2.MessageId));
						}
					}
					listMessagesResults1.Messages = messageDatas;
					listMessagesResults = listMessagesResults1;
				}
				return listMessagesResults;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbQueueContainer.PeekMessage"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<ListMessagesResult>(asyncResult);
		}

		private Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer LoadQueueContainer(DevelopmentStorageDbDataContext context)
		{
			return DbQueueContainer.LoadQueueContainer(context, this._queue);
		}

		internal static Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer LoadQueueContainer(DevelopmentStorageDbDataContext context, Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queue)
		{
			StorageStampHelpers.CheckContainerName(queue.QueueName, Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType.QueueContainer, false);
			Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = (
				from c in context.QueueContainers
				where (c.AccountName == queue.AccountName) && (c.QueueName == queue.QueueName)
				select c).FirstOrDefault<Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer>();
			if (queueContainer == null)
			{
				throw new ContainerNotFoundException();
			}
			return queueContainer;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbQueueContainer.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbQueueContainer.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, bool shouldUpdateCacheEntryOnRefresh, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbQueueContainer.GetProperties", callback, state);
			asyncIteratorContext.Begin(this.GetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.BeginSetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbQueueContainer.SetProperties", callback, state);
			asyncIteratorContext.Begin(this.SetPropertiesImpl(propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.EndGetProperties(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IContainer.EndSetProperties(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.BeginClearQueue(AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbQueueContainer.ClearQueue", callback, state);
			asyncIteratorContext.Begin(this.ClearQueueImpl(asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.BeginDeleteMessage(IQueueMessageReceipt queueMessageReceipt, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<bool> asyncIteratorContext = new AsyncIteratorContext<bool>("DbQueueContainer.DeleteMessage", callback, state);
			asyncIteratorContext.Begin(this.DeleteMessageImpl(queueMessageReceipt, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.BeginGetMessage(int numberOfMessages, TimeSpan visibilityTimeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IEnumerable<IMessageData>> asyncIteratorContext = new AsyncIteratorContext<IEnumerable<IMessageData>>("DbQueueContainer.GetMessage", callback, state);
			asyncIteratorContext.Begin(this.GetMessageImpl(numberOfMessages, visibilityTimeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.BeginGetQueueStatistics(bool includeInvisibleMessages, bool includeExpiredMessages, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IQueueStatistics> asyncIteratorContext = new AsyncIteratorContext<IQueueStatistics>("DbQueueContainer.GetQueueStatistics", callback, state);
			asyncIteratorContext.Begin(this.GetQueueStatisticsImpl(includeInvisibleMessages, includeExpiredMessages, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.BeginListMessages(string queueNameStart, DateTime? visibilityStart, Guid messageIdStart, int? subQueueId, bool includeInvisibleMessages, int numberOfMessages, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ListMessagesResult> asyncIteratorContext = new AsyncIteratorContext<ListMessagesResult>("DbQueueContainer.PeekMessage", callback, state);
			asyncIteratorContext.Begin(this.ListMessagesImpl(queueNameStart, visibilityStart, messageIdStart, subQueueId, includeInvisibleMessages, numberOfMessages, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.BeginPeekMessage(int numberOfMessages, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IEnumerable<IMessageData>> asyncIteratorContext = new AsyncIteratorContext<IEnumerable<IMessageData>>("DbQueueContainer.PeekMessage", callback, state);
			asyncIteratorContext.Begin(this.PeekMessageImpl(numberOfMessages, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.BeginPutMessage(List<PushedMessage> messagesList, bool usePutMessageRowCommand, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<List<IMessageData>> asyncIteratorContext = new AsyncIteratorContext<List<IMessageData>>("DbQueueContainer.PutMessage", callback, state);
			asyncIteratorContext.Begin(this.PutMessageImpl(messagesList, usePutMessageRowCommand, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.BeginUpdateMessage(IQueueMessageReceipt receipt, byte[] body, TimeSpan visibilityTimeout, TimeSpan? timeToLive, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IQueueMessageReceipt> asyncIteratorContext = new AsyncIteratorContext<IQueueMessageReceipt>("DbQueueContainer.UpdateMessage", callback, state);
			asyncIteratorContext.Begin(this.UpdateMessageImpl(receipt, body, visibilityTimeout, timeToLive, asyncIteratorContext));
			return asyncIteratorContext;
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.EndClearQueue(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		bool Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.EndDeleteMessage(IAsyncResult ar)
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

		IEnumerable<IMessageData> Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.EndGetMessage(IAsyncResult ar)
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

		IQueueStatistics Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.EndGetQueueStatistics(IAsyncResult ar)
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

		ListMessagesResult Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.EndListMessages(IAsyncResult ar)
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

		IEnumerable<IMessageData> Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.EndPeekMessage(IAsyncResult ar)
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

		List<IMessageData> Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.EndPutMessage(IAsyncResult ar)
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

		IQueueMessageReceipt Microsoft.Cis.Services.Nephos.Common.Storage.IQueueOperations.EndUpdateMessage(IAsyncResult ar)
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

		private IEnumerator<IAsyncResult> PeekMessageImpl(int numberOfMessages, AsyncIteratorContext<IEnumerable<IMessageData>> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<List<IMessageData>>((TimeSpan remaining) => {
				List<IMessageData> messageDatas;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = this.LoadQueueContainer(dbContext);
					this._queue = queueContainer;
					IQueryable<QueueMessage> queueMessages = (
						from m in dbContext.QueueMessages
						where (m.AccountName == this._queue.AccountName) && (m.QueueName == this._queue.QueueName) && (m.VisibilityStartTime <= DateTime.UtcNow) && (m.ExpiryTime >= DateTime.UtcNow)
						orderby m.VisibilityStartTime
						select m).Take<QueueMessage>(numberOfMessages);
					List<IMessageData> messageDatas1 = new List<IMessageData>();
					foreach (QueueMessage queueMessage in queueMessages)
					{
						messageDatas1.Add(new DbMessageData(queueMessage, true));
					}
					messageDatas = messageDatas1;
				}
				return messageDatas;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbQueueContainer.PeekMessage"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<List<IMessageData>>(asyncResult);
		}

		private IEnumerator<IAsyncResult> PutMessageImpl(List<PushedMessage> messagesList, bool usePutMessageRowCommand, AsyncIteratorContext<List<IMessageData>> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<List<IMessageData>>((TimeSpan remaining) => {
				List<IMessageData> messageDatas;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					DateTime utcNow = DateTime.UtcNow;
					this.LoadQueueContainer(dbContext);
					List<QueueMessage> queueMessages = new List<QueueMessage>();
					List<IMessageData> messageDatas1 = new List<IMessageData>();
					object obj = messagesList;
					if (obj == null)
					{
						obj = Enumerable.Empty<PushedMessage>();
					}
					foreach (PushedMessage pushedMessage in (IEnumerable<PushedMessage>)obj)
					{
						QueueMessage queueMessage = new QueueMessage()
						{
							AccountName = this._queue.AccountName,
							QueueName = this._queue.QueueName,
							VisibilityStartTime = utcNow.Add(pushedMessage.VisibilityTimeout),
							MessageId = Guid.NewGuid()
						};
						QueueMessage queueMessage1 = queueMessage;
						TimeSpan? messageTTL = pushedMessage.MessageTTL;
						queueMessage1.ExpiryTime = utcNow.Add((messageTTL.HasValue ? messageTTL.GetValueOrDefault() : new TimeSpan((long)0)));
						queueMessage.InsertionTime = utcNow;
						queueMessage.DequeueCount = 0;
						queueMessage.Data = pushedMessage.MessageText;
						QueueMessage queueMessage2 = queueMessage;
						queueMessages.Add(queueMessage2);
						messageDatas1.Add(new DbMessageData(queueMessage2, false));
					}
					dbContext.QueueMessages.InsertAllOnSubmit<QueueMessage>(queueMessages);
					dbContext.SubmitChanges();
					messageDatas = messageDatas1;
				}
				return messageDatas;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbQueueContainer.PutMessage"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<List<IMessageData>>(asyncResult);
		}

		private IEnumerator<IAsyncResult> SetPropertiesImpl(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute((TimeSpan remaining) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer applicationMetadata = this.LoadQueueContainer(dbContext);
					if ((propertyNames & ContainerPropertyNames.ApplicationMetadata) != ContainerPropertyNames.None)
					{
						StorageStampHelpers.ValidateApplicationMetadata(this.ApplicationMetadata);
						applicationMetadata.Metadata = this.ApplicationMetadata;
					}
					if ((propertyNames & ContainerPropertyNames.ServiceMetadata) != ContainerPropertyNames.None)
					{
						applicationMetadata.ServiceMetadata = ((IContainer)this).ServiceMetadata;
					}
					dbContext.SubmitChanges();
					this._queue = applicationMetadata;
				}
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbQueueContainer.SetProperties"));
			yield return asyncResult;
			this.StorageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> UpdateMessageImpl(IQueueMessageReceipt queueMessageReceipt, byte[] body, TimeSpan visibilityTimeout, TimeSpan? timeToLive, AsyncIteratorContext<IQueueMessageReceipt> context)
		{
			IAsyncResult asyncResult = this.StorageManager.AsyncProcessor.BeginExecute<IQueueMessageReceipt>((TimeSpan remaining) => {
				IQueueMessageReceipt receipt;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer queueContainer = this.LoadQueueContainer(dbContext);
					this._queue = queueContainer;
					QueueMessage queueMessage = (
						from m in dbContext.QueueMessages
						where (m.AccountName == this._queue.AccountName) && (m.QueueName == this._queue.QueueName) && (m.MessageId == ((Receipt)queueMessageReceipt).MessageId) && (m.VisibilityStartTime == ((Receipt)queueMessageReceipt).VisibilityStart)
						select m).FirstOrDefault<QueueMessage>();
					if (queueMessage == null)
					{
						throw new MessageNotFoundException();
					}
					DateTime utcNow = DateTime.UtcNow;
					QueueMessage queueMessage1 = new QueueMessage()
					{
						AccountName = queueMessage.AccountName,
						QueueName = queueMessage.QueueName,
						VisibilityStartTime = utcNow.Add(visibilityTimeout),
						MessageId = queueMessage.MessageId,
						ExpiryTime = queueMessage.ExpiryTime,
						InsertionTime = queueMessage.InsertionTime,
						DequeueCount = queueMessage.DequeueCount,
						Data = body ?? queueMessage.Data
					};
					dbContext.QueueMessages.DeleteOnSubmit(queueMessage);
					dbContext.QueueMessages.InsertOnSubmit(queueMessage1);
					dbContext.SubmitChanges();
					receipt = new Receipt()
					{
						MessageId = ((Receipt)queueMessageReceipt).MessageId,
						VisibilityStart = queueMessage1.VisibilityStartTime,
						DequeueCount = queueMessage1.DequeueCount
					};
				}
				return receipt;
			}, this.Timeout, context.GetResumeCallback(), context.GetResumeState("DbQueueContainer.UpdateMessage"));
			yield return asyncResult;
			context.ResultData = this.StorageManager.AsyncProcessor.EndExecute<IQueueMessageReceipt>(asyncResult);
		}
	}
}