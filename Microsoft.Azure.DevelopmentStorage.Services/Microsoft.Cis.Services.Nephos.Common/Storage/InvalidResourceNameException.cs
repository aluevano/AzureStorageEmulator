using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InvalidResourceNameException : StorageManagerException
	{
		public InvalidResourceNameException() : base("The resource name specified contains invalid characters.")
		{
		}

		public InvalidResourceNameException(string message) : base(message)
		{
		}

		public InvalidResourceNameException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidResourceNameException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidResourceNameException(this.Message, this);
		}
	}
}