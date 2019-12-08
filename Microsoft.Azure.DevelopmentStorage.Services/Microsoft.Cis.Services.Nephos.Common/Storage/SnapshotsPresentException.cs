using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class SnapshotsPresentException : StorageManagerException
	{
		public SnapshotsPresentException() : base("This operation is not permitted because the blob has snapshots.")
		{
		}

		public SnapshotsPresentException(string message) : base(message)
		{
		}

		public SnapshotsPresentException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected SnapshotsPresentException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new SnapshotsPresentException(this.Message, this);
		}
	}
}