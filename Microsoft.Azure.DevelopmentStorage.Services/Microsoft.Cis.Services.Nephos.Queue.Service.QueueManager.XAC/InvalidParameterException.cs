using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	[Serializable]
	public class InvalidParameterException : QueueException, IRethrowableException
	{
		public InvalidParameterException()
		{
		}

		public InvalidParameterException(string message) : base(message)
		{
		}

		public InvalidParameterException(string message, Exception cause) : base(message, cause)
		{
		}

		protected InvalidParameterException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			throw new InvalidParameterException(this.Message, this);
		}
	}
}