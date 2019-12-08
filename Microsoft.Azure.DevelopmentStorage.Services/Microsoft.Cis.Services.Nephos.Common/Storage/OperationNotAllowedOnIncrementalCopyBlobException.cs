using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class OperationNotAllowedOnIncrementalCopyBlobException : StorageManagerException
	{
		public OperationNotAllowedOnIncrementalCopyBlobException() : base("The specified operation is not allowed on an incremental copy blob.")
		{
		}

		public OperationNotAllowedOnIncrementalCopyBlobException(string message) : base(message)
		{
		}

		public OperationNotAllowedOnIncrementalCopyBlobException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected OperationNotAllowedOnIncrementalCopyBlobException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new OperationNotAllowedOnIncrementalCopyBlobException(this.Message, this);
		}
	}
}