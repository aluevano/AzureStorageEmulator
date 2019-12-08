using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class LeaseDurationNotInfiniteException : StorageManagerException
	{
		public LeaseDurationNotInfiniteException() : base("The lease duration must be infinite.")
		{
		}

		public LeaseDurationNotInfiniteException(string message) : base(message)
		{
		}

		public LeaseDurationNotInfiniteException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected LeaseDurationNotInfiniteException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new LeaseDurationNotInfiniteException(this.Message, this);
		}
	}
}