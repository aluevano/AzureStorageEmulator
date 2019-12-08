using System;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[Serializable]
	public class SemaphoreQueueExhaustedException : Exception, IRethrowableException
	{
		public SemaphoreQueueExhaustedException() : base("There are too many pending requests")
		{
		}

		public SemaphoreQueueExhaustedException(Exception innerException) : base("There are too many pending requests", innerException)
		{
		}

		public SemaphoreQueueExhaustedException(string message) : base(message)
		{
		}

		public SemaphoreQueueExhaustedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected SemaphoreQueueExhaustedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new SemaphoreQueueExhaustedException(this.Message, this);
		}
	}
}