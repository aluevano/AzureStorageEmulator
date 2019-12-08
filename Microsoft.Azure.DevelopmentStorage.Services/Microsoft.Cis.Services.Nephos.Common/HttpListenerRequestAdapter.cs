using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class HttpListenerRequestAdapter : IHttpListenerRequest
	{
		private HttpListenerRequest httpListenerRequest;

		private static PropertyInfo RequestIdPropertyInfo;

		public string[] AcceptTypes
		{
			get
			{
				return this.httpListenerRequest.AcceptTypes;
			}
		}

		public Encoding ContentEncoding
		{
			get
			{
				return this.httpListenerRequest.ContentEncoding;
			}
		}

		public long ContentLength64
		{
			get
			{
				return this.httpListenerRequest.ContentLength64;
			}
		}

		public string ContentType
		{
			get
			{
				return this.httpListenerRequest.ContentType;
			}
		}

		public bool HasEntityBody
		{
			get
			{
				return this.httpListenerRequest.HasEntityBody;
			}
		}

		public NameValueCollection Headers
		{
			get
			{
				return this.httpListenerRequest.Headers;
			}
		}

		public string HttpMethod
		{
			get
			{
				return this.httpListenerRequest.HttpMethod;
			}
		}

		public Stream InputStream
		{
			get
			{
				return this.httpListenerRequest.InputStream;
			}
		}

		public bool IsSecureConnection
		{
			get
			{
				return this.httpListenerRequest.IsSecureConnection;
			}
		}

		public Version ProtocolVersion
		{
			get
			{
				return this.httpListenerRequest.ProtocolVersion;
			}
		}

		public string RawUrl
		{
			get
			{
				return this.httpListenerRequest.RawUrl;
			}
		}

		public IPEndPoint RemoteEndPoint
		{
			get
			{
				return this.httpListenerRequest.RemoteEndPoint;
			}
		}

		public Guid RequestTraceIdentifier
		{
			get
			{
				return this.httpListenerRequest.RequestTraceIdentifier;
			}
		}

		public Uri Url
		{
			get
			{
				return this.httpListenerRequest.Url;
			}
		}

		public string UserAgent
		{
			get
			{
				return this.httpListenerRequest.UserAgent;
			}
		}

		public string UserHostName
		{
			get
			{
				return this.httpListenerRequest.UserHostName;
			}
		}

		static HttpListenerRequestAdapter()
		{
			HttpListenerRequestAdapter.RequestIdPropertyInfo = typeof(HttpListenerRequest).GetProperty("RequestId", BindingFlags.Instance | BindingFlags.NonPublic, null, typeof(ulong), Type.EmptyTypes, null);
			if (HttpListenerRequestAdapter.RequestIdPropertyInfo == null)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Could not find propertyName 'RequestId' as part of the reflected properties");
			}
		}

		public HttpListenerRequestAdapter(HttpListenerRequest request)
		{
			this.httpListenerRequest = request;
		}

		public IAsyncResult BeginGetClientCertificate(AsyncCallback requestCallback, object state)
		{
			return this.httpListenerRequest.BeginGetClientCertificate(requestCallback, state);
		}

		public X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult)
		{
			return this.httpListenerRequest.EndGetClientCertificate(asyncResult);
		}

		public ulong GetRequestId()
		{
			if (HttpListenerRequestAdapter.RequestIdPropertyInfo == null)
			{
				throw new InvalidOperationException("Property info for 'RequestId' property not found in the reflected properties of type 'HttpListenerRequest'");
			}
			return (ulong)HttpListenerRequestAdapter.RequestIdPropertyInfo.GetValue(this.httpListenerRequest, null);
		}
	}
}