using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public interface IHttpListenerRequest
	{
		string[] AcceptTypes
		{
			get;
		}

		Encoding ContentEncoding
		{
			get;
		}

		long ContentLength64
		{
			get;
		}

		string ContentType
		{
			get;
		}

		bool HasEntityBody
		{
			get;
		}

		NameValueCollection Headers
		{
			get;
		}

		string HttpMethod
		{
			get;
		}

		Stream InputStream
		{
			get;
		}

		bool IsSecureConnection
		{
			get;
		}

		Version ProtocolVersion
		{
			get;
		}

		string RawUrl
		{
			get;
		}

		IPEndPoint RemoteEndPoint
		{
			get;
		}

		Guid RequestTraceIdentifier
		{
			get;
		}

		Uri Url
		{
			get;
		}

		string UserAgent
		{
			get;
		}

		string UserHostName
		{
			get;
		}

		IAsyncResult BeginGetClientCertificate(AsyncCallback requestCallback, object state);

		X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult);

		ulong GetRequestId();
	}
}