using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	[Serializable]
	public abstract class QueueException : Exception
	{
		protected QueueException()
		{
		}

		protected QueueException(string message) : base(message)
		{
		}

		protected QueueException(string message, Exception cause) : base(message, cause)
		{
		}

		protected QueueException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}