using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InvalidBlockListException : StorageManagerException
	{
		public InvalidBlockListException() : base("Specified block list is invalid.")
		{
		}

		public InvalidBlockListException(string message) : base(message)
		{
		}

		public InvalidBlockListException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidBlockListException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidBlockListException(this.Message, this);
		}
	}
}