using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class RequestContext
	{
		private NameValueCollection queryParams;

		private WebHeaderCollection requestHeaders;

		private IPEndPoint clientIP;

		private string authorizationScheme;

		private string authorizationSchemeParameters;

		private string safeRequestUrl;

		private string clientIPAsString;

		private string clientIPAsUnmaskedString;

		private Uri requestUrl;

		private string userAgent;

		private uint startMillisecond;

		private Microsoft.Cis.Services.Nephos.Common.OperationStatus operationStatus;

		private Microsoft.Cis.Services.Nephos.Common.ProviderInjection providerInjection;

		public string AuthorizationScheme
		{
			get
			{
				if (this.authorizationScheme == null)
				{
					HttpRequestAccessorCommon.GetAuthorizationFieldValues(this.RequestHeaders, out this.authorizationScheme, out this.authorizationSchemeParameters);
				}
				return this.authorizationScheme;
			}
		}

		public string AuthorizationSchemeParameters
		{
			get
			{
				if (this.authorizationSchemeParameters == null)
				{
					HttpRequestAccessorCommon.GetAuthorizationFieldValues(this.RequestHeaders, out this.authorizationScheme, out this.authorizationSchemeParameters);
				}
				return this.authorizationSchemeParameters;
			}
		}

		public IPEndPoint ClientIP
		{
			get
			{
				if (this.clientIP == null)
				{
					this.clientIP = this.Request.RemoteEndPoint;
					this.UpdateClientIPIfMasked();
				}
				return this.clientIP;
			}
		}

		public string ClientIPAsString
		{
			get
			{
				if (this.clientIPAsString == null && this.ClientIP != null)
				{
					this.clientIPAsString = this.ClientIP.ToString();
				}
				return this.clientIPAsString;
			}
		}

		public string ClientIPAsUnmaskedString
		{
			get
			{
				if (this.clientIPAsUnmaskedString == null && this.ClientIP != null)
				{
					this.clientIPAsUnmaskedString = this.ClientIP.UnmaskedToString();
				}
				return this.clientIPAsUnmaskedString;
			}
		}

		public int CreatorManagedThreadId
		{
			get;
			set;
		}

		public DateTime DequeuedTime
		{
			get;
			set;
		}

		public TimeSpan ElapsedTime
		{
			get
			{
				return TimeSpan.FromMilliseconds((double)((float)(NativeMethods.timeGetTime() - this.startMillisecond)));
			}
		}

		public DateTime EnqueuedTime
		{
			get;
			set;
		}

		private IHttpListenerContext HttpListenerContext
		{
			get;
			set;
		}

		public Uri HttpListenerRequestUrl
		{
			get
			{
				return this.Request.Url;
			}
		}

		public string HttpMethod
		{
			get
			{
				return this.Request.HttpMethod;
			}
		}

		public ulong HttpSysRequestId
		{
			get
			{
				byte[] byteArray = this.Request.RequestTraceIdentifier.ToByteArray();
				ulong num = (ulong)byteArray[8] | (ulong)byteArray[9] << 8 | (ulong)byteArray[10] << 16 | (ulong)byteArray[11] << 24 | (ulong)byteArray[12] << 32 | (ulong)byteArray[13] << 40 | (ulong)byteArray[14] << 48 | (ulong)byteArray[15] << 56;
				return num;
			}
		}

		public EntryPointType InputEntryPointType
		{
			get;
			set;
		}

		public Stream InputStream
		{
			get
			{
				return this.Request.InputStream;
			}
		}

		public bool IsHttpListenerRequestUrlNull
		{
			get
			{
				return this.Request.Url == null;
			}
		}

		public bool IsRequestVersionAtLeastApril15
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastApril17
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastAugust11
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastAugust13
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastDecember15
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastDecember16
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastFebruary12
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastFebruary14
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastFebruary15
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastFebruary16
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastJuly09
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastJuly13
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastJuly15
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastMay16
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastOctober16
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastSeptember09
		{
			get;
			set;
		}

		public bool IsRequestVersionAtLeastSeptember12
		{
			get;
			set;
		}

		public bool IsSecureConnection
		{
			get
			{
				return this.Request.IsSecureConnection;
			}
		}

		public static bool MaskIPAddress
		{
			get;
			set;
		}

		public bool MultipleConditionalHeadersEnabled
		{
			get
			{
				return this.IsRequestVersionAtLeastAugust13;
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get
			{
				return this.operationStatus;
			}
		}

		public Version ProtocolVersion
		{
			get
			{
				return this.Request.ProtocolVersion;
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get
			{
				return this.providerInjection;
			}
		}

		public NameValueCollection QueryParameters
		{
			get
			{
				if (this.queryParams == null)
				{
					this.queryParams = HttpUtilities.GetQueryParameters(this.RequestUrl);
				}
				return this.queryParams;
			}
		}

		public int RemainingRequestsInQueueAtGet
		{
			get;
			set;
		}

		public int RemainingRequestsInQueueAtPut
		{
			get;
			set;
		}

		private IHttpListenerRequest Request
		{
			get
			{
				return this.HttpListenerContext.Request;
			}
		}

		public string[] RequestAcceptTypes
		{
			get
			{
				return this.Request.AcceptTypes;
			}
		}

		public string RequestContentCrc64
		{
			get;
			set;
		}

		public Encoding RequestContentEncoding
		{
			get
			{
				return this.Request.ContentEncoding;
			}
		}

		public long RequestContentLength
		{
			get;
			private set;
		}

		public string RequestContentMD5
		{
			get;
			set;
		}

		public string RequestContentType
		{
			get
			{
				return this.Request.ContentType;
			}
		}

		public bool RequestHasEntityBody
		{
			get
			{
				return this.Request.HasEntityBody;
			}
		}

		public string RequestHeaderRestVersion
		{
			get
			{
				return this.RequestHeaders["x-ms-version"];
			}
		}

		public WebHeaderCollection RequestHeaders
		{
			get
			{
				if (this.requestHeaders == null)
				{
					this.requestHeaders = this.Request.Headers as WebHeaderCollection;
				}
				return this.requestHeaders;
			}
		}

		public string RequestRawUrlString
		{
			get
			{
				return this.Request.RawUrl;
			}
		}

		public virtual Uri RequestUrl
		{
			get
			{
				if (this.requestUrl == null)
				{
					this.requestUrl = HttpUtilities.GetRequestUri(this.Request);
				}
				return this.requestUrl;
			}
		}

		public string RequestUrlString
		{
			get
			{
				if (this.HttpListenerRequestUrl != null)
				{
					return this.HttpListenerRequestUrl.ToString();
				}
				return this.Request.RawUrl;
			}
		}

		public string RequestUserAgent
		{
			get
			{
				return this.Request.UserAgent;
			}
		}

		public IHttpListenerResponse Response
		{
			get
			{
				return this.HttpListenerContext.Response;
			}
		}

		public string SafeRequestUrlString
		{
			get
			{
				if (this.safeRequestUrl == null)
				{
					this.safeRequestUrl = HttpUtilities.GetSafeUriString(this.RequestUrl.OriginalString);
				}
				return this.safeRequestUrl;
			}
		}

		public string ServerContentCrc64
		{
			get;
			set;
		}

		public string ServerContentMD5
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Common.ServiceType ServiceType
		{
			get
			{
				return this.OperationStatus.ServiceType;
			}
			set
			{
				this.OperationStatus.ServiceType = value;
			}
		}

		public TimeSpan SmbOpLockBreakLatency
		{
			get;
			set;
		}

		public DateTime TimeReceived
		{
			get;
			private set;
		}

		public bool ToDropRequest
		{
			get;
			set;
		}

		public bool ToSetKeepAliveToFalse
		{
			get;
			set;
		}

		public string UserAgent
		{
			get
			{
				if (this.userAgent == null && this.RequestHeaders != null)
				{
					this.userAgent = this.RequestHeaders["User-Agent"];
				}
				return this.userAgent;
			}
		}

		public string UserHostName
		{
			get
			{
				return this.RequestHeaders["Host"];
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.VnetSourceDetails VnetSourceDetails
		{
			get;
			set;
		}

		public bool WasInHighPriorityQueue
		{
			get;
			set;
		}

		public RequestContext()
		{
			this.operationStatus = new Microsoft.Cis.Services.Nephos.Common.OperationStatus();
			this.providerInjection = new Microsoft.Cis.Services.Nephos.Common.ProviderInjection();
		}

		public RequestContext(System.Net.HttpListenerContext httpListenerContext, DateTime timeReceived) : this(httpListenerContext)
		{
		}

		public RequestContext(System.Net.HttpListenerContext httpListenerContext) : this()
		{
			this.HttpListenerContext = new HttpListenerContextAdapter(httpListenerContext);
			this.TimeReceived = DateTime.UtcNow;
			this.startMillisecond = NativeMethods.timeGetTime();
			this.CreatorManagedThreadId = Thread.CurrentThread.ManagedThreadId;
			this.InputEntryPointType = EntryPointType.Invalid;
		}

		public void Initialize()
		{
			this.RequestContentLength = this.Request.ContentLength64;
		}

		private void UpdateClientIPIfMasked()
		{
			if (RequestContext.MaskIPAddress)
			{
				this.ClientIP.Address = new MaskedIPAddress(this.ClientIP.Address.GetAddressBytes());
			}
		}
	}
}