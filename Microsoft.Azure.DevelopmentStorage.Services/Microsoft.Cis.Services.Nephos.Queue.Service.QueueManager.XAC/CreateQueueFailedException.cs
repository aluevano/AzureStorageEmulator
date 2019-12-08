using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	[Serializable]
	public class CreateQueueFailedException : QueueException, IRethrowableException
	{
		public CreateQueueFailedException()
		{
		}

		public CreateQueueFailedException(string message) : base(message)
		{
		}

		public CreateQueueFailedException(string message, Exception cause) : base(message, cause)
		{
		}

		protected CreateQueueFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			throw new CreateQueueFailedException(this.Message, this);
		}
	}
}