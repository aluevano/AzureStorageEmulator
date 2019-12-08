using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlobPreviousSnapshotNotFoundException : StorageManagerException
	{
		public BlobPreviousSnapshotNotFoundException() : base("The previous snapshot is not found.")
		{
		}

		public BlobPreviousSnapshotNotFoundException(string message) : base(message)
		{
		}

		public BlobPreviousSnapshotNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlobPreviousSnapshotNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobPreviousSnapshotNotFoundException(this.Message, this);
		}
	}
}