using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class CopyIdMismatchException : StorageManagerException
	{
		public CopyIdMismatchException() : base("The copy ID did not match the copy ID of the pending copy operation.")
		{
		}

		public CopyIdMismatchException(string message) : base(message)
		{
		}

		public CopyIdMismatchException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected CopyIdMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new CopyIdMismatchException(this.Message, this);
		}
	}
}