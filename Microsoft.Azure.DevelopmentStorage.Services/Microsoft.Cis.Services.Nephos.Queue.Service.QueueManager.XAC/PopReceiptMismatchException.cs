using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	[Serializable]
	public class PopReceiptMismatchException : QueueException, IRethrowableException
	{
		public PopReceiptMismatchException()
		{
		}

		public PopReceiptMismatchException(string message) : base(message)
		{
		}

		public PopReceiptMismatchException(string message, Exception cause) : base(message, cause)
		{
		}

		protected PopReceiptMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			throw new PopReceiptMismatchException(this.Message, this);
		}
	}
}