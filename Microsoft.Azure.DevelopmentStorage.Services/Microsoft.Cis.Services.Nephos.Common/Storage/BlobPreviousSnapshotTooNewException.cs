using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlobPreviousSnapshotTooNewException : StorageManagerException
	{
		public BlobPreviousSnapshotTooNewException() : base("The previous snapshot cannot be newer than the root blob.")
		{
		}

		public BlobPreviousSnapshotTooNewException(string message) : base(message)
		{
		}

		public BlobPreviousSnapshotTooNewException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlobPreviousSnapshotTooNewException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobPreviousSnapshotTooNewException(this.Message, this);
		}
	}
}