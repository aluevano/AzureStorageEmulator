using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InvalidMarkerException : StorageManagerException
	{
		public InvalidMarkerException() : base("The marker passed in is invalid.")
		{
		}

		public InvalidMarkerException(string message) : base(message)
		{
		}

		public InvalidMarkerException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidMarkerException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidMarkerException(this.Message, this);
		}
	}
}