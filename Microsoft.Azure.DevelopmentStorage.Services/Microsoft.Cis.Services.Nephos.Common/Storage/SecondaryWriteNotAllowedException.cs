using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class SecondaryWriteNotAllowedException : StorageManagerException
	{
		public SecondaryWriteNotAllowedException() : base("Write operations are not allowed.")
		{
		}

		public SecondaryWriteNotAllowedException(string message) : base(message)
		{
		}

		public SecondaryWriteNotAllowedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected SecondaryWriteNotAllowedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new SecondaryWriteNotAllowedException(this.Message, this);
		}
	}
}