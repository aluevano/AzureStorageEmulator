using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Service;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	public abstract class RealQueueManager : AbstractQueueManager
	{
		private QueueManagerConfiguration config;

		public override Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		protected RealQueueManager(IStorageManager storageMgr, AuthorizationManager authMgr, QueueManagerConfiguration config)
		{
			this.authorizationManager = authMgr;
			this.storageManager = storageMgr;
			this.config = config;
		}

		private IAsyncResult BeginGetQueue(IAccountIdentifier identity, string account, string queue, PermissionLevel permission, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			return this.BeginGetQueue(identity, account, queue, permission, ContainerPropertyNames.None, timeout, requestContext, callback, state);
		}

		private IAsyncResult BeginGetQueue(IAccountIdentifier identity, string account, string queue, PermissionLevel permission, ContainerPropertyNames propertyNames, CacheRefreshOptions cacheRefreshOptions, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IQueueContainer> asyncIteratorContext = new AsyncIteratorContext<IQueueContainer>("QueueManager.GetQueue", callback, state);
			asyncIteratorContext.Begin(this.GetQueueImpl(identity, account, queue, permission, propertyNames, cacheRefreshOptions, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IAsyncResult BeginGetQueue(IAccountIdentifier identity, string account, string queue, PermissionLevel permission, ContainerPropertyNames propertyNames, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			return this.BeginGetQueue(identity, account, queue, permission, propertyNames, CacheRefreshOptions.None, timeout, requestContext, callback, state);
		}

		private IAsyncResult BeginGetQueue(IAccountIdentifier identity, string account, string queue, PermissionLevel permission, SASAuthorizationParameters sasParams, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			return this.BeginGetQueue(identity, account, queue, permission, sasParams, ContainerPropertyNames.None, timeout, requestContext, callback, state);
		}

		private IAsyncResult BeginGetQueue(IAccountIdentifier identity, string account, string queue, PermissionLevel permission, SASAuthorizationParameters sasParams, ContainerPropertyNames propertyNames, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IQueueContainer> asyncIteratorContext = new AsyncIteratorContext<IQueueContainer>("QueueManager.GetQueue", callback, state);
			asyncIteratorContext.Begin(this.GetQueueImpl(identity, account, queue, permission, sasParams, propertyNames, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		protected override IEnumerator<IAsyncResult> ClearQueueImpl(IAccountIdentifier identity, string account, string queue, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			RemainingTime remainingTime = new RemainingTime(timeout);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Delete
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.BeginGetQueue(identity, account, queue, PermissionLevel.Delete, sASAuthorizationParameter1, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.ClearQueueImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			operationStatus.Timeout = remainingTime;
			asyncResult = operationStatus.BeginClearQueue(context.GetResumeCallback(), context.GetResumeState("QueueManager.ClearQueueImpl"));
			yield return asyncResult;
			operationStatus.EndClearQueue(asyncResult);
		}

		protected override IEnumerator<IAsyncResult> CreateQueueImpl(IAccountIdentifier identity, string account, string queue, long defaultMessageTtl, long defaultMessageVisibilityTimeout, NameValueCollection metadata, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<CreateQueueResult> context)
		{
			RemainingTime remainingTime = new RemainingTime(timeout);
			List<SASPermission> sASPermissions = new List<SASPermission>()
			{
				SASPermission.Write,
				SASPermission.Create
			};
			SASAuthorizationParameters sASAuthorizationParameter = this.authorizationManager.CheckAccessWithMultiplePermissions(identity, account, null, null, PermissionLevel.Write, SasType.AccountSas, SasResourceType.Container, sASPermissions, remainingTime);
			if (metadata == null)
			{
				metadata = new NameValueCollection();
			}
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(account);
			IAsyncResult asyncResult = base.BeginGetQueueProperties(identity, account, queue, false, sASAuthorizationParameter, new TimeSpan?(remainingTime), requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.CreateQueueImpl"));
			yield return asyncResult;
			QueueProperties queueProperty = null;
			bool flag = true;
			try
			{
				queueProperty = base.EndGetQueueProperties(asyncResult);
			}
			catch (ContainerNotFoundException containerNotFoundException)
			{
				flag = false;
			}
			if (!flag)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Info.Log("Contacting XAC Server in order to create the queue");
				storageAccount.Timeout = remainingTime;
				DateTime? nullable = null;
				asyncResult = storageAccount.BeginCreateQueueContainer(queue, nullable, null, QueueHelpers.SerializeMetadata(metadata), context.GetResumeCallback(), context.GetResumeState("QueueManager.CreateQueueImpl"));
				yield return asyncResult;
				bool flag1 = false;
				try
				{
					storageAccount.EndCreateQueueContainer(asyncResult);
				}
				catch (ContainerAlreadyExistsException containerAlreadyExistsException)
				{
					flag1 = true;
				}
				if (flag1)
				{
					flag = true;
					asyncResult = base.BeginGetQueueProperties(identity, account, queue, false, new TimeSpan?(remainingTime), requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.CreateQueueImpl"));
					try
					{
						queueProperty = base.EndGetQueueProperties(asyncResult);
					}
					catch (ContainerNotFoundException containerNotFoundException1)
					{
						flag = false;
					}
					if (flag)
					{
						if (!QueueHelpers.IsMetadataSame(queueProperty.Metadata, metadata))
						{
							throw new ContainerAlreadyExistsException(string.Concat("Queue '", queue, "' already exists, and the metadata is different."));
						}
						context.ResultData = CreateQueueResult.AlreadyExists;
					}
				}
			}
			else
			{
				Logger<IRestProtocolHeadLogger>.Instance.Info.Log("Queue already exists no need to contact XAC Server.");
				if (!QueueHelpers.IsMetadataSame(queueProperty.Metadata, metadata))
				{
					throw new ContainerAlreadyExistsException(string.Concat("Queue '", queue, "' already exists, and the metadata is different."));
				}
				context.ResultData = CreateQueueResult.AlreadyExists;
			}
		}

		protected abstract IQueueMessageReceipt DecodeReceipt(byte[] data, Guid messageId);

		protected override IEnumerator<IAsyncResult> DeleteMessageImpl(IAccountIdentifier identity, string account, string queue, string messageId, byte[] popReceipt, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			RemainingTime remainingTime = new RemainingTime(timeout);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Process
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			NephosAssertionException.Assert(popReceipt != null);
			IAsyncResult asyncResult = this.BeginGetQueue(identity, account, queue, PermissionLevel.Delete, sASAuthorizationParameter1, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.DeleteMessageImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			try
			{
				Guid guid = new Guid(messageId);
			}
			catch (Exception exception)
			{
				throw new MessageNotFoundException("Invalid message name", exception);
			}
			IQueueMessageReceipt queueMessageReceipt = this.DecodeReceipt(popReceipt, new Guid(messageId));
			operationStatus.Timeout = remainingTime;
			asyncResult = operationStatus.BeginDeleteMessage(queueMessageReceipt, context.GetResumeCallback(), context.GetResumeState("QueueManager.DeleteMessageImpl"));
			yield return asyncResult;
			bool flag = operationStatus.EndDeleteMessage(asyncResult);
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] str = new object[] { flag, queueMessageReceipt.ToString() };
			verboseDebug.Log("DeleteMessage response: status={0} message={1}", str);
			if (!flag)
			{
				throw new MessageNotFoundException("The message could not be deleted or pop receipt invalid.");
			}
		}

		protected override IEnumerator<IAsyncResult> DeleteQueueImpl(IAccountIdentifier identity, string account, string queue, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			RemainingTime remainingTime = new RemainingTime(timeout);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Delete
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identity, account, null, null, PermissionLevel.Delete, sASAuthorizationParameter1, remainingTime, context.GetResumeCallback(), context.GetResumeState("QueueManager.DeleteQueueImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(account);
			storageAccount.Timeout = remainingTime;
			asyncResult = storageAccount.BeginDeleteQueueContainer(queue, null, context.GetResumeCallback(), context.GetResumeState("QueueManager.DeleteQueueImpl"));
			yield return asyncResult;
			storageAccount.EndDeleteQueueContainer(asyncResult);
		}

		protected abstract byte[] EncodeReceipt(IMessageData message);

		private IQueueContainer EndGetQueue(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<IQueueContainer> asyncIteratorContext = (AsyncIteratorContext<IQueueContainer>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		protected override IEnumerator<IAsyncResult> GetMessagesImpl(IAccountIdentifier identity, string account, string queue, int numMessages, long? visibilityTimeout, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<IEnumerable<PoppedMessage>> context)
		{
			object obj;
			this.ValidateNumMessages(numMessages, this.config.MaxMessagesToReturn);
			RemainingTime remainingTime = new RemainingTime(timeout);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Process
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.BeginGetQueue(identity, account, queue, PermissionLevel.ReadDelete, sASAuthorizationParameter1, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.GetMessagesImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			long? nullable = visibilityTimeout;
			obj = (nullable.HasValue ? nullable.GetValueOrDefault() : this.config.DefaultVisibilityTimeoutSeconds);
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)obj);
			operationStatus.Timeout = remainingTime;
			asyncResult = operationStatus.BeginGetMessage(numMessages, timeSpan, context.GetResumeCallback(), context.GetResumeState("QueueManager.GetMessagesImpl"));
			yield return asyncResult;
			IEnumerable<IMessageData> messageDatas = operationStatus.EndGetMessage(asyncResult);
			StringBuilder stringBuilder = new StringBuilder();
			context.ResultData = this.WrapGetMessageResults(messageDatas);
			int num = 0;
			foreach (PoppedMessage resultDatum in context.ResultData)
			{
				stringBuilder.Append(string.Format("[{0}],", resultDatum.ToString()));
				num++;
			}
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] str = new object[] { num, stringBuilder.ToString() };
			verboseDebug.Log("GetMessages response: Count={0} Messages={1}", str);
		}

		protected abstract PoppedMessage GetPoppedMessageInfoFromReceipt(IQueueMessageReceipt popreceipt);

		protected override IEnumerator<IAsyncResult> GetQueueAclImpl(IAccountIdentifier identifier, string account, string queue, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ContainerAclSettings> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(queue))
			{
				throw new ArgumentException("queue", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetQueueAcl");
			}
			RemainingTime remainingTime = new RemainingTime(timeout);
			IAsyncResult asyncResult = this.BeginGetQueue(identifier, account, queue, PermissionLevel.ReadAcl, ContainerPropertyNames.ServiceMetadata, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.GetQueueAclImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			context.ResultData = new ContainerAclSettings(operationStatus.ServiceMetadata);
		}

		private IEnumerator<IAsyncResult> GetQueueImpl(IAccountIdentifier identity, string account, string queue, PermissionLevel permission, ContainerPropertyNames propertyNames, CacheRefreshOptions cacheRefreshOptions, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IQueueContainer> context)
		{
			RemainingTime remainingTime = new RemainingTime(timeout);
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identity, account, null, null, permission, remainingTime, context.GetResumeCallback(), context.GetResumeState("QueueManager.GetQueueImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			string str = account;
			string str1 = queue;
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(str);
			IQueueContainer queueContainer = storageAccount.CreateQueueContainerInstance(str1);
			queueContainer.Timeout = remainingTime;
			asyncResult = queueContainer.BeginGetProperties(propertyNames, null, cacheRefreshOptions, context.GetResumeCallback(), context.GetResumeState("QueueManager.GetQueueImpl"));
			yield return asyncResult;
			queueContainer.EndGetProperties(asyncResult);
			context.ResultData = queueContainer;
		}

		private IEnumerator<IAsyncResult> GetQueueImpl(IAccountIdentifier identity, string account, string queue, PermissionLevel permission, SASAuthorizationParameters sasParams, ContainerPropertyNames propertyNames, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IQueueContainer> context)
		{
			RemainingTime remainingTime = new RemainingTime(timeout);
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identity, account, null, null, permission, sasParams, remainingTime, context.GetResumeCallback(), context.GetResumeState("QueueManager.GetQueueImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(account);
			IQueueContainer queueContainer = storageAccount.CreateQueueContainerInstance(queue);
			queueContainer.Timeout = remainingTime;
			asyncResult = queueContainer.BeginGetProperties(propertyNames, null, context.GetResumeCallback(), context.GetResumeState("QueueManager.GetQueueImpl"));
			yield return asyncResult;
			queueContainer.EndGetProperties(asyncResult);
			context.ResultData = queueContainer;
		}

		protected override IEnumerator<IAsyncResult> GetQueuePropertiesImpl(IAccountIdentifier identity, string account, string queue, bool getMessageCount, SASAuthorizationParameters sasParams, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<QueueProperties> context)
		{
			RemainingTime remainingTime = new RemainingTime(timeout);
			IAsyncResult asyncResult = this.BeginGetQueue(identity, account, queue, PermissionLevel.Read, sasParams, ContainerPropertyNames.ApplicationMetadata, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.GetQueuePropertiesImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			NameValueCollection nameValueCollection = QueueHelpers.DeserializeMetadata(operationStatus.ApplicationMetadata);
			long? nullable = null;
			if (getMessageCount)
			{
				operationStatus.Timeout = remainingTime;
				asyncResult = operationStatus.BeginGetQueueStatistics(true, true, context.GetResumeCallback(), context.GetResumeState("QueueManager.GetQueuePropertiesImpl"));
				yield return asyncResult;
				nullable = new long?(operationStatus.EndGetQueueStatistics(asyncResult).TotalMessages);
			}
			context.ResultData = new QueueProperties(account, queue, nullable, nameValueCollection);
		}

		protected override IEnumerator<IAsyncResult> GetQueuePropertiesImpl(IAccountIdentifier identity, string account, string queue, bool getMessageCount, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<QueueProperties> context)
		{
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			return this.GetQueuePropertiesImpl(identity, account, queue, getMessageCount, sASAuthorizationParameter1, timeout, requestContext, context);
		}

		protected override IEnumerator<IAsyncResult> GetQueueServicePropertiesImpl(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<AnalyticsSettings> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(ownerAccountName))
			{
				throw new ArgumentException("ownerAccountName", "Cannot be null");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetQueueServiceProperties");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, ownerAccountName, null, null, PermissionLevel.Read | PermissionLevel.Owner, sASAuthorizationParameter1, timeout, context.GetResumeCallback(), context.GetResumeState("RealQueueManager.GetQueueServicePropertiesImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(ownerAccountName);
			storageAccount.Timeout = timeout;
			AccountCondition accountCondition = new AccountCondition(false, false, storageAccount.LastModificationTime, null);
			asyncResult = storageAccount.BeginGetProperties(new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)65536)), accountCondition, context.GetResumeCallback(), context.GetResumeState("RealQueueManager.GetQueueServicePropertiesImpl"));
			yield return asyncResult;
			storageAccount.EndGetProperties(asyncResult);
			context.ResultData = storageAccount.ServiceMetadata.QueueAnalyticsSettings;
		}

		protected override IEnumerator<IAsyncResult> GetQueueServiceStatsImpl(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<GeoReplicationStats> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(ownerAccountName))
			{
				throw new ArgumentException("ownerAccountName", "Cannot be null");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetQueueServiceStats");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, ownerAccountName, null, null, PermissionLevel.Read | PermissionLevel.Owner, sASAuthorizationParameter1, timeout, context.GetResumeCallback(), context.GetResumeState("RealQueueManager.GetQueueServiceStatsImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(ownerAccountName);
			storageAccount.Timeout = timeout;
			AccountCondition accountCondition = new AccountCondition(false, false, storageAccount.LastModificationTime, null);
			asyncResult = storageAccount.BeginGetProperties(new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)1073741824)), accountCondition, context.GetResumeCallback(), context.GetResumeState("RealQueueManager.GetQueueServiceStatsImpl"));
			yield return asyncResult;
			storageAccount.EndGetProperties(asyncResult);
			context.ResultData = storageAccount.ServiceMetadata.QueueGeoReplicationStats;
		}

		protected override IEnumerator<IAsyncResult> ListMessagesImpl(IAccountIdentifier identity, string account, string queue, string messageMarker, bool includeInvisibleMessages, bool incldueMessageTextProperty, int maxMessages, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<ListMessagesResult> context)
		{
			string nextQueueStart;
			DateTime? nextVisibilityStart;
			Guid guid;
			int? subQueueId;
			object obj;
			this.ValidateNumMessages(maxMessages, this.config.MaxMessagesToReturnForListMessages);
			RemainingTime remainingTime = new RemainingTime(timeout);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.BeginGetQueue(identity, account, queue, PermissionLevel.Read, sASAuthorizationParameter1, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.PeekMessagesImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			operationStatus.Timeout = remainingTime;
			ListMessagesMarker listMessagesMarker = SummaryResult.DecodeMarker<ListMessagesMarker>(messageMarker);
			if (listMessagesMarker == null)
			{
				Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("ListMessageImpl marker is null");
			}
			else
			{
				IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
				verboseDebug.Log("ListMessageImpl marker.NextQueueStart: {0}", new object[] { listMessagesMarker.NextQueueStart });
				IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
				object[] objArray = new object[] { listMessagesMarker.SubQueueId };
				stringDataEventStream.Log("ListMessageImpl marker.SubQueueId: {0}", objArray);
				IStringDataEventStream verboseDebug1 = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
				object[] nextMessageIdStart = new object[] { listMessagesMarker.NextMessageIdStart };
				verboseDebug1.Log("ListMessageImpl marker.NextMessageIdStart: {0}", nextMessageIdStart);
				IStringDataEventStream stringDataEventStream1 = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
				object[] objArray1 = new object[1];
				object[] objArray2 = objArray1;
				obj = (listMessagesMarker.NextVisibilityStart.HasValue ? listMessagesMarker.NextVisibilityStart.Value.ToString("mm/dd/yy hh:mm:ss.fff") : "null");
				objArray2[0] = obj;
				stringDataEventStream1.Log("ListMessageImpl marker.NextVisibilityStart: {0}", objArray1);
			}
			IQueueContainer queueContainer = operationStatus;
			if (listMessagesMarker == null)
			{
				nextQueueStart = null;
			}
			else
			{
				nextQueueStart = listMessagesMarker.NextQueueStart;
			}
			if (listMessagesMarker == null)
			{
				nextVisibilityStart = null;
			}
			else
			{
				nextVisibilityStart = listMessagesMarker.NextVisibilityStart;
			}
			guid = (listMessagesMarker == null ? Guid.Empty : listMessagesMarker.NextMessageIdStart);
			if (listMessagesMarker == null)
			{
				subQueueId = null;
			}
			else
			{
				subQueueId = listMessagesMarker.SubQueueId;
			}
			asyncResult = queueContainer.BeginListMessages(nextQueueStart, nextVisibilityStart, guid, subQueueId, includeInvisibleMessages, maxMessages, context.GetResumeCallback(), context.GetResumeState("QueueManager.PeekMessagesImpl"));
			yield return asyncResult;
			ListMessagesResult listMessagesResults = operationStatus.EndListMessages(asyncResult);
			listMessagesResults.PoppedMessages = this.WrapGetMessageResults(listMessagesResults.Messages);
			context.ResultData = listMessagesResults;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (PoppedMessage poppedMessage in context.ResultData.PoppedMessages)
			{
				stringBuilder.Append(string.Format("[{0}],", poppedMessage.ToString()));
				num++;
			}
			IStringDataEventStream verboseDebug2 = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			object[] str = new object[] { num, stringBuilder.ToString() };
			verboseDebug2.Log("ListMessages response: Count={0} Messages={1}", str);
		}

		protected override IEnumerator<IAsyncResult> ListQueuesImpl(IAccountIdentifier identity, string account, string queuePrefix, string delimiter, string marker, int maxResults, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<IListQueuesResultCollection> context)
		{
			RemainingTime remainingTime = new RemainingTime(timeout);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.List
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identity, account, null, null, PermissionLevel.Read, sASAuthorizationParameter1, remainingTime, context.GetResumeCallback(), context.GetResumeState("QueueManager.ListQueuesImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(account);
			storageAccount.Timeout = remainingTime;
			asyncResult = storageAccount.BeginListQueueContainers(queuePrefix, ContainerPropertyNames.ApplicationMetadata, delimiter, marker, null, maxResults, context.GetResumeCallback(), context.GetResumeState("QueueManager.ListQueuesImpl"));
			yield return asyncResult;
			context.ResultData = new ListQueuesResult(storageAccount.EndListQueueContainers(asyncResult));
		}

		protected override IEnumerator<IAsyncResult> PeekMessagesImpl(IAccountIdentifier identity, string account, string queue, int numMessages, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<IEnumerable<PeekedMessage>> context)
		{
			this.ValidateNumMessages(numMessages, this.config.MaxMessagesToReturn);
			RemainingTime remainingTime = new RemainingTime(timeout);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.BeginGetQueue(identity, account, queue, PermissionLevel.Read, sASAuthorizationParameter1, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.PeekMessagesImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			operationStatus.Timeout = remainingTime;
			asyncResult = operationStatus.BeginPeekMessage(numMessages, context.GetResumeCallback(), context.GetResumeState("QueueManager.PeekMessagesImpl"));
			yield return asyncResult;
			IEnumerable<IMessageData> messageDatas = operationStatus.EndPeekMessage(asyncResult);
			context.ResultData = this.WrapPeekedMessageResults(messageDatas);
		}

		protected override IEnumerator<IAsyncResult> PutMessageImpl(IAccountIdentifier identity, string account, string queue, List<PushedMessage> messagesList, TimeSpan? timeout, RequestContext requestContext, bool usePutMessageRowCommand, AsyncIteratorContext<IEnumerable<PoppedMessage>> context)
		{
			TimeSpan timeSpan;
			RemainingTime remainingTime = new RemainingTime(timeout);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Add
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.BeginGetQueue(identity, account, queue, PermissionLevel.Write, sASAuthorizationParameter1, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.PutMessageImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			object obj = messagesList;
			if (obj == null)
			{
				obj = Enumerable.Empty<PushedMessage>();
			}
			foreach (PushedMessage pushedMessage in (IEnumerable<PushedMessage>)obj)
			{
				PushedMessage nullable = pushedMessage;
				TimeSpan? messageTTL = pushedMessage.MessageTTL;
				timeSpan = (messageTTL.HasValue ? messageTTL.GetValueOrDefault() : TimeSpan.FromSeconds((double)this.config.MaxTtlSeconds));
				nullable.MessageTTL = new TimeSpan?(timeSpan);
			}
			operationStatus.Timeout = remainingTime;
			asyncResult = operationStatus.BeginPutMessage(messagesList, usePutMessageRowCommand, context.GetResumeCallback(), context.GetResumeState("QueueManager.PutMessageImpl"));
			yield return asyncResult;
			List<IMessageData> messageDatas = operationStatus.EndPutMessage(asyncResult);
			context.ResultData = this.WrapGetMessageResults(messageDatas);
			IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
			verboseDebug.Log("PutMessage response: MessageId={0}", new object[] { context.ResultData });
		}

		protected override IEnumerator<IAsyncResult> SetQueueAclImpl(IAccountIdentifier identifier, string account, string queue, ContainerAclSettings acl, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			byte[] numArray;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(queue))
			{
				throw new ArgumentException("queue", "Cannot be null or empty");
			}
			if (acl == null)
			{
				throw new ArgumentNullException("acl");
			}
			if (acl.SASIdentifiers == null)
			{
				throw new ArgumentNullException("sasidentifiers");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SetQueueAcl");
			}
			RemainingTime remainingTime = new RemainingTime(timeout);
			IAsyncResult asyncResult = this.BeginGetQueue(identifier, account, queue, PermissionLevel.WriteAcl, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.SetQueueAclImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			acl.EncodeToServiceMetadata(out numArray);
			operationStatus.ServiceMetadata = numArray;
			operationStatus.Timeout = remainingTime;
			asyncResult = operationStatus.BeginSetProperties(ContainerPropertyNames.ServiceMetadata, null, context.GetResumeCallback(), context.GetResumeState("QueueManager.SetQueueAclImpl"));
			yield return asyncResult;
			operationStatus.EndSetProperties(asyncResult);
		}

		protected override IEnumerator<IAsyncResult> SetQueueMetadataImpl(IAccountIdentifier identity, string account, string queue, NameValueCollection metadata, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			RemainingTime remainingTime = new RemainingTime(timeout);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.BeginGetQueue(identity, account, queue, PermissionLevel.Write, sASAuthorizationParameter1, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.SetQueueMetadataImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			operationStatus.ApplicationMetadata = QueueHelpers.SerializeMetadata(metadata);
			operationStatus.Timeout = remainingTime;
			asyncResult = operationStatus.BeginSetProperties(ContainerPropertyNames.ApplicationMetadata, null, context.GetResumeCallback(), context.GetResumeState("QueueManager.SetQueueMetadataImpl"));
			yield return asyncResult;
			operationStatus.EndSetProperties(asyncResult);
		}

		protected override IEnumerator<IAsyncResult> SetQueueServicePropertiesImpl(IAccountIdentifier identifier, string ownerAccountName, AnalyticsSettings settings, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(ownerAccountName))
			{
				throw new ArgumentException("ownerAccountName", "Cannot be null");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SetQueueServiceProperties");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, ownerAccountName, null, null, PermissionLevel.Write | PermissionLevel.Owner, sASAuthorizationParameter1, timeout, context.GetResumeCallback(), context.GetResumeState("RealQueueManager.SetQueueServicePropertiesImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount accountServiceMetadatum = null;
			AccountCondition accountCondition = new AccountCondition(false, false, null, null);
			accountServiceMetadatum = this.storageManager.CreateAccountInstance(ownerAccountName);
			accountServiceMetadatum.ServiceMetadata = new AccountServiceMetadata()
			{
				QueueAnalyticsSettings = settings
			};
			accountServiceMetadatum.Timeout = timeout;
			asyncResult = accountServiceMetadatum.BeginSetProperties(new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)65536)), accountCondition, context.GetResumeCallback(), context.GetResumeState("RealQueueManager.SetQueueServicePropertiesImpl"));
			yield return asyncResult;
			accountServiceMetadatum.EndSetProperties(asyncResult);
		}

		protected override IEnumerator<IAsyncResult> UpdateMessageImpl(IAccountIdentifier identity, string account, string queue, string messageId, byte[] popReceipt, byte[] body, TimeSpan visibilityTimeout, TimeSpan? timeout, RequestContext requestContext, AsyncIteratorContext<PoppedMessage> context)
		{
			Guid guid;
			RemainingTime remainingTime = new RemainingTime(timeout);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Update
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.BeginGetQueue(identity, account, queue, PermissionLevel.Write, sASAuthorizationParameter1, remainingTime, requestContext, context.GetResumeCallback(), context.GetResumeState("QueueManager.UpdateMessageImpl"));
			yield return asyncResult;
			IQueueContainer operationStatus = this.EndGetQueue(asyncResult);
			if (requestContext != null)
			{
				operationStatus.OperationStatus = requestContext.OperationStatus;
			}
			operationStatus.Timeout = remainingTime;
			try
			{
				guid = new Guid(messageId);
			}
			catch (Exception exception)
			{
				throw new MessageNotFoundException("Invalid message name", exception);
			}
			IQueueMessageReceipt queueMessageReceipt = this.DecodeReceipt(popReceipt, guid);
			TimeSpan? nullable = null;
			asyncResult = operationStatus.BeginUpdateMessage(queueMessageReceipt, body, visibilityTimeout, nullable, context.GetResumeCallback(), context.GetResumeState("QueueManager.UpdateMessageImpl"));
			yield return asyncResult;
			IQueueMessageReceipt queueMessageReceipt1 = operationStatus.EndUpdateMessage(asyncResult);
			context.ResultData = this.GetPoppedMessageInfoFromReceipt(queueMessageReceipt1);
		}

		private void ValidateNumMessages(int numMessages, int maxMessages)
		{
			if (numMessages <= 0)
			{
				throw new InvalidParameterException("The number of messages requested must be at least 1.");
			}
			if (numMessages > maxMessages)
			{
				throw new InvalidParameterException(string.Concat("The number of messages requested can be at most ", maxMessages, "."));
			}
		}

		public IEnumerable<PoppedMessage> WrapGetMessageResults(IEnumerable<IMessageData> data)
		{
			foreach (IMessageData datum in data)
			{
				byte[] numArray = this.EncodeReceipt(datum);
				Guid id = datum.Id;
				PoppedMessage poppedMessage = new PoppedMessage(id.ToString(), datum.Message, datum.InsertionTime, datum.ExpiryTime, datum.VisibilityStart, datum.DequeueCount, numArray);
				yield return poppedMessage;
			}
		}

		private IEnumerable<PeekedMessage> WrapPeekedMessageResults(IEnumerable<IMessageData> data)
		{
			foreach (IMessageData datum in data)
			{
				Guid id = datum.Id;
				PeekedMessage peekedMessage = new PeekedMessage(id.ToString(), datum.Message, datum.InsertionTime, datum.ExpiryTime, datum.DequeueCount);
				yield return peekedMessage;
			}
		}
	}
}