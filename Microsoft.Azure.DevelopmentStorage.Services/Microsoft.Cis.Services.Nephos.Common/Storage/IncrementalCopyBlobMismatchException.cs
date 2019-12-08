using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class IncrementalCopyBlobMismatchException : StorageManagerException
	{
		public IncrementalCopyBlobMismatchException() : base("The specified source blob is different than the copy source of the existing incremental copy blob.")
		{
		}

		public IncrementalCopyBlobMismatchException(string message) : base(message)
		{
		}

		public IncrementalCopyBlobMismatchException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected IncrementalCopyBlobMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new IncrementalCopyBlobMismatchException(this.Message, this);
		}
	}
}