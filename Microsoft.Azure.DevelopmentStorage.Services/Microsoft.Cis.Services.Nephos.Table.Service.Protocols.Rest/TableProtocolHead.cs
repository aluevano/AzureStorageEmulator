using AsyncHelper;
using AsyncHelper.Streams;
using MeasurementEvents;
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
using Microsoft.Cis.Services.Nephos.Table.Service.Common;
using Microsoft.Cis.Services.Nephos.Table.Service.DataModel;
using Microsoft.Cis.Services.Nephos.Table.Service.TableManager;
using Microsoft.Data.OData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Services;
using System.Data.Services.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class TableProtocolHead : BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>, IDataServiceHost2, IDataServiceHost
	{
		private const int HttpResponseAccumulatorStreamSingleBufferSize = 4194304;

		private const int DefaultMaxPayloadSize = 4194304;

		private const long SizeOfEachSasIdentifier = 2000L;

		private const long MaxContentLengthForXmlSetContainerAcl = 10000L;

		public const int StreamCopyBufferSize = 65536;

		private bool GoingToAstoria;

		private ITableManager tableManager;

		private Stream measuredRequestStream;

		private Stream alternateRequestStream;

		private AccumulatorStream accumulatedResponseStream;

		private NephosErrorDetails errorDetails;

		private readonly DateTimeTimer flushTimer;

		private readonly IErrorResponseWriter errorResponseWriter;

		private readonly static Regex JsonVerboseRegex;

		private readonly object requestHttpMethodSyncObj = new object();

		private volatile string requestHttpMethod;

		public Uri AbsoluteRequestUri
		{
			get
			{
				return this.TableRequestUrl;
			}
		}

		public Uri AbsoluteServiceUri
		{
			get
			{
				Uri tableRequestUrl = this.TableRequestUrl;
				NephosAssertionException.Assert(base.UriComponents != null);
				if (!base.IsUriPathStyle)
				{
					return new Uri(tableRequestUrl.GetLeftPart(UriPartial.Authority));
				}
				string[] leftPart = new string[] { tableRequestUrl.GetLeftPart(UriPartial.Authority), tableRequestUrl.Segments[1] };
				return new Uri(string.Join("/", leftPart));
			}
		}

		public int BatchInnerOperationCount
		{
			get;
			internal set;
		}

		private ODataVersion EffectiveRequestMaxVersion
		{
			get
			{
				ODataVersion oDataVersion = ODataVersion.V1;
				if (base.RequestContext.IsRequestVersionAtLeastAugust13)
				{
					oDataVersion = ODataVersion.V3;
				}
				else if (base.RequestContext.IsRequestVersionAtLeastAugust11)
				{
					oDataVersion = ODataVersion.V2;
				}
				string item = this.RequestHeaders["MaxDataServiceVersion"];
				if (string.IsNullOrWhiteSpace(item))
				{
					item = this.RequestHeaders["DataServiceVersion"];
				}
				if (!string.IsNullOrWhiteSpace(item))
				{
					try
					{
						oDataVersion = ODataUtils.StringToODataVersion(item);
					}
					catch (ODataException oDataException)
					{
					}
				}
				return oDataVersion;
			}
		}

		public static HttpProcessorConfiguration HttpProcessorConfigurationDefaultInstance
		{
			get;
			set;
		}

		protected override bool IgnoreDisposeOnResponseStream
		{
			get
			{
				return true;
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get
			{
				return this.tableManager.OperationStatus;
			}
		}

		public bool PremiumTableAccountRequest
		{
			get;
			set;
		}

		public string RequestAccept
		{
			get
			{
				if (!base.RequestContext.IsRequestVersionAtLeastDecember15)
				{
					this.EnsureSupportedRequestAccept();
				}
				return this.RequestHeaders["Accept"];
			}
		}

		public string RequestAcceptCharSet
		{
			get
			{
				return this.RequestHeaders["Accept-Charset"];
			}
		}

		public new long RequestContentLength
		{
			get
			{
				return base.RequestContentLength;
			}
		}

		public new string RequestContentType
		{
			get
			{
				if (!base.RequestContext.IsRequestVersionAtLeastDecember15)
				{
					this.EnsureSupportedRequestContentType();
				}
				return base.RequestContentType;
			}
		}

		public WebHeaderCollection RequestHeaders
		{
			get
			{
				return base.RequestHeadersCollection;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1065")]
		public string RequestHttpMethod
		{
			get
			{
				string str;
				if (this.requestHttpMethod == null)
				{
					lock (this.requestHttpMethodSyncObj)
					{
						if (this.requestHttpMethod == null)
						{
							string httpMethod = base.RequestContext.HttpMethod;
							string[] values = this.RequestHeaders.GetValues("X-HTTP-Method");
							if (values == null || (int)values.Length == 0)
							{
								str = httpMethod;
							}
							else
							{
								if ((int)values.Length != 1)
								{
									throw new TableServiceProtocolException(TableStatusEntries.XMethodIncorrectCount);
								}
								str = values[0];
								if (httpMethod != "POST")
								{
									throw new TableServiceProtocolException(TableStatusEntries.XMethodNotUsingPost);
								}
								if (str != "DELETE" && str != "PUT" && str != "MERGE")
								{
									throw new TableServiceProtocolException(TableStatusEntries.XMethodIncorrectValue);
								}
							}
							this.requestHttpMethod = str;
						}
					}
				}
				return this.requestHttpMethod;
			}
		}

		public string RequestIfMatch
		{
			get
			{
				return this.RequestHeaders["If-Match"];
			}
		}

		public string RequestIfNoneMatch
		{
			get
			{
				return this.RequestHeaders["If-None-Match"];
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1065")]
		public string RequestMaxVersion
		{
			get
			{
				return JustDecompileGenerated_get_RequestMaxVersion();
			}
			set
			{
				JustDecompileGenerated_set_RequestMaxVersion(value);
			}
		}

		public string JustDecompileGenerated_get_RequestMaxVersion()
		{
			return this.RequestHeaders["MaxDataServiceVersion"];
		}

		public void JustDecompileGenerated_set_RequestMaxVersion(string value)
		{
			this.RequestHeaders["MaxDataServiceVersion"] = value;
		}

		public Stream RequestStream
		{
			get
			{
				return JustDecompileGenerated_get_RequestStream();
			}
			set
			{
				JustDecompileGenerated_set_RequestStream(value);
			}
		}

		public new Stream JustDecompileGenerated_get_RequestStream()
		{
			if (this.alternateRequestStream != null)
			{
				return this.alternateRequestStream;
			}
			return this.measuredRequestStream;
		}

		public void JustDecompileGenerated_set_RequestStream(Stream value)
		{
			this.alternateRequestStream = value;
		}

		public string RequestVersion
		{
			get
			{
				return this.RequestHeaders["DataServiceVersion"];
			}
		}

		public string ResponseCacheControl
		{
			get
			{
				return base.Response.Headers[HttpResponseHeader.CacheControl];
			}
			set
			{
				base.Response.Headers.Set(HttpResponseHeader.CacheControl, value);
			}
		}

		public string ResponseContentType
		{
			get
			{
				return base.Response.ContentType;
			}
			set
			{
				this.EnsureSupportedMediaType(value);
				base.Response.ContentType = value;
			}
		}

		public string ResponseETag
		{
			get
			{
				return base.Response.Headers[HttpResponseHeader.ETag];
			}
			set
			{
				base.Response.Headers.Set(HttpResponseHeader.ETag, value);
			}
		}

		public WebHeaderCollection ResponseHeaders
		{
			get
			{
				return base.Response.Headers;
			}
		}

		public string ResponseLocation
		{
			get
			{
				return base.Response.RedirectLocation;
			}
			set
			{
				base.Response.RedirectLocation = value;
			}
		}

		public int ResponseStatusCode
		{
			get
			{
				return (int)base.StatusCode;
			}
			set
			{
				this.LogResponseStatusCodeChange(value);
				base.StatusCode = (HttpStatusCode)value;
			}
		}

		public new Stream ResponseStream
		{
			get
			{
				return this.accumulatedResponseStream;
			}
		}

		public string ResponseVersion
		{
			get
			{
				return this.ResponseHeaders["DataServiceVersion"];
			}
			set
			{
			}
		}

		protected override string ServerResponseHeaderValue
		{
			get
			{
				if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
				{
					return "Windows-Azure-Table/1.0";
				}
				return "Table Service Version 1.0";
			}
		}

		public bool ShouldReadRequestBody
		{
			get
			{
				bool flag = (base.Method == RestMethod.GET ? false : base.Method != RestMethod.HEAD);
				if (!false)
				{
					return flag;
				}
				return true;
			}
		}

		private Uri TableRequestUrl
		{
			get
			{
				if (base.RequestContext.IsRequestVersionAtLeastFebruary12)
				{
					return base.RequestUrl;
				}
				UriBuilder uriBuilder = new UriBuilder(base.RequestContext.HttpListenerRequestUrl)
				{
					Port = base.RequestUrl.Port
				};
				return uriBuilder.Uri;
			}
		}

		static TableProtocolHead()
		{
			TableProtocolHead.JsonVerboseRegex = new Regex(";odata=(verbose|\\\"\\s*(verbose)\\s*\\\")", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}

		public TableProtocolHead(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, IStorageManager storageManager, Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager authenticationManager, ITableManager tableManager, HttpProcessorConfiguration configuration, TransformExceptionDelegate transformProviderException, IIpThrottlingTable ipThrottlingTable) : base(requestContext, storageManager, authenticationManager, configuration, transformProviderException, ipThrottlingTable)
		{
			this.tableManager = tableManager;
			this.flushTimer = new DateTimeTimer();
			if (base.IsRequestVersionAtLeast("2013-08-15"))
			{
				this.errorResponseWriter = new ODataErrorResponseWriter();
				return;
			}
			this.errorResponseWriter = new XmlErrorResponseWriter();
		}

		private void CheckPermissionCallback(PermissionLevel permissionLevel)
		{
			object[] accountName;
			CultureInfo invariantCulture;
			switch (permissionLevel)
			{
				case PermissionLevel.Read:
				{
					if (this.operationContext.CallerIdentity.IsReadAllowed)
					{
						break;
					}
					throw new NephosUnauthorizedAccessException(this.operationContext.AccountName, this.operationContext.ContainerName, null, this.operationContext.CallerIdentity, PermissionLevel.Read, AuthorizationFailureReason.PermissionMismatch);
				}
				case PermissionLevel.Write:
				{
					if (this.operationContext.CallerIdentity.IsWriteAllowed)
					{
						break;
					}
					if (!this.operationContext.CallerIdentity.IsSecondaryAccess)
					{
						throw new NephosUnauthorizedAccessException(this.operationContext.AccountName, this.operationContext.ContainerName, null, this.operationContext.CallerIdentity, PermissionLevel.Write, AuthorizationFailureReason.PermissionMismatch);
					}
					throw new SecondaryWriteNotAllowedException();
				}
				case PermissionLevel.ReadWrite:
				{
					invariantCulture = CultureInfo.InvariantCulture;
					accountName = new object[] { this.operationContext.AccountName, this.operationContext.ContainerName };
					throw new NephosUnauthorizedAccessException(string.Format(invariantCulture, "Account {0} is not authorized to perform operation on table {1}.", accountName));
				}
				case PermissionLevel.Delete:
				{
					if (this.operationContext.CallerIdentity.IsDeleteAllowed)
					{
						break;
					}
					if (!this.operationContext.CallerIdentity.IsSecondaryAccess)
					{
						throw new NephosUnauthorizedAccessException(this.operationContext.AccountName, this.operationContext.ContainerName, null, this.operationContext.CallerIdentity, PermissionLevel.Delete, AuthorizationFailureReason.PermissionMismatch);
					}
					throw new SecondaryWriteNotAllowedException();
				}
				default:
				{
					invariantCulture = CultureInfo.InvariantCulture;
					accountName = new object[] { this.operationContext.AccountName, this.operationContext.ContainerName };
					throw new NephosUnauthorizedAccessException(string.Format(invariantCulture, "Account {0} is not authorized to perform operation on table {1}.", accountName));
				}
			}
		}

		internal bool CheckWhetherToSkipAcceptHeaderCheckPostJul15(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			if (value.Contains("atom"))
			{
				throw new TableServiceProtocolException(TableStatusEntries.AtomFormatNotSupported);
			}
			return true;
		}

		protected override BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.RestMethodImpl ChooseRestMethodHandler(RestMethod method)
		{
			if (method == RestMethod.Unknown)
			{
				throw new UnknownVerbProtocolException(base.HttpVerb);
			}
			if (base.RequestProtocolVersion == HttpVersion.Version10)
			{
				throw new HttpVersionNotSupportedException(base.RequestProtocolVersion, base.RequestVia);
			}
			base.ThrowIfApiNotSupportedForVersion(base.RequestSettings != null);
			if (method == RestMethod.OPTIONS)
			{
				this.operationContext.OperationMeasurementEvent = new TablePreflightRequestMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.TablePreflightApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.RestMethodImpl(this.TablePreflightRequestHandlerImpl);
			}
			if (this.GoingToAstoria)
			{
				if (!this.IsBatchRequest())
				{
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.TableAndRowOperationApisEnabled);
				}
				else
				{
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.TableBatchApiEnabled);
				}
				return new BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.RestMethodImpl(this.TableAndRowOperationImpl);
			}
			string subResource = this.operationContext.SubResource;
			if (!this.operationContext.ResourceIsAccount)
			{
				if (!this.operationContext.ResourceIsTable || !string.Equals(subResource, "acl", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidQueryParameterProtocolException("comp", subResource, null);
				}
				switch (method)
				{
					case RestMethod.GET:
					{
						this.operationContext.OperationMeasurementEvent = new GetTableAclMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetTableAclApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.RestMethodImpl(this.GetTableAclImpl);
					}
					case RestMethod.PUT:
					{
						this.operationContext.OperationMeasurementEvent = new SetTableAclMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SetTableAclApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.RestMethodImpl(this.SetTableAclImpl);
					}
				}
				throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.ReadWriteOperations);
			}
			if (subResource == null)
			{
				throw new InvalidUrlProtocolException(base.RequestUrl);
			}
			if (!this.operationContext.IsResourceTypeService || !string.Equals(subResource, "properties", StringComparison.OrdinalIgnoreCase))
			{
				if (!this.operationContext.IsResourceTypeService || !Comparison.StringEqualsIgnoreCase(subResource, "stats"))
				{
					throw new InvalidQueryParameterProtocolException("comp", subResource, null);
				}
				if (method != RestMethod.GET)
				{
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.GetOnlyOperations);
				}
				this.operationContext.OperationMeasurementEvent = new GetTableServiceStatsMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetTableServiceStatsApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.RestMethodImpl(this.GetTableServiceStatsImpl);
			}
			switch (method)
			{
				case RestMethod.GET:
				{
					this.operationContext.OperationMeasurementEvent = new GetTableServicePropertiesMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetTableServicePropertiesApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.RestMethodImpl(this.GetTableServicePropertiesImpl);
				}
				case RestMethod.PUT:
				{
					this.operationContext.OperationMeasurementEvent = new SetTableServicePropertiesMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SetTableServicePropertiesApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.RestMethodImpl(this.SetTableServicePropertiesImpl);
				}
			}
			throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.ReadWriteOperations);
		}

		public static IProcessor Create(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, IStorageManager storageManager, Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager authenticationManager, ITableManager tableManager, HttpProcessorConfiguration configuration, TransformExceptionDelegate transformProviderException, IIpThrottlingTable ipThrottlingTable)
		{
			return new TableProtocolHead(requestContext, storageManager, authenticationManager, tableManager, configuration, transformProviderException, ipThrottlingTable);
		}

		internal void EnsureAcceptHeaderValuePostJul15(string mediaType)
		{
			if (string.IsNullOrWhiteSpace(mediaType))
			{
				throw new TableServiceProtocolException(TableStatusEntries.AtomFormatNotSupported);
			}
			this.EnsureMediaTypeDoesNotContainAtom(mediaType);
			this.EnsureSupportedMediaType(mediaType);
		}

		private void EnsureAcceptTypeIncludesXml()
		{
			string[] requestAcceptTypes = base.RequestContext.RequestAcceptTypes;
			bool flag = true;
			if (requestAcceptTypes != null)
			{
				flag = false;
				string[] strArrays = requestAcceptTypes;
				int num = 0;
				while (num < (int)strArrays.Length)
				{
					string str = strArrays[num];
					if (string.IsNullOrEmpty(str) || !TableProtocolHeadHelper.IsMediaTypeEqual("application/xml", str))
					{
						num++;
					}
					else
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				throw new TableServiceProtocolException(TableStatusEntries.MediaTypeNotSupported);
			}
		}

		internal void EnsureContentTypeHeaderValuePostJul15(string mediaType)
		{
			if (string.IsNullOrWhiteSpace(mediaType))
			{
				return;
			}
			this.EnsureMediaTypeDoesNotContainAtom(mediaType);
			this.EnsureSupportedMediaType(mediaType);
		}

		private void EnsureContentTypeIsXml()
		{
			if (!string.IsNullOrEmpty(base.RequestContentType) && !TableProtocolHeadHelper.IsMediaTypeEqual("application/xml", base.RequestContentType))
			{
				throw new TableServiceProtocolException(TableStatusEntries.MediaTypeNotSupported);
			}
		}

		private void EnsureMediaTypeDoesNotContainAtom(string mediaType)
		{
			if (mediaType.Contains("atom"))
			{
				throw new TableServiceProtocolException(TableStatusEntries.AtomFormatNotSupported);
			}
		}

		internal void EnsureSupportedMediaType(string mediaType)
		{
			if (!string.IsNullOrEmpty(mediaType) && TableProtocolHeadHelper.IsMediaTypeJSON(mediaType))
			{
				if (!base.RequestContext.IsRequestVersionAtLeastAugust13 || this.EffectiveRequestMaxVersion < ODataVersion.V3)
				{
					throw new TableServiceProtocolException(TableStatusEntries.JsonFormatNotSupported);
				}
				if (TableProtocolHead.JsonVerboseRegex.Match(mediaType).Success)
				{
					throw new TableServiceProtocolException(TableStatusEntries.JsonVerboseFormatNotSupported);
				}
			}
		}

		internal void EnsureSupportedRequestAccept()
		{
			if (base.RequestContext.RequestAcceptTypes != null && base.RequestContext.RequestAcceptTypes.Any<string>())
			{
				this.EnsureSupportedMediaType(base.RequestContext.RequestAcceptTypes[0]);
			}
		}

		internal void EnsureSupportedRequestContentType()
		{
			this.EnsureSupportedMediaType(base.RequestContentType);
		}

		protected override OperationContextWithAuthAndAccountContainer ExtractOperationContext(NephosUriComponents uriComponents)
		{
			if (string.IsNullOrEmpty(uriComponents.AccountName))
			{
				throw new InvalidUrlProtocolException(base.RequestUrl);
			}
			TableOperationContext tableOperationContext = new TableOperationContext(base.RequestContext.ElapsedTime)
			{
				AccountName = uriComponents.AccountName,
				ResourceIsAccount = string.IsNullOrEmpty(uriComponents.ContainerName),
				SubResource = base.RequestQueryParameters["comp"]
			};
			this.GoingToAstoria = (tableOperationContext.ResourceIsAccount ? false : tableOperationContext.SubResource == null);
			if (!string.IsNullOrEmpty(uriComponents.ContainerName) && !string.Equals(uriComponents.ContainerName, "$batch", StringComparison.OrdinalIgnoreCase) && uriComponents.ContainerName.IndexOf('(') == -1 && !string.Equals(uriComponents.ContainerName, "Tables", StringComparison.OrdinalIgnoreCase))
			{
				tableOperationContext.ContainerName = uriComponents.ContainerName;
				tableOperationContext.ResourceIsTable = true;
			}
			if (base.RequestQueryParameters["restype"] == "service")
			{
				tableOperationContext.IsResourceTypeService = true;
			}
			if (base.RequestContext.IsRequestVersionAtLeastMay16)
			{
				TimeSpan timeSpan = base.ExtractTimeoutFromContext();
				int num = -1;
				if (num != -1 && timeSpan.Seconds < num)
				{
					timeSpan = TimeSpan.FromSeconds((double)num);
				}
				tableOperationContext.SetUserTimeout(timeSpan, BasicHttpProcessor.DefaultMaxAllowedTimeout);
			}
			return tableOperationContext;
		}

		public Dictionary<string, string> GetContinuationTokenFromRequest(NameValueCollection queryParameters)
		{
			Dictionary<string, string> strs = null;
			if (queryParameters == null)
			{
				return strs;
			}
			string[] allKeys = queryParameters.AllKeys;
			for (int i = 0; i < (int)allKeys.Length; i++)
			{
				string str = allKeys[i];
				if (!string.IsNullOrEmpty(str))
				{
					string str1 = queryParameters.Get(str);
					if (str.StartsWith("Next", StringComparison.Ordinal))
					{
						if (strs == null)
						{
							strs = new Dictionary<string, string>();
						}
						try
						{
							strs[str.Substring("Next".Length)] = ContinuationTokenParser.DecodeContinuationToken(str1);
						}
						catch (ArgumentException argumentException)
						{
							throw new TableServiceArgumentException("ContinuationToken", argumentException);
						}
					}
				}
			}
			return strs;
		}

		public static NephosErrorDetails GetErrorDetailsForDataServiceException(DataServiceException dse, Microsoft.Cis.Services.Nephos.Common.RequestContext context)
		{
			object[] statusCode;
			IStringDataEventStream error;
			NephosErrorDetails nephosErrorDetail = null;
			NephosStatusEntry conditionNotMetWithNotModifiedStatusCode = null;
			MeasurementEventStatus expectedFailure = NephosRESTEventStatus.ExpectedFailure;
			HttpStatusCode httpStatusCode = (HttpStatusCode)dse.StatusCode;
			if (httpStatusCode <= HttpStatusCode.MethodNotAllowed)
			{
				if (httpStatusCode == HttpStatusCode.NotModified)
				{
					conditionNotMetWithNotModifiedStatusCode = CommonStatusEntries.ConditionNotMetWithNotModifiedStatusCode;
				}
				else if (httpStatusCode == HttpStatusCode.BadRequest)
				{
					conditionNotMetWithNotModifiedStatusCode = (context.IsRequestVersionAtLeastMay16 ? new NephosStatusEntry("InvalidInput", HttpStatusCode.BadRequest, dse.Message) : CommonStatusEntries.InvalidInput);
				}
				else
				{
					switch (httpStatusCode)
					{
						case HttpStatusCode.NotFound:
						{
							conditionNotMetWithNotModifiedStatusCode = CommonStatusEntries.ResourceNotFound;
							break;
						}
						case HttpStatusCode.MethodNotAllowed:
						{
							conditionNotMetWithNotModifiedStatusCode = TableStatusEntries.MethodNotAllowed;
							break;
						}
						default:
						{
							error = Logger<IRestProtocolHeadLogger>.Instance.Error;
							statusCode = new object[] { dse.StatusCode, dse.ToString() };
							error.Log("A DataServiceException was thrown for which a corresponding mapping doesn't exist. DataServiceException.StatusCode: {0}\nException: {1}.", statusCode);
							conditionNotMetWithNotModifiedStatusCode = TableProtocolHead.GetStatusEntryForDataServiceException(dse);
							expectedFailure = NephosRESTEventStatus.UnknownFailure;
							nephosErrorDetail = new NephosErrorDetails(conditionNotMetWithNotModifiedStatusCode, expectedFailure, dse);
							NephosAssertionException.Assert(dse.StatusCode == (int)nephosErrorDetail.StatusEntry.StatusCodeHttp, "A DataServiceException's HttpStatusCode was mapped to a different HttpStatusCode.");
							return nephosErrorDetail;
						}
					}
				}
			}
			else if (httpStatusCode == HttpStatusCode.PreconditionFailed)
			{
				conditionNotMetWithNotModifiedStatusCode = CommonStatusEntries.DefaultConditionNotMet;
			}
			else if (httpStatusCode == HttpStatusCode.UnsupportedMediaType)
			{
				conditionNotMetWithNotModifiedStatusCode = TableStatusEntries.MediaTypeNotSupported;
			}
			else
			{
				if (httpStatusCode != HttpStatusCode.NotImplemented)
				{
					error = Logger<IRestProtocolHeadLogger>.Instance.Error;
					statusCode = new object[] { dse.StatusCode, dse.ToString() };
					error.Log("A DataServiceException was thrown for which a corresponding mapping doesn't exist. DataServiceException.StatusCode: {0}\nException: {1}.", statusCode);
					conditionNotMetWithNotModifiedStatusCode = TableProtocolHead.GetStatusEntryForDataServiceException(dse);
					expectedFailure = NephosRESTEventStatus.UnknownFailure;
					nephosErrorDetail = new NephosErrorDetails(conditionNotMetWithNotModifiedStatusCode, expectedFailure, dse);
					NephosAssertionException.Assert(dse.StatusCode == (int)nephosErrorDetail.StatusEntry.StatusCodeHttp, "A DataServiceException's HttpStatusCode was mapped to a different HttpStatusCode.");
					return nephosErrorDetail;
				}
				conditionNotMetWithNotModifiedStatusCode = CommonStatusEntries.NotImplemented;
			}
			nephosErrorDetail = new NephosErrorDetails(conditionNotMetWithNotModifiedStatusCode, expectedFailure, dse);
			NephosAssertionException.Assert(dse.StatusCode == (int)nephosErrorDetail.StatusEntry.StatusCodeHttp, "A DataServiceException's HttpStatusCode was mapped to a different HttpStatusCode.");
			return nephosErrorDetail;
		}

		[SuppressMessage("Microsoft.Usage", "CA2204:LiteralsShouldBeSpelledCorrectly", MessageId="Rethrowable")]
		public override NephosErrorDetails GetErrorDetailsForException(Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("exception");
			}
			Exception innerException = exception;
			IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
			object[] logString = new object[] { exception.GetLogString() };
			this.Log(info, "Getting error details for exception: {0}.", logString);
			if (innerException is RethrowableTableServiceException)
			{
				innerException = innerException.InnerException;
				NephosAssertionException.Assert(innerException != null, "Exception e is RethrowableTableServiceException but its inner exception was null.");
			}
			if (innerException is DataServiceException)
			{
				return TableProtocolHead.GetErrorDetailsForDataServiceException(innerException as DataServiceException, base.RequestContext);
			}
			if (innerException is TableServiceGeneralException)
			{
				return TableProtocolHead.GetErrorDetailsForTableServiceGeneralException(innerException as TableServiceGeneralException);
			}
			if (innerException is TableKeyTooLargeException)
			{
				return this.GetErrorDetailsForTableKeyTooLargeException(innerException as TableKeyTooLargeException);
			}
			if (innerException is TableServiceProtocolException)
			{
				return new NephosErrorDetails((innerException as TableServiceProtocolException).StatusEntry, NephosRESTEventStatus.ProtocolFailure, innerException);
			}
			if (innerException is TableServiceOverflowException)
			{
				return new NephosErrorDetails(CommonStatusEntries.OutOfRangeInput, NephosRESTEventStatus.ExpectedFailure, innerException);
			}
			if (innerException is TableServiceArgumentOutOfRangeException)
			{
				return new NephosErrorDetails(CommonStatusEntries.OutOfRangeInput, NephosRESTEventStatus.ExpectedFailure, innerException);
			}
			if (innerException is TableServiceArgumentException)
			{
				return new NephosErrorDetails(CommonStatusEntries.InvalidInput, NephosRESTEventStatus.ExpectedFailure, innerException);
			}
			if (innerException is RequestTooLargeException)
			{
				NameValueCollection nameValueCollection = new NameValueCollection()
				{
					{ "Cache-Control", "no-cache" }
				};
				NephosErrorDetails nephosErrorDetail = new NephosErrorDetails(TableStatusEntries.ContentLengthExceeded, NephosRESTEventStatus.ExpectedFailure, innerException, nameValueCollection, null)
				{
					StatusEntry = new NephosStatusEntry(nephosErrorDetail.StatusEntry.StatusId, nephosErrorDetail.StatusEntry.StatusCodeHttp, "Bad Request")
				};
				return nephosErrorDetail;
			}
			if (innerException is NotSupportedException)
			{
				return new NephosErrorDetails(CommonStatusEntries.NotImplemented, NephosRESTEventStatus.ExpectedFailure, innerException);
			}
			if (innerException is ContainerNotFoundException)
			{
				return new NephosErrorDetails(TableStatusEntries.TableNotFound, NephosRESTEventStatus.ExpectedFailure, innerException);
			}
			if (innerException is ContainerAlreadyExistsException)
			{
				return new NephosErrorDetails(TableStatusEntries.TableAlreadyExists, NephosRESTEventStatus.ExpectedFailure, innerException);
			}
			if (innerException is SemaphoreQueueExhaustedException)
			{
				this.Log(Logger<IRestProtocolHeadLogger>.Instance.Warning, "Got SemaphoreQueueExhaustedException - too many requests pending in the queue.");
				return new NephosErrorDetails(CommonStatusEntries.ServerBusy, NephosRESTEventStatus.ThrottlingFailure, innerException);
			}
			if (innerException is XmlException)
			{
				return new NephosErrorDetails(CommonStatusEntries.InvalidXmlDocument, NephosRESTEventStatus.ExpectedFailure, innerException);
			}
			if (innerException is XStoreArgumentException)
			{
				return new NephosErrorDetails(CommonStatusEntries.InvalidInput, NephosRESTEventStatus.ExpectedFailure, innerException);
			}
			if (innerException is TableBatchDuplicateRowKeyException)
			{
				return new NephosErrorDetails((base.RequestContext.IsRequestVersionAtLeastMay16 ? TableStatusEntries.InvalidDuplicateRow : CommonStatusEntries.InvalidInput), NephosRESTEventStatus.ExpectedFailure, innerException);
			}
			if (innerException is InvalidOperationException)
			{
				return new NephosErrorDetails(CommonStatusEntries.InvalidInput, NephosRESTEventStatus.ExpectedFailure, innerException);
			}
			IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Info;
			object[] objArray = new object[] { exception.GetLogString() };
			this.Log(stringDataEventStream, "Table service did not map exception: {0}. Reverting to common exception handling.", objArray);
			return base.GetErrorDetailsForException(innerException);
		}

		public NephosErrorDetails GetErrorDetailsForTableKeyTooLargeException(TableKeyTooLargeException e)
		{
			switch (e.KeyType)
			{
				case KeyType.PartitionKey:
				{
					return new NephosErrorDetails(TableStatusEntries.PartitionKeyValueTooLarge, NephosRESTEventStatus.ExpectedFailure, e);
				}
				case KeyType.RowKey:
				{
					return new NephosErrorDetails(TableStatusEntries.RowKeyValueTooLarge, NephosRESTEventStatus.ExpectedFailure, e);
				}
			}
			IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
			object[] str = new object[] { e.ToString() };
			error.Log("Unknown KeyType encountered in {0}", str);
			return base.GetErrorDetailsForException(e);
		}

		public static NephosErrorDetails GetErrorDetailsForTableServiceGeneralException(TableServiceGeneralException xse)
		{
			switch (xse.ErrorCode)
			{
				case TableServiceError.TableHasNoProperties:
				{
					return new NephosErrorDetails(TableStatusEntries.TableHasNoProperties, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.CommandsInBatchActOnDifferentPartitions:
				{
					return new NephosErrorDetails(TableStatusEntries.CommandsInBatchActOnDifferentPartitions, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.DuplicatePropertiesSpecified:
				{
					return new NephosErrorDetails(TableStatusEntries.DuplicatePropertiesSpecified, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.TableHasNoSuchProperty:
				{
					return new NephosErrorDetails(TableStatusEntries.TableHasNoSuchProperty, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.DuplicateKeyPropertySpecified:
				{
					return new NephosErrorDetails(TableStatusEntries.DuplicateKeyPropertySpecified, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.EntityNotFound:
				{
					return new NephosErrorDetails(CommonStatusEntries.ResourceNotFound, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.EntityAlreadyExists:
				{
					return new NephosErrorDetails(TableStatusEntries.EntityAlreadyExists, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.PartitionKeyNotSpecified:
				{
					return new NephosErrorDetails(TableStatusEntries.PartitionKeyNotSpecified, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.OperatorInvalid:
				{
					return new NephosErrorDetails(TableStatusEntries.OperatorInvalid, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.OperationTimedOut:
				{
					return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.UnknownFailure, xse);
				}
				case TableServiceError.UpdateConditionNotSatisfied:
				{
					return new NephosErrorDetails(TableStatusEntries.UpdateConditionNotSatisfied, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.PropertiesNeedValue:
				{
					return new NephosErrorDetails(TableStatusEntries.PropertiesNeedValue, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.PartitionKeyPropertyCannotBeUpdated:
				{
					return new NephosErrorDetails(TableStatusEntries.PartitionKeyPropertyCannotBeUpdated, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.EntityTooLarge:
				{
					return new NephosErrorDetails(TableStatusEntries.EntityTooLarge, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.PropertyValueTooLarge:
				{
					return new NephosErrorDetails(TableStatusEntries.PropertyValueTooLarge, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.InvalidValueType:
				{
					return new NephosErrorDetails(TableStatusEntries.InvalidValueType, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.PropertyNameTooLong:
				{
					return new NephosErrorDetails(TableStatusEntries.PropertyNameTooLong, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.TooManyProperties:
				{
					return new NephosErrorDetails(TableStatusEntries.TooManyProperties, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.PropertyNameInvalid:
				{
					return new NephosErrorDetails(TableStatusEntries.PropertyNameInvalid, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				case TableServiceError.ContentLengthExceeded:
				{
					return new NephosErrorDetails(TableStatusEntries.ContentLengthExceeded, NephosRESTEventStatus.ExpectedFailure, xse);
				}
				default:
				{
					return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.UnknownFailure, xse);
				}
			}
		}

		public string GetQueryStringItem(string item)
		{
			return base.RequestQueryParameters[item];
		}

		public static NephosStatusEntry GetStatusEntryForDataServiceException(DataServiceException dse)
		{
			int statusCode = dse.StatusCode;
			return new NephosStatusEntry(statusCode.ToString(CultureInfo.InvariantCulture), (HttpStatusCode)dse.StatusCode, dse.Message);
		}

		protected override string GetStringToSign(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, NephosUriComponents uriComponents, SupportedAuthScheme requestAuthScheme)
		{
			switch (requestAuthScheme)
			{
				case SupportedAuthScheme.SharedKey:
				{
					return TableAuthenticationHelper.GetStringToSignForNephosSharedKeyAuth(requestContext, uriComponents);
				}
				case SupportedAuthScheme.SharedKeyLite:
				{
					return TableAuthenticationHelper.GetStringToSignForNephosSharedKeyLiteAuth(requestContext, uriComponents);
				}
			}
			CultureInfo installedUICulture = CultureInfo.InstalledUICulture;
			object[] objArray = new object[] { requestAuthScheme };
			throw new NotSupportedException(string.Format(installedUICulture, "Authentication scheme {0} is not supported.", objArray));
		}

		private IEnumerator<IAsyncResult> GetTableAclImpl(AsyncIteratorContext<NoResults> async)
		{
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.EnsureAcceptTypeIncludesXml();
			}
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			IAsyncResult stream = this.tableManager.BeginGetTableAcl(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.RemainingTimeout(), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("TableProtocolHead.GetTableAclImpl"));
			yield return stream;
			ContainerAclSettings containerAclSetting = this.tableManager.EndGetTableAcl(stream);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			ListContainersAclXmlListEncoder listContainersAclXmlListEncoder = new ListContainersAclXmlListEncoder(base.RequestContext.IsRequestVersionAtLeastApril17);
			using (Stream stream1 = base.GenerateMeasuredResponseStream(false))
			{
				base.Response.SendChunked = true;
				base.Response.ContentType = "application/xml";
				stream = listContainersAclXmlListEncoder.BeginEncodeListToStream(base.RequestUrl, containerAclSetting.SASIdentifiers, containerAclSetting, stream1, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("TableProtocolHead.GetTableAclImpl"));
				yield return stream;
				listContainersAclXmlListEncoder.EndEncodeListToStream(stream);
			}
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetTableServicePropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			IAsyncResult asyncResult = this.tableManager.BeginGetTableServiceProperties(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("TableProtocolHead.GetTableServicePropertiesImpl"));
			yield return asyncResult;
			AnalyticsSettings analyticsSetting = this.tableManager.EndGetTableServiceProperties(asyncResult);
			base.AddServiceResponseHeadersBeforeSendingResponse();
			asyncResult = base.BeginWriteAnalyticsSettings(analyticsSetting, AnalyticsSettingsHelper.GetSettingVersion(base.RequestContext), base.RequestContext.ServiceType, async.GetResumeCallback(), async.GetResumeState("TableProtocolHead.GetTableServicePropertiesImpl"));
			yield return asyncResult;
			base.EndWriteAnalyticsSettings(asyncResult);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetTableServiceStatsImpl(AsyncIteratorContext<NoResults> async)
		{
			object obj;
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			if (!base.UriComponents.IsSecondaryAccountAccess)
			{
				throw new InvalidQueryParameterProtocolException("comp", this.operationContext.SubResource, null);
			}
			IAsyncResult asyncResult = this.tableManager.BeginGetTableServiceStats(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("TableProtocolHead.GetTableServiceStatsImpl"));
			yield return asyncResult;
			GeoReplicationStats geoReplicationStat = this.tableManager.EndGetTableServiceStats(asyncResult);
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] containerName = new object[] { this.operationContext.ContainerName, null };
			object[] objArray = containerName;
			obj = (geoReplicationStat != null ? geoReplicationStat.ToString() : "null");
			objArray[1] = obj;
			verbose.Log("Table {0} GeoReplicationStats is '{1}'", containerName);
			base.AddServiceResponseHeadersBeforeSendingResponse();
			asyncResult = base.BeginWriteServiceStats(geoReplicationStat, async.GetResumeCallback(), async.GetResumeState("TableProtocolHead.GetTableServicePropertiesImpl"));
			yield return asyncResult;
			base.EndWriteServiceStats(asyncResult);
			base.SendSuccessResponse(false);
		}

		public bool IsBatchRequest()
		{
			return TableProtocolHeadHelper.IsBatchRequest(this.AbsoluteRequestUri, this.AbsoluteServiceUri);
		}

		internal void Log(IStringDataEventStream loggerStream, string format, params object[] args)
		{
			string str = string.Format(CultureInfo.InvariantCulture, format, args);
			this.Log(loggerStream, str);
		}

		internal void Log(IStringDataEventStream loggerStream, string message)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] method = new object[] { base.Method, base.SafeRequestUrlString, message };
			loggerStream.Log(string.Format(invariantCulture, "For request {0} {1}: {2}", method));
		}

		internal void LogResponseStatusCodeChange(int statusCode)
		{
			IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
			object[] method = new object[] { base.Method, base.SafeRequestUrlString, statusCode };
			info.Log("For request {0} {1}: Setting status code to: {2}", method);
		}

		public void OnErrorInAstoriaProcessing(NephosErrorDetails nephosErrorDetails)
		{
			if (!this.IsBatchRequest() || !base.StatusCodeIsSet)
			{
				base.StatusCode = nephosErrorDetails.StatusEntry.StatusCodeHttp;
			}
			this.errorDetails = nephosErrorDetails;
			base.IsErrorSerializationHandled = true;
		}

		public void ProcessContinuationTokenAvailable(Dictionary<string, string> continuationToken)
		{
			if (continuationToken == null)
			{
				throw new ArgumentNullException("continuationToken");
			}
			NephosAssertionException.Assert((continuationToken == null ? false : continuationToken.Count > 0));
			StringBuilder stringBuilder = StringBuilderPool.Allocate();
			foreach (KeyValuePair<string, string> keyValuePair in continuationToken)
			{
				string str = string.Concat("x-ms-continuation-", keyValuePair.Key);
				base.Response.Headers[str] = ContinuationTokenParser.EncodeContinuationToken(keyValuePair.Value);
				stringBuilder.AppendFormat("{0}={1} ;", keyValuePair.Key, keyValuePair.Value);
			}
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] objArray = new object[] { stringBuilder.ToString() };
			this.Log(verbose, "TableProtocolHead: ProcessContinuationTokensAvailable called with continuationToken: {0}", objArray);
			StringBuilderPool.Release(stringBuilder);
		}

		public void ProcessException(HandleExceptionArgs args)
		{
		}

		public void ProcessQueryRowCommandProperties(QueryRowCommandProperties queryRowCommandProperties)
		{
			if (queryRowCommandProperties == null)
			{
				throw new ArgumentNullException("queryRowCommandProperties");
			}
			if (queryRowCommandProperties.IsGetRowCommand)
			{
				GetRowMeasurementEvent getRowMeasurementEvent = new GetRowMeasurementEvent()
				{
					AccountName = this.operationContext.OperationMeasurementEvent.AccountName,
					IsAdmin = this.operationContext.OperationMeasurementEvent.IsAdmin,
					Origin = this.operationContext.OperationMeasurementEvent.Origin
				};
				ITableMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as ITableMeasurementEvent;
				NephosAssertionException.Assert(operationMeasurementEvent != null);
				((ITableMeasurementEvent)getRowMeasurementEvent).TableName = operationMeasurementEvent.TableName;
				this.operationContext.OperationMeasurementEvent = getRowMeasurementEvent;
			}
		}

		[SuppressMessage("Anvil.Leak!Disposable", "27400", Justification="The measurement events are hard to dispose.  They should probably not implement IDisposable.")]
		private void ProcessRequestStarted(OperationInfo info)
		{
			string str;
			this.operationContext.ContainerName = info.TableName;
			if (!SpecialNames.CheckTableContainerNameAllowedForVersion(this.operationContext.ContainerName, base.RequestRestVersion, out str))
			{
				throw new DeprecatedResourceNameException(string.Format("The container name '{0}' is not allowed using '{1}' version", this.operationContext.ContainerName, base.RequestRestVersion), this.operationContext.ContainerName, str);
			}
			this.operationContext.MaxAllowedTimeout = BasicHttpProcessor.DefaultMaxAllowedTimeout;
			if (!this.IsBatchRequest())
			{
				switch (info.Resource)
				{
					case Resource.Table:
					{
						string requestHttpMethod = this.RequestHttpMethod;
						string str1 = requestHttpMethod;
						if (requestHttpMethod != null)
						{
							if (str1 == "POST")
							{
								this.operationContext.OperationMeasurementEvent = new CreateTableMeasurementEvent();
							}
							else if (str1 == "DELETE")
							{
								this.operationContext.OperationMeasurementEvent = new DeleteTableMeasurementEvent();
							}
							else if (str1 == "GET")
							{
								string absolutePath = this.AbsoluteRequestUri.AbsolutePath;
								if (absolutePath.IndexOf('(') <= 0 || absolutePath.IndexOf("Name") <= 0)
								{
									this.operationContext.OperationMeasurementEvent = new GetTablesMeasurementEvent();
								}
								else
								{
									this.operationContext.OperationMeasurementEvent = new GetTableMeasurementEvent();
								}
							}
						}
						this.operationContext.ContainerName = Resource.Table.ToString();
						if (this.operationContext.OperationMeasurementEvent == null)
						{
							break;
						}
						((ITableMeasurementEvent)this.operationContext.OperationMeasurementEvent).TableName = this.operationContext.ContainerName;
						break;
					}
					case Resource.Row:
					{
						string requestHttpMethod1 = this.RequestHttpMethod;
						string str2 = requestHttpMethod1;
						if (requestHttpMethod1 == null)
						{
							break;
						}
						if (str2 == "POST")
						{
							this.operationContext.OperationMeasurementEvent = new InsertRowMeasurementEvent();
							break;
						}
						else if (str2 == "PUT")
						{
							if (base.RequestHeadersCollection["If-Match"] != null)
							{
								this.operationContext.OperationMeasurementEvent = new UpdateRowMeasurementEvent(false);
								break;
							}
							else
							{
								this.operationContext.OperationMeasurementEvent = new UpsertRowMeasurementEvent(false);
								break;
							}
						}
						else if (str2 == "MERGE")
						{
							if (base.RequestHeadersCollection["If-Match"] != null)
							{
								this.operationContext.OperationMeasurementEvent = new UpdateRowMeasurementEvent(true);
								break;
							}
							else
							{
								this.operationContext.OperationMeasurementEvent = new UpsertRowMeasurementEvent(true);
								break;
							}
						}
						else if (str2 == "DELETE")
						{
							this.operationContext.OperationMeasurementEvent = new DeleteRowMeasurementEvent();
							break;
						}
						else if (str2 == "GET")
						{
							this.operationContext.OperationMeasurementEvent = new GetRowsMeasurementEvent();
							this.operationContext.MaxAllowedTimeout = BasicHttpProcessor.GetDefaultMaxTimeoutForListCommands(base.RequestRestVersion);
							break;
						}
						else
						{
							break;
						}
					}
				}
			}
			else
			{
				this.operationContext.OperationMeasurementEvent = new BatchMeasurementEvent();
			}
			if (this.operationContext.HttpRequestMeasurementEvent != null)
			{
				HttpTableRequestProcessedMeasurementEvent httpRequestMeasurementEvent = this.operationContext.HttpRequestMeasurementEvent as HttpTableRequestProcessedMeasurementEvent;
				NephosAssertionException.Assert(httpRequestMeasurementEvent != null);
				httpRequestMeasurementEvent.TableName = info.TableName;
			}
			if (this.operationContext.OperationMeasurementEvent == null)
			{
				IStringDataEventStream warning = Logger<IRestProtocolHeadLogger>.Instance.Warning;
				object[] resource = new object[] { info.Resource, this.RequestHttpMethod };
				warning.Log("Operation type could not be determined from resource '{0}' and HTTP verb '{1}'.", resource);
				return;
			}
			this.operationContext.OperationMeasurementEvent.AccountName = info.AccountName;
			this.operationContext.OperationMeasurementEvent.IsAdmin = this.operationContext.CallerIdentity.IsAdmin;
			this.operationContext.OperationMeasurementEvent.Origin = RequestOrigin.External;
			ITableMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as ITableMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			operationMeasurementEvent.TableName = info.TableName;
		}

		public string ReadContentIdFromRequest(int maxValueLength, int maxSearchSize)
		{
			StreamReader streamReader = new StreamReader(this.RequestStream);
			string str = "Content-ID:";
			char[] chrArray = new char[maxSearchSize];
			int num = streamReader.Read(chrArray, 0, maxSearchSize);
			StringReader stringReader = new StringReader(new string(chrArray, 0, num));
			string str1 = stringReader.ReadLine();
			while (str1 != null)
			{
				if (!str1.StartsWith(str, StringComparison.OrdinalIgnoreCase))
				{
					str1 = stringReader.ReadLine();
				}
				else
				{
					str1 = str1.Substring(str.Length).Trim();
					if (str1.Length <= maxValueLength)
					{
						break;
					}
					str1 = null;
					break;
				}
			}
			return str1;
		}

		public void SendBatchRequestTooLargeResponse(string contentId)
		{
			string str = string.Concat("--batchresponse_48d85d9e-1ff5-458e-a330-8e2c201a0547\r\nContent-Type: multipart/mixed; boundary=changesetresponse_8e7f3c54-660e-466c-87d4-2f829afe49ff\r\n\r\n--changesetresponse_8e7f3c54-660e-466c-87d4-2f829afe49ff\r\nContent-Type: application/http\r\nContent-Transfer-Encoding: binary\r\n\r\nHTTP/1.1 400 Bad Request\r\nContent-ID: ", contentId, "\r\nCache-Control: no-cache\r\nDataServiceVersion: 1.0;\r\nContent-Type: application/xml\r\n\r\n");
			this.ResponseStatusCode = 202;
			this.ResponseCacheControl = "no-cache";
			this.ResponseContentType = "multipart/mixed; boundary=batchresponse_48d85d9e-1ff5-458e-a330-8e2c201a0547";
			RequestTooLargeException requestTooLargeException = new RequestTooLargeException();
			NephosErrorDetails errorDetailsForException = this.GetErrorDetailsForException(requestTooLargeException);
			this.OnErrorInAstoriaProcessing(errorDetailsForException);
			BufferWrapper buffer = null;
			try
			{
				buffer = BufferPool.GetBuffer(32768);
				byte[] numArray = buffer.Buffer;
				int bytes = Encoding.UTF8.GetBytes(str, 0, str.Length, numArray, 0);
				using (MemoryStream memoryStream = new MemoryStream(numArray))
				{
					memoryStream.Seek((long)bytes, SeekOrigin.Begin);
					this.WriteErrorInfoToMemoryStream(errorDetailsForException, requestTooLargeException, null, memoryStream);
					bytes = (int)memoryStream.Position;
				}
				bytes += Encoding.UTF8.GetBytes("\r\n--changesetresponse_8e7f3c54-660e-466c-87d4-2f829afe49ff--\r\n--batchresponse_48d85d9e-1ff5-458e-a330-8e2c201a0547--\r\n", 0, "\r\n--changesetresponse_8e7f3c54-660e-466c-87d4-2f829afe49ff--\r\n--batchresponse_48d85d9e-1ff5-458e-a330-8e2c201a0547--\r\n".Length, numArray, bytes);
				this.ResponseStream.Write(numArray, 0, bytes);
			}
			finally
			{
				if (buffer != null)
				{
					BufferPool.ReleaseBuffer(buffer);
				}
			}
		}

		private IEnumerator<IAsyncResult> SetTableAclImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.EnsureContentTypeIsXml();
				this.EnsureAcceptTypeIncludesXml();
			}
			ContainerAclSettings containerAclSetting = new ContainerAclSettings(new bool?(false), base.RequestRestVersion);
			if (this.RequestContentLength > (long)0)
			{
				if (this.RequestContentLength > (long)10000)
				{
					throw new RequestEntityTooLargeException(new long?((long)10000));
				}
				using (BufferPoolMemoryStream bufferPoolMemoryStream = new BufferPoolMemoryStream(65536))
				{
					using (Stream stream = base.GenerateMeasuredRequestStream())
					{
						IAsyncResult asyncResult = AsyncStreamCopy.BeginAsyncStreamCopy(stream, bufferPoolMemoryStream, this.RequestContentLength, 65536, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("TableProtocolHead.SetTableAclImpl"));
						yield return asyncResult;
						AsyncStreamCopy.EndAsyncStreamCopy(asyncResult);
					}
					bufferPoolMemoryStream.Seek((long)0, SeekOrigin.Begin);
					containerAclSetting.SASIdentifiers = BasicHttpProcessor.DecodeSASIdentifiersFromStream(bufferPoolMemoryStream, BasicHttpProcessorWithAuthAndAccountContainer<TableOperationContext>.DefaultXmlReaderSettings, true, base.RequestContext.IsRequestVersionAtLeastApril15, SASPermission.Table);
					if (containerAclSetting.SASIdentifiers.Count <= SASIdentifier.MaxSASIdentifiers)
					{
						goto Label0;
					}
					throw new InvalidXmlProtocolException(string.Concat("At most ", SASIdentifier.MaxSASIdentifiers, " signed identifier is allowed in the request body"));
				}
			}
			else if (this.RequestContentLength < (long)0)
			{
				long requestContentLength = this.RequestContentLength;
				throw new InvalidHeaderProtocolException("Content-Length", requestContentLength.ToString(CultureInfo.InvariantCulture));
			}
		Label0:
			IAsyncResult asyncResult1 = this.tableManager.BeginSetTableAcl(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, containerAclSetting, this.operationContext.RemainingTimeout(), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("TableProtocolHead.SetTableAclImpl"));
			yield return asyncResult1;
			this.tableManager.EndSetTableAcl(asyncResult1);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.NoContent);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> SetTableServicePropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			IAsyncResult asyncResult = base.BeginReadAnalyticsSettings(AnalyticsSettingsHelper.GetSettingVersion(base.RequestContext), base.RequestContext.ServiceType, async.GetResumeCallback(), async.GetResumeState("TableProtocolHead.SetTableServicePropertiesImpl"));
			yield return asyncResult;
			AnalyticsSettings analyticsSetting = base.EndReadAnalyticsSettings(asyncResult);
			asyncResult = this.tableManager.BeginSetTableServiceProperties(this.operationContext.CallerIdentity, this.operationContext.AccountName, analyticsSetting, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("TableProtocolHead.SetTableServicePropertiesImpl"));
			yield return asyncResult;
			this.tableManager.EndSetTableServiceProperties(asyncResult);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Accepted);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> TableAndRowOperationImpl(AsyncIteratorContext<NoResults> asynContext)
		{
			bool flag;
			IAsyncResult innerStream;
			Dictionary<string, string> continuationTokenFromRequest = this.GetContinuationTokenFromRequest(base.RequestQueryParameters);
			bool flag1 = true;
			if (base.RequestHeadersCollection["If-Match"] == "*")
			{
				flag1 = false;
			}
			TableProtocolHead tableProtocolHead = this;
			AccumulatorStream accumulatorStream = new AccumulatorStream(base.GenerateMeasuredResponseStream(false), 4194304, true);
			AccumulatorStream accumulatorStream1 = accumulatorStream;
			tableProtocolHead.accumulatedResponseStream = accumulatorStream;
			using (accumulatorStream1)
			{
				TableProtocolHead tableProtocolHead1 = this;
				Stream stream = base.GenerateMeasuredRequestStream();
				Stream stream1 = stream;
				tableProtocolHead1.measuredRequestStream = stream;
				using (stream1)
				{
					innerStream = this.tableManager.BeginPerformOperation(this.operationContext.CallerIdentity, this.operationContext.AccountName, this, new RequestStartedCallback(this.ProcessRequestStarted), new CheckPermissionDelegate(this.CheckPermissionCallback), new QueryRowCommandPropertiesAvailableCallback(this.ProcessQueryRowCommandProperties), continuationTokenFromRequest, new ContinuationTokenAvailableCallback(this.ProcessContinuationTokenAvailable), flag1, base.RequestContext, asynContext.GetResumeCallback(), asynContext.GetResumeState("TableProtocolHead.TableAndRowOperationImpl"));
					yield return innerStream;
					try
					{
						this.tableManager.EndPerformOperation(innerStream);
						if (this.BatchInnerOperationCount > 0 && this.operationContext.OperationMeasurementEvent is BatchMeasurementEvent)
						{
							(this.operationContext.OperationMeasurementEvent as BatchMeasurementEvent).OperationCount = this.BatchInnerOperationCount;
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
						object[] logString = new object[] { exception.GetLogString() };
						error.Log("Caught exception during performing table operation and rethrow: {0}", logString);
						base.IsErrorSerializationHandled = false;
						throw;
					}
				}
				base.AddServiceResponseHeadersBeforeSendingResponse();
				if (!base.ResponseCanHaveContentBody())
				{
					this.accumulatedResponseStream.EmptyStream();
				}
				else
				{
					this.flushTimer.Start();
					innerStream = this.accumulatedResponseStream.BeginFlushToInnerStream(this.operationContext.RemainingTimeout(), asynContext.GetResumeCallback(), asynContext.GetResumeState("TableProtocolHead.TableAndRowOperationImpl.Flush"));
					yield return innerStream;
					try
					{
						this.accumulatedResponseStream.EndFlushToInnerStream(innerStream);
						this.flushTimer.Stop();
					}
					catch (Exception exception3)
					{
						Exception exception2 = exception3;
						IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Error;
						object[] objArray = new object[] { exception2.GetLogString() };
						stringDataEventStream.Log("Caught exception during flushing table operation response into network and rethrow: {0}", objArray);
						base.IsErrorSerializationHandled = false;
						throw;
					}
				}
				flag = (this.accumulatedResponseStream.BytesAccumulated == 0 ? true : !base.IsErrorSerializationHandled);
				NephosAssertionException.Assert(flag);
			}
			this.measuredRequestStream = null;
			this.accumulatedResponseStream = null;
			if (this.errorDetails == null)
			{
				base.SendSuccessResponse(false);
			}
			else
			{
				base.SendErrorResponse(this.errorDetails);
			}
		}

		private IEnumerator<IAsyncResult> TablePreflightRequestHandlerImpl(AsyncIteratorContext<NoResults> async)
		{
			base.HandlePreflightCorsRequest(this.storageAccount.ServiceMetadata.TableAnalyticsSettings);
			yield break;
		}

		internal Exception TransformExceptionInternal(Exception e)
		{
			return base.TransformException(e);
		}

		protected override void WriteErrorInfoToMemoryStream(NephosErrorDetails errorDetails, Exception e, NameValueCollection additionalUserDetails, MemoryStream outStream)
		{
			if (base.IsRequestVersionAtLeast("2013-08-15"))
			{
				string requestMaxVersion = null;
				if (string.IsNullOrWhiteSpace(base.RequestHeadersCollection["MaxDataServiceVersion"]))
				{
					requestMaxVersion = (string.IsNullOrWhiteSpace(base.RequestHeadersCollection["DataServiceVersion"]) ? DataServiceProtocolVersion.V3.GetHeaderValue() : this.RequestVersion);
				}
				else
				{
					requestMaxVersion = this.RequestMaxVersion;
				}
				string str = "application/xml";
				if (base.RequestContext != null && base.RequestContext.RequestAcceptTypes != null)
				{
					string[] requestAcceptTypes = base.RequestContext.RequestAcceptTypes;
					int num = 0;
					while (num < (int)requestAcceptTypes.Length)
					{
						string str1 = requestAcceptTypes[num];
						if (TableProtocolHeadHelper.IsMediaTypeEqual("application/json", str1))
						{
							str = "application/json";
							break;
						}
						else if (!TableProtocolHeadHelper.IsMediaTypeEqual("application/xml", str1))
						{
							num++;
						}
						else
						{
							str = "application/xml";
							break;
						}
					}
				}
				if (errorDetails.ResponseHeaders == null)
				{
					errorDetails.ResponseHeaders = new NameValueCollection();
				}
				errorDetails.ResponseHeaders.Add("Content-Type", str);
				errorDetails.ResponseHeaders.Add("DataServiceVersion", requestMaxVersion);
			}
			this.errorResponseWriter.Write(outStream, errorDetails, e, base.IncludeInternalDetailsInErrorResponses);
		}

		private class ODataRequestMessage : IODataRequestMessage
		{
			private readonly Stream stream;

			private readonly Dictionary<string, string> headers;

			public IEnumerable<KeyValuePair<string, string>> Headers
			{
				get
				{
					return this.headers;
				}
			}

			public string Method
			{
				get;
				set;
			}

			public Uri Url
			{
				get;
				set;
			}

			public ODataRequestMessage(Stream stream)
			{
				this.stream = stream;
			}

			public string GetHeader(string headerName)
			{
				string str = null;
				this.headers.TryGetValue(headerName, out str);
				return str;
			}

			public Stream GetStream()
			{
				return this.stream;
			}

			public void SetHeader(string headerName, string headerValue)
			{
				this.headers[headerName] = headerValue;
			}
		}
	}
}