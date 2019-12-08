using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class CopySourceCannotBeIncrementalCopyBlobException : StorageManagerException
	{
		public CopySourceCannotBeIncrementalCopyBlobException() : base("Source blob of a copy operation cannot be non-snapshot incremental copy blob.")
		{
		}

		public CopySourceCannotBeIncrementalCopyBlobException(string message) : base(message)
		{
		}

		public CopySourceCannotBeIncrementalCopyBlobException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected CopySourceCannotBeIncrementalCopyBlobException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new CopySourceCannotBeIncrementalCopyBlobException(this.Message, this);
		}
	}
}