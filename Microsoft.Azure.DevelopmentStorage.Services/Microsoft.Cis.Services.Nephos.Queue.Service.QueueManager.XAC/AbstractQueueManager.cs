using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Service;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	public abstract class AbstractQueueManager : BaseServiceManager
	{
		public abstract Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		protected AbstractQueueManager()
		{
		}

		public IAsyncResult BeginClearQueue(IAccountIdentifier identity, string account, string queue, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("QueueManager.ClearQueue", callback, state);
			asyncIteratorContext.Begin(this.ClearQueueImpl(identity, account, queue, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginCreateQueue(IAccountIdentifier identity, string account, string queue, long defaultMessageTtl, long defaultMessageVisibilityTimeout, NameValueCollection metadata, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<CreateQueueResult> asyncIteratorContext = new AsyncIteratorContext<CreateQueueResult>("QueueManager.CreateQueue", callback, state);
			asyncIteratorContext.Begin(this.CreateQueueImpl(identity, account, queue, defaultMessageTtl, defaultMessageVisibilityTimeout, metadata, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginDeleteMessage(IAccountIdentifier identity, string account, string queue, string messageId, byte[] popReceipt, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("QueueManager.DeleteMessage", callback, state);
			asyncIteratorContext.Begin(this.DeleteMessageImpl(identity, account, queue, messageId, popReceipt, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginDeleteQueue(IAccountIdentifier identity, string account, string queue, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("QueueManager.DeleteQueue", callback, state);
			asyncIteratorContext.Begin(this.DeleteQueueImpl(identity, account, queue, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetMessages(IAccountIdentifier identity, string account, string queue, int numMessages, long? visibilityTimeout, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IEnumerable<PoppedMessage>> asyncIteratorContext = new AsyncIteratorContext<IEnumerable<PoppedMessage>>("QueueManager.PopMessages", callback, state);
			asyncIteratorContext.Begin(this.GetMessagesImpl(identity, account, queue, numMessages, visibilityTimeout, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetQueueAcl(IAccountIdentifier identifier, string account, string queue, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ContainerAclSettings> asyncIteratorContext = new AsyncIteratorContext<ContainerAclSettings>("QueueManager.GetQueueAcl", callback, state);
			asyncIteratorContext.Begin(this.GetQueueAclImpl(identifier, account, queue, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetQueueProperties(IAccountIdentifier identity, string account, string queue, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			return this.BeginGetQueueProperties(identity, account, queue, true, timeout, requestContext, callback, state);
		}

		public IAsyncResult BeginGetQueueProperties(IAccountIdentifier identity, string account, string queue, bool getMessageCount, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<QueueProperties> asyncIteratorContext = new AsyncIteratorContext<QueueProperties>("QueueManager.GetQueueProperties", callback, state);
			asyncIteratorContext.Begin(this.GetQueuePropertiesImpl(identity, account, queue, getMessageCount, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetQueueProperties(IAccountIdentifier identity, string account, string queue, bool getMessageCount, SASAuthorizationParameters sasParams, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<QueueProperties> asyncIteratorContext = new AsyncIteratorContext<QueueProperties>("QueueManager.GetQueueProperties", callback, state);
			asyncIteratorContext.Begin(this.GetQueuePropertiesImpl(identity, account, queue, getMessageCount, sasParams, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetQueueServiceProperties(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<AnalyticsSettings> asyncIteratorContext = new AsyncIteratorContext<AnalyticsSettings>("AbstractQueueManager.GetQueueServiceProperties", callback, state);
			asyncIteratorContext.Begin(this.GetQueueServicePropertiesImpl(identifier, ownerAccountName, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginGetQueueServiceStats(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<GeoReplicationStats> asyncIteratorContext = new AsyncIteratorContext<GeoReplicationStats>("AbstractQueueManager.GetQueueServiceStats", callback, state);
			asyncIteratorContext.Begin(this.GetQueueServiceStatsImpl(identifier, ownerAccountName, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginListMessages(IAccountIdentifier identity, string account, string queue, string messageMarker, bool includeInvisibleMessages, bool incldueMessageTextProperty, int maxMessages, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ListMessagesResult> asyncIteratorContext = new AsyncIteratorContext<ListMessagesResult>("QueueManager.PopMessages", callback, state);
			asyncIteratorContext.Begin(this.ListMessagesImpl(identity, account, queue, messageMarker, includeInvisibleMessages, incldueMessageTextProperty, maxMessages, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginListQueues(IAccountIdentifier identity, string account, string queuePrefix, string delimiter, string marker, int maxResults, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IListQueuesResultCollection> asyncIteratorContext = new AsyncIteratorContext<IListQueuesResultCollection>("QueueManager.ListQueues", callback, state);
			asyncIteratorContext.Begin(this.ListQueuesImpl(identity, account, queuePrefix, delimiter, marker, maxResults, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginPeekMessages(IAccountIdentifier identity, string account, string queue, int numMessages, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IEnumerable<PeekedMessage>> asyncIteratorContext = new AsyncIteratorContext<IEnumerable<PeekedMessage>>("QueueManager.PeekMessages", callback, state);
			asyncIteratorContext.Begin(this.PeekMessagesImpl(identity, account, queue, numMessages, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginPutMessage(IAccountIdentifier identity, string account, string queue, List<PushedMessage> messagesList, TimeSpan? timeout, RequestContext requestContext, bool usePutMessageRowCommand, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IEnumerable<PoppedMessage>> asyncIteratorContext = new AsyncIteratorContext<IEnumerable<PoppedMessage>>("QueueManager.PushMessage", callback, state);
			asyncIteratorContext.Begin(this.PutMessageImpl(identity, account, queue, messagesList, timeout, requestContext, usePutMessageRowCommand, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetQueueAcl(IAccountIdentifier identifier, string account, string queue, ContainerAclSettings acl, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("QueueManager.SetQueueAcl", callback, state);
			asyncIteratorContext.Begin(this.SetQueueAclImpl(identifier, account, queue, acl, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetQueueMetadata(IAccountIdentifier identity, string account, string queue, NameValueCollection metadata, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("QueueManager.SetQueueMetadata", callback, state);
			asyncIteratorContext.Begin(this.SetQueueMetadataImpl(identity, account, queue, metadata, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginSetQueueServiceProperties(IAccountIdentifier identifier, string ownerAccountName, AnalyticsSettings settings, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("AbstractQueueManager.SetQueueServiceProperties", callback, state);
			asyncIteratorContext.Begin(this.SetQueueServicePropertiesImpl(identifier, ownerAccountName, settings, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginUpdateMessage(IAccountIdentifier identity, string account, string queue, string messageId, byte[] popReceipt, byte[] body, TimeSpan visibilityTimeout, TimeSpan? timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<PoppedMessage> asyncIteratorContext = new AsyncIteratorContext<PoppedMessage>("QueueManager.UpdateMessage", callback, state);
			asyncIteratorContext.Begin(this.UpdateMessageImpl(identity, account, queue, messageId, popReceipt, body, visibilityTimeout, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public void ClearQueue(IAccountIdentifier identity, string account, string queue, TimeSpan? timeout)
		{
			this.EndClearQueue(this.BeginClearQueue(identity, account, queue, timeout, null, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> ClearQueueImpl(IAccountIdentifier identity, string account, string queue, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context);

		public void CreateQueue(IAccountIdentifier identity, string account, string queue, long defaultMessageTtl, long defaultMessageVisibilityTimeout, NameValueCollection metadata, TimeSpan? timeout)
		{
			this.EndCreateQueue(this.BeginCreateQueue(identity, account, queue, defaultMessageTtl, defaultMessageVisibilityTimeout, metadata, timeout, null, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> CreateQueueImpl(IAccountIdentifier identity, string account, string queue, long defaultMessageTtl, long defaultMessageVisibilityTimeout, NameValueCollection metadata, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<CreateQueueResult> context);

		public void DeleteMessage(IAccountIdentifier identity, string account, string queue, string messageId, byte[] popReceipt, TimeSpan? timeout)
		{
			this.EndDeleteMessage(this.BeginDeleteMessage(identity, account, queue, messageId, popReceipt, timeout, null, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> DeleteMessageImpl(IAccountIdentifier identity, string account, string queue, string messageId, byte[] popReceipt, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context);

		public void DeleteQueue(IAccountIdentifier identity, string account, string queue, TimeSpan? timeout)
		{
			this.EndDeleteQueue(this.BeginDeleteQueue(identity, account, queue, timeout, null, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> DeleteQueueImpl(IAccountIdentifier identity, string account, string queue, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context);

		public void EndClearQueue(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public CreateQueueResult EndCreateQueue(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<CreateQueueResult> asyncIteratorContext = (AsyncIteratorContext<CreateQueueResult>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public void EndDeleteMessage(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndDeleteQueue(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public IEnumerable<PoppedMessage> EndGetMessages(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<IEnumerable<PoppedMessage>> asyncIteratorContext = (AsyncIteratorContext<IEnumerable<PoppedMessage>>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public ContainerAclSettings EndGetQueueAcl(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<ContainerAclSettings> asyncIteratorContext = (AsyncIteratorContext<ContainerAclSettings>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public QueueProperties EndGetQueueProperties(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<QueueProperties> asyncIteratorContext = (AsyncIteratorContext<QueueProperties>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public AnalyticsSettings EndGetQueueServiceProperties(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<AnalyticsSettings> asyncIteratorContext = (AsyncIteratorContext<AnalyticsSettings>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public GeoReplicationStats EndGetQueueServiceStats(IAsyncResult ar)
		{
			return ar.End<GeoReplicationStats>(RethrowableWrapperBehavior.NoWrap);
		}

		public ListMessagesResult EndListMessages(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<ListMessagesResult> asyncIteratorContext = (AsyncIteratorContext<ListMessagesResult>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public IListQueuesResultCollection EndListQueues(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<IListQueuesResultCollection> asyncIteratorContext = (AsyncIteratorContext<IListQueuesResultCollection>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public IEnumerable<PeekedMessage> EndPeekMessages(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<IEnumerable<PeekedMessage>> asyncIteratorContext = (AsyncIteratorContext<IEnumerable<PeekedMessage>>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public IEnumerable<PoppedMessage> EndPutMessage(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<IEnumerable<PoppedMessage>> asyncIteratorContext = (AsyncIteratorContext<IEnumerable<PoppedMessage>>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public void EndSetQueueAcl(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndSetQueueMetadata(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public void EndSetQueueServiceProperties(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public PoppedMessage EndUpdateMessage(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<PoppedMessage> asyncIteratorContext = (AsyncIteratorContext<PoppedMessage>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public IEnumerable<PoppedMessage> GetMessages(IAccountIdentifier identity, string account, string queue, int numMessages, long? visibilityTimeout, TimeSpan? timeout)
		{
			return this.EndGetMessages(this.BeginGetMessages(identity, account, queue, numMessages, visibilityTimeout, timeout, null, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> GetMessagesImpl(IAccountIdentifier identity, string account, string queue, int numMessages, long? visibilityTimeout, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<IEnumerable<PoppedMessage>> context);

		protected abstract IEnumerator<IAsyncResult> GetQueueAclImpl(IAccountIdentifier identifier, string account, string queue, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ContainerAclSettings> context);

		public QueueProperties GetQueueProperties(IAccountIdentifier identity, string account, string queue, TimeSpan? timeout)
		{
			return this.EndGetQueueProperties(this.BeginGetQueueProperties(identity, account, queue, timeout, null, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> GetQueuePropertiesImpl(IAccountIdentifier identity, string account, string queue, bool getMessageCount, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<QueueProperties> context);

		protected abstract IEnumerator<IAsyncResult> GetQueuePropertiesImpl(IAccountIdentifier identity, string account, string queue, bool getMessageCount, SASAuthorizationParameters sasParams, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<QueueProperties> context);

		protected abstract IEnumerator<IAsyncResult> GetQueueServicePropertiesImpl(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<AnalyticsSettings> context);

		protected abstract IEnumerator<IAsyncResult> GetQueueServiceStatsImpl(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<GeoReplicationStats> context);

		protected abstract IEnumerator<IAsyncResult> ListMessagesImpl(IAccountIdentifier identity, string account, string queue, string messageMarker, bool includeInvisibleMessages, bool incldueMessageTextProperty, int maxMessages, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<ListMessagesResult> context);

		public IListQueuesResultCollection ListQueues(IAccountIdentifier identity, string account, string queuePrefix, string delimiter, string marker, int maxResults, TimeSpan? timeout)
		{
			return this.EndListQueues(this.BeginListQueues(identity, account, queuePrefix, delimiter, marker, maxResults, timeout, null, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> ListQueuesImpl(IAccountIdentifier identity, string account, string queuePrefix, string delimiter, string marker, int maxResults, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<IListQueuesResultCollection> context);

		public IEnumerable<PeekedMessage> PeekMessages(IAccountIdentifier identity, string account, string queue, int numMessages, TimeSpan? timeout)
		{
			return this.EndPeekMessages(this.BeginPeekMessages(identity, account, queue, numMessages, timeout, null, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> PeekMessagesImpl(IAccountIdentifier identity, string account, string queue, int numMessages, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<IEnumerable<PeekedMessage>> context);

		public IEnumerable<PoppedMessage> PutMessage(IAccountIdentifier identity, string account, string queue, TimeSpan visibilityTimeout, TimeSpan? messageTtl, byte[] body, TimeSpan? timeout)
		{
			List<PushedMessage> pushedMessages = new List<PushedMessage>()
			{
				new PushedMessage(body, visibilityTimeout, messageTtl)
			};
			return this.EndPutMessage(this.BeginPutMessage(identity, account, queue, pushedMessages, timeout, null, true, null, null));
		}

		public IEnumerable<PoppedMessage> PutMessage(IAccountIdentifier identity, string account, string queue, List<PushedMessage> messagesList, TimeSpan? timeout)
		{
			return this.EndPutMessage(this.BeginPutMessage(identity, account, queue, messagesList, timeout, null, true, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> PutMessageImpl(IAccountIdentifier identity, string account, string queue, List<PushedMessage> messagesList, TimeSpan? timeout, RequestContext requestContext, bool usePutMessageRowCommand, AsyncIteratorContext<IEnumerable<PoppedMessage>> context);

		protected abstract IEnumerator<IAsyncResult> SetQueueAclImpl(IAccountIdentifier identifier, string account, string queue, ContainerAclSettings acl, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context);

		public void SetQueueMetadata(IAccountIdentifier identity, string account, string queue, NameValueCollection metadata, TimeSpan? timeout)
		{
			this.EndSetQueueMetadata(this.BeginSetQueueMetadata(identity, account, queue, metadata, timeout, null, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> SetQueueMetadataImpl(IAccountIdentifier identity, string account, string queue, NameValueCollection metadata, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context);

		protected abstract IEnumerator<IAsyncResult> SetQueueServicePropertiesImpl(IAccountIdentifier identifier, string ownerAccountName, AnalyticsSettings settings, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context);

		public PoppedMessage UpdateMessage(IAccountIdentifier identity, string account, string queue, string messageId, byte[] popReceipt, byte[] body, TimeSpan visibilityTimeout, TimeSpan? timeout)
		{
			return this.EndUpdateMessage(this.BeginUpdateMessage(identity, account, queue, messageId, popReceipt, body, visibilityTimeout, timeout, null, null, null));
		}

		protected abstract IEnumerator<IAsyncResult> UpdateMessageImpl(IAccountIdentifier identity, string account, string queue, string messageId, byte[] popReceipt, byte[] body, TimeSpan visibilityTimeout, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<PoppedMessage> context);
	}
}