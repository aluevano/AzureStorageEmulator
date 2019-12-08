using AsyncHelper;
using AsyncHelper.Streams;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using Microsoft.Cis.Services.Nephos.Common.Versioning;
using Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class QueueProtocolHead : BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>
	{
		private const int MaxNumMessagesForGet = 32;

		private const int MinNumMessagesForGet = 1;

		private const int MaxNumMessagesForPut = 32;

		private const int MinNumMessagesForPut = 1;

		private const int MaxMessageTtl = 604800;

		private const int MinMessageTtl = 1;

		private const int MaxVisibilityTimeoutAfterAugust11 = 604800;

		private const int MinVisibilityTimeoutForPutAfterAugust11 = 0;

		private const int MinVisibilityTimeoutForGet = 1;

		private const int MaxVisibilityTimeoutPreAugust11 = 7200;

		private const long MaxMessageSizePreAug11 = 8192L;

		private const long MaxMessageSizeSinceAugust11 = 65536L;

		private const long MaxContentLengthForPutMessageRequestPreAug11 = 16896L;

		private const long MaxContentLengthForPutMessageRequestSinceAugust11 = 131584L;

		private const long MaxContentLengthForPutMessageRequestSinceSeptember12 = 4211200L;

		private const long SizeOfEachSasIdentifier = 2000L;

		private const long MaxContentLengthForXmlSetContainerAcl = 10000L;

		private const int StreamCopyBufferSize = 65536;

		private static List<string> ValidListQueuesIncludeQueryParamValues;

		private AbstractQueueManager queueManager;

		private readonly static XmlWriterSettings XmlWriterSettingsForDecodingMessageData;

		public static HttpProcessorConfiguration HttpProcessorConfigurationDefaultInstance
		{
			get;
			set;
		}

		protected override string ServerResponseHeaderValue
		{
			get
			{
				if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
				{
					return "Windows-Azure-Queue/1.0";
				}
				return "Queue Service Version 1.0";
			}
		}

		static QueueProtocolHead()
		{
			string[] strArrays = new string[] { "metadata", "invisiblemessages" };
			QueueProtocolHead.ValidListQueuesIncludeQueryParamValues = new List<string>(strArrays);
			QueueProtocolHead.XmlWriterSettingsForDecodingMessageData = new XmlWriterSettings();
		}

		private QueueProtocolHead(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, IStorageManager storageManager, Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager authenticationManager, AbstractQueueManager queueManager, HttpProcessorConfiguration configuration, TransformExceptionDelegate transformProviderException, IIpThrottlingTable ipThrottlingTable) : base(requestContext, storageManager, authenticationManager, configuration, transformProviderException, ipThrottlingTable)
		{
			NephosAssertionException.Assert(queueManager != null);
			this.queueManager = queueManager;
		}

		private void AddCacheControlHeaderIfNecessary()
		{
			base.Response.Headers["Cache-Control"] = "no-cache";
		}

		protected override void ApplyResourceToMeasurementEvent(INephosBaseMeasurementEvent measurementEvent)
		{
			if (measurementEvent == null)
			{
				return;
			}
			IQueueMeasurementEvent accountName = measurementEvent as IQueueMeasurementEvent;
			if (accountName != null)
			{
				accountName.AccountName = this.operationContext.AccountName;
				accountName.QueueName = this.operationContext.ContainerName;
			}
			base.ApplyResourceToMeasurementEvent(measurementEvent);
		}

		protected override BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl ChooseRestMethodHandler(RestMethod method)
		{
			GetMessageMeasurementEvent peekMessagesMeasurementEvent;
			if (method == RestMethod.Unknown)
			{
				throw new UnknownVerbProtocolException(base.HttpVerb);
			}
			if (base.RequestProtocolVersion == HttpVersion.Version10)
			{
				throw new HttpVersionNotSupportedException(base.RequestProtocolVersion, base.RequestVia);
			}
			string subResource = this.operationContext.SubResource;
			base.ThrowIfApiNotSupportedForVersion(base.RequestSettings != null);
			if (method == RestMethod.OPTIONS)
			{
				this.operationContext.OperationMeasurementEvent = new QueuePreflightRequestMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.QueuePreflightApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.QueuePreflightRequestHandlerImpl);
			}
			if (this.operationContext.ResourceIsAccount)
			{
				if (subResource == "list")
				{
					if (method != RestMethod.GET)
					{
						throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.GetOnlyOperations);
					}
					this.operationContext.OperationMeasurementEvent = new ListQueuesMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.ListQueuesApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.ListQueuesImpl);
				}
				if (this.operationContext.IsResourceTypeService && string.Equals(subResource, "properties", StringComparison.OrdinalIgnoreCase))
				{
					switch (method)
					{
						case RestMethod.GET:
						{
							this.operationContext.OperationMeasurementEvent = new GetQueueServicePropertiesMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetQueueServicePropertiesApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.GetQueueServicePropertiesImpl);
						}
						case RestMethod.PUT:
						{
							this.operationContext.OperationMeasurementEvent = new SetQueueServicePropertiesMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SetQueueServicePropertiesApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.SetQueueServicePropertiesImpl);
						}
					}
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.ReadWriteOperations);
				}
				if (this.operationContext.IsResourceTypeService && Comparison.StringEqualsIgnoreCase(subResource, "stats"))
				{
					if (method != RestMethod.GET)
					{
						throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.GetOnlyOperations);
					}
					this.operationContext.OperationMeasurementEvent = new GetQueueServiceStatsMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetQueueServiceStatsApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.GetQueueServiceStatsImpl);
				}
			}
			else if (this.operationContext.ResourceIsQueue)
			{
				if (subResource == null)
				{
					RestMethod restMethod = method;
					if (restMethod == RestMethod.PUT)
					{
						this.operationContext.OperationMeasurementEvent = new CreateQueueMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.CreateQueueApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.CreateQueueImpl);
					}
					if (restMethod != RestMethod.DELETE)
					{
						throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.ReadWriteDeleteOperations);
					}
					this.operationContext.OperationMeasurementEvent = new DeleteQueueMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.DeleteQueueApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.DeleteQueueImpl);
				}
				if (subResource == "metadata")
				{
					RestMethod restMethod1 = method;
					switch (restMethod1)
					{
						case RestMethod.GET:
						{
							this.operationContext.OperationMeasurementEvent = new GetQueueMetadataMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetQueueMetadataApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.GetQueuePropertiesImpl);
						}
						case RestMethod.PUT:
						{
							this.operationContext.OperationMeasurementEvent = new SetQueueMetadataMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SetQueueMetadataApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.SetQueuePropertiesImpl);
						}
						default:
						{
							if (restMethod1 == RestMethod.HEAD)
							{
								this.operationContext.OperationMeasurementEvent = new GetQueueMetadataMeasurementEvent();
								base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetQueueMetadataApiEnabled);
								return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.GetQueuePropertiesImpl);
							}
							throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.ReadWriteDeleteOperations);
						}
					}
				}
				if (subResource == "acl")
				{
					switch (method)
					{
						case RestMethod.GET:
						{
							this.operationContext.OperationMeasurementEvent = new GetQueueAclMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetQueueAclApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.GetQueueAclImpl);
						}
						case RestMethod.PUT:
						{
							this.operationContext.OperationMeasurementEvent = new SetQueueAclMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SetQueueAclApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.SetQueueAclImpl);
						}
					}
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.ReadWriteOperations);
				}
			}
			else if (subResource == null)
			{
				if (Comparison.StringEqualsIgnoreCase(this.operationContext.Operation, "messages"))
				{
					switch (method)
					{
						case RestMethod.GET:
						{
							long? nullable = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "numofmessages", null);
							long num = (nullable.HasValue ? nullable.GetValueOrDefault() : (long)1);
							bool? nullable1 = base.ParseOptionalBoolInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "peekonly");
							if ((nullable1.HasValue ? nullable1.GetValueOrDefault() : false))
							{
								if (num != (long)1)
								{
									peekMessagesMeasurementEvent = new PeekMessagesMeasurementEvent();
								}
								else
								{
									peekMessagesMeasurementEvent = new PeekMessageMeasurementEvent();
								}
							}
							else if (num != (long)1)
							{
								peekMessagesMeasurementEvent = new GetMessagesMeasurementEvent();
							}
							else
							{
								peekMessagesMeasurementEvent = new GetMessageMeasurementEvent();
							}
							peekMessagesMeasurementEvent.NumMessagesRequested = num;
							this.operationContext.OperationMeasurementEvent = peekMessagesMeasurementEvent;
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetMessagesApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.GetMessageImpl);
						}
						case RestMethod.PUT:
						case RestMethod.MERGE:
						{
							throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.ReadWriteDeleteOperations);
						}
						case RestMethod.POST:
						{
							this.operationContext.OperationMeasurementEvent = new PutMessageMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.PutMessageApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.PutMessageImpl);
						}
						case RestMethod.DELETE:
						{
							this.operationContext.OperationMeasurementEvent = new ClearMessagesMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.ClearMessagesApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.ClearQueueImpl);
						}
						default:
						{
							throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.ReadWriteDeleteOperations);
						}
					}
				}
				if (this.operationContext.Operation.Length > "messages".Length && Comparison.StringEqualsIgnoreCase(this.operationContext.Operation.Substring(0, "messages".Length + 1), "messages/"))
				{
					RestMethod restMethod2 = method;
					if (restMethod2 == RestMethod.PUT)
					{
						this.operationContext.OperationMeasurementEvent = new UpdateMessageMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.UpdateMessageApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.UpdateMessageImpl);
					}
					if (restMethod2 != RestMethod.DELETE)
					{
						throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.DeleteOnlyOperations);
					}
					this.operationContext.OperationMeasurementEvent = new DeleteMessageMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.DeleteMessageApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.RestMethodImpl(this.DeleteMessageImpl);
				}
			}
			throw new InvalidUrlProtocolException(base.RequestUrl.AbsolutePath);
		}

		private IEnumerator<IAsyncResult> ClearQueueImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			IAsyncResult asyncResult = this.queueManager.BeginClearQueue(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.ClearQueueImpl"));
			yield return asyncResult;
			this.queueManager.EndClearQueue(asyncResult);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.NoContent);
			base.SendSuccessResponse(false);
		}

		public static IProcessor Create(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, IStorageManager storageManager, Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager authenticationManager, AbstractQueueManager serviceManager, HttpProcessorConfiguration configuration, TransformExceptionDelegate transformProviderException, IIpThrottlingTable ipThrottlingTable)
		{
			return new QueueProtocolHead(requestContext, storageManager, authenticationManager, serviceManager, configuration, transformProviderException, ipThrottlingTable);
		}

		private IEnumerator<IAsyncResult> CreateQueueImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			long num = (long)0;
			long num1 = (long)0;
			NameValueCollection nameValueCollection = new NameValueCollection();
			base.AddMetadataFromRequest(nameValueCollection);
			IAsyncResult asyncResult = this.queueManager.BeginCreateQueue(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, num, num1, nameValueCollection, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.CreateQueue"));
			yield return asyncResult;
			if (this.queueManager.EndCreateQueue(asyncResult) != CreateQueueResult.Created)
			{
				this.SetStatusCodeAndServiceHeaders(HttpStatusCode.NoContent);
			}
			else
			{
				this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			}
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> DeleteMessageImpl(AsyncIteratorContext<NoResults> async)
		{
			byte[] numArray;
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			string str = this.operationContext.Operation.Substring("messages".Length + 1);
			(this.operationContext.OperationMeasurementEvent as DeleteMessageMeasurementEvent).MessageId = str;
			string item = base.RequestQueryParameters["popreceipt"];
			if (item == null)
			{
				throw new InvalidQueryParameterProtocolException("popreceipt", null, "query parameter is required");
			}
			try
			{
				numArray = Convert.FromBase64String(item);
			}
			catch (FormatException formatException)
			{
				throw new InvalidQueryParameterProtocolException("popreceipt", item, "Invalid pop receipt format", formatException);
			}
			IAsyncResult asyncResult = this.queueManager.BeginDeleteMessage(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, str, numArray, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.DeleteMessageImpl"));
			yield return asyncResult;
			this.queueManager.EndDeleteMessage(asyncResult);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.NoContent);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> DeleteQueueImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			IAsyncResult asyncResult = this.queueManager.BeginDeleteQueue(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.DeleteQueueImpl"));
			yield return asyncResult;
			this.queueManager.EndDeleteQueue(asyncResult);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.NoContent);
			base.SendSuccessResponse(false);
		}

		private ListQueuesOperationContext ExtractListQueuesOperationContext()
		{
			ListQueuesOperationContext listQueuesOperationContext = new ListQueuesOperationContext(base.RequestRestVersion)
			{
				Prefix = base.RequestQueryParameters["prefix"]
			};
			if (XmlUtilities.HasInvalidXmlCharacters(listQueuesOperationContext.Prefix, false))
			{
				throw new InvalidQueryParameterProtocolException("prefix", XmlUtilities.EscapeInvalidXmlCharacters(listQueuesOperationContext.Prefix, false), "The string includes invalid Xml Characters");
			}
			listQueuesOperationContext.Marker = base.RequestQueryParameters["marker"];
			if (XmlUtilities.HasInvalidXmlCharacters(listQueuesOperationContext.Marker, false))
			{
				throw new InvalidQueryParameterProtocolException("marker", XmlUtilities.EscapeInvalidXmlCharacters(listQueuesOperationContext.Marker, false), "The string includes invalid Xml Characters");
			}
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				listQueuesOperationContext.MaxResults = base.GetMaxResultsQueryParamPreSeptember09VersionForQueue();
			}
			else
			{
				listQueuesOperationContext.MaxResults = base.GetMaxResultsQueryParam();
			}
			listQueuesOperationContext.IsIncludingUrlInResponse = !base.RequestContext.IsRequestVersionAtLeastAugust13;
			return listQueuesOperationContext;
		}

		protected override OperationContextWithAuthAndAccountContainer ExtractOperationContext(NephosUriComponents uriComponents)
		{
			if (string.IsNullOrEmpty(uriComponents.AccountName))
			{
				throw new InvalidUrlProtocolException(base.RequestUrl);
			}
			QueueOperationContext queueOperationContext = new QueueOperationContext(base.RequestContext.ElapsedTime)
			{
				AccountName = uriComponents.AccountName,
				ContainerName = uriComponents.ContainerName,
				Operation = uriComponents.RemainingPart
			};
			if (base.RequestQueryParameters["restype"] == "service")
			{
				queueOperationContext.IsResourceTypeService = true;
			}
			queueOperationContext.SetUserTimeout(base.ExtractTimeoutFromContext());
			queueOperationContext.SubResource = base.ExtractSubResourceFromContext();
			return queueOperationContext;
		}

		private void FillListMessagesOperationContext(MessageListingContext listMessagesContext)
		{
			listMessagesContext.Marker = base.RequestQueryParameters["marker"];
			if (XmlUtilities.HasInvalidXmlCharacters(listMessagesContext.Marker, false))
			{
				throw new InvalidQueryParameterProtocolException("marker", XmlUtilities.EscapeInvalidXmlCharacters(listMessagesContext.Marker, false), "The string includes invalid Xml Characters");
			}
			listMessagesContext.MaxResults = base.GetMaxResultsQueryParam();
			List<string> strs = base.ParseOptionalListInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "include", QueueProtocolHead.ValidListQueuesIncludeQueryParamValues, ",");
			if (strs != null && strs.Count > 0)
			{
				foreach (string str in strs)
				{
					if (!str.Equals("invisiblemessages", StringComparison.OrdinalIgnoreCase))
					{
						throw new InvalidQueryParameterProtocolException("include", str, "Invalid query parameter value.");
					}
					listMessagesContext.IncludeInvisibleMessages = true;
				}
			}
		}

		public override NephosErrorDetails GetErrorDetailsForException(Exception e)
		{
			if (e is InvalidParameterException)
			{
				return new NephosErrorDetails(CommonStatusEntries.InvalidInput, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is MessageNotFoundException)
			{
				return new NephosErrorDetails(QueueStatusEntries.MessageNotFound, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is PopReceiptMismatchException)
			{
				return new NephosErrorDetails(QueueStatusEntries.PopReceiptMismatch, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is QueueNotEmptyException)
			{
				return new NephosErrorDetails(QueueStatusEntries.QueueNotEmpty, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is InvalidStreamLengthException)
			{
				return new NephosErrorDetails(QueueStatusEntries.MessageTooLarge, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is ContainerNotFoundException)
			{
				return new NephosErrorDetails(QueueStatusEntries.QueueNotFound, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is ContainerAlreadyExistsException)
			{
				return new NephosErrorDetails(QueueStatusEntries.QueueAlreadyExists, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (!(e is InvalidMarkerException))
			{
				return base.GetErrorDetailsForException(e);
			}
			return new NephosErrorDetails(QueueStatusEntries.InvalidMarker, NephosRESTEventStatus.ExpectedFailure, e);
		}

		private void GetMaxMessageAndContentLength(out long maxMessageSize, out long maxContentLength)
		{
			if (base.RequestContext.IsRequestVersionAtLeastAugust11)
			{
				maxMessageSize = (long)65536;
				maxContentLength = (long)131584;
				return;
			}
			maxMessageSize = (long)8192;
			maxContentLength = (long)16896;
		}

		private IEnumerator<IAsyncResult> GetMessageImpl(AsyncIteratorContext<NoResults> async)
		{
			IAsyncResult stream;
			IEnumerable<PeekedMessage> peekedMessages;
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			GetMessageMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as GetMessageMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			long numMessagesRequested = operationMeasurementEvent.NumMessagesRequested;
			long? nullable = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "visibilitytimeout", null);
			bool operationType = operationMeasurementEvent.OperationType == OperationType.Peek;
			if (nullable.HasValue && operationType)
			{
				throw new InvalidQueryParameterProtocolException("visibilitytimeout", nullable.ToString(), "Not a valid parameter in conjunction with peek.");
			}
			this.ValidateVisibilityTimeout(nullable, false);
			if (numMessagesRequested < (long)1 || numMessagesRequested > (long)32)
			{
				string str = numMessagesRequested.ToString();
				int num = 1;
				int num1 = 32;
				throw new OutOfRangeQueryParameterProtocolException("numofmessages", str, num.ToString(), num1.ToString());
			}
			if (!operationType)
			{
				stream = this.queueManager.BeginGetMessages(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, (int)numMessagesRequested, nullable, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetMessageImpl"));
				yield return stream;
				peekedMessages = this.queueManager.EndGetMessages(stream);
			}
			else
			{
				stream = this.queueManager.BeginPeekMessages(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, (int)numMessagesRequested, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetMessageImpl"));
				yield return stream;
				peekedMessages = this.queueManager.EndPeekMessages(stream);
			}
			base.Response.SendChunked = true;
			base.Response.ContentType = "application/xml";
			this.AddCacheControlHeaderIfNecessary();
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			QueueProtocolHead.MessagesXmlEncoder messagesXmlEncoder = new QueueProtocolHead.MessagesXmlEncoder(base.RequestRestVersion, false);
			using (Stream stream1 = base.GenerateMeasuredResponseStream(false))
			{
				stream = messagesXmlEncoder.BeginEncodeListToStream(base.RequestUrl, peekedMessages, null, stream1, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetMessageImpl"));
				yield return stream;
				messagesXmlEncoder.EndEncodeListToStream(stream);
				this.operationContext.OperationMeasurementEvent.ItemsReturnedCount = messagesXmlEncoder.TotalCount;
			}
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetQueueAclImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			IAsyncResult stream = this.queueManager.BeginGetQueueAcl(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.RemainingTimeout(), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetQueueAclImpl"));
			yield return stream;
			ContainerAclSettings containerAclSetting = this.queueManager.EndGetQueueAcl(stream);
			this.AddCacheControlHeaderIfNecessary();
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			if (base.Method != RestMethod.HEAD)
			{
				ListContainersAclXmlListEncoder listContainersAclXmlListEncoder = new ListContainersAclXmlListEncoder(base.RequestContext.IsRequestVersionAtLeastApril17);
				using (Stream stream1 = base.GenerateMeasuredResponseStream(false))
				{
					base.Response.SendChunked = true;
					base.Response.ContentType = "application/xml";
					stream = listContainersAclXmlListEncoder.BeginEncodeListToStream(base.RequestUrl, containerAclSetting.SASIdentifiers, containerAclSetting, stream1, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetQueueAclImpl"));
					yield return stream;
					listContainersAclXmlListEncoder.EndEncodeListToStream(stream);
				}
			}
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetQueuePropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			IAsyncResult asyncResult = this.queueManager.BeginGetQueueProperties(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetQueueProperties"));
			yield return asyncResult;
			QueueProperties queueProperty = this.queueManager.EndGetQueueProperties(asyncResult);
			IHttpListenerResponse response = base.Response;
			long value = queueProperty.VisibleMessageCount.Value;
			response.AddHeader("x-ms-approximate-messages-count", value.ToString(CultureInfo.InvariantCulture));
			this.AddCacheControlHeaderIfNecessary();
			base.SetMetadataOnResponse(queueProperty.Metadata);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetQueueServicePropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			IAsyncResult asyncResult = this.queueManager.BeginGetQueueServiceProperties(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetQueueServicePropertiesImpl"));
			yield return asyncResult;
			AnalyticsSettings analyticsSetting = this.queueManager.EndGetQueueServiceProperties(asyncResult);
			this.AddCacheControlHeaderIfNecessary();
			base.AddServiceResponseHeadersBeforeSendingResponse();
			asyncResult = base.BeginWriteAnalyticsSettings(analyticsSetting, AnalyticsSettingsHelper.GetSettingVersion(base.RequestContext), base.RequestContext.ServiceType, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetQueueServicePropertiesImpl"));
			yield return asyncResult;
			base.EndWriteAnalyticsSettings(asyncResult);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetQueueServiceStatsImpl(AsyncIteratorContext<NoResults> async)
		{
			object obj;
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			if (!base.UriComponents.IsSecondaryAccountAccess)
			{
				throw new InvalidQueryParameterProtocolException("comp", this.operationContext.SubResource, null);
			}
			IAsyncResult asyncResult = this.queueManager.BeginGetQueueServiceStats(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetQueueServicePropertiesImpl"));
			yield return asyncResult;
			GeoReplicationStats geoReplicationStat = this.queueManager.EndGetQueueServiceStats(asyncResult);
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] containerName = new object[] { this.operationContext.ContainerName, null };
			object[] objArray = containerName;
			obj = (geoReplicationStat != null ? geoReplicationStat.ToString() : "null");
			objArray[1] = obj;
			verbose.Log("Queue {0} GeoReplicationStats is '{1}'", containerName);
			this.AddCacheControlHeaderIfNecessary();
			base.AddServiceResponseHeadersBeforeSendingResponse();
			asyncResult = base.BeginWriteServiceStats(geoReplicationStat, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetQueueServicePropertiesImpl"));
			yield return asyncResult;
			base.EndWriteServiceStats(asyncResult);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> ListMessagesImpl(AsyncIteratorContext<NoResults> async)
		{
			int num;
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.GetDefaultMaxTimeoutForListCommands(base.RequestRestVersion));
			NephosAssertionException.Assert(this.operationContext.OperationMeasurementEvent as ListMessagesMeasurementEvent != null);
			MessageListingContext messageListingContext = new MessageListingContext(base.RequestRestVersion);
			this.FillListMessagesOperationContext(messageListingContext);
			IAsyncResult stream = null;
			AbstractQueueManager abstractQueueManager = this.queueManager;
			IAccountIdentifier callerIdentity = this.operationContext.CallerIdentity;
			string accountName = this.operationContext.AccountName;
			string containerName = this.operationContext.ContainerName;
			string marker = messageListingContext.Marker;
			bool includeInvisibleMessages = messageListingContext.IncludeInvisibleMessages;
			int? maxResults = messageListingContext.MaxResults;
			num = (maxResults.HasValue ? maxResults.GetValueOrDefault() : 5000);
			stream = abstractQueueManager.BeginListMessages(callerIdentity, accountName, containerName, marker, includeInvisibleMessages, true, num, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.GetMessageImpl"));
			yield return stream;
			ListMessagesResult listMessagesResults = this.queueManager.EndListMessages(stream);
			base.Response.SendChunked = true;
			base.Response.ContentType = "application/xml";
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			this.operationContext.OperationMeasurementEvent.ItemsReturnedCount = (int)listMessagesResults.Count;
			using (Stream stream1 = base.GenerateMeasuredResponseStream(false))
			{
				QueueProtocolHead.ListMessagesResultEncoder listMessagesResultEncoder = new QueueProtocolHead.ListMessagesResultEncoder(base.RequestRestVersion, this.operationContext.ContainerName);
				stream = listMessagesResultEncoder.BeginEncodeListToStream(base.RequestUrl, listMessagesResults, null, stream1, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.ListMessagesImpl"));
				yield return stream;
				listMessagesResultEncoder.EndEncodeListToStream(stream);
			}
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> ListQueuesImpl(AsyncIteratorContext<NoResults> async)
		{
			int num;
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.GetDefaultMaxTimeoutForListCommands(base.RequestRestVersion));
			NephosAssertionException.Assert(this.operationContext.OperationMeasurementEvent != null);
			ListQueuesOperationContext listQueuesOperationContext = this.ExtractListQueuesOperationContext();
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				List<string> strs = base.ParseOptionalListInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "include", QueueProtocolHead.ValidListQueuesIncludeQueryParamValues, ",");
				if (strs != null && strs.Count > 0)
				{
					foreach (string str in strs)
					{
						if (!str.Equals("metadata", StringComparison.OrdinalIgnoreCase))
						{
							throw new InvalidQueryParameterProtocolException("include", str, "Invalid query parameter value.");
						}
						listQueuesOperationContext.IsFetchingMetadata = true;
					}
				}
			}
			AbstractQueueManager abstractQueueManager = this.queueManager;
			IAccountIdentifier callerIdentity = this.operationContext.CallerIdentity;
			string accountName = this.operationContext.AccountName;
			string prefix = listQueuesOperationContext.Prefix;
			string marker = listQueuesOperationContext.Marker;
			int? maxResults = listQueuesOperationContext.MaxResults;
			num = (maxResults.HasValue ? maxResults.GetValueOrDefault() : 5000);
			IAsyncResult stream = abstractQueueManager.BeginListQueues(callerIdentity, accountName, prefix, null, marker, num, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.ListQueuesImpl"));
			yield return stream;
			IListQueuesResultCollection listQueuesResultCollections = this.queueManager.EndListQueues(stream);
			base.Response.SendChunked = true;
			base.Response.ContentType = "application/xml";
			this.AddCacheControlHeaderIfNecessary();
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			QueueProtocolHead.ListQueuesXmlListEncoder listQueuesXmlListEncoder = new QueueProtocolHead.ListQueuesXmlListEncoder();
			using (Stream stream1 = base.GenerateMeasuredResponseStream(false))
			{
				stream = listQueuesXmlListEncoder.BeginEncodeListToStream(base.RequestUrl, listQueuesResultCollections, listQueuesOperationContext, stream1, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.ListQueuesImpl"));
				yield return stream;
				listQueuesXmlListEncoder.EndEncodeListToStream(stream);
				this.operationContext.OperationMeasurementEvent.ItemsReturnedCount = listQueuesXmlListEncoder.TotalCount;
			}
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> PutMessageImpl(AsyncIteratorContext<NoResults> async)
		{
			object obj;
			long num;
			long num1;
			long num2;
			IAsyncResult stream;
			long num3;
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			PutMessageMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as PutMessageMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			long? nullable = null;
			if (base.RequestContext.IsRequestVersionAtLeastAugust11)
			{
				nullable = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "visibilitytimeout", null);
				this.ValidateVisibilityTimeout(nullable, false);
			}
			obj = (nullable.HasValue ? nullable.Value : (long)0);
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)obj);
			TimeSpan? nullable1 = null;
			long? nullable2 = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "messagettl", null);
			if (!nullable2.HasValue)
			{
				nullable2 = new long?((long)604800);
			}
			else
			{
				if (nullable2.Value < (long)1 || nullable2.Value > (long)604800)
				{
					string str = nullable2.ToString();
					int num4 = 1;
					int num5 = 604800;
					throw new OutOfRangeQueryParameterProtocolException("messagettl", str, num4.ToString(), num5.ToString());
				}
				nullable1 = new TimeSpan?(TimeSpan.FromSeconds((double)nullable2.Value));
			}
			if ((double)nullable2.Value <= timeSpan.TotalSeconds)
			{
				double totalSeconds = timeSpan.TotalSeconds;
				throw new InvalidQueryParameterProtocolException("visibilitytimeout", totalSeconds.ToString(), string.Format("{0} must be greater than {1}", "messagettl", "visibilitytimeout"));
			}
			this.GetMaxMessageAndContentLength(out num1, out num2);
			if (base.RequestContentLength > num2)
			{
				throw new RequestEntityTooLargeException(new long?(num2));
			}
			BufferWrapper buffer = BufferPool.GetBuffer((int)num2);
			List<PushedMessage> pushedMessages = null;
			try
			{
				using (MemoryStream memoryStream = new MemoryStream(buffer.Buffer))
				{
					Stream stream1 = base.GenerateMeasuredRequestStream();
					byte[] requestMD5 = base.GetRequestMD5("Content-MD5");
					if (requestMD5 != null)
					{
						MD5ReaderStream mD5ReaderStream = new MD5ReaderStream(stream1, base.RequestContentLength, true, base.RequestContext)
						{
							HashToVerifyAgainst = requestMD5
						};
						stream1 = mD5ReaderStream;
					}
					using (stream1)
					{
						num = (base.RequestContentLength > (long)0 ? base.RequestContentLength : num2 + (long)1);
						long num6 = num;
						stream = AsyncStreamCopy.BeginAsyncStreamCopy(stream1, memoryStream, num6, 65536, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.PutMessageImpl"));
						yield return stream;
						num3 = AsyncStreamCopy.EndAsyncStreamCopy(stream);
						if (num3 <= num2)
						{
							goto Label0;
						}
						IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
						verboseDebug.Log("Request body exceeds max permissible limit of {0}", new object[] { num2 });
						throw new RequestEntityTooLargeException(new long?(num2));
					}
					using (MemoryStream memoryStream1 = new MemoryStream(buffer.Buffer, 0, (int)num3))
					{
						pushedMessages = this.UnwrapMessage(memoryStream1, timeSpan, nullable1);
						object obj1 = pushedMessages;
						if (obj1 == null)
						{
							obj1 = Enumerable.Empty<PushedMessage>();
						}
						foreach (PushedMessage pushedMessage in (IEnumerable<PushedMessage>)obj1)
						{
							IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
							object[] length = new object[] { (int)pushedMessage.MessageText.Length };
							verbose.Log("Length of new message is {0} bytes.", length);
							if ((int)pushedMessage.MessageText.Length <= (int)num1)
							{
								continue;
							}
							IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
							object[] objArray = new object[] { (int)pushedMessage.MessageText.Length, num1 };
							stringDataEventStream.Log("Message length {0} is greater than max allowed {1}", objArray);
							throw new RequestEntityTooLargeException(new long?(num1));
						}
					}
				}
			}
			finally
			{
				BufferPool.ReleaseBuffer(buffer);
				buffer = null;
			}
			bool isRequestVersionAtLeastMay16 = base.RequestContext.IsRequestVersionAtLeastMay16;
			bool flag = true;
			bool flag1 = isRequestVersionAtLeastMay16;
			stream = this.queueManager.BeginPutMessage(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, pushedMessages, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, flag, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.PutMessageImpl"));
			yield return stream;
			IEnumerable<PoppedMessage> poppedMessages = this.queueManager.EndPutMessage(stream);
			operationMeasurementEvent.MessageList = poppedMessages;
			if (flag1)
			{
				base.Response.ContentType = "application/xml";
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			if (isRequestVersionAtLeastMay16)
			{
				QueueProtocolHead.MessagesXmlEncoder messagesXmlEncoder = new QueueProtocolHead.MessagesXmlEncoder(base.RequestRestVersion, true);
				using (Stream stream2 = base.GenerateMeasuredResponseStream(false))
				{
					stream = messagesXmlEncoder.BeginEncodeListToStream(base.RequestUrl, poppedMessages, null, stream2, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.PutMessageImpl"));
					yield return stream;
					messagesXmlEncoder.EndEncodeListToStream(stream);
				}
			}
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> QueuePreflightRequestHandlerImpl(AsyncIteratorContext<NoResults> async)
		{
			base.HandlePreflightCorsRequest(this.storageAccount.ServiceMetadata.QueueAnalyticsSettings);
			yield break;
		}

		private IEnumerator<IAsyncResult> SetQueueAclImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			ContainerAclSettings containerAclSetting = new ContainerAclSettings(new bool?(false), base.RequestRestVersion);
			if (base.RequestContentLength > (long)0)
			{
				if (base.RequestContentLength > (long)10000)
				{
					throw new RequestEntityTooLargeException(new long?((long)10000));
				}
				using (BufferPoolMemoryStream bufferPoolMemoryStream = new BufferPoolMemoryStream(65536))
				{
					using (Stream stream = base.GenerateMeasuredRequestStream())
					{
						IAsyncResult asyncResult = AsyncStreamCopy.BeginAsyncStreamCopy(stream, bufferPoolMemoryStream, base.RequestContentLength, 65536, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.SetQueueAclImpl"));
						yield return asyncResult;
						AsyncStreamCopy.EndAsyncStreamCopy(asyncResult);
					}
					bufferPoolMemoryStream.Seek((long)0, SeekOrigin.Begin);
					containerAclSetting.SASIdentifiers = BasicHttpProcessor.DecodeSASIdentifiersFromStream(bufferPoolMemoryStream, BasicHttpProcessorWithAuthAndAccountContainer<QueueOperationContext>.DefaultXmlReaderSettings, true, base.RequestContext.IsRequestVersionAtLeastApril15, SASPermission.Queue);
					if (containerAclSetting.SASIdentifiers.Count <= SASIdentifier.MaxSASIdentifiers)
					{
						goto Label0;
					}
					throw new InvalidXmlProtocolException(string.Concat("At most ", SASIdentifier.MaxSASIdentifiers, " signed identifier is allowed in the request body"));
				}
			}
			else if (base.RequestContentLength < (long)0)
			{
				long requestContentLength = base.RequestContentLength;
				throw new InvalidHeaderProtocolException("Content-Length", requestContentLength.ToString(CultureInfo.InvariantCulture));
			}
		Label0:
			IAsyncResult asyncResult1 = this.queueManager.BeginSetQueueAcl(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, containerAclSetting, this.operationContext.RemainingTimeout(), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.SetQueueAclImpl"));
			yield return asyncResult1;
			this.queueManager.EndSetQueueAcl(asyncResult1);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.NoContent);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> SetQueuePropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			NameValueCollection nameValueCollection = new NameValueCollection();
			base.AddMetadataFromRequest(nameValueCollection);
			IAsyncResult asyncResult = this.queueManager.BeginSetQueueMetadata(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, nameValueCollection, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.SetQueueProperties"));
			yield return asyncResult;
			this.queueManager.EndSetQueueMetadata(asyncResult);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.NoContent);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> SetQueueServicePropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			IAsyncResult asyncResult = base.BeginReadAnalyticsSettings(AnalyticsSettingsHelper.GetSettingVersion(base.RequestContext), base.RequestContext.ServiceType, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.SetQueueServicePropertiesImpl"));
			yield return asyncResult;
			AnalyticsSettings analyticsSetting = base.EndReadAnalyticsSettings(asyncResult);
			asyncResult = this.queueManager.BeginSetQueueServiceProperties(this.operationContext.CallerIdentity, this.operationContext.AccountName, analyticsSetting, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.SetQueueServicePropertiesImpl"));
			yield return asyncResult;
			this.queueManager.EndSetQueueServiceProperties(asyncResult);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Accepted);
			base.SendSuccessResponse(false);
		}

		private byte[] UnwrapMessage(Stream inputStream)
		{
			byte[] bytes;
			try
			{
				using (XmlTextReader xmlTextReader = new XmlTextReader(inputStream))
				{
					xmlTextReader.DtdProcessing = DtdProcessing.Ignore;
					xmlTextReader.XmlResolver = null;
					XmlDocument xmlDocument = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					xmlDocument.Load(xmlTextReader);
					XmlNode xmlNodes = xmlDocument.SelectSingleNode(QueueXPathQueries.SelectMessageTextQuery);
					if (xmlNodes == null || xmlNodes.InnerText == null)
					{
						throw new InvalidXmlProtocolException("Invalid envelope for the message");
					}
					bytes = Encoding.UTF8.GetBytes(xmlNodes.InnerText);
				}
			}
			catch (XmlException xmlException1)
			{
				XmlException xmlException = xmlException1;
				throw new InvalidXmlProtocolException("Error parsing Xml content", xmlException.LineNumber, xmlException.LinePosition, xmlException);
			}
			return bytes;
		}

		private List<PushedMessage> UnwrapMessage(Stream inputStream, TimeSpan visibilityTimeout, TimeSpan? messageTtl)
		{
			List<PushedMessage> pushedMessages;
			List<PushedMessage> pushedMessages1 = new List<PushedMessage>();
			try
			{
				using (XmlTextReader xmlTextReader = new XmlTextReader(inputStream))
				{
					xmlTextReader.DtdProcessing = DtdProcessing.Ignore;
					xmlTextReader.XmlResolver = null;
					XmlDocument xmlDocument = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					xmlDocument.Load(xmlTextReader);
					XmlNode xmlNodes = xmlDocument.SelectSingleNode(QueueXPathQueries.SelectMessageTextQuery);
					if (xmlNodes == null || xmlNodes.InnerText == null)
					{
						throw new InvalidXmlProtocolException("Invalid envelope for the message");
					}
					pushedMessages1.Add(new PushedMessage(Encoding.UTF8.GetBytes(xmlNodes.InnerText), visibilityTimeout, messageTtl));
					pushedMessages = pushedMessages1;
				}
			}
			catch (XmlException xmlException1)
			{
				XmlException xmlException = xmlException1;
				throw new InvalidXmlProtocolException("Error parsing Xml content", xmlException.LineNumber, xmlException.LinePosition, xmlException);
			}
			return pushedMessages;
		}

		private List<PushedMessage> UnwrapPutMessagesList(Stream inputStream, TimeSpan visibilityTimeoutInQuery, TimeSpan? messageTTLInQuery)
		{
			List<PushedMessage> pushedMessages;
			string str;
			try
			{
				using (XmlTextReader xmlTextReader = new XmlTextReader(inputStream))
				{
					xmlTextReader.DtdProcessing = DtdProcessing.Ignore;
					xmlTextReader.XmlResolver = null;
					List<PushedMessage> pushedMessages1 = new List<PushedMessage>();
					XmlDocument xmlDocument = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					xmlDocument.Load(xmlTextReader);
					if (xmlDocument.SelectSingleNode(QueueXPathQueries.SelectMessagesListQuery) != null)
					{
						if (!base.RequestContext.IsRequestVersionAtLeastSeptember12)
						{
							throw new XmlNodeNotSupportedProtocolException("QueueMessagesList", "QueueMessagesList element is not supported in REST versions before 2012-09-19");
						}
						XmlNodeList xmlNodeLists = xmlDocument.SelectNodes(QueueXPathQueries.SelectAllMessagesQuery);
						if (xmlNodeLists == null || xmlNodeLists.Count < 1 || xmlNodeLists.Count > 32)
						{
							str = (xmlNodeLists == null ? "0" : xmlNodeLists.Count.ToString());
							int num = 1;
							int num1 = 32;
							throw new OutOfRangeXmlNodeCountProtocolException("numofmessages", str, num.ToString(), num1.ToString());
						}
						foreach (XmlNode xmlNodes in xmlNodeLists)
						{
							if (!xmlNodes.HasChildNodes)
							{
								throw new InvalidXmlProtocolException("Invalid envelope for the message");
							}
							XmlNode xmlNodes1 = xmlNodes.SelectSingleNode("MessageText");
							if (xmlNodes1 == null)
							{
								throw new InvalidXmlProtocolException("Queue message text is required");
							}
							TimeSpan timeSpan = new TimeSpan((long)0);
							XmlNode xmlNodes2 = xmlNodes.SelectSingleNode("VisibilityTimeOut");
							if (xmlNodes2 != null)
							{
								long? nullable = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.RequestBody, "VisibilityTimeOut", xmlNodes2.InnerText);
								if (nullable.HasValue)
								{
									this.ValidateVisibilityTimeout(new long?(nullable.Value), true);
									timeSpan = TimeSpan.FromSeconds((double)nullable.Value);
								}
							}
							TimeSpan timeSpan1 = new TimeSpan(0, 0, 604800);
							XmlNode xmlNodes3 = xmlNodes.SelectSingleNode("MessageTTL");
							if (xmlNodes3 != null)
							{
								long? nullable1 = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.RequestBody, "MessageTTL", xmlNodes3.InnerText);
								if (nullable1.HasValue)
								{
									if (nullable1.Value < (long)1 || nullable1.Value > (long)604800)
									{
										string str1 = nullable1.Value.ToString();
										int num2 = 1;
										int num3 = 604800;
										throw new OutOfRangeXmlArgumentProtocolException("messagettl", str1, num2.ToString(), num3.ToString());
									}
									timeSpan1 = TimeSpan.FromSeconds((double)nullable1.Value);
								}
							}
							if (timeSpan1.TotalSeconds <= timeSpan.TotalSeconds)
							{
								double totalSeconds = timeSpan.TotalSeconds;
								throw new InvalidXmlNodeProtocolException("visibilitytimeout", totalSeconds.ToString(), string.Format("{0} must be greater than {1}", "messagettl", "visibilitytimeout"));
							}
							pushedMessages1.Add(new PushedMessage(Encoding.UTF8.GetBytes(xmlNodes1.InnerText), timeSpan, new TimeSpan?(timeSpan1)));
						}
					}
					else
					{
						if (base.RequestContext.IsRequestVersionAtLeastSeptember12)
						{
							throw new InvalidXmlProtocolException("QueueMessagesList element is required for REST versions starting from 2012-09-19");
						}
						XmlNode xmlNodes4 = xmlDocument.SelectSingleNode(QueueXPathQueries.SelectMessageTextQuery);
						if (xmlNodes4 == null || xmlNodes4.InnerText == null)
						{
							throw new InvalidXmlProtocolException("Invalid envelope for the message");
						}
						pushedMessages1.Add(new PushedMessage(Encoding.UTF8.GetBytes(xmlNodes4.InnerText), visibilityTimeoutInQuery, messageTTLInQuery));
					}
					pushedMessages = pushedMessages1;
				}
			}
			catch (XmlException xmlException1)
			{
				XmlException xmlException = xmlException1;
				throw new InvalidXmlProtocolException("Error parsing Xml content", xmlException.LineNumber, xmlException.LinePosition, xmlException);
			}
			return pushedMessages;
		}

		private IEnumerator<IAsyncResult> UpdateMessageImpl(AsyncIteratorContext<NoResults> async)
		{
			long num;
			byte[] numArray;
			long num1;
			long num2;
			IAsyncResult asyncResult;
			long num3;
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			string str = this.operationContext.Operation.Substring("messages".Length + 1);
			UpdateMessageMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as UpdateMessageMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			operationMeasurementEvent.MessageId = str;
			long? nullable = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "visibilitytimeout", null);
			if (!nullable.HasValue)
			{
				throw new InvalidQueryParameterProtocolException("visibilitytimeout", null, "query parameter is required");
			}
			this.ValidateVisibilityTimeout(nullable, false);
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)nullable.Value);
			string item = base.RequestQueryParameters["popreceipt"];
			if (item == null)
			{
				throw new InvalidQueryParameterProtocolException("popreceipt", null, "query parameter is required");
			}
			try
			{
				numArray = Convert.FromBase64String(item);
			}
			catch (FormatException formatException)
			{
				throw new InvalidQueryParameterProtocolException("popreceipt", item, "Invalid pop receipt format", formatException);
			}
			this.GetMaxMessageAndContentLength(out num1, out num2);
			if (base.RequestContentLength > num2)
			{
				throw new RequestEntityTooLargeException(new long?(num2));
			}
			BufferWrapper buffer = BufferPool.GetBuffer((int)num2);
			byte[] numArray1 = null;
			try
			{
				using (MemoryStream memoryStream = new MemoryStream(buffer.Buffer))
				{
					Stream stream = base.GenerateMeasuredRequestStream();
					byte[] requestMD5 = base.GetRequestMD5("Content-MD5");
					if (requestMD5 != null)
					{
						MD5ReaderStream mD5ReaderStream = new MD5ReaderStream(stream, base.RequestContentLength, true, base.RequestContext)
						{
							HashToVerifyAgainst = requestMD5
						};
						stream = mD5ReaderStream;
					}
					using (stream)
					{
						num = (base.RequestContentLength > (long)0 ? base.RequestContentLength : num2 + (long)1);
						long num4 = num;
						asyncResult = AsyncStreamCopy.BeginAsyncStreamCopy(stream, memoryStream, num4, 65536, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.UpdateMessageImpl"));
						yield return asyncResult;
						num3 = AsyncStreamCopy.EndAsyncStreamCopy(asyncResult);
						if (num3 <= num2)
						{
							goto Label0;
						}
						IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
						verboseDebug.Log("Request body exceeds max permissible limit of {0}", new object[] { num2 });
						throw new RequestEntityTooLargeException(new long?(num2));
					}
					using (MemoryStream memoryStream1 = new MemoryStream(buffer.Buffer, 0, (int)num3))
					{
						if (memoryStream1.Length > (long)0)
						{
							numArray1 = this.UnwrapMessage(memoryStream1);
							IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
							object[] length = new object[] { (int)numArray1.Length };
							verbose.Log("Length of updated message is {0} bytes.", length);
							if ((int)numArray1.Length > (int)num1)
							{
								IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
								object[] objArray = new object[] { (int)numArray1.Length, num1 };
								stringDataEventStream.Log("Message length {0} is greater than max allowed {1}", objArray);
								throw new RequestEntityTooLargeException(new long?(num1));
							}
						}
					}
				}
			}
			finally
			{
				BufferPool.ReleaseBuffer(buffer);
				buffer = null;
			}
			asyncResult = this.queueManager.BeginUpdateMessage(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, str, numArray, numArray1, timeSpan, new TimeSpan?(this.operationContext.RemainingTimeout()), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("QueueProtocolHead.PutMessageImpl"));
			yield return asyncResult;
			PoppedMessage poppedMessage = this.queueManager.EndUpdateMessage(asyncResult);
			base.Response.AddHeader("x-ms-popreceipt", Convert.ToBase64String(poppedMessage.PopReceipt));
			string httpString = HttpUtilities.ConvertDateTimeToHttpString(poppedMessage.TimeNextVisible);
			base.Response.AddHeader("x-ms-time-next-visible", httpString);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.NoContent);
			base.SendSuccessResponse(false);
		}

		private void ValidateVisibilityTimeout(long? visibilityTimeout, bool inRequestBody = false)
		{
			long num;
			long num1;
			if (!base.RequestContext.IsRequestVersionAtLeastAugust11)
			{
				NephosAssertionException.Assert(base.Method == RestMethod.GET);
				num = (long)1;
				num1 = (long)7200;
			}
			else
			{
				num = (base.Method != RestMethod.GET ? (long)0 : (long)1);
				num1 = (long)604800;
			}
			if (visibilityTimeout.HasValue && (visibilityTimeout.Value < num || visibilityTimeout.Value > num1))
			{
				if (!inRequestBody)
				{
					throw new OutOfRangeQueryParameterProtocolException("visibilitytimeout", visibilityTimeout.ToString(), num.ToString(), num1.ToString());
				}
				throw new OutOfRangeXmlArgumentProtocolException("visibilitytimeout", visibilityTimeout.ToString(), num.ToString(), num1.ToString());
			}
		}

		private class ListMessagesResultEncoder : XmlListEncoder<ListMessagesResult, PoppedMessage, object>
		{
			private string queueName;

			private string version;

			private Encoding encoder;

			public ListMessagesResultEncoder(string version, string queueName)
			{
				this.version = version;
				this.queueName = queueName;
			}

			protected override void EncodeEndElements(XmlWriter xmlWriter, ListMessagesResult result, object loc)
			{
				xmlWriter.WriteEndElement();
				xmlWriter.WriteElementString("NextMarker", result.NextMarker);
				xmlWriter.WriteEndElement();
			}

			protected override void EncodeEntry(Uri requestUrl, object loc, PoppedMessage message, XmlWriter xmlWriter)
			{
				xmlWriter.WriteStartElement("QueueMessage");
				xmlWriter.WriteElementString("MessageId", message.Id);
				xmlWriter.WriteElementString("InsertionTime", HttpUtilities.ConvertDateTimeToHttpString(message.InsertionTime));
				xmlWriter.WriteElementString("ExpirationTime", HttpUtilities.ConvertDateTimeToHttpString(message.ExpiryTime));
				if (!VersioningHelper.IsPreSeptember09OrInvalidVersion(this.version))
				{
					xmlWriter.WriteElementString("DequeueCount", Convert.ToString(message.DequeueCount));
				}
				PoppedMessage poppedMessage = message;
				if (poppedMessage != null)
				{
					xmlWriter.WriteElementString("PopReceipt", Convert.ToBase64String(poppedMessage.PopReceipt));
					xmlWriter.WriteElementString("TimeNextVisible", HttpUtilities.ConvertDateTimeToHttpString(poppedMessage.TimeNextVisible));
				}
				xmlWriter.WriteElementString("MessageText", this.encoder.GetString(message.Body));
				xmlWriter.WriteEndElement();
			}

			protected override void EncodeInitialElements(Uri requestUrl, object loc, ListMessagesResult result, XmlWriter xmlWriter)
			{
				xmlWriter.WriteStartElement("EnumerationResults");
				xmlWriter.WriteAttributeString("QueueName", this.queueName);
				xmlWriter.WriteStartElement("QueueMessagesList");
			}
		}

		public class ListQueuesXmlListEncoder : XmlListEncoder<IListQueuesResultCollection, IListQueuesResultQueueProperties, ListQueuesOperationContext>
		{
			public ListQueuesXmlListEncoder()
			{
			}

			protected override void EncodeEndElements(XmlWriter xmlWriter, IListQueuesResultCollection result, ListQueuesOperationContext loc)
			{
				xmlWriter.WriteEndElement();
				xmlWriter.WriteElementString("NextMarker", result.NextMarker);
				xmlWriter.WriteEndElement();
			}

			protected override void EncodeEntry(Uri requestUrl, ListQueuesOperationContext loc, IListQueuesResultQueueProperties queueProperties, XmlWriter xmlWriter)
			{
				xmlWriter.WriteStartElement("Queue");
				if (!VersioningHelper.IsPreSeptember09OrInvalidVersion(loc.RequestVersion))
				{
					xmlWriter.WriteElementString("Name", queueProperties.QueueName);
				}
				else
				{
					xmlWriter.WriteElementString("QueueName", queueProperties.QueueName);
				}
				if (loc.IsIncludingUrlInResponse)
				{
					string str = string.Concat(HttpRequestAccessorCommon.TrimEndSlash(requestUrl.GetLeftPart(UriPartial.Path)), "/", queueProperties.QueueName);
					xmlWriter.WriteElementString("Url", str);
				}
				if (loc.IsFetchingMetadata)
				{
					MetadataEncoding.WriteMetadataToXml(xmlWriter, queueProperties.Metadata, true, loc.RequestVersion);
				}
				xmlWriter.WriteEndElement();
			}

			protected override void EncodeInitialElements(Uri requestUrl, ListQueuesOperationContext loc, IListQueuesResultCollection result, XmlWriter xmlWriter)
			{
				xmlWriter.WriteStartElement("EnumerationResults");
				if (!VersioningHelper.IsPreAugust13OrInvalidVersion(loc.RequestVersion))
				{
					string str = string.Concat(HttpRequestAccessorCommon.TrimEndSlash(requestUrl.GetLeftPart(UriPartial.Path)), "/");
					xmlWriter.WriteAttributeString("ServiceEndpoint", str);
				}
				else
				{
					xmlWriter.WriteAttributeString("AccountName", requestUrl.GetLeftPart(UriPartial.Path));
				}
				QueueProtocolHead.ListQueuesXmlListEncoder.WriteListOperationInfoToXml(xmlWriter, loc);
				xmlWriter.WriteStartElement("Queues");
			}

			private static void WriteListOperationInfoToXml(XmlWriter xmlWriter, ListQueuesOperationContext loc)
			{
				if (!string.IsNullOrEmpty(loc.Prefix))
				{
					xmlWriter.WriteElementString("Prefix", loc.Prefix);
				}
				if (!string.IsNullOrEmpty(loc.Marker))
				{
					xmlWriter.WriteElementString("Marker", loc.Marker);
				}
				if (loc.MaxResults.HasValue)
				{
					int value = loc.MaxResults.Value;
					xmlWriter.WriteElementString("MaxResults", value.ToString(CultureInfo.InvariantCulture));
				}
			}
		}

		private class MessagesXmlEncoder : XmlListEncoder<IEnumerable, PeekedMessage, object>
		{
			private bool skipMessageBody;

			private Encoding encoder;

			private string version;

			public MessagesXmlEncoder(string version, bool skipMessageBody = false)
			{
				this.version = version;
				this.skipMessageBody = skipMessageBody;
			}

			protected override void EncodeEndElements(XmlWriter xmlWriter, IEnumerable result, object loc)
			{
				xmlWriter.WriteEndElement();
			}

			protected override void EncodeEntry(Uri requestUrl, object loc, PeekedMessage message, XmlWriter xmlWriter)
			{
				xmlWriter.WriteStartElement("QueueMessage");
				xmlWriter.WriteElementString("MessageId", message.Id);
				xmlWriter.WriteElementString("InsertionTime", HttpUtilities.ConvertDateTimeToHttpString(message.InsertionTime));
				xmlWriter.WriteElementString("ExpirationTime", HttpUtilities.ConvertDateTimeToHttpString(message.ExpiryTime));
				PoppedMessage poppedMessage = message as PoppedMessage;
				if (poppedMessage != null)
				{
					xmlWriter.WriteElementString("PopReceipt", Convert.ToBase64String(poppedMessage.PopReceipt));
					xmlWriter.WriteElementString("TimeNextVisible", HttpUtilities.ConvertDateTimeToHttpString(poppedMessage.TimeNextVisible));
				}
				if (!this.skipMessageBody)
				{
					if (!VersioningHelper.IsPreSeptember09OrInvalidVersion(this.version))
					{
						xmlWriter.WriteElementString("DequeueCount", Convert.ToString(message.DequeueCount));
					}
					xmlWriter.WriteElementString("MessageText", this.encoder.GetString(message.Body));
				}
				xmlWriter.WriteEndElement();
			}

			protected override void EncodeInitialElements(Uri requestUrl, object loc, IEnumerable result, XmlWriter xmlWriter)
			{
				xmlWriter.WriteStartElement("QueueMessagesList");
			}
		}
	}
}