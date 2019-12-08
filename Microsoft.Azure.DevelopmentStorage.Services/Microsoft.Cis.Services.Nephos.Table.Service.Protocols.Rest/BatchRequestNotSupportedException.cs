using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	[Serializable]
	internal class BatchRequestNotSupportedException : Exception, IRethrowableException
	{
		public BatchRequestNotSupportedException()
		{
		}

		public BatchRequestNotSupportedException(string message) : base(message)
		{
		}

		public BatchRequestNotSupportedException(Exception innerException) : base(string.Empty, innerException)
		{
		}

		public BatchRequestNotSupportedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BatchRequestNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new BatchRequestNotSupportedException(this.Message, this);
		}
	}
}