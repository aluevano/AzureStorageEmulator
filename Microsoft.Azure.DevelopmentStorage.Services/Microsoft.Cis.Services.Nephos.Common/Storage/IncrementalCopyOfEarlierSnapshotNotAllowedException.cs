using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class IncrementalCopyOfEarlierSnapshotNotAllowedException : StorageManagerException
	{
		public IncrementalCopyOfEarlierSnapshotNotAllowedException() : base("The specified snapshot is earlier than the last snapshot copied into the incremental copy blob.")
		{
		}

		public IncrementalCopyOfEarlierSnapshotNotAllowedException(string message) : base(message)
		{
		}

		public IncrementalCopyOfEarlierSnapshotNotAllowedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected IncrementalCopyOfEarlierSnapshotNotAllowedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new IncrementalCopyOfEarlierSnapshotNotAllowedException(this.Message, this);
		}
	}
}