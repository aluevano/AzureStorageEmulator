using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlobPreviousSnapshotGenerationIdNotFoundException : StorageManagerException
	{
		public BlobPreviousSnapshotGenerationIdNotFoundException() : base("Differential get page ranges is not supported on the previous snapshot.")
		{
		}

		public BlobPreviousSnapshotGenerationIdNotFoundException(string message) : base(message)
		{
		}

		public BlobPreviousSnapshotGenerationIdNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlobPreviousSnapshotGenerationIdNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobPreviousSnapshotGenerationIdNotFoundException(this.Message, this);
		}
	}
}