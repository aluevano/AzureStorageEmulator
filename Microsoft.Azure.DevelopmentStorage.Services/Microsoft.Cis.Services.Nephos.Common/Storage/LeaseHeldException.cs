using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class LeaseHeldException : StorageManagerException
	{
		public LeaseHeldException() : base("There is currently a lease on the blob and the specified lease ID did not match.")
		{
		}

		public LeaseHeldException(string message) : base(message)
		{
		}

		public LeaseHeldException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected LeaseHeldException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new LeaseHeldException(this.Message, this);
		}
	}
}