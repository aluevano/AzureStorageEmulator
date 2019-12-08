using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class HttpRequestDuplicateHeaderException : Exception, IRethrowableException
	{
		public HttpRequestDuplicateHeaderException()
		{
		}

		public HttpRequestDuplicateHeaderException(string message) : base(message)
		{
		}

		public HttpRequestDuplicateHeaderException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HttpRequestDuplicateHeaderException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public virtual Exception GetRethrowableClone()
		{
			return new HttpRequestDuplicateHeaderException(base.Message, this);
		}
	}
}