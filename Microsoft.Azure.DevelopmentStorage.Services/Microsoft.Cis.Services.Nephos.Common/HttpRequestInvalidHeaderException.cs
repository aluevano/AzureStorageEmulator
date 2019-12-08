using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class HttpRequestInvalidHeaderException : Exception, IRethrowableException
	{
		public HttpRequestInvalidHeaderException()
		{
		}

		public HttpRequestInvalidHeaderException(string message) : base(message)
		{
		}

		public HttpRequestInvalidHeaderException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HttpRequestInvalidHeaderException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public virtual Exception GetRethrowableClone()
		{
			return new HttpRequestInvalidHeaderException(base.Message, this);
		}
	}
}