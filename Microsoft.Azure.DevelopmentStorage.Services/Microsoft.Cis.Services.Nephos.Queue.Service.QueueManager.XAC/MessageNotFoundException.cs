using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	[Serializable]
	public class MessageNotFoundException : QueueException, IRethrowableException
	{
		public MessageNotFoundException()
		{
		}

		public MessageNotFoundException(string message) : base(message)
		{
		}

		public MessageNotFoundException(string message, Exception cause) : base(message, cause)
		{
		}

		protected MessageNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			throw new MessageNotFoundException(this.Message, this);
		}
	}
}