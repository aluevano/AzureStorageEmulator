using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	[Serializable]
	public class InvalidStreamLengthException : Exception, IRethrowableException
	{
		public InvalidStreamLengthException() : base("The length of the stream is invalid.")
		{
		}

		public InvalidStreamLengthException(string message) : base(message)
		{
		}

		public InvalidStreamLengthException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidStreamLengthException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new InvalidStreamLengthException(this.Message, this);
		}
	}
}