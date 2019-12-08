using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	[Serializable]
	public class QueueNotEmptyException : QueueException, IRethrowableException
	{
		public QueueNotEmptyException()
		{
		}

		public QueueNotEmptyException(string message) : base(message)
		{
		}

		public QueueNotEmptyException(string message, Exception cause) : base(message, cause)
		{
		}

		protected QueueNotEmptyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			throw new QueueNotEmptyException(this.Message, this);
		}
	}
}