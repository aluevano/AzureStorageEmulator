using System;
using System.IO;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public interface IHttpListenerResponse
	{
		long ContentLength64
		{
			get;
			set;
		}

		string ContentType
		{
			get;
			set;
		}

		WebHeaderCollection Headers
		{
			get;
		}

		bool KeepAlive
		{
			get;
			set;
		}

		Stream OutputStream
		{
			get;
		}

		string RedirectLocation
		{
			get;
			set;
		}

		bool SendChunked
		{
			get;
			set;
		}

		int StatusCode
		{
			get;
			set;
		}

		string StatusDescription
		{
			get;
			set;
		}

		void Abort();

		void AddHeader(string name, string value);

		void AppendHeader(string name, string value);

		void Close();
	}
}