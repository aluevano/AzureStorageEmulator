using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class InvalidUrlException : Exception, IRethrowableException
	{
		public InvalidUrlException() : base("HttpListenerRequest.Url did not return a valid url.")
		{
		}

		public InvalidUrlException(string message) : base(message)
		{
		}

		public InvalidUrlException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidUrlException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new InvalidUrlException(this.Message, base.InnerException);
		}
	}
}