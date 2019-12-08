using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InvalidVersionForCopyAppendBlobOperationException : StorageManagerException
	{
		public InvalidVersionForCopyAppendBlobOperationException() : base("The copy blob operation on append blobs requires a different version")
		{
		}

		public InvalidVersionForCopyAppendBlobOperationException(string message) : base(message)
		{
		}

		public InvalidVersionForCopyAppendBlobOperationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidVersionForCopyAppendBlobOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidVersionForCopyAppendBlobOperationException(this.Message, this);
		}
	}
}