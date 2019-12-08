using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class HttpRequestHeaderNotFoundException : Exception, IRethrowableException
	{
		public HttpRequestHeaderNotFoundException()
		{
		}

		public HttpRequestHeaderNotFoundException(string message) : base(message)
		{
		}

		public HttpRequestHeaderNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HttpRequestHeaderNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public virtual Exception GetRethrowableClone()
		{
			return new HttpRequestHeaderNotFoundException(base.Message, this);
		}
	}
}