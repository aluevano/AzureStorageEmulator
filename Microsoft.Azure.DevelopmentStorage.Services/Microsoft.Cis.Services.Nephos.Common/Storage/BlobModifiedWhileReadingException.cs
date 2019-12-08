using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlobModifiedWhileReadingException : StorageManagerException
	{
		public BlobModifiedWhileReadingException() : base("The blob is modified while being read.")
		{
		}

		public BlobModifiedWhileReadingException(string message) : base(message)
		{
		}

		public BlobModifiedWhileReadingException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlobModifiedWhileReadingException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobModifiedWhileReadingException(this.Message, this);
		}
	}
}