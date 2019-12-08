using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InvalidBlockException : StorageManagerException
	{
		public InvalidBlockException() : base("Specified block is invalid.")
		{
		}

		public InvalidBlockException(string message) : base(message)
		{
		}

		public InvalidBlockException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidBlockException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidBlockException(this.Message, this);
		}
	}
}