using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	[Serializable]
	public class RequestTooLargeException : Exception, IRethrowableException
	{
		public RequestTooLargeException()
		{
		}

		public RequestTooLargeException(string message) : base(message)
		{
		}

		public RequestTooLargeException(Exception innerException) : base(string.Empty, innerException)
		{
		}

		public RequestTooLargeException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected RequestTooLargeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new RequestTooLargeException(this.Message, this);
		}
	}
}