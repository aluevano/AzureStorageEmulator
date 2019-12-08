using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InvalidVersionForBlobTypeInBlobListException : StorageManagerException
	{
		public InvalidVersionForBlobTypeInBlobListException() : base("The type of a blob in the blob list is unrecognized by this version.")
		{
		}

		public InvalidVersionForBlobTypeInBlobListException(string message) : base(message)
		{
		}

		public InvalidVersionForBlobTypeInBlobListException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidVersionForBlobTypeInBlobListException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidVersionForBlobTypeInBlobListException(this.Message, this);
		}
	}
}