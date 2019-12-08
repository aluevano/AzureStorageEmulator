using Microsoft.Data.OData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	internal class ResponseMessage : IODataResponseMessage
	{
		private readonly Stream stream;

		private readonly Dictionary<string, string> headers = new Dictionary<string, string>();

		public IEnumerable<KeyValuePair<string, string>> Headers
		{
			get
			{
				return this.headers;
			}
		}

		public int StatusCode
		{
			get;
			set;
		}

		public ResponseMessage(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
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
			this.headers.Add(headerName, headerValue);
		}
	}
}