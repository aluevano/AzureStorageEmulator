using System;
using System.IO;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class HttpListenerResponseAdapter : IHttpListenerResponse
	{
		private HttpListenerResponse httpListenerResponse;

		public long ContentLength64
		{
			get
			{
				return this.httpListenerResponse.ContentLength64;
			}
			set
			{
				this.httpListenerResponse.ContentLength64 = value;
			}
		}

		public string ContentType
		{
			get
			{
				return this.httpListenerResponse.ContentType;
			}
			set
			{
				this.httpListenerResponse.ContentType = value;
			}
		}

		public WebHeaderCollection Headers
		{
			get
			{
				return JustDecompileGenerated_get_Headers();
			}
			set
			{
				JustDecompileGenerated_set_Headers(value);
			}
		}

		public WebHeaderCollection JustDecompileGenerated_get_Headers()
		{
			return this.httpListenerResponse.Headers;
		}

		public void JustDecompileGenerated_set_Headers(WebHeaderCollection value)
		{
			throw new NotImplementedException();
		}

		public bool KeepAlive
		{
			get
			{
				return this.httpListenerResponse.KeepAlive;
			}
			set
			{
				this.httpListenerResponse.KeepAlive = value;
			}
		}

		public Stream OutputStream
		{
			get
			{
				return this.httpListenerResponse.OutputStream;
			}
		}

		public string RedirectLocation
		{
			get
			{
				return this.httpListenerResponse.RedirectLocation;
			}
			set
			{
				this.httpListenerResponse.RedirectLocation = value;
			}
		}

		public bool SendChunked
		{
			get
			{
				return this.httpListenerResponse.SendChunked;
			}
			set
			{
				this.httpListenerResponse.SendChunked = value;
			}
		}

		public int StatusCode
		{
			get
			{
				return this.httpListenerResponse.StatusCode;
			}
			set
			{
				this.httpListenerResponse.StatusCode = value;
			}
		}

		public string StatusDescription
		{
			get
			{
				return this.httpListenerResponse.StatusDescription;
			}
			set
			{
				this.httpListenerResponse.StatusDescription = value;
			}
		}

		public HttpListenerResponseAdapter(HttpListenerResponse response)
		{
			this.httpListenerResponse = response;
		}

		public void Abort()
		{
			this.httpListenerResponse.Abort();
		}

		public void AddHeader(string name, string value)
		{
			this.httpListenerResponse.AddHeader(name, value);
		}

		public void AppendHeader(string name, string value)
		{
			this.httpListenerResponse.AppendHeader(name, value);
		}

		public void Close()
		{
			this.httpListenerResponse.Close();
		}
	}
}