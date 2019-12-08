using AsyncHelper;
using AsyncHelper.Streams;
using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using Microsoft.Cis.Services.Nephos.Common.Versioning;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public abstract class BasicHttpProcessorWithAuthAndAccountContainer<T> : BasicHttpProcessor
	where T : OperationContextWithAuthAndAccountContainer, new()
	{
		private const int MaxContentLengthForXmlAnalyticsSettings = 51200;

		protected const int DefaultBufferSizeForBufferedMemoryStream = 8192;

		private const int MemoryBufferSizeForErrorInfo = 32768;

		private const string MessageDetails = "MessageDetails";

		public static string TransactionTypeSystem;

		public static string TransactionTypeUser;

		private readonly static TimeSpan MinimumTimeoutForErrorInfo;

		private bool? supportCrc64;

		private Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager authenticationManager;

		public T operationContext;

		protected IStorageAccount storageAccount;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="ProcessorConfiguration is intended to be readonly, even though the underlying type is not immutable.")]
		protected readonly HttpProcessorConfiguration ProcessorConfiguration;

		protected IIpThrottlingTable ipThrottlingTable;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="ReadOnlyCollection<T> is an immutable type.")]
		protected readonly static ReadOnlyCollection<RestMethod> ReadWriteDeleteOperations;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="ReadOnlyCollection<T> is an immutable type.")]
		protected readonly static ReadOnlyCollection<RestMethod> ReadWriteOperations;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="ReadOnlyCollection<T> is an immutable type.")]
		protected readonly static ReadOnlyCollection<RestMethod> ReadWriteNoHeadOperations;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="ReadOnlyCollection<T> is an immutable type.")]
		protected readonly static ReadOnlyCollection<RestMethod> ReadDeleteOperations;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="ReadOnlyCollection<T> is an immutable type.")]
		protected readonly static ReadOnlyCollection<RestMethod> WriteOperations;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="ReadOnlyCollection<T> is an immutable type.")]
		protected readonly static ReadOnlyCollection<RestMethod> PostGetOperations;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="ReadOnlyCollection<T> is an immutable type.")]
		protected readonly static ReadOnlyCollection<RestMethod> HeadOnlyOperations;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="ReadOnlyCollection<T> is an immutable type.")]
		protected readonly static ReadOnlyCollection<RestMethod> GetOnlyOperations;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="ReadOnlyCollection<T> is an immutable type.")]
		protected readonly static ReadOnlyCollection<RestMethod> DeleteOnlyOperations;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="DefaultXmlReaderSettings is intended to be readonly, even though the underlying type is not immutable.")]
		protected readonly static XmlReaderSettings DefaultXmlReaderSettings;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="DefaultXmlWriterSettings is intended to be readonly, even though the underlying type is not immutable.")]
		protected readonly static XmlWriterSettings DefaultXmlWriterSettings;

		private static Dictionary<string, long> unknownMeasurementEventStatusList;

		protected readonly TimeSpan MinimumSizeBasedTimeout;

		private bool shouldProcessRequestForCors;

		public int? AccountConcurrentRequestCount
		{
			get;
			set;
		}

		protected virtual bool AdjustRequestVersionBasedOnContainerAclSettings
		{
			get
			{
				return false;
			}
		}

		public bool IncludeInternalDetailsInErrorResponses
		{
			get
			{
				return this.ProcessorConfiguration.IncludeInternalDetailsInErrorResponses;
			}
		}

		protected bool IsErrorSerializationHandled
		{
			get;
			set;
		}

		protected bool IsStorageDomainNameUsed
		{
			get;
			private set;
		}

		protected bool IsUriPathStyle
		{
			get;
			private set;
		}

		public string OverrideRequestContentType
		{
			get;
			set;
		}

		public string OverrideResponseContentType
		{
			get;
			set;
		}

		protected bool RequestBilled
		{
			get;
			private set;
		}

		protected bool RequestMeasurementEventProcessed
		{
			get;
			private set;
		}

		protected abstract string ServerResponseHeaderValue
		{
			get;
		}

		public T ServiceOperationContext
		{
			get
			{
				return this.operationContext;
			}
		}

		public IStorageManager StorageManager
		{
			get;
			protected set;
		}

		public bool SupportCrc64
		{
			get
			{
				if (!this.supportCrc64.HasValue)
				{
					this.supportCrc64 = new bool?(false);
				}
				return this.supportCrc64.Value;
			}
		}

		protected TransformExceptionDelegate TransformProviderException
		{
			get;
			set;
		}

		protected NephosUriComponents UriComponents
		{
			get;
			private set;
		}

		static BasicHttpProcessorWithAuthAndAccountContainer()
		{
			BasicHttpProcessorWithAuthAndAccountContainer<T>.TransactionTypeSystem = "system";
			BasicHttpProcessorWithAuthAndAccountContainer<T>.TransactionTypeUser = "user";
			BasicHttpProcessorWithAuthAndAccountContainer<T>.MinimumTimeoutForErrorInfo = TimeSpan.FromSeconds(2);
			List<RestMethod> restMethods = new List<RestMethod>()
			{
				RestMethod.GET,
				RestMethod.HEAD,
				RestMethod.PUT,
				RestMethod.DELETE
			};
			BasicHttpProcessorWithAuthAndAccountContainer<T>.ReadWriteDeleteOperations = new ReadOnlyCollection<RestMethod>(restMethods);
			List<RestMethod> restMethods1 = new List<RestMethod>()
			{
				RestMethod.GET,
				RestMethod.HEAD,
				RestMethod.PUT
			};
			BasicHttpProcessorWithAuthAndAccountContainer<T>.ReadWriteOperations = new ReadOnlyCollection<RestMethod>(restMethods1);
			BasicHttpProcessorWithAuthAndAccountContainer<T>.ReadWriteNoHeadOperations = new ReadOnlyCollection<RestMethod>(new List<RestMethod>()
			{
				RestMethod.GET,
				RestMethod.PUT
			});
			List<RestMethod> restMethods2 = new List<RestMethod>()
			{
				RestMethod.GET,
				RestMethod.HEAD,
				RestMethod.DELETE
			};
			BasicHttpProcessorWithAuthAndAccountContainer<T>.ReadDeleteOperations = new ReadOnlyCollection<RestMethod>(restMethods2);
			BasicHttpProcessorWithAuthAndAccountContainer<T>.WriteOperations = new ReadOnlyCollection<RestMethod>(new List<RestMethod>()
			{
				RestMethod.PUT
			});
			BasicHttpProcessorWithAuthAndAccountContainer<T>.PostGetOperations = new ReadOnlyCollection<RestMethod>(new List<RestMethod>()
			{
				RestMethod.POST,
				RestMethod.GET
			});
			BasicHttpProcessorWithAuthAndAccountContainer<T>.HeadOnlyOperations = new ReadOnlyCollection<RestMethod>(new List<RestMethod>()
			{
				RestMethod.HEAD
			});
			BasicHttpProcessorWithAuthAndAccountContainer<T>.GetOnlyOperations = new ReadOnlyCollection<RestMethod>(new List<RestMethod>()
			{
				RestMethod.GET
			});
			BasicHttpProcessorWithAuthAndAccountContainer<T>.DeleteOnlyOperations = new ReadOnlyCollection<RestMethod>(new List<RestMethod>()
			{
				RestMethod.DELETE
			});
			XmlReaderSettings xmlReaderSetting = new XmlReaderSettings()
			{
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true,
				XmlResolver = null
			};
			BasicHttpProcessorWithAuthAndAccountContainer<T>.DefaultXmlReaderSettings = xmlReaderSetting;
			BasicHttpProcessorWithAuthAndAccountContainer<T>.DefaultXmlWriterSettings = new XmlWriterSettings()
			{
				NewLineHandling = NewLineHandling.None
			};
			BasicHttpProcessorWithAuthAndAccountContainer<T>.unknownMeasurementEventStatusList = new Dictionary<string, long>();
		}

		protected BasicHttpProcessorWithAuthAndAccountContainer(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, IStorageManager storageManager, Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager authenticationManager, HttpProcessorConfiguration configuration) : this(requestContext, storageManager, authenticationManager, configuration, null, null)
		{
		}

		protected BasicHttpProcessorWithAuthAndAccountContainer(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, IStorageManager storageManager, Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager authenticationManager, HttpProcessorConfiguration configuration, TransformExceptionDelegate transformProviderException, IIpThrottlingTable ipThrottlingTable) : base(requestContext)
		{
			NephosAssertionException.Assert(authenticationManager != null);
			NephosAssertionException.Assert(configuration != null);
			this.StorageManager = storageManager;
			this.authenticationManager = authenticationManager;
			this.ProcessorConfiguration = configuration;
			this.TransformProviderException = transformProviderException;
			this.ipThrottlingTable = ipThrottlingTable;
		}

		internal static string AccountUsageStatusToString(AccountUsageStatus? status)
		{
			if (!status.HasValue)
			{
				return "Unknown";
			}
			switch (status.Value)
			{
				case AccountUsageStatus.None:
				{
					return "None";
				}
				case AccountUsageStatus.BelowQuota:
				{
					return "BelowQuota";
				}
				case AccountUsageStatus.AboveQuota:
				{
					return "AboveQuota";
				}
			}
			return status.ToString();
		}

		public void AddCorsResponseHeaders(AnalyticsSettings settings)
		{
			bool flag = (settings == null ? false : settings.CorsEnabled);
			Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("CORS: Start processing actual CORS request");
			bool flag1 = (base.Method == RestMethod.GET ? true : base.Method == RestMethod.HEAD);
			string item = base.RequestHeadersCollection["Origin"];
			bool flag2 = !string.IsNullOrEmpty(item);
			if (!flag2 && !flag1)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("CORS: Origin request header doesn't exist and request method is not GET/HEAD so no CORS headers needed");
				return;
			}
			CorsRule corsRule = null;
			bool flag3 = false;
			if ((flag3 ? true : this.CheckCorsRuleMatch(settings, base.HttpVerb, item, out corsRule)))
			{
				if (corsRule.AllowAllOrigins)
				{
					this.AddExposedHeadersToResponse(corsRule, flag3);
					base.Response.Headers.Add("Access-Control-Allow-Origin", AnalyticsSettings.WildCard);
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] wildCard = new object[] { "Access-Control-Allow-Origin", AnalyticsSettings.WildCard };
					verbose.Log("CORS: Setting {0}:{1} header on actual CORS request", wildCard);
					return;
				}
				if (flag2)
				{
					this.AddExposedHeadersToResponse(corsRule, flag3);
					base.Response.Headers.Add("Access-Control-Allow-Origin", item);
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("CORS: Setting {0}:{1} header on actual CORS request", new object[] { "Access-Control-Allow-Origin", item });
					base.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
				}
				if (flag1)
				{
					base.Response.Headers.Add("Vary", "Origin");
					return;
				}
			}
			else if (flag1 && flag)
			{
				base.Response.Headers.Add("Vary", "Origin");
			}
		}

		public void AddCorsResponseHeadersIfNecessary(Microsoft.Cis.Services.Nephos.Common.ServiceType serviceType)
		{
			if (!this.shouldProcessRequestForCors)
			{
				return;
			}
			AnalyticsSettings blobAnalyticsSettings = null;
			if (this.storageAccount != null && this.storageAccount.ServiceMetadata != null)
			{
				switch (base.ServiceType)
				{
					case Microsoft.Cis.Services.Nephos.Common.ServiceType.BlobService:
					{
						blobAnalyticsSettings = this.storageAccount.ServiceMetadata.BlobAnalyticsSettings;
						break;
					}
					case Microsoft.Cis.Services.Nephos.Common.ServiceType.QueueService:
					{
						blobAnalyticsSettings = this.storageAccount.ServiceMetadata.QueueAnalyticsSettings;
						break;
					}
					case Microsoft.Cis.Services.Nephos.Common.ServiceType.TableService:
					{
						blobAnalyticsSettings = this.storageAccount.ServiceMetadata.TableAnalyticsSettings;
						break;
					}
					case Microsoft.Cis.Services.Nephos.Common.ServiceType.LocationAccountService:
					case Microsoft.Cis.Services.Nephos.Common.ServiceType.LocationService:
					{
						return;
					}
					case Microsoft.Cis.Services.Nephos.Common.ServiceType.FileService:
					{
						blobAnalyticsSettings = this.storageAccount.ServiceMetadata.FileAnalyticsSettings;
						break;
					}
					default:
					{
						return;
					}
				}
				this.shouldProcessRequestForCors = false;
				this.AddCorsResponseHeaders(blobAnalyticsSettings);
				return;
			}
		}

		private void AddExposedHeadersToResponse(CorsRule succeededRule, bool corsMatchesDC)
		{
			HashSet<string> strs = new HashSet<string>();
			if (succeededRule.ExposedPrefixedHeaders != null && succeededRule.ExposedPrefixedHeaders.Count > 0)
			{
				if (!succeededRule.AllowAllExposedHeaders)
				{
					foreach (string exposedPrefixedHeader in succeededRule.ExposedPrefixedHeaders)
					{
						foreach (string key in base.Response.Headers.Keys)
						{
							if (!key.StartsWith(exposedPrefixedHeader, StringComparison.OrdinalIgnoreCase) || key.Equals("Content-Length") || key.Equals("Date") || key.Equals("Transfer-Encoding"))
							{
								continue;
							}
							strs.Add(key);
						}
						if ("Content-Length".StartsWith(exposedPrefixedHeader, StringComparison.OrdinalIgnoreCase))
						{
							strs.Add("Content-Length");
						}
						if ("Date".StartsWith(exposedPrefixedHeader, StringComparison.OrdinalIgnoreCase))
						{
							strs.Add("Date");
						}
						if (!"Transfer-Encoding".StartsWith(exposedPrefixedHeader, StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						strs.Add("Transfer-Encoding");
					}
				}
				else
				{
					foreach (string str in base.Response.Headers.Keys)
					{
						if (str.Equals("Content-Length") || str.Equals("Date") || str.Equals("Transfer-Encoding"))
						{
							continue;
						}
						strs.Add(str);
					}
					strs.Add("Content-Length");
					strs.Add("Date");
					strs.Add("Transfer-Encoding");
				}
			}
			if (succeededRule.ExposedLiteralHeaders != null)
			{
				strs.UnionWith(succeededRule.ExposedLiteralHeaders);
			}
			if (strs.Count > 0)
			{
				string str1 = string.Join(",", strs.ToArray<string>());
				base.Response.Headers.Add("Access-Control-Expose-Headers", str1);
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("CORS: Setting {0}:{1} header on actual CORS request", new object[] { "Access-Control-Expose-Headers", str1 });
			}
		}

		protected void AddMetadataFromRequest(NameValueCollection container)
		{
			int num;
			int num1;
			bool applicationMetadataFromHeaders = HttpRequestAccessorCommon.GetApplicationMetadataFromHeaders(base.RequestHeadersCollection, base.RequestRestVersion, container, out num, out num1);
			if (this.operationContext.OperationMeasurementEvent != null)
			{
				INephosBaseOperationMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent;
				operationMeasurementEvent.MetadataKeySize = operationMeasurementEvent.MetadataKeySize + (long)num;
				INephosBaseOperationMeasurementEvent metadataValueSize = this.operationContext.OperationMeasurementEvent;
				metadataValueSize.MetadataValueSize = metadataValueSize.MetadataValueSize + (long)num1;
			}
			if (applicationMetadataFromHeaders)
			{
				throw new EmptyMetadataKeyProtocolException(base.RequestHeadersCollection["x-ms-meta-"]);
			}
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] count = new object[] { container.Count };
			verbose.Log("AddMetadataFromRequest >> Found {0} metadata items.", count);
		}

		public void AddServiceResponseHeadersBeforeSendingResponse()
		{
			if (base.Method != RestMethod.OPTIONS)
			{
				this.AddCorsResponseHeadersIfNecessary(base.ServiceType);
			}
		}

		private IEnumerator<IAsyncResult> AdjustRequestVersionBasedOnContainerAclSettingsImpl(NephosUriComponents uriComponents, TimeSpan timeout, AsyncIteratorContext<bool> context)
		{
			Duration startingNow = Duration.StartingNow;
			bool item = base.RequestQueryParameters["restype"] == "container";
			string str = uriComponents.ContainerName;
			if (string.IsNullOrEmpty(uriComponents.RemainingPart) && !item)
			{
				str = "$root";
			}
			using (IBlobContainer blobContainer = this.StorageManager.CreateBlobContainerInstance(uriComponents.AccountName, str))
			{
				blobContainer.Timeout = startingNow.Remaining(timeout);
				IAsyncResult asyncResult = blobContainer.BeginGetProperties(ContainerPropertyNames.ServiceMetadata, null, context.GetResumeCallback(), context.GetResumeState("ProcessImpl.AdjustRequestVersionBasedOnContainerAclSettingsImpl"));
				yield return asyncResult;
				try
				{
					blobContainer.EndGetProperties(asyncResult);
				}
				catch (TimeoutException timeoutException)
				{
					Logger<IRestProtocolHeadLogger>.Instance.ErrorDebug.Log("Request timed out while trying to get container Acl settings during version adjustment so not continuing");
					throw;
				}
				catch (ServerBusyException serverBusyException)
				{
					Logger<IRestProtocolHeadLogger>.Instance.ErrorDebug.Log("Got ServerBusy while trying to get container Acl settings during version adjustment so not continuing");
					throw;
				}
				catch (StorageManagerException storageManagerException1)
				{
					StorageManagerException storageManagerException = storageManagerException1;
					IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
					verboseDebug.Log("Got and swallowed exception when accessing container acl setting for version adjustment: {0}", new object[] { storageManagerException });
					goto Label0;
				}
				string str1 = (new ContainerAclSettings(blobContainer.ServiceMetadata)).PublicAccessLevel;
				if (Comparison.StringEqualsIgnoreCase(str1, "blob") || Comparison.StringEqualsIgnoreCase(str1, "container"))
				{
					IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
					object[] objArray = new object[] { "2009-09-19", str, str1 };
					stringDataEventStream.Log("Adjusting request version to {0} due to container {1} acl setting of {2}", objArray);
					base.ContainerAclAdjustedRequestRestVersion = "2009-09-19";
					context.ResultData = true;
				}
				else
				{
					IStringDataEventStream verboseDebug1 = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
					object[] objArray1 = new object[] { "2009-09-19", str, str1 };
					verboseDebug1.Log("Not Adjusting request version to {0} due to container {1} acl setting of {2}", objArray1);
					context.ResultData = false;
				}
			}
		Label0:
			yield break;
		}

		protected virtual void ApplyResourceToMeasurementEvent(INephosBaseMeasurementEvent measurementEvent)
		{
			NephosAssertionException.Assert(measurementEvent != null);
			measurementEvent.AccountName = this.operationContext.AccountName;
			measurementEvent.IsAdmin = this.operationContext.CallerIdentity.IsAdmin;
			measurementEvent.Origin = RequestOrigin.External;
		}

		private IAsyncResult BeginAdjustRequestVersionBasedOnContainerAclSettings(NephosUriComponents uriComponents, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<bool> asyncIteratorContext = new AsyncIteratorContext<bool>("BasicHttpProcessorWithAuthAndAccountContainer.BeginAdjustRequestVersionBasedOnContainerAclSettings", callback, state);
			asyncIteratorContext.Begin(this.AdjustRequestVersionBasedOnContainerAclSettingsImpl(uriComponents, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		protected IAsyncResult BeginGetResourceAccountProperties(string accountName, bool isAdminAccess, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IStorageAccount> asyncIteratorContext = new AsyncIteratorContext<IStorageAccount>("BasicHttpProcessorWithAuthAndAccountContainer.CheckAccountPermissions", callback, state);
			asyncIteratorContext.Begin(this.GetResourceAccountPropertiesImpl(accountName, isAdminAccess, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		protected IAsyncResult BeginGetUriComponents(Uri requestUrl, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NephosUriComponents> asyncIteratorContext = new AsyncIteratorContext<NephosUriComponents>("BasicHttpProcessorWithAuthAndAccountContainer.GetUriComponents", callback, state);
			asyncIteratorContext.Begin(this.GetUriComponentsImpl(requestUrl, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		protected IAsyncResult BeginPerformOperation(BasicHttpProcessorWithAuthAndAccountContainer<T>.RestMethodImpl methodImpl, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("HttpRestProcessor.BeginPerformOperation", callback, state);
			asyncIteratorContext.Begin(methodImpl(asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IAsyncResult BeginProcessException(Exception e, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BasicHttpProcessorWithAuthAndAccountContainer.ProcessException", callback, state);
			asyncIteratorContext.Begin(this.ProcessExceptionImpl(e, asyncIteratorContext));
			return asyncIteratorContext;
		}

		protected IAsyncResult BeginReadAnalyticsSettings(AnalyticsSettingsVersion settingsVersion, Microsoft.Cis.Services.Nephos.Common.ServiceType service, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<AnalyticsSettings> asyncIteratorContext = new AsyncIteratorContext<AnalyticsSettings>("BasicHttpProcessorWithAuthAndAccountContainer.ReadAnalyticsSettingsImpl", callback, state);
			asyncIteratorContext.Begin(this.ReadAnalyticsSettingsImpl(settingsVersion, service, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IAsyncResult BeginSendErrorResponse(NephosErrorDetails errorDetails, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BasicHttpProcessorWithAuthAndAccountContainer.SendErrorResponse", callback, state);
			asyncIteratorContext.Begin(this.SendErrorResponseImpl(errorDetails, asyncIteratorContext));
			return asyncIteratorContext;
		}

		protected IAsyncResult BeginWriteAnalyticsSettings(AnalyticsSettings settings, AnalyticsSettingsVersion settingsVersion, Microsoft.Cis.Services.Nephos.Common.ServiceType service, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BasicHttpProcessorWithAuthAndAccountContainer.WriteAnalyticsSettingsImpl", callback, state);
			asyncIteratorContext.Begin(this.WriteAnalyticsSettingsImpl(settings, settingsVersion, service, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IAsyncResult BeginWriteErrorInfoToResponseStream(NephosErrorDetails errorDetails, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BasicHttpProcessorWithAuthAndAccountContainer.WriteErrorInfoToResponseStream", callback, state);
			asyncIteratorContext.Begin(this.WriteErrorInfoToResponseStreamImpl(errorDetails, asyncIteratorContext));
			return asyncIteratorContext;
		}

		protected IAsyncResult BeginWriteServiceStats(GeoReplicationStats stats, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("BasicHttpProcessorWithAuthAndAccountContainer.WriteServiceStats", callback, state);
			asyncIteratorContext.Begin(this.WriteServiceStatsImpl(stats, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private void BlockHttpRestrictedChars(string validateString)
		{
			if (!string.IsNullOrEmpty(validateString))
			{
				for (int i = 0; i < validateString.Length; i++)
				{
					try
					{
						int utf32 = char.ConvertToUtf32(validateString, i);
						if (char.IsHighSurrogate(validateString, i))
						{
							i++;
						}
						if (HttpUtilities.IsHttpRestrictedCodepoint(utf32))
						{
							Logger<IRestProtocolHeadLogger>.Instance.Error.Log("BlockHttpRestrictedChar: 0x{0:X}", new object[] { utf32 });
							throw new InvalidUrlProtocolException(validateString);
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						Logger<IRestProtocolHeadLogger>.Instance.Error.Log("BlockHttpRestrictedChars: Exception: {0}", new object[] { exception });
						throw new InvalidUrlProtocolException(validateString);
					}
				}
			}
		}

		public void CaptureRequestContextState()
		{
			if (this.operationContext != null)
			{
				Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext = base.RequestContext;
			}
		}

		private bool CheckCorsRuleMatch(AnalyticsSettings settings, string requestMethod, string origin, out CorsRule succeededRule)
		{
			bool flag;
			succeededRule = null;
			if (settings == null || !settings.CorsEnabled)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("CORS: No CORS rules exist for this account service");
				return false;
			}
			List<CorsRule>.Enumerator enumerator = settings.CorsRules.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CorsRule current = enumerator.Current;
					if (!current.AllowedMethods.Contains(requestMethod) || !current.AllowAllOrigins && !current.AllowedOrigins.Contains(origin))
					{
						continue;
					}
					succeededRule = current;
					flag = true;
					return flag;
				}
				return false;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return flag;
		}

		private void CheckPreflightValidRequest()
		{
			this.EnsureMaxTimeoutIsNotExceeded(this.operationContext.MaxAllowedTimeout);
			if (string.IsNullOrEmpty(base.RequestHeadersCollection["Origin"]))
			{
				throw new CorsPreflightMissingHeaderException("Origin");
			}
			if (string.IsNullOrEmpty(base.RequestHeadersCollection["Access-Control-Request-Method"]))
			{
				throw new CorsPreflightMissingHeaderException("Access-Control-Request-Method");
			}
		}

		private bool CheckRequestHeaders(string requestHeaders, CorsRule corsRule)
		{
			if (corsRule.AllowAllAllowedHeaders)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(requestHeaders))
			{
				string[] strArrays = base.RequestHeadersCollection["Access-Control-Request-Headers"].Split(new char[] { ',' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i].Trim();
					if (!corsRule.AllowedLiteralHeaders.Contains(str))
					{
						bool flag = false;
						foreach (string allowedPrefixedHeader in corsRule.AllowedPrefixedHeaders)
						{
							if (!str.StartsWith(allowedPrefixedHeader, StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}
							flag = true;
							break;
						}
						if (!flag)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		protected abstract BasicHttpProcessorWithAuthAndAccountContainer<T>.RestMethodImpl ChooseRestMethodHandler(RestMethod method);

		protected BlobObjectCondition ConvertToBlobObjectCondition(ConditionInformation conditionInfo, Guid? leaseId, ComparisonOperator? sequenceNumberOperator, long? sequenceNumber, BlobType requiredBlobType, bool checkDiskMountState, bool skipLMTUpdate, bool isOperationAllowedOnArchivedBlobs, bool skipLeaseCheck, DateTime? lmt, string generationId, Guid? internalArchiveRequestId = null)
		{
			BlobObjectCondition blobObjectCondition = null;
			if (conditionInfo != null)
			{
				if (conditionInfo.IfModifiedSince.HasValue)
				{
					blobObjectCondition = new BlobObjectCondition()
					{
						IfModifiedSinceTime = conditionInfo.IfModifiedSince
					};
				}
				if (conditionInfo.IfNotModifiedSince.HasValue)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.IfNotModifiedSinceTime = conditionInfo.IfNotModifiedSince;
				}
				if (conditionInfo.IfMatch != null)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.IfLastModificationTimeMatch = conditionInfo.IfMatch;
				}
				if (conditionInfo.IfNoneMatch != null)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.IfLastModificationTimeMismatch = conditionInfo.IfNoneMatch;
				}
				if (blobObjectCondition != null)
				{
					blobObjectCondition.IsMultipleConditionalHeaderEnabled = conditionInfo.IsMultipleConditionalHeaderEnabled;
				}
			}
			if (leaseId.HasValue)
			{
				if (blobObjectCondition == null)
				{
					blobObjectCondition = new BlobObjectCondition();
				}
				blobObjectCondition.LeaseId = leaseId;
			}
			if (sequenceNumberOperator.HasValue || sequenceNumber.HasValue)
			{
				if (blobObjectCondition == null)
				{
					blobObjectCondition = new BlobObjectCondition();
				}
				blobObjectCondition.SequenceNumberOperator = sequenceNumberOperator;
				blobObjectCondition.SequenceNumber = sequenceNumber;
			}
			if (requiredBlobType != BlobType.None)
			{
				if (blobObjectCondition == null)
				{
					blobObjectCondition = new BlobObjectCondition();
				}
				blobObjectCondition.RequiredBlobType = requiredBlobType;
			}
			if (checkDiskMountState)
			{
				if (blobObjectCondition == null)
				{
					blobObjectCondition = new BlobObjectCondition();
				}
				blobObjectCondition.IsDiskMountStateConditionRequired = checkDiskMountState;
			}
			if (false)
			{
				if (skipLMTUpdate)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.IsSkippingLastModificationTimeUpdate = true;
				}
				if (isOperationAllowedOnArchivedBlobs)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.IsOperationAllowedOnArchivedBlobs = true;
				}
				if (skipLeaseCheck)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.SkipLeaseCheck = true;
				}
				if (lmt.HasValue)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.InternalArchiveBlobLMT = lmt;
				}
				if (!string.IsNullOrEmpty(generationId))
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.InternalArchiveBlobGenerationId = generationId;
				}
				if (internalArchiveRequestId.HasValue)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.InternalArchiveRequestId = internalArchiveRequestId;
				}
			}
			if (blobObjectCondition != null)
			{
				blobObjectCondition.IsIncludingUncommittedBlobs = true;
				if (true && base.RequestContext.IsRequestVersionAtLeastOctober16 && conditionInfo != null)
				{
					ResourceExistenceCondition? resourceExistsCondition = conditionInfo.ResourceExistsCondition;
					if ((resourceExistsCondition.GetValueOrDefault() != ResourceExistenceCondition.MustExist ? false : resourceExistsCondition.HasValue))
					{
						blobObjectCondition.IsIncludingUncommittedBlobs = false;
					}
				}
			}
			return blobObjectCondition;
		}

		protected static ContainerCondition ConvertToContainerCondition(ConditionInformation conditionInfo)
		{
			ContainerCondition containerCondition = null;
			if (conditionInfo != null)
			{
				if (conditionInfo.IfModifiedSince.HasValue)
				{
					containerCondition = new ContainerCondition()
					{
						IfModifiedSinceTime = conditionInfo.IfModifiedSince
					};
				}
				if (conditionInfo.IfNotModifiedSince.HasValue)
				{
					if (containerCondition == null)
					{
						containerCondition = new ContainerCondition();
					}
					containerCondition.IfNotModifiedSinceTime = conditionInfo.IfNotModifiedSince;
				}
			}
			return containerCondition;
		}

		protected BlobObjectCondition ConvertToSourceBlobObjectCondition(ConditionInformation conditionInfo, Guid? leaseId, ComparisonOperator? sequenceNumberOperator, long? sequenceNumber, BlobType requiredBlobType, bool checkDiskMountState, bool skipLMTUpdate, bool isOperationAllowedOnArchivedBlobs, bool skipLeaseCheck)
		{
			BlobObjectCondition blobObjectCondition = null;
			if (conditionInfo != null)
			{
				if (conditionInfo.CopySourceIfModifiedSince.HasValue)
				{
					blobObjectCondition = new BlobObjectCondition()
					{
						IfModifiedSinceTime = conditionInfo.CopySourceIfModifiedSince
					};
				}
				if (conditionInfo.CopySourceIfNotModifiedSince.HasValue)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.IfNotModifiedSinceTime = conditionInfo.CopySourceIfNotModifiedSince;
				}
				if (conditionInfo.CopySourceIfMatch.HasValue)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					DateTime[] value = new DateTime[1];
					DateTime? copySourceIfMatch = conditionInfo.CopySourceIfMatch;
					value[0] = copySourceIfMatch.Value;
					blobObjectCondition.IfLastModificationTimeMatch = value;
				}
				if (conditionInfo.CopySourceIfNoneMatch.HasValue)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					DateTime[] dateTimeArray = new DateTime[1];
					DateTime? copySourceIfNoneMatch = conditionInfo.CopySourceIfNoneMatch;
					dateTimeArray[0] = copySourceIfNoneMatch.Value;
					blobObjectCondition.IfLastModificationTimeMismatch = dateTimeArray;
				}
			}
			if (leaseId.HasValue)
			{
				if (blobObjectCondition == null)
				{
					blobObjectCondition = new BlobObjectCondition();
				}
				blobObjectCondition.LeaseId = leaseId;
			}
			if (sequenceNumberOperator.HasValue || sequenceNumber.HasValue)
			{
				if (blobObjectCondition == null)
				{
					blobObjectCondition = new BlobObjectCondition();
				}
				blobObjectCondition.SequenceNumberOperator = sequenceNumberOperator;
				blobObjectCondition.SequenceNumber = sequenceNumber;
			}
			if (requiredBlobType != BlobType.None)
			{
				if (blobObjectCondition == null)
				{
					blobObjectCondition = new BlobObjectCondition();
				}
				blobObjectCondition.RequiredBlobType = requiredBlobType;
			}
			if (checkDiskMountState)
			{
				if (blobObjectCondition == null)
				{
					blobObjectCondition = new BlobObjectCondition();
				}
				blobObjectCondition.IsDiskMountStateConditionRequired = true;
			}
			if (false)
			{
				if (skipLMTUpdate)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.IsSkippingLastModificationTimeUpdate = true;
				}
				if (isOperationAllowedOnArchivedBlobs)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.IsOperationAllowedOnArchivedBlobs = true;
				}
				if (skipLeaseCheck)
				{
					if (blobObjectCondition == null)
					{
						blobObjectCondition = new BlobObjectCondition();
					}
					blobObjectCondition.SkipLeaseCheck = true;
				}
			}
			if (blobObjectCondition != null)
			{
				blobObjectCondition.IsIncludingUncommittedBlobs = true;
				if (true && base.RequestContext.IsRequestVersionAtLeastOctober16 && conditionInfo != null)
				{
					ResourceExistenceCondition? resourceExistsCondition = conditionInfo.ResourceExistsCondition;
					if ((resourceExistsCondition.GetValueOrDefault() != ResourceExistenceCondition.MustExist ? false : resourceExistsCondition.HasValue))
					{
						blobObjectCondition.IsIncludingUncommittedBlobs = false;
					}
				}
			}
			return blobObjectCondition;
		}

		protected static FileObjectCondition ConvertToSourceFileObjectCondition(ConditionInformation conditionInfo)
		{
			FileObjectCondition fileObjectCondition = null;
			if (conditionInfo != null)
			{
				if (conditionInfo.CopySourceIfModifiedSince.HasValue)
				{
					fileObjectCondition = new FileObjectCondition()
					{
						IfModifiedSinceTime = conditionInfo.CopySourceIfModifiedSince
					};
				}
				if (conditionInfo.CopySourceIfNotModifiedSince.HasValue)
				{
					if (fileObjectCondition == null)
					{
						fileObjectCondition = new FileObjectCondition();
					}
					fileObjectCondition.IfNotModifiedSinceTime = conditionInfo.CopySourceIfNotModifiedSince;
				}
				if (conditionInfo.CopySourceIfMatch.HasValue)
				{
					if (fileObjectCondition == null)
					{
						fileObjectCondition = new FileObjectCondition();
					}
					DateTime[] value = new DateTime[1];
					DateTime? copySourceIfMatch = conditionInfo.CopySourceIfMatch;
					value[0] = copySourceIfMatch.Value;
					fileObjectCondition.IfLastModificationTimeMatch = value;
				}
				if (conditionInfo.CopySourceIfNoneMatch.HasValue)
				{
					if (fileObjectCondition == null)
					{
						fileObjectCondition = new FileObjectCondition();
					}
					DateTime[] dateTimeArray = new DateTime[1];
					DateTime? copySourceIfNoneMatch = conditionInfo.CopySourceIfNoneMatch;
					dateTimeArray[0] = copySourceIfNoneMatch.Value;
					fileObjectCondition.IfLastModificationTimeMismatch = dateTimeArray;
				}
			}
			return fileObjectCondition;
		}

		private void CreateAndCompleteQoSEventForPreAuthFailures(MeasurementEventStatus measurementStatus, string internalOperationStatusString)
		{
			INephosBaseOperationMeasurementEvent pThrottlingFailureMeasurementEvent = null;
			string clientIPAsString = base.RequestContext.ClientIPAsString;
			if (measurementStatus.Equals(NephosRESTEventStatus.IPThrottlingFailure))
			{
				pThrottlingFailureMeasurementEvent = new IPThrottlingFailureMeasurementEvent();
				(pThrottlingFailureMeasurementEvent as IPThrottlingFailureMeasurementEvent).ClientIPAddress = clientIPAsString;
			}
			else if (!measurementStatus.Equals(NephosRESTEventStatus.AuthenticationFailure))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Billing: Status before authentication with status code {0}", new object[] { measurementStatus });
				pThrottlingFailureMeasurementEvent = new PreAuthenticationFailureMeasurementEvent();
			}
			else
			{
				pThrottlingFailureMeasurementEvent = new AuthenticationFailureMeasurementEvent();
			}
			if (this.operationContext != null)
			{
				if (this.operationContext.AccountName != null)
				{
					pThrottlingFailureMeasurementEvent.AccountName = this.operationContext.AccountName;
				}
				if (this.operationContext.CallerIdentity != null)
				{
					pThrottlingFailureMeasurementEvent.IsAdmin = this.operationContext.CallerIdentity.IsAdmin;
				}
			}
			if (pThrottlingFailureMeasurementEvent.AccountName == null && this.UriComponents != null)
			{
				if (this.IsValidAccountName(this.UriComponents.AccountName))
				{
					pThrottlingFailureMeasurementEvent.AccountName = this.UriComponents.AccountName;
				}
			}
			pThrottlingFailureMeasurementEvent.Complete(measurementStatus);
			this.LogPerfCounter(pThrottlingFailureMeasurementEvent, internalOperationStatusString, measurementStatus, false, null);
			pThrottlingFailureMeasurementEvent.Dispose();
		}

		private void CreateAndCompleteQoSEventForPreOperationFailures(MeasurementEventStatus measurementStatus, string internalOperationStatusString)
		{
			NephosAssertionException.Assert(this.operationContext != null);
			string clientIPAsString = base.RequestContext.ClientIPAsString;
			INephosBaseOperationMeasurementEvent preOperationFailureMeasurementEvent = null;
			if (!measurementStatus.Equals(NephosRESTEventStatus.AuthenticationFailure))
			{
				preOperationFailureMeasurementEvent = new PreOperationFailureMeasurementEvent()
				{
					RequestHeaderBytesRead = (long)HttpUtilities.GetRequestHeaderLength(base.RequestHeadersCollection, base.HttpVerb, base.RequestRawUrlString),
					ResponseHeaderBytesWritten = (long)HttpUtilities.GetResponseHeaderLength(base.Response)
				};
			}
			else
			{
				preOperationFailureMeasurementEvent = new AuthenticationFailureMeasurementEvent();
			}
			if (this.operationContext.AccountName != null)
			{
				preOperationFailureMeasurementEvent.AccountName = this.operationContext.AccountName;
			}
			preOperationFailureMeasurementEvent.Complete(measurementStatus);
			this.LogPerfCounter(preOperationFailureMeasurementEvent, internalOperationStatusString, measurementStatus, false, null);
			this.operationContext.OperationMeasurementEvent = preOperationFailureMeasurementEvent;
		}

		private T CreateBasicOperationContext()
		{
			T name = Activator.CreateInstance<T>();
			name.SetElapsedTime(base.RequestContext.ElapsedTime);
			name.AccountName = this.storageAccount.Name;
			name.ContainerName = string.Empty;
			return name;
		}

		protected void CreateContainerAclSettingsFromHeader(bool mustBePresent, out ContainerAclSettings acl)
		{
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				bool? nullable = this.ParseOptionalBoolInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, "x-ms-prop-publicaccess");
				if (mustBePresent && !nullable.HasValue)
				{
					throw new RequiredHeaderNotPresentProtocolException("x-ms-prop-publicaccess");
				}
				acl = new ContainerAclSettings(nullable, base.RequestRestVersion);
				return;
			}
			string item = base.RequestHeadersCollection["x-ms-blob-public-access"];
			if (!string.IsNullOrEmpty(item) && !Comparison.StringEqualsIgnoreCase(item, "blob") && !Comparison.StringEqualsIgnoreCase(item, "container"))
			{
				throw new InvalidHeaderProtocolException("x-ms-blob-public-access", item);
			}
			acl = new ContainerAclSettings(item, base.RequestRestVersion);
		}

		protected override void DecrementAccountConcurrentRequestCountIfNecessary()
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				base.Dispose(disposing);
				if (this.storageAccount != null)
				{
					this.storageAccount.Dispose();
					this.storageAccount = null;
				}
			}
		}

		private void EmitQoSEventAndCollectBillingDataForPreAuthFailures(MeasurementEventStatus measurementStatus, bool skipBillingAndMetrics)
		{
			try
			{
				this.CreateAndCompleteQoSEventForPreAuthFailures(QoSUtilities.TransformNephosStatusForPreAuthFailures(measurementStatus), string.Empty);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Billing: EmitQoSEventAndCollectBillingDataForPreAuthFailures Exception : {0}", new object[] { exception });
			}
		}

		private void EmitQoSEventAndCollectBillingDataForPreOperationFailures(MeasurementEventStatus measurementStatus, bool skipBillingAndMetrics)
		{
			try
			{
				bool flag = true;
				MeasurementEventStatus measurementEventStatu = QoSUtilities.TransformNephosStatusForPreOperationFailures(measurementStatus, this.operationContext.IsRequestAnonymous, flag);
				this.CreateAndCompleteQoSEventForPreOperationFailures(measurementEventStatu, string.Empty);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Billing: EmitQoSEventAndCollectBillingDataForPreOperationFailures Exception : {0}", new object[] { exception });
			}
		}

		private bool EndAdjustRequestVersionBasedOnContainerAclSettings(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<bool> asyncIteratorContext = (AsyncIteratorContext<bool>)asyncResult;
			asyncIteratorContext.End(out exception, RethrowableWrapperBehavior.Wrap);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		protected IStorageAccount EndGetResourceAccountProperties(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<IStorageAccount> asyncIteratorContext = (AsyncIteratorContext<IStorageAccount>)asyncResult;
			asyncIteratorContext.End(out exception, RethrowableWrapperBehavior.Wrap);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		protected NephosUriComponents EndGetUriComponents(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<NephosUriComponents> asyncIteratorContext = (AsyncIteratorContext<NephosUriComponents>)asyncResult;
			asyncIteratorContext.End(out exception, RethrowableWrapperBehavior.Wrap);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		protected void EndPerformOperation(IAsyncResult ar)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private void EndProcessException(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception, RethrowableWrapperBehavior.Wrap);
			if (exception != null)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Info.Log("ProcessException is throwing fatal exception {0}", new object[] { exception });
				throw exception;
			}
		}

		protected AnalyticsSettings EndReadAnalyticsSettings(IAsyncResult ar)
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

		private void EndSendErrorResponse(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception, RethrowableWrapperBehavior.Wrap);
			if (exception != null)
			{
				throw exception;
			}
		}

		protected void EndWriteAnalyticsSettings(IAsyncResult ar)
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

		private void EndWriteErrorInfoToResponseStream(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		protected void EndWriteServiceStats(IAsyncResult ar)
		{
			ar.End<NoResults>(RethrowableWrapperBehavior.NoWrap);
		}

		public void EnsureMaxTimeoutIsNotExceeded(TimeSpan maxAllowedTimeout)
		{
			this.operationContext.MaxAllowedTimeout = maxAllowedTimeout;
			if (this.operationContext.OperationTimeout > maxAllowedTimeout)
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] totalMilliseconds = new object[2];
				TimeSpan operationTimeout = this.operationContext.OperationTimeout;
				totalMilliseconds[0] = (long)operationTimeout.TotalMilliseconds;
				totalMilliseconds[1] = (long)maxAllowedTimeout.TotalMilliseconds;
				verbose.Log("Limiting operation timeout '{0}'ms to Max allowed  timeout '{1}ms'.", totalMilliseconds);
				this.operationContext.OperationTimeout = maxAllowedTimeout;
			}
		}

		protected abstract OperationContextWithAuthAndAccountContainer ExtractOperationContext(NephosUriComponents uriComponents);

		protected string ExtractSubResourceFromContext()
		{
			return base.RequestQueryParameters["comp"];
		}

		protected TimeSpan ExtractTimeoutFromContext()
		{
			string item = base.RequestQueryParameters["timeout"];
			TimeSpan maxValue = TimeSpan.MaxValue;
			if (!string.IsNullOrEmpty(item))
			{
				try
				{
					maxValue = TimeSpan.FromSeconds((double)int.Parse(item, CultureInfo.InvariantCulture));
				}
				catch (FormatException formatException)
				{
					throw new InvalidQueryParameterProtocolException("timeout", item, "Not a valid integer.", formatException);
				}
				catch (OverflowException overflowException1)
				{
					OverflowException overflowException = overflowException1;
					int num = -2147483648;
					int num1 = 2147483647;
					throw new OutOfRangeQueryParameterProtocolException("timeout", item, num.ToString(CultureInfo.InvariantCulture), num1.ToString(CultureInfo.InvariantCulture), overflowException);
				}
			}
			return maxValue;
		}

		protected void FinishMeasurementAndSetStatusIfNotCompleted(MeasurementEventStatus measurementStatus, bool skipBillingAndMetrics = false, NephosStatusEntry errorStatus = null)
		{
			if (this.RequestMeasurementEventProcessed)
			{
				return;
			}
			this.RequestMeasurementEventProcessed = true;
			int requestHeaderLength = HttpUtilities.GetRequestHeaderLength(base.RequestHeadersCollection, base.HttpVerb, base.RequestRawUrlString);
			int responseHeaderLength = HttpUtilities.GetResponseHeaderLength(base.Response);
			this.UpdateOverallAccountThrottlingProbability();
			this.UpdateBandwidthUsage(requestHeaderLength, responseHeaderLength);
			if (this.operationContext == null)
			{
				if (!this.RequestBilled)
				{
					this.EmitQoSEventAndCollectBillingDataForPreAuthFailures(measurementStatus, skipBillingAndMetrics);
					this.RequestBilled = !skipBillingAndMetrics;
				}
				return;
			}
			if (this.operationContext.OperationDuration.Elapsed < TimeSpan.Zero)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Encountered an operation duration that was less than zero. This typically indicates that time went backwards (usually due to clock synchronization).");
			}
			this.operationContext.Complete();
			base.RequestContext.OperationStatus.UpdateFETimeBasedOnFinalServerProcessingTime(this.operationContext.FinalServerProcessingTime);
			if (this.operationContext.HttpRequestMeasurementEvent != null && !this.operationContext.HttpRequestMeasurementEvent.Completed)
			{
				this.operationContext.HttpRequestMeasurementEvent.Complete(measurementStatus, this.operationContext.FinalTotalTime);
			}
			this.operationContext.HttpRequestMeasurementEvent = null;
			if (this.operationContext.OperationMeasurementEvent != null && !this.operationContext.OperationMeasurementEvent.Completed)
			{
				this.operationContext.OperationMeasurementEvent.RequestHeaderBytesRead = (long)requestHeaderLength;
				this.operationContext.OperationMeasurementEvent.ResponseHeaderBytesWritten = (long)responseHeaderLength;
				MeasurementEventStatus measurementEventStatu = QoSUtilities.TransformNephosStatusForAuthenticatedRequests(measurementStatus, this.operationContext.IsRequestAnonymous, true);
				this.operationContext.OperationMeasurementEvent.Complete(measurementEventStatu, this.operationContext.FinalTotalTime, this.operationContext.FinalServerProcessingTime);
				string empty = string.Empty;
				this.LogPerfCounter(this.operationContext.OperationMeasurementEvent, empty, measurementStatus, true, errorStatus);
				this.RequestBilled = !skipBillingAndMetrics;
			}
			else if (this.operationContext.OperationMeasurementEvent == null && !this.RequestBilled)
			{
				if (this.operationContext.CallerIdentity != null)
				{
					this.EmitQoSEventAndCollectBillingDataForPreOperationFailures(measurementStatus, skipBillingAndMetrics);
				}
				else
				{
					this.EmitQoSEventAndCollectBillingDataForPreAuthFailures(measurementStatus, skipBillingAndMetrics);
				}
				this.RequestBilled = !skipBillingAndMetrics;
			}
			this.operationContext.OperationMeasurementEvent = null;
		}

		protected Stream GenerateMeasuredRequestStream()
		{
			TallyKeepingStream tallyKeepingStream = new TallyKeepingStream(base.RequestStream, true, new IngressEgressUsageContext());
			tallyKeepingStream.DisposingEvent += new EventHandler(this.RequestStreamDisposeEventHandler);
			return tallyKeepingStream;
		}

		protected Stream GenerateMeasuredResponseStream(bool errorResponse = false)
		{
			TallyKeepingStream tallyKeepingStream = new TallyKeepingStream(base.ResponseStream, false, new IngressEgressUsageContext());
			tallyKeepingStream.DisposingEvent += new EventHandler(this.ResponseStreamDisposeEventHandler);
			return tallyKeepingStream;
		}

		protected static string GetConditionsUsed(ConditionInformation conditionInfo)
		{
			object obj;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			if (conditionInfo != null)
			{
				if (conditionInfo.IfModifiedSince.HasValue)
				{
					DateTime value = conditionInfo.IfModifiedSince.Value;
					stringBuilder.AppendFormat("If-Modified-Since={0}", value.ToString(HttpUtilities.RFC850DateTimePattern));
					flag = true;
				}
				if (conditionInfo.IfNotModifiedSince.HasValue)
				{
					StringBuilder stringBuilder1 = stringBuilder;
					obj = (flag ? ";" : "");
					DateTime dateTime = conditionInfo.IfNotModifiedSince.Value;
					stringBuilder1.AppendFormat("{0}If-Unmodified-Since={1}", obj, dateTime.ToString(HttpUtilities.RFC850DateTimePattern));
					flag = true;
				}
				if (conditionInfo.IfMatch != null && (int)conditionInfo.IfMatch.Length > 0)
				{
					stringBuilder.AppendFormat("{0}If-Match={1}", (flag ? ";" : ""), conditionInfo.IfMatch[0].ToString(HttpUtilities.RFC850DateTimePattern));
					flag = true;
				}
				if (conditionInfo.IfNoneMatch != null && (int)conditionInfo.IfNoneMatch.Length > 0)
				{
					stringBuilder.AppendFormat("{0}If-None-Match={1}", (flag ? ";" : ""), conditionInfo.IfNoneMatch[0].ToString(HttpUtilities.RFC850DateTimePattern));
				}
			}
			return stringBuilder.ToString();
		}

		public virtual NephosErrorDetails GetErrorDetailsForException(Exception e)
		{
			NephosStatusEntry defaultConditionNotMet;
			if (e is AuthenticationFailureException)
			{
				NameValueCollection nameValueCollection = new NameValueCollection()
				{
					{ "AuthenticationErrorDetail", e.Message }
				};
				return new NephosErrorDetails(CommonStatusEntries.AuthenticationFailed, NephosRESTEventStatus.AuthenticationFailure, e, null, nameValueCollection);
			}
			if (e is InvalidAuthenticationInfoException)
			{
				return new NephosErrorDetails(CommonStatusEntries.InvalidAuthenticationInfo, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is ProtocolException)
			{
				ProtocolException protocolException = e as ProtocolException;
				return new NephosErrorDetails(protocolException.StatusEntry, NephosRESTEventStatus.ProtocolFailure, protocolException, protocolException.GetResponseHeaders(), protocolException.GetAdditionalUserDetails());
			}
			if (e is UnexpectedVersionException)
			{
				UnexpectedVersionException unexpectedVersionException = e as UnexpectedVersionException;
				if (unexpectedVersionException.Critical)
				{
					UnexpectedVersionException.RaiseAlert(unexpectedVersionException);
				}
				return new NephosErrorDetails(CommonStatusEntries.InvalidInput, NephosRESTEventStatus.UnknownFailure, e);
			}
			if (e is NephosStorageDataCorruptionException)
			{
				NephosStorageDataCorruptionException.RaiseAlert(e as NephosStorageDataCorruptionException);
				return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.UnknownFailure, e);
			}
			if (e is HttpRequestDuplicateHeaderException)
			{
				NameValueCollection nameValueCollection1 = new NameValueCollection()
				{
					{ "MessageDetails", e.Message }
				};
				return new NephosErrorDetails(CommonStatusEntries.InvalidHeaderValue, NephosRESTEventStatus.ExpectedFailure, e, null, nameValueCollection1);
			}
			if (e is MD5MismatchException)
			{
				MD5MismatchException mD5MismatchException = e as MD5MismatchException;
				NameValueCollection nameValueCollection2 = new NameValueCollection(2)
				{
					{ "UserSpecifiedMd5", Convert.ToBase64String(mD5MismatchException.GetSpecifiedMD5()) },
					{ "ServerCalculatedMd5", Convert.ToBase64String(mD5MismatchException.GetCalculatedMD5()) }
				};
				return new NephosErrorDetails(CommonStatusEntries.Md5Mismatch, NephosRESTEventStatus.ExpectedFailure, mD5MismatchException, null, nameValueCollection2);
			}
			if (e is CrcMismatchException)
			{
				if (!this.SupportCrc64)
				{
					return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.UnknownFailure, e);
				}
				CrcMismatchException crcMismatchException = e as CrcMismatchException;
				NameValueCollection nameValueCollection3 = new NameValueCollection(2)
				{
					{ "UserSpecifiedCrc", Convert.ToBase64String(BitConverter.GetBytes(crcMismatchException.SpecifiedCrc)) },
					{ "ServerCalculatedCrc", Convert.ToBase64String(BitConverter.GetBytes(crcMismatchException.CalculatedCrc)) }
				};
				return new NephosErrorDetails(CommonStatusEntries.Crc64Mismatch, NephosRESTEventStatus.ExpectedFailure, crcMismatchException, null, nameValueCollection3);
			}
			if (e is CorsPreflightFailureException)
			{
				NameValueCollection nameValueCollection4 = new NameValueCollection(1)
				{
					{ "MessageDetails", e.Message }
				};
				return new NephosErrorDetails(CommonStatusEntries.CorsFailure, NephosRESTEventStatus.ExpectedFailure, e, null, nameValueCollection4);
			}
			if (e is CorsPreflightMissingHeaderException)
			{
				NameValueCollection nameValueCollection5 = new NameValueCollection(1)
				{
					{ "MessageDetails", e.Message }
				};
				return new NephosErrorDetails(CommonStatusEntries.CorsRequiredHeader, NephosRESTEventStatus.ExpectedFailure, e, null, nameValueCollection5);
			}
			if (e is MD5InvalidException)
			{
				return new NephosErrorDetails(CommonStatusEntries.InvalidMd5, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is TimeoutException)
			{
				return this.GetErrorDetailsForTimeoutException(e);
			}
			if (e is InvalidResourceNameException)
			{
				if (base.RequestContext.IsRequestVersionAtLeastAugust13)
				{
					return new NephosErrorDetails(CommonStatusEntries.InvalidResourceName, NephosRESTEventStatus.ExpectedFailure, e);
				}
				return new NephosErrorDetails(CommonStatusEntries.OutOfRangeInput, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is DeprecatedResourceNameException)
			{
				NameValueCollection nameValueCollection6 = new NameValueCollection();
				DeprecatedResourceNameException deprecatedResourceNameException = e as DeprecatedResourceNameException;
				if (deprecatedResourceNameException.DeprecatedResourceName != null)
				{
					nameValueCollection6.Add("DeprecatedResourceName", deprecatedResourceNameException.DeprecatedResourceName);
				}
				if (deprecatedResourceNameException.NewResourceName != null)
				{
					nameValueCollection6.Add("NewResourceName", deprecatedResourceNameException.NewResourceName);
				}
				return new NephosErrorDetails(CommonStatusEntries.DeprecatedResourceName, NephosRESTEventStatus.ExpectedFailure, e, null, nameValueCollection6);
			}
			if (e is XStoreArgumentOutOfRangeException)
			{
				if (!base.RequestContext.IsRequestVersionAtLeastAugust13)
				{
					return new NephosErrorDetails(CommonStatusEntries.OutOfRangeInput, NephosRESTEventStatus.ExpectedFailure, e);
				}
				NephosStatusEntry nephosStatusEntry = new NephosStatusEntry("OutOfRangeInput", HttpStatusCode.BadRequest, (e as XStoreArgumentOutOfRangeException).UserResponseMessage);
				return new NephosErrorDetails(nephosStatusEntry, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is XStoreArgumentException)
			{
				XStoreArgumentException xStoreArgumentException = e as XStoreArgumentException;
				if (string.IsNullOrEmpty(xStoreArgumentException.UserResponseMessage))
				{
					return new NephosErrorDetails(CommonStatusEntries.InvalidInput, NephosRESTEventStatus.ExpectedFailure, e);
				}
				NephosStatusEntry nephosStatusEntry1 = new NephosStatusEntry("InvalidInput", HttpStatusCode.BadRequest, xStoreArgumentException.UserResponseMessage);
				return new NephosErrorDetails(nephosStatusEntry1, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is ContinuationTokenParserException)
			{
				return new NephosErrorDetails(CommonStatusEntries.InvalidInput, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is HttpListenerException)
			{
				HttpListenerException httpListenerException = e as HttpListenerException;
				Logger<IRestProtocolHeadLogger>.Instance.NetworkFailure.Log(string.Concat("SecurityWarning: An error was encountered communicating with the client: ", HttpUtilities.GetStringForHttpListenerException(httpListenerException)));
				return this.GetErrorDetailsForNetworkException(e);
			}
			if (e is ProtocolViolationException)
			{
				return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.ProtocolFailure, e);
			}
			if (e is NephosUnauthorizedAccessException)
			{
				NephosUnauthorizedAccessException nephosUnauthorizedAccessException = e as NephosUnauthorizedAccessException;
				MeasurementEventStatus authorizationFailure = NephosRESTEventStatus.AuthorizationFailure;
				NameValueCollection nameValueCollection7 = new NameValueCollection();
				AccountSasAccessIdentifier callerIdentity = null;
				SignedAccessAccountIdentifier signedAccessAccountIdentifier = null;
				if (this.operationContext != null)
				{
					callerIdentity = this.operationContext.CallerIdentity as AccountSasAccessIdentifier;
					signedAccessAccountIdentifier = this.operationContext.CallerIdentity as SignedAccessAccountIdentifier;
				}
				AuthorizationFailureReason failureReason = nephosUnauthorizedAccessException.FailureReason;
				if (failureReason == AuthorizationFailureReason.UnauthorizedBlobOverwrite)
				{
					failureReason = AuthorizationFailureReason.PermissionMismatch;
				}
				if (failureReason == AuthorizationFailureReason.AccessPermissionFailureSAS || failureReason == AuthorizationFailureReason.InvalidOperationSAS)
				{
					authorizationFailure = NephosRESTEventStatus.PermissionFailureSAS;
				}
				else
				{
					if (failureReason == AuthorizationFailureReason.SourceIPMismatch)
					{
						if (signedAccessAccountIdentifier != null || callerIdentity != null)
						{
							nameValueCollection7.Add("SourceIP", base.RequestContext.ClientIP.Address.ToString());
						}
						authorizationFailure = NephosRESTEventStatus.AuthorizationFailure;
						Logger<IRestProtocolHeadLogger>.Instance.AuthorizationFailure.Log("Unauthorized signed ip");
						string str = string.Format(CommonStatusEntries.AuthorizationSourceIPMismatch.UserMessage, base.RequestContext.ClientIP.Address.ToString());
						NephosStatusEntry nephosStatusEntry2 = new NephosStatusEntry(CommonStatusEntries.AuthorizationSourceIPMismatch.StatusId, CommonStatusEntries.AuthorizationSourceIPMismatch.StatusCodeHttp, str);
						return new NephosErrorDetails(nephosStatusEntry2, authorizationFailure, e, null, nameValueCollection7);
					}
					if (failureReason == AuthorizationFailureReason.ProtocolMismatch)
					{
						if (signedAccessAccountIdentifier != null || callerIdentity != null)
						{
							nameValueCollection7.Add("Protocol", base.RequestContext.RequestUrl.Scheme);
						}
						authorizationFailure = NephosRESTEventStatus.AuthorizationFailure;
						Logger<IRestProtocolHeadLogger>.Instance.AuthorizationFailure.Log("Unauthorized signed protocol");
						return new NephosErrorDetails(CommonStatusEntries.AuthorizationProtocolMismatch, authorizationFailure, e, null, nameValueCollection7);
					}
					if (failureReason == AuthorizationFailureReason.ServiceMismatch)
					{
						authorizationFailure = NephosRESTEventStatus.AuthorizationFailure;
						Logger<IRestProtocolHeadLogger>.Instance.AuthorizationFailure.Log("Unauthorized signed service");
						return new NephosErrorDetails(CommonStatusEntries.AuthorizationServiceMismatch, authorizationFailure, e, null);
					}
					if (failureReason != AuthorizationFailureReason.PermissionMismatch)
					{
						if (failureReason == AuthorizationFailureReason.ResourceTypeMismatch)
						{
							authorizationFailure = NephosRESTEventStatus.PermissionFailureSAS;
							Logger<IRestProtocolHeadLogger>.Instance.AuthorizationFailure.Log("Unauthorized resource type");
							return new NephosErrorDetails(CommonStatusEntries.AuthorizationResourceTypeMismatch, authorizationFailure, e, null);
						}
						if (failureReason == AuthorizationFailureReason.UnauthorizedAccountSasRequest || callerIdentity != null)
						{
							authorizationFailure = NephosRESTEventStatus.AuthorizationFailure;
							Logger<IRestProtocolHeadLogger>.Instance.AuthorizationFailure.Log("Unauthorized account SAS request");
							return new NephosErrorDetails(CommonStatusEntries.AuthorizationFailure, authorizationFailure, e, null);
						}
						authorizationFailure = NephosRESTEventStatus.AuthorizationFailure;
					}
					else
					{
						Logger<IRestProtocolHeadLogger>.Instance.AuthorizationFailure.Log("Unauthorized permission");
						if (callerIdentity != null)
						{
							authorizationFailure = NephosRESTEventStatus.PermissionFailureSAS;
							return new NephosErrorDetails(CommonStatusEntries.AuthorizationPermissionMismatch, authorizationFailure, e, null);
						}
						if (signedAccessAccountIdentifier == null)
						{
							authorizationFailure = NephosRESTEventStatus.AuthorizationFailure;
						}
						else
						{
							authorizationFailure = NephosRESTEventStatus.PermissionFailureSAS;
							if (base.RequestContext.IsRequestVersionAtLeastApril15)
							{
								return new NephosErrorDetails(CommonStatusEntries.AuthorizationPermissionMismatch, authorizationFailure, e, null);
							}
						}
					}
				}
				IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.AuthorizationFailure;
				object[] message = new object[] { nephosUnauthorizedAccessException.Message, failureReason };
				stringDataEventStream.Log("Unauthorized: {0} with FailureReason {1} ", message);
				if (callerIdentity == null && (signedAccessAccountIdentifier == null || !base.RequestContext.IsRequestVersionAtLeastApril15))
				{
					return new NephosErrorDetails(CommonStatusEntries.UnAuthorized, authorizationFailure, e);
				}
				return new NephosErrorDetails(CommonStatusEntries.AuthorizationFailure, authorizationFailure, e, null);
			}
			if (e is ContainerUnauthorizedException)
			{
				Logger<IRestProtocolHeadLogger>.Instance.AuthorizationFailure.Log("Container Unauthorized: {0}", new object[] { e.Message });
				return new NephosErrorDetails(CommonStatusEntries.UnAuthorized, NephosRESTEventStatus.AuthorizationFailure, e);
			}
			if (e is BlobNotFoundException)
			{
				BlobNotFoundException blobNotFoundException = e as BlobNotFoundException;
				if (this.operationContext.IsRequestAnonymous)
				{
					return new NephosErrorDetails(CommonStatusEntries.BlobNotFound, NephosRESTEventStatus.AuthorizationFailure, blobNotFoundException);
				}
				return new NephosErrorDetails(CommonStatusEntries.BlobNotFound, NephosRESTEventStatus.ExpectedFailure, blobNotFoundException);
			}
			if (e is ConditionNotMetException)
			{
				ConditionNotMetException conditionNotMetException = (ConditionNotMetException)e;
				if (this.operationContext != null && conditionNotMetException.UserCondition != null)
				{
					this.operationContext.ConditionsUsed = conditionNotMetException.UserCondition;
				}
				bool flag = false;
				if (this.operationContext.RequestConditionInformation == null)
				{
					defaultConditionNotMet = CommonStatusEntries.DefaultConditionNotMet;
				}
				else
				{
					HttpStatusCode conditionFailStatusCode = this.operationContext.RequestConditionInformation.ConditionFailStatusCode;
					if (base.RequestContext.MultipleConditionalHeadersEnabled && this.operationContext.RequestConditionInformation.IsMultipleConditionalHeaderEnabled && conditionNotMetException.HttpStatusCode.HasValue)
					{
						conditionFailStatusCode = conditionNotMetException.HttpStatusCode.Value;
					}
					NephosStatusEntry defaultConditionNotMet1 = null;
					if (conditionNotMetException.IsForSource.HasValue)
					{
						defaultConditionNotMet1 = (!conditionNotMetException.IsForSource.Value ? CommonStatusEntries.TargetConditionNotMet : CommonStatusEntries.SourceConditionNotMet);
					}
					else
					{
						if (base.Method == RestMethod.GET && base.IsRequestAnonymous && !this.operationContext.IsRequestSAS)
						{
							flag = true;
						}
						defaultConditionNotMet1 = CommonStatusEntries.DefaultConditionNotMet;
					}
					defaultConditionNotMet = new NephosStatusEntry(defaultConditionNotMet1.StatusId, conditionFailStatusCode, defaultConditionNotMet1.UserMessage);
				}
				return new NephosErrorDetails(defaultConditionNotMet, (flag ? NephosRESTEventStatus.AnonymousExpectedGetPreconditionFailure : NephosRESTEventStatus.ConditionNotMetFailure), e);
			}
			if (e is UnrecognizedIfMatchConditionException)
			{
				return new NephosErrorDetails(((UnrecognizedIfMatchConditionException)e).StatusEntry, NephosRESTEventStatus.ConditionNotMetFailure, e);
			}
			if (e is SequenceNumberConditionNotMetException)
			{
				return new NephosErrorDetails(CommonStatusEntries.SequenceNumberConditionNotMet, NephosRESTEventStatus.ConditionNotMetFailure, e);
			}
			if (e is ServerBusyException)
			{
				ServerBusyException serverBusyException = (ServerBusyException)e;
				return new NephosErrorDetails(CommonStatusEntries.ServerBusy, (serverBusyException.ExpectedFailure ? NephosRESTEventStatus.ExpectedThrottlingFailure : NephosRESTEventStatus.ThrottlingFailure), e);
			}
			if (e is ObjectMetadataOverLimitException)
			{
				if (base.RequestContext.IsRequestVersionAtLeastAugust13)
				{
					return new NephosErrorDetails(CommonStatusEntries.MetadataTooLarge, NephosRESTEventStatus.ExpectedFailure, e);
				}
				return new NephosErrorDetails(CommonStatusEntries.OutOfRangeInput, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is MetadataFormatException)
			{
				return new NephosErrorDetails(CommonStatusEntries.InvalidMetadata, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is ContainerNotFoundException)
			{
				return new NephosErrorDetails(CommonStatusEntries.ContainerNotFound, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is ContainerAlreadyExistsException)
			{
				return new NephosErrorDetails(CommonStatusEntries.ContainerAlreadyExists, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is AccountNotFoundException)
			{
				return new NephosErrorDetails(CommonStatusEntries.AccountNotFound, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is SecondaryWriteNotAllowedException)
			{
				return new NephosErrorDetails(CommonStatusEntries.SecondaryWriteNotAllowed, NephosRESTEventStatus.AuthorizationFailure, e);
			}
			if (e is SecondaryReadDisabledException)
			{
				return new NephosErrorDetails(CommonStatusEntries.SecondaryReadDisabled, NephosRESTEventStatus.AuthorizationFailure, e);
			}
			if (e is UnsatisfiableConditionException)
			{
				return new NephosErrorDetails(CommonStatusEntries.UnsatisfiableCondition, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is UnsupportedPermissionInStoredAccessPolicyException)
			{
				return new NephosErrorDetails(CommonStatusEntries.UnsupportedPermissionInStoredAccessPolicy, NephosRESTEventStatus.PermissionFailureSAS, e);
			}
			if (e is BlobMD5MigrationException)
			{
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] logString = new object[] { e.GetLogString() };
				error.Log("Internal Blob MD5 Migration Failed: {0}", logString);
				return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.UnknownFailure, e);
			}
			if (e is StorageManagerException)
			{
				IStringDataEventStream unexpectedXStoreError = Logger<IRestProtocolHeadLogger>.Instance.UnexpectedXStoreError;
				object[] objArray = new object[] { e.ToString() };
				unexpectedXStoreError.Log("An unexpected XStore error was encountered: {0}", objArray);
				return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.UnknownFailure, e);
			}
			if (e is InvalidStreamLengthException)
			{
				IStringDataEventStream error1 = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] str1 = new object[] { e.ToString() };
				error1.Log("InvalidStreamLengthError (.NETBUG encountered): {0}", str1);
				return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.UnknownFailure, e);
			}
			if (e is MultipleVersionsSpecifiedException)
			{
				return new NephosErrorDetails(CommonStatusEntries.MultipleVersionsSpecified, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (!(e is DstsAuthenticationFailureException))
			{
				return this.GetNephosErrorDetailsForUnknownException(e);
			}
			DstsAuthenticationFailureException dstsAuthenticationFailureException = e as DstsAuthenticationFailureException;
			return new NephosErrorDetails(new NephosStatusEntry(CommonStatusEntries.UnAuthorized.StatusId, HttpStatusCode.Unauthorized, CommonStatusEntries.UnAuthorized.UserMessage), NephosRESTEventStatus.AuthorizationFailure, e, dstsAuthenticationFailureException.Headers, null);
		}

		protected NephosErrorDetails GetErrorDetailsForNetworkException(Exception e)
		{
			MeasurementEventStatus timeoutFailure = NephosRESTEventStatus.TimeoutFailure;
			if (this.operationContext != null)
			{
				TimeSpan maxAllowedTimeout = this.operationContext.MaxAllowedTimeout;
				TimeSpan userTimeout = this.operationContext.UserTimeout;
				IPerfTimer operationDuration = this.operationContext.OperationDuration;
				if (TimeSpan.Equals(maxAllowedTimeout, TimeSpan.MaxValue))
				{
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Using default max timeout for failed operation.");
					maxAllowedTimeout = BasicHttpProcessor.DefaultMaxAllowedTimeout;
				}
				TimeSpan elapsed = operationDuration.Elapsed;
				if (elapsed > userTimeout || elapsed >= maxAllowedTimeout)
				{
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] totalMilliseconds = new object[] { (long)elapsed.TotalMilliseconds, (long)userTimeout.TotalMilliseconds, (long)maxAllowedTimeout.TotalMilliseconds };
					verbose.Log("Converting to timeout failure since duration={0}ms exceeded either UserTimeout={1}ms or MaxAllowedTimeout={2}ms", totalMilliseconds);
					return this.GetErrorDetailsForTimeoutException(e);
				}
				if (!BasicHttpProcessorWithAuthAndAccountContainer<T>.IsOperationWithinSla(this.operationContext, true))
				{
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Converting to (unexpected) timeout failure since operation did not complete within SLA time.");
					return new NephosErrorDetails(CommonStatusEntries.OperationTimedOut, NephosRESTEventStatus.TimeoutFailure, e);
				}
			}
			return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.NetworkFailure, e);
		}

		protected NephosErrorDetails GetErrorDetailsForTimeoutException(Exception e)
		{
			MeasurementEventStatus timeoutFailure = NephosRESTEventStatus.TimeoutFailure;
			if (this.operationContext == null)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Got timeout failure before extracting operation context; Using UnexpectedTimeoutFailure");
			}
			else
			{
				if (BasicHttpProcessorWithAuthAndAccountContainer<T>.IsOperationWithinSla(this.operationContext, false))
				{
					timeoutFailure = NephosRESTEventStatus.ExpectedTimeoutFailure;
				}
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] str = new object[] { timeoutFailure.ToString() };
				verbose.Log("Timeout failure is categorized as Status={0}", str);
			}
			return new NephosErrorDetails(CommonStatusEntries.OperationTimedOut, timeoutFailure, e);
		}

		protected virtual bool GetIsSlbProbeDown()
		{
			return false;
		}

		protected int? GetMaxResultsQueryParam()
		{
			string item = base.RequestQueryParameters["maxresults"];
			int? nullable = null;
			if (!string.IsNullOrEmpty(item))
			{
				nullable = new int?(this.ParseMaxResultString(item));
				int? nullable1 = nullable;
				if ((nullable1.GetValueOrDefault() >= 1 ? false : nullable1.HasValue))
				{
					int num = 1;
					int num1 = 2147483647;
					throw new OutOfRangeQueryParameterProtocolException("maxresults", item, num.ToString(CultureInfo.InvariantCulture), num1.ToString(CultureInfo.InvariantCulture));
				}
				int? nullable2 = nullable;
				if ((nullable2.GetValueOrDefault() <= 5000 ? false : nullable2.HasValue))
				{
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] objArray = new object[] { nullable, 5000 };
					verbose.Log("Reducing user's maxResults {0} to {1}.", objArray);
					nullable = new int?(5000);
				}
			}
			return nullable;
		}

		protected int? GetMaxResultsQueryParamPreSeptember09VersionForBlob()
		{
			string item = base.RequestQueryParameters["maxresults"];
			int? nullable = null;
			if (!string.IsNullOrEmpty(item))
			{
				nullable = new int?(this.ParseMaxResultString(item));
				int? nullable1 = nullable;
				if ((nullable1.GetValueOrDefault() >= 1 ? false : nullable1.HasValue))
				{
					throw new InvalidQueryParameterProtocolException("maxresults", item, "maxresults must be positive if specified");
				}
				int? nullable2 = nullable;
				if ((nullable2.GetValueOrDefault() <= 5000 ? false : nullable2.HasValue))
				{
					throw new XStoreArgumentOutOfRangeException("maxBlobs", string.Format("MaxBlobs parameter should be >= {0} and <= {1}", 1, 5000));
				}
			}
			return nullable;
		}

		protected int? GetMaxResultsQueryParamPreSeptember09VersionForQueue()
		{
			string item = base.RequestQueryParameters["maxresults"];
			int? nullable = null;
			if (string.IsNullOrEmpty(item))
			{
				return nullable;
			}
			return new int?(this.ParseMaxResultString(item));
		}

		internal static string GetMethodName(RestMethod method)
		{
			switch (method)
			{
				case RestMethod.GET:
				{
					return "GET";
				}
				case RestMethod.PUT:
				{
					return "PUT";
				}
				case RestMethod.POST:
				{
					return "POST";
				}
				case RestMethod.MERGE:
				{
					return "MERGE";
				}
				case RestMethod.DELETE:
				{
					return "DELETE";
				}
				case RestMethod.HEAD:
				{
					return "HEAD";
				}
			}
			return method.ToString();
		}

		private NephosErrorDetails GetNephosErrorDetailsForUnknownException(Exception e)
		{
			if (!(e is FatalServerCrashingException) && !ExceptionProcessor.LogUnhandledException(e))
			{
				return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.UnknownFailure, e);
			}
			return new NephosErrorDetails(CommonStatusEntries.InternalError, NephosRESTEventStatus.UnknownFailure, e, null, null, null, true, false);
		}

		protected OverwriteOption GetOverwriteOption(ConditionInformation conditionInfo, Guid? leaseId)
		{
			OverwriteOption overwriteOption = OverwriteOption.CreateNewOrUpdateExisting;
			if (conditionInfo != null && conditionInfo.ResourceExistsCondition.HasValue)
			{
				if (conditionInfo.ResourceExistsCondition.Value == ResourceExistenceCondition.MustNotExist)
				{
					overwriteOption = OverwriteOption.CreateNewOnly;
				}
				else if (conditionInfo.ResourceExistsCondition.Value == ResourceExistenceCondition.MustExist && base.RequestContext.IsRequestVersionAtLeastSeptember09)
				{
					overwriteOption = OverwriteOption.UpdateExistingOnly;
				}
			}
			if (leaseId.HasValue && base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				overwriteOption = OverwriteOption.UpdateExistingOnly;
			}
			return overwriteOption;
		}

		public string GetProtocolVersionToUse(string sasVersion)
		{
			string str = sasVersion;
			if (sasVersion != null && VersioningConfigurationLookup.Instance.IsValidVersion(sasVersion) && VersioningHelper.CompareVersions(sasVersion, "2014-02-14") >= 0)
			{
				string item = base.RequestContext.QueryParameters["api-version"];
				string requestHeaderRestVersion = base.RequestContext.RequestHeaderRestVersion;
				if (item != null && !VersioningConfigurationLookup.Instance.IsValidVersion(item))
				{
					FutureVersionProtocolException.ThrowIfFutureVersion(item);
					throw new InvalidQueryParameterProtocolException("api-version", item, string.Format("{0} is not a valid version", item));
				}
				if (requestHeaderRestVersion != null && !VersioningConfigurationLookup.Instance.IsValidVersion(requestHeaderRestVersion))
				{
					FutureVersionProtocolException.ThrowIfFutureVersion(requestHeaderRestVersion);
					throw new InvalidHeaderProtocolException("x-ms-version", requestHeaderRestVersion);
				}
				if (item != null)
				{
					str = item;
				}
				else if (requestHeaderRestVersion != null)
				{
					str = requestHeaderRestVersion;
				}
			}
			return str;
		}

		protected long? GetRequestCrc64(string crc64HeaderName)
		{
			long? nullable;
			string item = base.RequestHeadersCollection[crc64HeaderName];
			if (item == null || item.Length <= 0)
			{
				return null;
			}
			try
			{
				nullable = new long?(BitConverter.ToInt64(Convert.FromBase64String(item), 0));
			}
			catch (FormatException formatException)
			{
				throw new InvalidHeaderProtocolException(crc64HeaderName, item, formatException);
			}
			return nullable;
		}

		protected byte[] GetRequestMD5(string md5HeaderName)
		{
			byte[] numArray;
			string item = base.RequestHeadersCollection[md5HeaderName];
			if (item == null || item.Length <= 0)
			{
				return null;
			}
			try
			{
				numArray = Convert.FromBase64String(item);
			}
			catch (FormatException formatException)
			{
				throw new InvalidHeaderProtocolException(md5HeaderName, item, formatException);
			}
			return numArray;
		}

		protected bool? GetRequestPutBlobComputeMD5(string putBlobComputeMD5HeaderName)
		{
			bool? nullable;
			string item = base.RequestHeadersCollection[putBlobComputeMD5HeaderName];
			if (item == null || item.Length <= 0)
			{
				return null;
			}
			try
			{
				nullable = new bool?(bool.Parse(item));
			}
			catch (FormatException formatException)
			{
				throw new InvalidHeaderProtocolException("x-ms-put-blob-compute-md5", item, formatException);
			}
			return nullable;
		}

		private IEnumerator<IAsyncResult> GetResourceAccountPropertiesImpl(string accountName, bool isAdminAccess, TimeSpan timeout, AsyncIteratorContext<IStorageAccount> context)
		{
			IStorageAccount operationStatus = this.StorageManager.CreateAccountInstance(accountName);
			operationStatus.OperationStatus = base.RequestContext.OperationStatus;
			operationStatus.Timeout = timeout;
			AccountCondition accountCondition = null;
			if (isAdminAccess)
			{
				accountCondition = new AccountCondition(true, true, null, null);
			}
			IAsyncResult asyncResult = operationStatus.BeginGetProperties(AccountPropertyNames.All, accountCondition, context.GetResumeCallback(), context.GetResumeState("GetResourceAccountPropertiesImpl.GetProperties"));
			yield return asyncResult;
			operationStatus.EndGetProperties(asyncResult);
			context.ResultData = operationStatus;
		}

		protected static string GetSourceConditionsUsed(ConditionInformation conditionInfo)
		{
			object obj;
			object obj1;
			object obj2;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			if (conditionInfo != null)
			{
				if (conditionInfo.CopySourceIfModifiedSince.HasValue)
				{
					DateTime value = conditionInfo.CopySourceIfModifiedSince.Value;
					stringBuilder.AppendFormat("If-Modified-Since={0}", value.ToString(HttpUtilities.RFC850DateTimePattern));
					flag = true;
				}
				if (conditionInfo.CopySourceIfNotModifiedSince.HasValue)
				{
					StringBuilder stringBuilder1 = stringBuilder;
					obj2 = (flag ? ";" : "");
					DateTime dateTime = conditionInfo.CopySourceIfNotModifiedSince.Value;
					stringBuilder1.AppendFormat("{0}If-Unmodified-Since={1}", obj2, dateTime.ToString(HttpUtilities.RFC850DateTimePattern));
					flag = true;
				}
				if (conditionInfo.CopySourceIfMatch.HasValue)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					obj1 = (flag ? ";" : "");
					DateTime value1 = conditionInfo.CopySourceIfMatch.Value;
					stringBuilder2.AppendFormat("{0}If-Match={1}", obj1, value1.ToString(HttpUtilities.RFC850DateTimePattern));
					flag = true;
				}
				if (conditionInfo.CopySourceIfNoneMatch.HasValue)
				{
					StringBuilder stringBuilder3 = stringBuilder;
					obj = (flag ? ";" : "");
					DateTime dateTime1 = conditionInfo.CopySourceIfNoneMatch.Value;
					stringBuilder3.AppendFormat("{0}If-None-Match={1}", obj, dateTime1.ToString(HttpUtilities.RFC850DateTimePattern));
				}
			}
			return stringBuilder.ToString();
		}

		protected virtual string GetStringToSign(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, NephosUriComponents uriComponents, SupportedAuthScheme requestAuthScheme)
		{
			if (requestAuthScheme == SupportedAuthScheme.SignedKey)
			{
				return AuthenticationManagerHelper.GetStringToSignForStandardSignedKeyAuth(requestContext, uriComponents, SupportedAuthScheme.SignedKey, false);
			}
			return AuthenticationManagerHelper.GetStringToSignForStandardSharedKeyAuth(requestContext, uriComponents, requestAuthScheme, false);
		}

		protected virtual NephosUriComponents GetUriComponents(Uri requestUrl, out bool isUriPathStyle)
		{
			NephosUriComponents nephosUriComponent = null;
			nephosUriComponent = (!base.RequestContext.IsRequestVersionAtLeastJuly09 ? HttpRequestAccessor.GetNephosUriComponents(requestUrl, this.ProcessorConfiguration.ValidHostSuffixes, false, out isUriPathStyle) : HttpRequestAccessorJuly09.GetNephosUriComponents(requestUrl, this.ProcessorConfiguration.ValidHostSuffixes, false, out isUriPathStyle));
			if (nephosUriComponent == null && HttpUtilities.StringIsIPAddress(requestUrl.Host))
			{
				IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
				object[] safeUriString = new object[] { HttpUtilities.GetSafeUriString(requestUrl.AbsoluteUri) };
				verboseDebug.Log("Host-style URI resolution failed. Allowing path-style for uri {0}", safeUriString);
				nephosUriComponent = (!base.RequestContext.IsRequestVersionAtLeastJuly09 ? HttpRequestAccessor.GetNephosUriComponents(requestUrl, this.ProcessorConfiguration.ValidHostSuffixes, true, out isUriPathStyle) : HttpRequestAccessorJuly09.GetNephosUriComponents(requestUrl, this.ProcessorConfiguration.ValidHostSuffixes, true, out isUriPathStyle));
			}
			return nephosUriComponent;
		}

		private IEnumerator<IAsyncResult> GetUriComponentsImpl(Uri requestUrl, TimeSpan timeout, AsyncIteratorContext<NephosUriComponents> context)
		{
			bool flag;
			NephosUriComponents uriComponents = this.GetUriComponents(requestUrl, out flag);
			this.IsUriPathStyle = flag;
			if (uriComponents == null)
			{
				if (!this.ProcessorConfiguration.AllowPathStyleUris)
				{
					throw new InvalidUrlProtocolException(requestUrl);
				}
				uriComponents = (!base.RequestContext.IsRequestVersionAtLeastJuly09 ? HttpRequestAccessor.GetNephosPathStyleUriComponents(requestUrl) : HttpRequestAccessorJuly09.GetNephosPathStyleUriComponents(requestUrl));
				if (uriComponents == null)
				{
					throw new InvalidUrlProtocolException(requestUrl);
				}
				this.IsUriPathStyle = true;
			}
			context.ResultData = uriComponents;
			yield break;
		}

		protected void HandlePreflightCorsRequest(AnalyticsSettings settings)
		{
			CorsRule corsRule;
			Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("CORS: Processing CORS preflight request");
			this.CheckPreflightValidRequest();
			string item = base.RequestHeadersCollection["Origin"];
			string str = base.RequestHeadersCollection["Access-Control-Request-Method"];
			string item1 = base.RequestHeadersCollection["Access-Control-Request-Headers"];
			if (!this.CheckCorsRuleMatch(settings, str, item, out corsRule) || !this.CheckRequestHeaders(item1, corsRule))
			{
				throw new CorsPreflightFailureException("No CORS rules matches this request");
			}
			base.Response.Headers.Add("Access-Control-Allow-Origin", item);
			base.Response.Headers.Add("Access-Control-Allow-Methods", str);
			if (!string.IsNullOrEmpty(item1))
			{
				base.Response.Headers.Add("Access-Control-Allow-Headers", item1);
			}
			base.Response.Headers.Add("Access-Control-Max-Age", Convert.ToString(corsRule.MaxAge));
			base.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
			base.StatusCode = HttpStatusCode.OK;
			this.SendSuccessResponse(false);
		}

		private static bool IsOperationWithinSla(T operationContext, bool isNetworkFailure)
		{
			bool flag = false;
			TimeSpan maxAllowedTimeout = operationContext.MaxAllowedTimeout;
			TimeSpan userTimeout = operationContext.UserTimeout;
			TimeSpan timeSpan = operationContext.RemainingTimeForCategorization(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			TimeSpan currentServerProcessingTime = operationContext.CurrentServerProcessingTime;
			TimeSpan timeSpan1 = TimeSpan.FromSeconds(2);
			if (userTimeout < timeSpan1)
			{
				flag = true;
			}
			else if (isNetworkFailure)
			{
				if (currentServerProcessingTime < timeSpan1)
				{
					flag = true;
				}
			}
			else if (currentServerProcessingTime < timeSpan1 && timeSpan < (timeSpan1 - currentServerProcessingTime))
			{
				flag = true;
			}
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] totalMilliseconds = new object[] { (long)userTimeout.TotalMilliseconds, (long)maxAllowedTimeout.TotalMilliseconds, (long)timeSpan1.TotalMilliseconds, (long)currentServerProcessingTime.TotalMilliseconds, (long)timeSpan.TotalMilliseconds, flag };
			verbose.Log("Timeout failure is categorized based on UserTimeout={0}ms MaxAllowedTimeout={1}ms SlaTimeout={2}ms ServerProcessingTime={3}ms RemainingTime={4}ms IsOperationWithinSla={5}", totalMilliseconds);
			return flag;
		}

		private bool IsValidAccountName(string accountName)
		{
			bool flag = true;
			try
			{
				StorageStampHelpers.CheckAccountName(accountName);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] objArray = new object[] { accountName, exception.Message };
				error.Log("IsValidAccountName: accountName: {0} Exception Message: {1}", objArray);
				flag = false;
			}
			return flag;
		}

		private void LogPerfCounter(INephosBaseOperationMeasurementEvent measurementEvent, string internalOperationStatusString, MeasurementEventStatus measurementStatus, bool recordCompletePerfCounterLog, NephosStatusEntry errorStatus = null)
		{
			int num = 2048;
			int length = num;
			if (base.SafeRequestUrlString.Length < num)
			{
				length = base.SafeRequestUrlString.Length;
			}
			string str = base.SafeRequestUrlString.Substring(0, length);
			string str1 = "";
			if (!recordCompletePerfCounterLog)
			{
				TimeSpan zero = TimeSpan.Zero;
				IStringDataEventStream perf = Logger<IRestProtocolHeadLogger>.Instance.Perf;
				object[] accountName = new object[] { measurementEvent.AccountName ?? "null", measurementEvent.OperationName ?? "null", "null", (errorStatus != null ? errorStatus.StatusId : measurementEvent.OperationStatus), measurementEvent.RequestHeaderBytesRead, measurementEvent.RequestBytesRead, measurementEvent.ResponseHeaderBytesWritten, measurementEvent.ResponseBytesWritten, measurementEvent.ErrorResponseBytes, "NA", "NA", "NA", "NA", "NA", "NA", "NA", "NA", base.RequestContext.ClientIPAsString, base.RequestContext.UserAgent, base.RequestContext.RequestHeaderRestVersion ?? "null", base.RequestRestVersion ?? "null", str, base.RequestContext.RequestHeaders["x-ms-client-request-id"], measurementStatus.ToString(), base.Response.StatusCode, "NA", "NA", "NA", "NA", "NA", "NA", "NA", "NA", "NA", base.RequestContext.OperationStatus.TotalAccountCacheWaitTimeInMs, base.RequestContext.OperationStatus.TotalContainerCacheWaitTimeInMs, internalOperationStatusString, this.OverrideRequestContentType ?? base.RequestContext.RequestHeaders["Content-Type"], this.OverrideResponseContentType ?? base.Response.Headers["Content-Type"], measurementEvent.OperationPartitionKey, measurementEvent.ItemsReturnedCount, measurementEvent.BatchItemsCount, "NA", "NA", this.AccountConcurrentRequestCount, base.OverallConcurrentRequestCount, "NA", base.Response.Headers["x-ms-blob-type"] ?? base.RequestContext.RequestHeaders["x-ms-blob-type"], str1, "NA", "NA", "NA" };
				perf.Log("PerfCounters: Account={0} Operation={1} on Container={2} with Status={3} RequestHeaderSize={4} RequestSize={5} ResponseHeaderSize={6} ResponseSize={7} ErrorResponseByte={8} TimeInMs={9} ProcessingTimeInMs={10} UserTimeoutInMs={11} OperationTimeoutInMs={12} MaxAllowedTimeoutInMs={13} SlaUsedInMs={14} ReadLatencyInMs={15} WriteLatencyInMs={16} ClientIP={17} UserAgent='{18}' RequestVersion='{19}' ProcessorVersionUsed='{20}' RequestUrl='{21}' ClientRequestId='{22}' MeasurementStatus={23} HttpStatusCode={24} TotalFeTimeInMs={25} TotalTableServerTimeInMs={26} TotalTableServerRoundTripCount={27} TotalPartitionWaitTimeInMs={28} TotalXStreamTimeInMs={29} TotalXStreamRoundTripCount={30} SmbOpLockBreakLatency={31} LastTableServerInstanceName={32} LastTableServerErrorCode={33} TotalAccountCacheWaitTimeInMs={34} TotalContainerCacheWaitTimeInMs={35} InternalStatus={36} RequestContentType='{37}' ResponseContentType='{38}' PartitionKey='{39}' ItemsReturned='{40}' BatchOperationCount='{41}' LastXStreamErrorCode={42} AuthenticationType='{43}' AccountConcurrentReq='{44}' OverallConcurrentReq='{45}' Range='{46}' EntityType='{47}' {48} LastTSPartition={49} TotalXCacheTimeInMs={50} TotalXCacheRoundTripCount={51}", accountName);
			}
			else
			{
				TimeSpan timeSpan = TimeSpan.Zero;
				string str2 = "NA";
				string item = base.Response.Headers["x-ms-blob-type"] ?? base.RequestContext.RequestHeaders["x-ms-blob-type"];
				if (string.IsNullOrEmpty(item))
				{
					item = this.operationContext.OperationMeasurementEvent.GetObjectType();
				}
				string str3 = base.RequestContext.OperationStatus.LastTableServerErrorCode.ToString();
				string str4 = base.RequestContext.OperationStatus.LastXStreamErrorCode.ToString();
				string str5 = (!string.IsNullOrEmpty(this.operationContext.AccountName) ? this.operationContext.AccountName : measurementEvent.AccountName);
				IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Perf;
				object[] operationName = new object[] { str5, measurementEvent.OperationName, this.operationContext.ContainerName, (errorStatus != null ? errorStatus.StatusId : measurementEvent.OperationStatus), measurementEvent.RequestHeaderBytesRead, measurementEvent.RequestBytesRead, measurementEvent.ResponseHeaderBytesWritten, measurementEvent.ResponseBytesWritten, measurementEvent.ErrorResponseBytes, this.operationContext.FinalTotalTime.TotalMilliseconds, this.operationContext.FinalServerProcessingTime.TotalMilliseconds, this.operationContext.UserTimeout.TotalMilliseconds, this.operationContext.OperationTimeout.TotalMilliseconds, this.operationContext.MaxAllowedTimeout.TotalMilliseconds, timeSpan.TotalMilliseconds, this.operationContext.OperationClientReadNetworkLatency.TotalMilliseconds, this.operationContext.OperationClientWriteNetworkLatency.TotalMilliseconds, base.RequestContext.ClientIPAsString, base.RequestContext.UserAgent, base.RequestContext.RequestHeaderRestVersion, base.RequestRestVersion, str, base.RequestContext.RequestHeaders["x-ms-client-request-id"], measurementStatus.ToString(), base.Response.StatusCode, (double)base.RequestContext.OperationStatus.TotalFETimeInMicroSeconds / 1000, (double)base.RequestContext.OperationStatus.TotalTableServerTimeInMicroSeconds / 1000, base.RequestContext.OperationStatus.TotalTableServerRoundTripCount, base.RequestContext.OperationStatus.TotalPartitionWaitTimeInMs, (double)base.RequestContext.OperationStatus.TotalXStreamTimeInMicroSeconds / 1000, base.RequestContext.OperationStatus.TotalXStreamRoundTripCount, this.operationContext.SmbOpLockBreakLatency.TotalMilliseconds, base.RequestContext.OperationStatus.LastTableServerInstanceName, str3, base.RequestContext.OperationStatus.TotalAccountCacheWaitTimeInMs, base.RequestContext.OperationStatus.TotalContainerCacheWaitTimeInMs, internalOperationStatusString, this.OverrideRequestContentType ?? base.RequestContext.RequestHeaders["Content-Type"], this.OverrideResponseContentType ?? base.Response.Headers["Content-Type"], measurementEvent.OperationPartitionKey, measurementEvent.ItemsReturnedCount, measurementEvent.BatchItemsCount, str4, str2, this.AccountConcurrentRequestCount, base.OverallConcurrentRequestCount, this.Range, item, str1, base.RequestContext.OperationStatus.LastTableServerPartitionName, (double)base.RequestContext.OperationStatus.TotalXCacheTimeInMs + (double)base.RequestContext.OperationStatus.TotalXStreamXCacheReadTimeInMicroSeconds / 1000, base.RequestContext.OperationStatus.TotalXCacheRoundTripCount + base.RequestContext.OperationStatus.TotalXStreamXCacheReadRoundTripCount };
				stringDataEventStream.Log("PerfCounters: Account={0} Operation={1} on Container={2} with Status={3} RequestHeaderSize={4} RequestSize={5} ResponseHeaderSize={6} ResponseSize={7} ErrorResponseByte={8} TimeInMs={9} ProcessingTimeInMs={10} UserTimeoutInMs={11} OperationTimeoutInMs={12} MaxAllowedTimeoutInMs={13} SlaUsedInMs={14} ReadLatencyInMs={15} WriteLatencyInMs={16} ClientIP={17} UserAgent='{18}' RequestVersion='{19}' ProcessorVersionUsed='{20}' RequestUrl='{21}' ClientRequestId='{22}' MeasurementStatus={23} HttpStatusCode={24} TotalFeTimeInMs={25} TotalTableServerTimeInMs={26} TotalTableServerRoundTripCount={27} TotalPartitionWaitTimeInMs={28} TotalXStreamTimeInMs={29} TotalXStreamRoundTripCount={30} SmbOpLockBreakLatency={31} LastTableServerInstanceName={32} LastTableServerErrorCode={33} TotalAccountCacheWaitTimeInMs={34} TotalContainerCacheWaitTimeInMs={35} InternalStatus={36} RequestContentType='{37}' ResponseContentType='{38}' PartitionKey='{39}' ItemsReturned='{40}' BatchOperationCount='{41}' LastXStreamErrorCode={42} AuthenticationType='{43}' AccountConcurrentReq='{44}' OverallConcurrentReq='{45}' Range='{46}' EntityType='{47}' {48} LastTSPartition={49} TotalXCacheTimeInMs={50} TotalXCacheRoundTripCount={51}", operationName);
				if ((double)base.RequestContext.OperationStatus.TotalXCacheTimeInMs + (double)base.RequestContext.OperationStatus.TotalXStreamXCacheReadTimeInMicroSeconds / 1000 + (double)base.RequestContext.OperationStatus.TotalXCacheRoundTripCount + (double)base.RequestContext.OperationStatus.TotalXStreamXCacheReadRoundTripCount > 0)
				{
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] totalXCacheTimeInMs = new object[] { base.RequestContext.OperationStatus.TotalXCacheTimeInMs, (double)base.RequestContext.OperationStatus.TotalXStreamXCacheReadTimeInMicroSeconds / 1000, base.RequestContext.OperationStatus.TotalXCacheRoundTripCount, base.RequestContext.OperationStatus.TotalXStreamXCacheReadRoundTripCount };
					verbose.Log("TotalXCacheTimeInMs: {0} TotalXStreamXCacheReadTimeInMs: {1} TotalXCacheRoundTripCount: {2} TotalXStreamXCacheReadRoundTripCount: {3}", totalXCacheTimeInMs);
					return;
				}
			}
		}

		protected virtual void LogPerfMetric(IFrontEndPerfMetricsEventSettings settings, string account, string operation, string container, string status, long requestHeaderSize, long requestSize, long responseHeaderSize, long responseSize, long errorResponseByte, long timeInMs, long processingTimeInMs, long userTimeoutInMs, long operationTimeoutInMs, long maxAllowedTimeoutInMs, long slaUsedInMs, long readLatencyInMs, long writeLatencyInMs, string clientIP, string userAgent, string requestVersion, string processorVersionUsed, string requestUrl, string clientRequestId, string measurementStatus, long httpStatusCode, long totalFeTimeInMs, long totalTableServerTimeInMs, long totalTableServerRoundTripCount, long totalPartitionWaitTimeInMs, long totalXStreamTimeInMs, long totalXStreamRoundTripCount, long smbOpLockBreakLatency, string lastTableServerInstanceName, string lastTableServerErrorCode, string internalStatus, string requestContentType, string responseContentType, string partitionKey, long itemsReturned, long batchOperationCount, string lastXStreamErrorCode, string authenticationType, long accountConcurrentReq, long overallConcurrentReq, string range, string entityType, bool isSrpOperation, string lastTableServerPartition)
		{
			Logger<IFrontEndPerfMetricsLogger>.Instance.Metrics.LogMetric(settings, account, operation, container, status, requestHeaderSize, requestSize, responseHeaderSize, responseSize, errorResponseByte, timeInMs, processingTimeInMs, userTimeoutInMs, operationTimeoutInMs, maxAllowedTimeoutInMs, slaUsedInMs, readLatencyInMs, writeLatencyInMs, clientIP, userAgent, requestVersion, processorVersionUsed, requestUrl, clientRequestId, measurementStatus, httpStatusCode, totalFeTimeInMs, totalTableServerTimeInMs, totalTableServerRoundTripCount, totalPartitionWaitTimeInMs, totalXStreamTimeInMs, totalXStreamRoundTripCount, smbOpLockBreakLatency, lastTableServerInstanceName, lastTableServerErrorCode, internalStatus, requestContentType, responseContentType, partitionKey, itemsReturned, batchOperationCount, lastXStreamErrorCode, authenticationType, accountConcurrentReq, overallConcurrentReq, range, entityType, isSrpOperation, lastTableServerPartition);
		}

		private int ParseMaxResultString(string maxResultsString)
		{
			int num;
			try
			{
				num = Convert.ToInt32(maxResultsString, CultureInfo.InvariantCulture);
			}
			catch (FormatException formatException)
			{
				throw new InvalidQueryParameterProtocolException("maxresults", maxResultsString, "Not a valid integer.", formatException);
			}
			catch (OverflowException overflowException1)
			{
				OverflowException overflowException = overflowException1;
				int num1 = -2147483648;
				int num2 = 2147483647;
				throw new OutOfRangeQueryParameterProtocolException("maxresults", maxResultsString, num1.ToString(CultureInfo.InvariantCulture), num2.ToString(CultureInfo.InvariantCulture), overflowException);
			}
			return num;
		}

		protected bool? ParseOptionalBoolInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource paramSource, string paramName)
		{
			bool? nullable;
			string str = (paramSource == BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString ? base.RequestQueryParameters[paramName] : base.RequestHeadersCollection[paramName]);
			if (string.IsNullOrEmpty(str))
			{
				return null;
			}
			try
			{
				nullable = new bool?(bool.Parse(str));
			}
			catch (FormatException formatException1)
			{
				FormatException formatException = formatException1;
				if (paramSource != BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString)
				{
					throw new InvalidHeaderProtocolException(paramName, str, formatException);
				}
				throw new InvalidQueryParameterProtocolException(paramName, str, "Must be either 'true' or 'false'.", formatException);
			}
			return nullable;
		}

		protected DateTime? ParseOptionalDateTimeInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource paramSource, string paramName, bool isForSnapshot)
		{
			DateTime? nullable;
			string str = (paramSource == BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString ? base.RequestQueryParameters[paramName] : base.RequestHeadersCollection[paramName]);
			if (string.IsNullOrEmpty(str))
			{
				return null;
			}
			bool flag = false;
			flag = (isForSnapshot ? HttpUtilities.TryGetSnapshotDateTimeFromHttpString(str, out nullable) : HttpUtilities.TryGetDateTimeFromHttpString(str, out nullable));
			if (!flag)
			{
				if (paramSource != BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString)
				{
					throw new InvalidHeaderProtocolException(paramName, str);
				}
				throw new InvalidQueryParameterProtocolException(paramName, str, (!isForSnapshot ? "Must be in format of a supported date time string." : "Must be in the specific snapshot date time format."));
			}
			return new DateTime?(nullable.Value);
		}

		protected DateTime? ParseOptionalDateTimeInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource paramSource, string paramName)
		{
			return this.ParseOptionalDateTimeInput(paramSource, paramName, false);
		}

		protected List<string> ParseOptionalListInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource paramSource, string paramName, List<string> validValues, string delimiter)
		{
			string str = (paramSource == BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString ? base.RequestQueryParameters[paramName] : base.RequestHeadersCollection[paramName]);
			if (string.IsNullOrEmpty(str))
			{
				return null;
			}
			List<string> strs = new List<string>();
			string[] strArrays = new string[] { delimiter };
			string[] strArrays1 = str.Split(strArrays, StringSplitOptions.None);
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string str1 = strArrays1[i];
				bool flag = false;
				foreach (string validValue in validValues)
				{
					if (!str1.Equals(validValue, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					flag = true;
				}
				if (flag)
				{
					strs.Add(str1);
				}
				else if (!validValues.Contains(str1))
				{
					if (paramSource != BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString)
					{
						throw new InvalidHeaderProtocolException(paramName, str1);
					}
					throw new InvalidQueryParameterProtocolException(paramName, str1, "This query parameter value is invalid.");
				}
			}
			return strs;
		}

		protected long? ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource paramSource, string paramName, string paramValue = null)
		{
			long? nullable;
			string item;
			if (paramSource == BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString)
			{
				item = base.RequestQueryParameters[paramName];
			}
			else
			{
				item = (paramSource == BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue ? base.RequestHeadersCollection[paramName] : paramValue);
			}
			string str = item;
			if (string.IsNullOrEmpty(str))
			{
				return null;
			}
			try
			{
				nullable = new long?(long.Parse(str));
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (exception is FormatException || exception is OverflowException)
				{
					if (paramSource != BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString)
					{
						if (paramSource != BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue)
						{
							throw new InvalidXmlNodeProtocolException(paramName, paramValue, exception);
						}
						throw new InvalidHeaderProtocolException(paramName, str, exception);
					}
					throw new InvalidQueryParameterProtocolException(paramName, str, "Must be an integer value.", exception);
				}
				throw;
			}
			return nullable;
		}

		protected ulong? ParseOptionalULongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource paramSource, string paramName, string paramValue = null)
		{
			ulong? nullable;
			string item;
			if (paramSource == BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString)
			{
				item = base.RequestQueryParameters[paramName];
			}
			else
			{
				item = (paramSource == BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue ? base.RequestHeadersCollection[paramName] : paramValue);
			}
			string str = item;
			if (string.IsNullOrEmpty(str))
			{
				return null;
			}
			try
			{
				nullable = new ulong?(ulong.Parse(str));
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				if (exception is FormatException || exception is OverflowException)
				{
					if (paramSource != BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString)
					{
						if (paramSource != BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue)
						{
							throw new InvalidXmlNodeProtocolException(paramName, paramValue, exception);
						}
						throw new InvalidHeaderProtocolException(paramName, str, exception);
					}
					throw new InvalidQueryParameterProtocolException(paramName, str, "Must be an integer value.", exception);
				}
				throw;
			}
			return nullable;
		}

		protected virtual void PerformCommonRestWork()
		{
			if (!string.IsNullOrEmpty(this.ServerResponseHeaderValue))
			{
				base.Response.AddHeader("Server", this.ServerResponseHeaderValue);
			}
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				base.Response.AddHeader("x-ms-version", base.RequestRestVersion);
			}
			if (this.operationContext.HttpRequestMeasurementEvent != null)
			{
				this.ApplyResourceToMeasurementEvent(this.operationContext.HttpRequestMeasurementEvent);
			}
			if (this.operationContext.OperationMeasurementEvent != null)
			{
				this.ApplyResourceToMeasurementEvent(this.operationContext.OperationMeasurementEvent);
			}
		}

		protected override void PerformPreCloseTasks()
		{
			this.CaptureRequestContextState();
			base.PerformPreCloseTasks();
		}

		private IEnumerator<IAsyncResult> ProcessExceptionImpl(Exception e, AsyncIteratorContext<NoResults> context)
		{
			if (e is RequestIpThrottledException || e is ForbiddenRequestAbortedException)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Info.Log(e.Message);
				base.AbortResponse();
				if (!(e is RequestIpThrottledException))
				{
					this.FinishMeasurementAndSetStatusIfNotCompleted(NephosRESTEventStatus.ExpectedFailure, false, null);
				}
				else
				{
					this.FinishMeasurementAndSetStatusIfNotCompleted(NephosRESTEventStatus.IPThrottlingFailure, false, null);
				}
			}
			else
			{
				e = this.TransformException(e);
				NephosAssertionException.Assert(e != null);
				IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
				object[] logString = new object[] { e.GetLogString() };
				info.Log("Processing exception: {0}", logString);
				NephosErrorDetails errorDetailsForException = null;
				try
				{
					errorDetailsForException = this.GetErrorDetailsForException(e);
					if (errorDetailsForException == null)
					{
						IStringDataEventStream unhandledException = Logger<IRestProtocolHeadLogger>.Instance.UnhandledException;
						object[] str = new object[] { e.ToString() };
						unhandledException.Log("An unexpected/unrecognized exception occurred: {0}", str);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
					error.Log("GetErrorDetailsForException threw exception {0}", new object[] { exception });
				}
				if (errorDetailsForException == null)
				{
					errorDetailsForException = this.GetNephosErrorDetailsForUnknownException(e);
				}
				if (base.ResponseIsClosed || this.IsErrorSerializationHandled)
				{
					IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Info;
					object[] objArray = new object[] { e.GetLogString(), errorDetailsForException.StatusEntry.StatusId, errorDetailsForException.IsFatal, base.ResponseIsClosed, this.IsErrorSerializationHandled };
					stringDataEventStream.Log("Exception is raised after sending the response. Exception: {0}, Status code: {1}, IsFatal: {2} IsResponseClosed:{3} IsErrorSerialized:{4}", objArray);
					if (this.IsErrorSerializationHandled)
					{
						NephosAssertionException.Assert(base.ResponseIsClosed);
					}
				}
				else
				{
					IAsyncResult asyncResult = this.BeginSendErrorResponse(errorDetailsForException, context.GetResumeCallback(), context.GetResumeState("BasicHttpProcessorWithAuthAndAccountContainer.ProcessExceptionImpl"));
					yield return asyncResult;
					try
					{
						this.EndSendErrorResponse(asyncResult);
					}
					finally
					{
						this.FinishMeasurementAndSetStatusIfNotCompleted(errorDetailsForException.EventStatus, errorDetailsForException.SkipBillingAndMetrics, errorDetailsForException.StatusEntry);
					}
				}
				if (errorDetailsForException.IsFatal)
				{
					throw new FatalServerCrashingException(string.Format("The fatal unexpected exception '{0}' encountered during processing of request.", e.Message), e);
				}
			}
		}

		protected override IEnumerator<IAsyncResult> ProcessImpl(AsyncIteratorContext<NoResults> async)
		{
			bool flag;
			bool flag1;
			bool flag2;
			bool flag3;
			Exception nephosUnauthorizedAccessException = null;
			IAsyncResult asyncResult1 = null;
			string str = "HttpRestProcessor.ProcessImpl";
			try
			{
				base.RequestContext.Initialize();
				if (base.RequestContext.ToSetKeepAliveToFalse)
				{
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Set KeepAlive to false in the response of this request");
					base.Response.KeepAlive = false;
				}
				string requestRawUrlString = base.RequestContext.RequestRawUrlString;
				if (string.IsNullOrEmpty(base.RequestHost) && requestRawUrlString.StartsWith("/"))
				{
					throw new HostInformationNotPresentProtocolException();
				}
				try
				{
					Uri requestUrl = base.RequestUrl;
					IPEndPoint clientIP = base.RequestContext.ClientIP;
				}
				catch (ArgumentException argumentException1)
				{
					ArgumentException argumentException = argumentException1;
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { requestRawUrlString };
					throw new RequestUrlFailedToParseProtocolException(string.Format(invariantCulture, "The raw url '{0}' could not be parsed", objArray), argumentException);
				}
				catch (UriFormatException uriFormatException1)
				{
					UriFormatException uriFormatException = uriFormatException1;
					CultureInfo cultureInfo = CultureInfo.InvariantCulture;
					object[] objArray1 = new object[] { requestRawUrlString };
					throw new RequestUrlFailedToParseProtocolException(string.Format(cultureInfo, "The raw url '{0}' could not be parsed", objArray1), uriFormatException);
				}
				IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
				object[] methodName = new object[] { BasicHttpProcessorWithAuthAndAccountContainer<T>.GetMethodName(base.Method), base.SafeRequestUrlString, base.RequestHeaderRestVersion };
				verboseDebug.Log("Got request {0} {1} {2}", methodName);
				this.RunVersionCheck();
				this.SetVersionFlags();
				if (!Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.IsInvalidAccess(base.RequestContext) && Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.IsSignatureAccess(base.RequestContext))
				{
					string item = base.RequestContext.QueryParameters["sv"];
					if (item != null && VersioningConfigurationLookup.Instance.IsValidVersion(item) && VersioningHelper.CompareVersions(item, "2012-02-12") > 0)
					{
						base.SASVersion = this.GetProtocolVersionToUse(item);
						this.SetVersionFlags();
					}
				}
				bool inputEntryPointType = (base.RequestContext.InputEntryPointType & EntryPointType.SlbInterface) == EntryPointType.SlbInterface;
				bool flag4 = false;
				if (inputEntryPointType)
				{
					if (base.RequestUrl.PathAndQuery.StartsWith("/$ping"))
					{
						this.ProcessPingRequest();
						goto Label0;
					}
					else if (!flag4)
					{
						throw new ForbiddenRequestAbortedException("Unsupported operation requested at Slb Interface.");
					}
				}
			}
			catch (Exception exception)
			{
				nephosUnauthorizedAccessException = exception;
			}
			if (nephosUnauthorizedAccessException == null)
			{
				IAsyncResult asyncResult2 = this.BeginGetUriComponents(base.RequestUrl, BasicHttpProcessor.DefaultMaxAllowedTimeout, async.GetResumeCallback(), async.GetResumeState("ProcessImpl.GetUriComponents"));
				yield return asyncResult2;
				try
				{
					this.UriComponents = this.EndGetUriComponents(asyncResult2);
					if (this.UriComponents == null)
					{
						throw new InvalidUrlProtocolException(base.RequestUrl);
					}
				}
				catch (Exception exception1)
				{
					nephosUnauthorizedAccessException = exception1;
				}
				if (nephosUnauthorizedAccessException == null)
				{
					string empty = string.Empty;
					bool flag5 = false;
					if (this.UriComponents.AccountName != null)
					{
						IAsyncResult asyncResult3 = this.BeginGetResourceAccountProperties(this.UriComponents.AccountName, false, BasicHttpProcessor.DefaultMaxAllowedTimeout, async.GetResumeCallback(), async.GetResumeState("ProcessImpl.GetResourceAccountProperties"));
						yield return asyncResult3;
						try
						{
							this.storageAccount = this.EndGetResourceAccountProperties(asyncResult3);
							if (this.UriComponents.IsSecondaryAccountAccess)
							{
								bool value = false;
								if (this.storageAccount.ServiceMetadata.SecondaryReadEnabled.HasValue)
								{
									value = this.storageAccount.ServiceMetadata.SecondaryReadEnabled.Value;
								}
								if (!value)
								{
									throw new SecondaryReadDisabledException();
								}
								if (value)
								{
									this.storageAccount.IsSecondaryAccess = true;
									this.storageAccount.Permissions = new AccountPermissions?(AccountPermissions.Read);
								}
							}
						}
						catch (AccountNotFoundException accountNotFoundException)
						{
							IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
							object[] accountName = new object[] { this.UriComponents.AccountName };
							info.Log("Returning ResourceNotFound for non-admin requests since the resource account {0} does not exist", accountName);
							nephosUnauthorizedAccessException = new NephosUnauthorizedAccessException();
						}
						catch (Exception exception2)
						{
							nephosUnauthorizedAccessException = exception2;
						}
						if (nephosUnauthorizedAccessException == null)
						{
							goto Label1;
						}
						IAsyncResult asyncResult4 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessException.CheckAccountPermisssions"));
						yield return asyncResult4;
						this.EndProcessException(asyncResult4);
						goto Label0;
					}
				Label1:
					bool flag6 = false;
					if (this.AdjustRequestVersionBasedOnContainerAclSettings && !flag5)
					{
						IAsyncResult asyncResult5 = this.BeginAdjustRequestVersionBasedOnContainerAclSettings(this.UriComponents, BasicHttpProcessor.DefaultMaxAllowedTimeout, async.GetResumeCallback(), async.GetResumeState("ProcessImpl.AdjustRequestVersionBasedOnContainerAclSettings"));
						yield return asyncResult5;
						try
						{
							flag6 = this.EndAdjustRequestVersionBasedOnContainerAclSettings(asyncResult5);
						}
						catch (Exception exception3)
						{
							nephosUnauthorizedAccessException = exception3;
						}
						if (nephosUnauthorizedAccessException == null)
						{
							goto Label2;
						}
						IAsyncResult asyncResult6 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessException.AdjustRequestVersionBasedOnContainerAclSettings"));
						yield return asyncResult6;
						this.EndProcessException(asyncResult6);
						goto Label0;
					}
				Label2:
					if (flag6 || flag5)
					{
						try
						{
							IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
							object[] requestRestVersion = new object[] { base.RequestRestVersion, flag5, flag6 };
							verbose.Log("Adjusting versioning flags since the request version was adjusted to {0} IsViaServiceSetting={1} IsViaContainerAcl={2}", requestRestVersion);
							this.SetVersionFlags();
						}
						catch (Exception exception4)
						{
							nephosUnauthorizedAccessException = exception4;
						}
						if (nephosUnauthorizedAccessException != null)
						{
							IAsyncResult asyncResult7 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessException.SetAdjustedVersionFlags"));
							yield return asyncResult7;
							this.EndProcessException(asyncResult7);
							goto Label0;
						}
						else if (!this.IsStorageDomainNameUsed)
						{
							Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Getting UriComponents second time due to adjustment of request version");
							IAsyncResult asyncResult = this.BeginGetUriComponents(base.RequestUrl, BasicHttpProcessor.DefaultMaxAllowedTimeout, async.GetResumeCallback(), async.GetResumeState("ProcessImpl.GetUriComponents"));
							yield return asyncResult;
							try
							{
								this.UriComponents = this.EndGetUriComponents(asyncResult);
								if (this.UriComponents == null)
								{
									throw new InvalidUrlProtocolException(base.RequestUrl);
								}
							}
							catch (Exception exception5)
							{
								nephosUnauthorizedAccessException = exception5;
							}
							if (nephosUnauthorizedAccessException == null)
							{
								goto Label3;
							}
							IAsyncResult asyncResult8 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessException.GetUriComponents"));
							yield return asyncResult8;
							this.EndProcessException(asyncResult8);
							goto Label0;
						}
					}
				Label3:
					try
					{
						this.operationContext = (T)this.ExtractOperationContext(this.UriComponents);
					}
					catch (Exception exception6)
					{
						nephosUnauthorizedAccessException = exception6;
					}
					if (nephosUnauthorizedAccessException == null)
					{
						try
						{
							try
							{
								this.RunPreOperationCheck();
							}
							catch (Exception exception7)
							{
								nephosUnauthorizedAccessException = exception7;
							}
							if (nephosUnauthorizedAccessException == null)
							{
								TimeSpan defaultMaxAllowedTimeout = this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout);
								if (defaultMaxAllowedTimeout > BasicHttpProcessor.DefaultMaxAllowedTimeout)
								{
									defaultMaxAllowedTimeout = BasicHttpProcessor.DefaultMaxAllowedTimeout;
								}
								Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager authenticationManager = this.authenticationManager;
								IStorageAccount storageAccount = this.storageAccount;
								Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext = base.RequestContext;
								NephosUriComponents uriComponents = this.UriComponents;
								BasicHttpProcessorWithAuthAndAccountContainer<T> basicHttpProcessorWithAuthAndAccountContainer = this;
								asyncResult1 = authenticationManager.BeginAuthenticate(storageAccount, requestContext, uriComponents, new Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.GetStringToSignCallback(basicHttpProcessorWithAuthAndAccountContainer.GetStringToSign), defaultMaxAllowedTimeout, async.GetResumeCallback(), async.GetResumeState(str));
								yield return asyncResult1;
								try
								{
									IAuthenticationResult authenticationResult = this.authenticationManager.EndAuthenticate(asyncResult1);
									string authenticationVersion = null;
									if (authenticationResult != null)
									{
										authenticationVersion = authenticationResult.AuthenticationVersion;
										this.operationContext.CallerIdentity = authenticationResult.AccountIdentifier;
										if (authenticationResult.IsSignatureAccess && authenticationVersion != null && VersioningHelper.CompareVersions(authenticationVersion, "2014-02-14") >= 0)
										{
											authenticationVersion = this.GetProtocolVersionToUse(authenticationVersion);
										}
									}
									if (authenticationVersion != base.SASVersion)
									{
										base.SASVersion = authenticationVersion;
										this.SetVersionFlags();
									}
									if (this.operationContext.CallerIdentity != null && this.operationContext.CallerIdentity.AccountName.Equals("anonymous918b409cd64240648287969f2827f0c1"))
									{
										this.operationContext.IsRequestAnonymous = true;
									}
								}
								catch (Exception exception8)
								{
									nephosUnauthorizedAccessException = exception8;
								}
								if (nephosUnauthorizedAccessException == null)
								{
									this.operationContext.IsRequestSAS = Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager.IsSignatureAccess(base.RequestContext);
									try
									{
										AuthorizationResult authorizationResult = AuthorizationManager.PreAuthorizeRequest(this.operationContext.CallerIdentity, base.RequestContext);
										if (!authorizationResult.Authorized)
										{
											throw new NephosUnauthorizedAccessException(string.Format("Account {0} is not pre-authorized", this.operationContext.AccountName), authorizationResult.FailureReason, this.operationContext.CallerIdentity);
										}
									}
									catch (Exception exception9)
									{
										nephosUnauthorizedAccessException = exception9;
									}
									if (nephosUnauthorizedAccessException == null)
									{
										BasicHttpProcessorWithAuthAndAccountContainer<T>.RestMethodImpl restMethodImpl = null;
										try
										{
											restMethodImpl = this.ChooseRestMethodHandler(base.Method);
											this.PerformCommonRestWork();
										}
										catch (Exception exception10)
										{
											nephosUnauthorizedAccessException = exception10;
										}
										if (nephosUnauthorizedAccessException == null)
										{
											asyncResult1 = this.BeginPerformOperation(restMethodImpl, async.GetResumeCallback(), async.GetResumeState(str));
											yield return asyncResult1;
											try
											{
												this.EndPerformOperation(asyncResult1);
												NephosAssertionException.Assert(base.ResponseIsClosed, "An operation implementation returned successfully but response is not closed.");
												flag = (this.operationContext.OperationMeasurementEvent == null ? true : this.operationContext.OperationMeasurementEvent.Completed);
												NephosAssertionException.Assert(flag, "OperationMeasurementEvent is not completed on successful completion of an Impl method.");
												flag1 = (this.operationContext.HttpRequestMeasurementEvent == null ? true : this.operationContext.HttpRequestMeasurementEvent.Completed);
												NephosAssertionException.Assert(flag1, "HttpRequestMeasurementEvent is not completed on successful completion of an Impl method.");
											}
											catch (Exception exception11)
											{
												nephosUnauthorizedAccessException = exception11;
												this.CaptureRequestContextState();
											}
											if (nephosUnauthorizedAccessException == null)
											{
												try
												{
													NephosAssertionException.Assert(base.ResponseIsClosed, "Response is not closed.");
													flag2 = (this.operationContext.OperationMeasurementEvent == null ? true : this.operationContext.OperationMeasurementEvent.Completed);
													NephosAssertionException.Assert(flag2);
													flag3 = (this.operationContext.HttpRequestMeasurementEvent == null ? true : this.operationContext.HttpRequestMeasurementEvent.Completed);
													NephosAssertionException.Assert(flag3);
												}
												catch (Exception exception12)
												{
													nephosUnauthorizedAccessException = exception12;
												}
												if (nephosUnauthorizedAccessException == null)
												{
													goto Label0;
												}
												IAsyncResult asyncResult9 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessImpl.AssertCompleted"));
												yield return asyncResult9;
												this.EndProcessException(asyncResult9);
											}
											else
											{
												IAsyncResult asyncResult10 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessImpl.EndPerformOperation"));
												yield return asyncResult10;
												this.EndProcessException(asyncResult10);
											}
										}
										else
										{
											IAsyncResult asyncResult11 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessImpl.ChooseRestMethodHandler"));
											yield return asyncResult11;
											this.EndProcessException(asyncResult11);
										}
									}
									else
									{
										IAsyncResult asyncResult12 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessImpl.PreAuthorize"));
										yield return asyncResult12;
										this.EndProcessException(asyncResult12);
									}
								}
								else
								{
									IAsyncResult asyncResult13 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessImpl.Authenticate"));
									yield return asyncResult13;
									this.EndProcessException(asyncResult13);
								}
							}
							else
							{
								IAsyncResult asyncResult14 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessImpl.RunPreOperationCheck"));
								yield return asyncResult14;
								this.EndProcessException(asyncResult14);
							}
						}
						finally
						{
							this.FinishMeasurementAndSetStatusIfNotCompleted(NephosRESTEventStatus.UnknownFailure, false, null);
							this.operationContext.Dispose();
							this.operationContext = default(T);
						}
					}
					else
					{
						IAsyncResult asyncResult15 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessException.ExtractOperationContext"));
						yield return asyncResult15;
						this.EndProcessException(asyncResult15);
						NephosAssertionException.Assert(this.operationContext == null);
					}
				}
				else
				{
					IAsyncResult asyncResult16 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessException.GetUriComponents"));
					yield return asyncResult16;
					this.EndProcessException(asyncResult16);
				}
			}
			else
			{
				IAsyncResult asyncResult17 = this.BeginProcessException(nephosUnauthorizedAccessException, async.GetResumeCallback(), async.GetResumeState("ProcessImpl"));
				yield return asyncResult17;
				this.EndProcessException(asyncResult17);
				NephosAssertionException.Assert(this.operationContext == null);
			}
		Label0:
			yield break;
		}

		private void ProcessPingRequest()
		{
			Logger<IRestProtocolHeadLogger>.Instance.InfoDebug.Log("[FePing] Got Ping request");
			base.StatusCode = HttpStatusCode.OK;
			if (this.operationContext == null)
			{
				this.operationContext = Activator.CreateInstance<T>();
				this.operationContext.SetElapsedTime(base.RequestContext.ElapsedTime);
				this.operationContext.AccountName = "/$ping".TrimStart("/".ToCharArray());
				this.operationContext.ContainerName = string.Empty;
			}
			this.operationContext.OperationMeasurementEvent = new PingMeasurementEvent();
			this.SendSuccessResponse(true);
		}

		private IEnumerator<IAsyncResult> ReadAnalyticsSettingsImpl(AnalyticsSettingsVersion settingsVersion, Microsoft.Cis.Services.Nephos.Common.ServiceType service, AsyncIteratorContext<AnalyticsSettings> context)
		{
			this.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			AnalyticsSettings analyticsSetting = null;
			if (base.RequestContentLength <= (long)0)
			{
				long requestContentLength = base.RequestContentLength;
				throw new InvalidHeaderProtocolException("Content-Length", requestContentLength.ToString(CultureInfo.InvariantCulture));
			}
			if (base.RequestContentLength > (long)51200)
			{
				throw new RequestEntityTooLargeException(new long?((long)51200));
			}
			using (BufferPoolMemoryStream bufferPoolMemoryStream = new BufferPoolMemoryStream(8192))
			{
				using (Stream stream = this.GenerateMeasuredRequestStream())
				{
					IAsyncResult asyncResult = AsyncStreamCopy.BeginAsyncStreamCopy(stream, bufferPoolMemoryStream, base.RequestContentLength, 8192, this.operationContext.RemainingTimeout(), context.GetResumeCallback(), context.GetResumeState("BasicHttpProcessorWithAuthAndAccountContainer<T>.ReadAnalyticsSettingsImpl"));
					yield return asyncResult;
					AsyncStreamCopy.EndAsyncStreamCopy(asyncResult);
					bufferPoolMemoryStream.Seek((long)0, SeekOrigin.Begin);
					using (XmlReader xmlReader = XmlReader.Create(bufferPoolMemoryStream, BasicHttpProcessorWithAuthAndAccountContainer<T>.DefaultXmlReaderSettings))
					{
						bool flag = false;
						analyticsSetting = AnalyticsSettingsHelper.DeserializeAnalyticsSettingsFromReader(xmlReader, settingsVersion, service, false, flag);
					}
				}
			}
			context.ResultData = analyticsSetting;
		}

		protected void RequestStreamDisposeEventHandler(object sender, EventArgs e)
		{
			TallyKeepingStream tallyKeepingStream = sender as TallyKeepingStream;
			if (this.operationContext != null)
			{
				long ticks = this.operationContext.OperationClientReadNetworkLatency.Ticks;
				TimeSpan readNetworkLatency = tallyKeepingStream.ReadNetworkLatency;
				this.operationContext.OperationClientReadNetworkLatency = new TimeSpan(ticks + readNetworkLatency.Ticks);
				if (this.operationContext.OperationMeasurementEvent != null)
				{
					INephosBaseOperationMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent;
					operationMeasurementEvent.RequestBytesRead = operationMeasurementEvent.RequestBytesRead + tallyKeepingStream.BytesRead;
				}
			}
		}

		protected bool ResponseCanHaveContentBody()
		{
			if (base.Method != RestMethod.HEAD && !HttpUtilities.StatusCodeIndicatesNoResponseBody(base.StatusCode))
			{
				return true;
			}
			return false;
		}

		protected void ResponseStreamDisposeEventHandler(object sender, EventArgs e)
		{
			TallyKeepingStream tallyKeepingStream = sender as TallyKeepingStream;
			if (this.operationContext != null)
			{
				long ticks = this.operationContext.OperationClientWriteNetworkLatency.Ticks;
				TimeSpan writeNetworkLatency = tallyKeepingStream.WriteNetworkLatency;
				this.operationContext.OperationClientWriteNetworkLatency = new TimeSpan(ticks + writeNetworkLatency.Ticks);
				if (this.operationContext.OperationMeasurementEvent != null)
				{
					INephosBaseOperationMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent;
					operationMeasurementEvent.ResponseBytesWritten = operationMeasurementEvent.ResponseBytesWritten + tallyKeepingStream.BytesWritten;
				}
			}
		}

		private void RunPreOperationCheck()
		{
			if (this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout) <= TimeSpan.Zero)
			{
				throw new TimeoutException(string.Format("This request was timed out instantly because it already timed out before we started processing it. User's Timeout: {0}, TimeRequestReceived: {1}, TimeElapsedBeforeProcessing: {2}", this.operationContext.OperationTimeout, base.RequestContext.TimeReceived, base.RequestContext.ElapsedTime));
			}
		}

		private void RunVersionCheck()
		{
			if (!string.IsNullOrEmpty(base.RequestHeaderRestVersion) && !VersioningConfigurationLookup.Instance.IsValidVersion(base.RequestHeaderRestVersion))
			{
				FutureVersionProtocolException.ThrowIfFutureVersion(base.RequestHeaderRestVersion);
				throw new InvalidHeaderProtocolException("x-ms-version", base.RequestHeaderRestVersion);
			}
		}

		protected void SendErrorResponse(NephosErrorDetails errorDetails)
		{
			this.SendResponse();
			this.FinishMeasurementAndSetStatusIfNotCompleted(errorDetails.EventStatus, errorDetails.SkipBillingAndMetrics, errorDetails.StatusEntry);
			if (errorDetails.IsFatal)
			{
				throw new FatalServerCrashingException(string.Format("The fatal unexpected exception '{0}' was handled by encountered during processing of request.", errorDetails.ErrorException.Message), errorDetails.ErrorException);
			}
		}

		private IEnumerator<IAsyncResult> SendErrorResponseImpl(NephosErrorDetails errorDetails, AsyncIteratorContext<NoResults> context)
		{
			string str;
			string str1;
			string str2;
			NephosAssertionException.Assert(errorDetails != null, "errorDetails cannot be null.");
			NephosAssertionException.Assert(errorDetails.StatusEntry != null, "statusEntry cannot be null.");
			bool flag = false;
			bool flag1 = false;
			try
			{
				if (flag)
				{
					base.Response.KeepAlive = false;
				}
				if (!this.SetStatusCodeOrAbort(errorDetails.StatusEntry.StatusCodeHttp))
				{
					str = (errorDetails.ErrorException != null ? errorDetails.ErrorException.GetLogString() : "<null>");
					string str3 = str;
					IStringDataEventStream warning = Logger<IRestProtocolHeadLogger>.Instance.Warning;
					object[] statusCodeHttp = new object[] { errorDetails.StatusEntry.StatusCodeHttp, errorDetails.StatusEntry.UserMessage, str3 };
					warning.Log("Couldn't set the status to {0}, Desc: {1}, Exception: {2}", statusCodeHttp);
				}
				else
				{
					base.StatusDescription = errorDetails.StatusEntry.UserMessage;
					if (errorDetails.ResponseHeaders != null)
					{
						base.Response.Headers.Add(errorDetails.ResponseHeaders);
					}
					if (!this.ResponseCanHaveContentBody())
					{
						goto Label0;
					}
					IAsyncResult responseStream = this.BeginWriteErrorInfoToResponseStream(errorDetails, context.GetResumeCallback(), context.GetResumeState("SendErrorResponseImpl.WriteErrorInfoToResponseStream"));
					yield return responseStream;
					try
					{
						this.EndWriteErrorInfoToResponseStream(responseStream);
						flag1 = true;
					}
					catch (HttpListenerException httpListenerException1)
					{
						HttpListenerException httpListenerException = httpListenerException1;
						str1 = (errorDetails.ErrorException != null ? errorDetails.ErrorException.GetLogString() : "<null>");
						string str4 = str1;
						IStringDataEventStream networkFailure = Logger<IRestProtocolHeadLogger>.Instance.NetworkFailure;
						object[] stringForHttpListenerException = new object[] { HttpUtilities.GetStringForHttpListenerException(httpListenerException), str4 };
						networkFailure.Log("SecurityWarning: Ignoring the HttpListenerException {0} that occurred while writing error info into response stream for the exception: {1}", stringForHttpListenerException);
					}
					catch (TimeoutException timeoutException)
					{
						str2 = (errorDetails.ErrorException != null ? errorDetails.ErrorException.GetLogString() : "<null>");
						string str5 = str2;
						IStringDataEventStream requestTimeout = Logger<IRestProtocolHeadLogger>.Instance.RequestTimeout;
						object[] statusId = new object[] { errorDetails.StatusEntry.StatusId, str5 };
						requestTimeout.Log("SecurityWarning: Timed out while writing error info for status id {0}, exception {1}", statusId);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Warning;
						object[] logString = new object[] { exception.GetLogString() };
						stringDataEventStream.Log("EndWriteErrorInfoToResponseStream threw exception {0}", logString);
						throw;
					}
				}
			}
			finally
			{
				if (flag && flag1)
				{
					base.AbortResponse();
					return;
				}
				this.SendResponse();
			}
		Label0:
			yield break;
		}

		private void SendResponse()
		{
			NephosAssertionException.Assert(!base.ResponseIsClosed);
			try
			{
				NephosAssertionException.Assert(base.StatusCodeIsSet, "An operation implementation is sending the response without having set the HTTP Status Code");
			}
			finally
			{
				base.CloseResponse();
			}
		}

		protected void SendSuccessResponse(MeasurementEventStatus status, bool skipBillingAndMetrics = false)
		{
			this.SendResponse();
			this.FinishMeasurementAndSetStatusIfNotCompleted(status, skipBillingAndMetrics, null);
		}

		protected void SendSuccessResponse(bool skipBillingAndMetrics = false)
		{
			this.SendSuccessResponse(NephosRESTEventStatus.Success, skipBillingAndMetrics);
		}

		protected void SetContainerAclOnResponse(ContainerAclSettings acl)
		{
			NephosAssertionException.Assert(acl != null);
			string str = null;
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				str = acl.PublicAccess.ToString(CultureInfo.InvariantCulture);
				base.Response.AddHeader("x-ms-prop-publicaccess", str);
			}
			else
			{
				if (!string.IsNullOrEmpty(acl.PublicAccessLevel) && !Comparison.StringEqualsIgnoreCase(acl.PublicAccessLevel, bool.FalseString))
				{
					if (!base.RequestContext.IsRequestVersionAtLeastMay16)
					{
						str = acl.PublicAccessLevel.ToString(CultureInfo.InvariantCulture);
					}
					else if (Comparison.StringEqualsIgnoreCase(acl.PublicAccessLevel, "container") || Comparison.StringEqualsIgnoreCase(acl.PublicAccessLevel, bool.TrueString))
					{
						str = "container";
					}
					else if (Comparison.StringEqualsIgnoreCase(acl.PublicAccessLevel, "blob"))
					{
						str = "blob";
					}
				}
				if (!string.IsNullOrEmpty(str))
				{
					base.Response.AddHeader("x-ms-blob-public-access", str);
					return;
				}
			}
		}

		protected void SetMetadataOnResponse(NameValueCollection metadata)
		{
			NephosAssertionException.Assert(metadata != null);
			string[] allKeys = metadata.AllKeys;
			for (int i = 0; i < (int)allKeys.Length; i++)
			{
				string str = allKeys[i];
				string item = metadata[str];
				base.Response.AddHeader(string.Concat("x-ms-meta-", str), item);
				if (this.operationContext.OperationMeasurementEvent != null)
				{
					INephosBaseOperationMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent;
					operationMeasurementEvent.MetadataKeySize = operationMeasurementEvent.MetadataKeySize + (long)str.Length;
					INephosBaseOperationMeasurementEvent metadataValueSize = this.operationContext.OperationMeasurementEvent;
					metadataValueSize.MetadataValueSize = metadataValueSize.MetadataValueSize + (long)item.Length;
				}
			}
		}

		protected override void SetStatusCodeAndServiceHeaders(HttpStatusCode statusCode)
		{
			base.SetStatusCodeAndServiceHeaders(statusCode);
			this.AddServiceResponseHeadersBeforeSendingResponse();
		}

		protected bool SetStatusCodeOrAbort(HttpStatusCode statusCode)
		{
			if (base.CanStatusCodeBeSet)
			{
				this.SetStatusCodeAndServiceHeaders(statusCode);
				return true;
			}
			IStringDataEventStream infoDebug = Logger<IRestProtocolHeadLogger>.Instance.InfoDebug;
			object[] safeRequestUrlString = new object[] { base.SafeRequestUrlString, base.Method, statusCode };
			infoDebug.Log("Aborting the response as headers are already sent. Url: {0}, Method: {1}, Status code: {2}", safeRequestUrlString);
			try
			{
				base.Response.Abort();
			}
			catch (HttpListenerException httpListenerException1)
			{
				HttpListenerException httpListenerException = httpListenerException1;
				IStringDataEventStream networkFailure = Logger<IRestProtocolHeadLogger>.Instance.NetworkFailure;
				object[] stringForHttpListenerException = new object[] { HttpUtilities.GetStringForHttpListenerException(httpListenerException) };
				networkFailure.Log("SecurityWarning: Could not abort the response: '{0}'", stringForHttpListenerException);
			}
			return false;
		}

		private void SetVersionFlags()
		{
			base.RequestContext.IsRequestVersionAtLeastJuly09 = base.IsRequestVersionAtLeast("2009-07-17");
			base.RequestContext.IsRequestVersionAtLeastSeptember09 = base.IsRequestVersionAtLeast("2009-09-19");
			base.RequestContext.IsRequestVersionAtLeastAugust11 = base.IsRequestVersionAtLeast("2011-08-18");
			base.RequestContext.IsRequestVersionAtLeastFebruary12 = base.IsRequestVersionAtLeast("2012-02-12");
			base.RequestContext.IsRequestVersionAtLeastSeptember12 = base.IsRequestVersionAtLeast("2012-09-19");
			base.RequestContext.IsRequestVersionAtLeastJuly13 = base.IsRequestVersionAtLeast("2013-07-14");
			base.RequestContext.IsRequestVersionAtLeastAugust13 = base.IsRequestVersionAtLeast("2013-08-15");
			base.RequestContext.IsRequestVersionAtLeastFebruary14 = base.IsRequestVersionAtLeast("2014-02-14");
			base.RequestContext.IsRequestVersionAtLeastFebruary15 = base.IsRequestVersionAtLeast("2015-02-21");
			base.RequestContext.IsRequestVersionAtLeastApril15 = base.IsRequestVersionAtLeast("2015-04-05");
			base.RequestContext.IsRequestVersionAtLeastJuly15 = base.IsRequestVersionAtLeast("2015-07-08");
			base.RequestContext.IsRequestVersionAtLeastDecember15 = base.IsRequestVersionAtLeast("2015-12-11");
			base.RequestContext.IsRequestVersionAtLeastFebruary16 = base.IsRequestVersionAtLeast("2016-02-19");
			base.RequestContext.IsRequestVersionAtLeastMay16 = base.IsRequestVersionAtLeast("2016-05-31");
			base.RequestContext.IsRequestVersionAtLeastOctober16 = base.IsRequestVersionAtLeast("2016-10-16");
			base.RequestContext.IsRequestVersionAtLeastApril17 = base.IsRequestVersionAtLeast("2017-04-17");
		}

		protected void ThrowIfApiNotSupportedForVersion(bool isApiSupported)
		{
			if (!isApiSupported)
			{
				throw new InvalidHeaderProtocolException("x-ms-version", base.RequestHeaderRestVersion);
			}
		}

		protected Exception TransformException(Exception e)
		{
			if (this.TransformProviderException != null)
			{
				Exception transformProviderException = this.TransformProviderException(e);
				if (transformProviderException != null)
				{
					return transformProviderException;
				}
			}
			return e;
		}

		private void UpdateBandwidthUsage(int requestHeaderLength, int responseHeaderLength)
		{
		}

		private void UpdateOverallAccountThrottlingProbability()
		{
		}

		protected virtual void ValidateAndAdjustContext(T context, NephosUriComponents uriComponents, bool suppressException)
		{
		}

		private void ValidateStorageDomainRequestVersion()
		{
			if (!string.IsNullOrEmpty(base.RequestHeadersCollection["Authorization"]))
			{
				if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
				{
					Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Authorized Storage domain name {0} requests are only allowed for September09+ versions", new object[] { base.RequestHost });
					throw new InvalidHeaderProtocolException("x-ms-version", base.RequestRestVersion);
				}
			}
			else if (!string.IsNullOrEmpty(base.RequestHeaderRestVersion) && !base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Anonymous Storage domain name {0} requests are only allowed for no version or September09+ versions", new object[] { base.RequestHost });
				throw new InvalidHeaderProtocolException("x-ms-version", base.RequestRestVersion);
			}
		}

		private IEnumerator<IAsyncResult> WriteAnalyticsSettingsImpl(AnalyticsSettings settings, AnalyticsSettingsVersion settingsVersion, Microsoft.Cis.Services.Nephos.Common.ServiceType service, AsyncIteratorContext<NoResults> async)
		{
			base.StatusCode = HttpStatusCode.OK;
			using (Stream stream = this.GenerateMeasuredResponseStream(false))
			{
				base.Response.SendChunked = true;
				base.Response.ContentType = "application/xml";
				using (AccumulatorStream accumulatorStream = new AccumulatorStream(stream, 51200, false))
				{
					XmlWriter xmlWriter = XmlWriter.Create(accumulatorStream, BasicHttpProcessorWithAuthAndAccountContainer<T>.DefaultXmlWriterSettings);
					AnalyticsSettingsHelper.SerializeAnalyticsSettingsToWriter(xmlWriter, settings, settingsVersion, service, false);
					xmlWriter.Flush();
					IAsyncResult innerStream = accumulatorStream.BeginFlushToInnerStream(this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), async.GetResumeCallback(), async.GetResumeState("BasicHttpProcessorWithAuthAndAccountContainer<T>.WriteAnalyticsSettingsImpl"));
					yield return innerStream;
					accumulatorStream.EndFlushToInnerStream(innerStream);
					xmlWriter.Close();
				}
			}
		}

		protected virtual void WriteErrorInfoToMemoryStream(NephosErrorDetails errorDetails, Exception e, NameValueCollection additionalUserDetails, MemoryStream outStream)
		{
			using (XmlWriter xmlWriter = XmlWriter.Create(outStream, BasicHttpProcessorWithAuthAndAccountContainer<T>.DefaultXmlWriterSettings))
			{
				NephosStatusEntry statusEntry = errorDetails.StatusEntry;
				xmlWriter.WriteStartElement("Error");
				xmlWriter.WriteElementString("Code", statusEntry.StatusId);
				xmlWriter.WriteElementString("Message", errorDetails.UserSafeErrorMessage);
				if (additionalUserDetails != null)
				{
					string[] allKeys = additionalUserDetails.AllKeys;
					for (int i = 0; i < (int)allKeys.Length; i++)
					{
						string str = allKeys[i];
						xmlWriter.WriteElementString(str, additionalUserDetails[str]);
					}
				}
				if (this.IncludeInternalDetailsInErrorResponses && e != null)
				{
					xmlWriter.WriteStartElement("ExceptionDetails");
					xmlWriter.WriteElementString("ExceptionMessage", e.Message);
					xmlWriter.WriteElementString("StackTrace", e.ToString());
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
			}
		}

		private IEnumerator<IAsyncResult> WriteErrorInfoToResponseStreamImpl(NephosErrorDetails errorDetails, AsyncIteratorContext<NoResults> context)
		{
			TimeSpan timeSpan;
			byte[] buffer;
			BufferWrapper bufferWrapper = null;
			if (!errorDetails.HasErrorResponse)
			{
				bufferWrapper = BufferPool.GetBuffer(32768);
				buffer = bufferWrapper.Buffer;
			}
			else
			{
				buffer = errorDetails.ErrorResponse;
			}
			try
			{
				using (MemoryStream memoryStream = new MemoryStream(buffer))
				{
					if (!errorDetails.HasErrorResponse)
					{
						try
						{
							this.WriteErrorInfoToMemoryStream(errorDetails, errorDetails.ErrorException, errorDetails.AdditionalDetails, memoryStream);
						}
						catch (NotSupportedException notSupportedException1)
						{
							NotSupportedException notSupportedException = notSupportedException1;
							IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
							object[] capacity = new object[] { memoryStream.Capacity, notSupportedException.ToString() };
							info.Log("Error info exceeded {0} bytes. Exception is {1}", capacity);
							goto Label0;
						}
						catch (ArgumentException argumentException1)
						{
							ArgumentException argumentException = argumentException1;
							IStringDataEventStream warning = Logger<IRestProtocolHeadLogger>.Instance.Warning;
							object[] str = new object[] { argumentException.ToString() };
							warning.Log("No Extended Error Information is being sent due ArgumentException while writing extended error. Exception: {0}", str);
							goto Label0;
						}
					}
					string item = null;
					if (errorDetails.ResponseHeaders != null)
					{
						item = errorDetails.ResponseHeaders["Content-Type"];
					}
					if (string.IsNullOrWhiteSpace(item))
					{
						item = "application/xml";
					}
					base.Response.ContentType = item;
					base.Response.ContentLength64 = memoryStream.Position;
					timeSpan = (this.operationContext != null ? this.operationContext.RemainingTimeout() : TimeSpan.MaxValue);
					TimeSpan minimumTimeoutForErrorInfo = timeSpan;
					if (minimumTimeoutForErrorInfo < BasicHttpProcessorWithAuthAndAccountContainer<T>.MinimumTimeoutForErrorInfo)
					{
						minimumTimeoutForErrorInfo = BasicHttpProcessorWithAuthAndAccountContainer<T>.MinimumTimeoutForErrorInfo;
					}
					using (Stream stream = this.GenerateMeasuredResponseStream(true))
					{
						IAsyncResult asyncResult = stream.BeginWrite(buffer, 0, (int)memoryStream.Position, context.GetResumeCallback(), context.GetResumeState("this.ResponseStream.Write"));
						yield return asyncResult;
						stream.EndWrite(asyncResult);
					}
					if (this.operationContext == null)
					{
						goto Label0;
					}
					if (this.operationContext.HttpRequestMeasurementEvent != null)
					{
						INephosBaseMeasurementEvent httpRequestMeasurementEvent = this.operationContext.HttpRequestMeasurementEvent;
						httpRequestMeasurementEvent.ErrorResponseBytes = httpRequestMeasurementEvent.ErrorResponseBytes + base.Response.ContentLength64;
					}
					if (this.operationContext.OperationMeasurementEvent == null)
					{
						goto Label0;
					}
					INephosBaseOperationMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent;
					operationMeasurementEvent.ErrorResponseBytes = operationMeasurementEvent.ErrorResponseBytes + base.Response.ContentLength64;
				}
			}
			finally
			{
				if (bufferWrapper != null)
				{
					BufferPool.ReleaseBuffer(bufferWrapper);
					bufferWrapper = null;
				}
			}
		Label0:
			yield break;
		}

		private IEnumerator<IAsyncResult> WriteServiceStatsImpl(GeoReplicationStats stats, AsyncIteratorContext<NoResults> async)
		{
			base.StatusCode = HttpStatusCode.OK;
			using (Stream stream = this.GenerateMeasuredResponseStream(false))
			{
				base.Response.SendChunked = true;
				base.Response.ContentType = "application/xml";
				using (AccumulatorStream accumulatorStream = new AccumulatorStream(stream, 51200, false))
				{
					XmlWriter xmlWriter = XmlWriter.Create(accumulatorStream, BasicHttpProcessorWithAuthAndAccountContainer<T>.DefaultXmlWriterSettings);
					ServiceStatsHelper.SerializeServiceStatsToWriter(xmlWriter, stats);
					xmlWriter.Flush();
					IAsyncResult innerStream = accumulatorStream.BeginFlushToInnerStream(this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), async.GetResumeCallback(), async.GetResumeState("BasicHttpProcessorWithAuthAndAccountContainer<T>.WriteServiceStatsImpl"));
					yield return innerStream;
					accumulatorStream.EndFlushToInnerStream(innerStream);
					xmlWriter.Close();
				}
			}
		}

		protected enum ParameterSource
		{
			QueryString,
			HeaderValue,
			RequestBody
		}

		protected delegate IEnumerator<IAsyncResult> RestMethodImpl(AsyncIteratorContext<NoResults> asyncContext);
	}
}