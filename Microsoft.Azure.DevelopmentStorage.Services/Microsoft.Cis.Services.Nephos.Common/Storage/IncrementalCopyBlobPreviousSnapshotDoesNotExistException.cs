using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class IncrementalCopyBlobPreviousSnapshotDoesNotExistException : StorageManagerException
	{
		public IncrementalCopyBlobPreviousSnapshotDoesNotExistException() : base("The previously copied source snapshot does not exist.")
		{
		}

		public IncrementalCopyBlobPreviousSnapshotDoesNotExistException(string message) : base(message)
		{
		}

		public IncrementalCopyBlobPreviousSnapshotDoesNotExistException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected IncrementalCopyBlobPreviousSnapshotDoesNotExistException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new IncrementalCopyBlobPreviousSnapshotDoesNotExistException(this.Message, this);
		}
	}
}