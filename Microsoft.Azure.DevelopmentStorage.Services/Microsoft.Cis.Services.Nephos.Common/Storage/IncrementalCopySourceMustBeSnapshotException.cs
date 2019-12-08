using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class IncrementalCopySourceMustBeSnapshotException : StorageManagerException
	{
		public IncrementalCopySourceMustBeSnapshotException() : base("Copy source in incremental copy must be a snapshot.")
		{
		}

		public IncrementalCopySourceMustBeSnapshotException(string message) : base(message)
		{
		}

		public IncrementalCopySourceMustBeSnapshotException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected IncrementalCopySourceMustBeSnapshotException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new IncrementalCopySourceMustBeSnapshotException(this.Message, this);
		}
	}
}