using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class LeaseLostException : StorageManagerException
	{
		public LeaseLostException() : base("The lease has been lost.")
		{
		}

		public LeaseLostException(string message) : base(message)
		{
		}

		public LeaseLostException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected LeaseLostException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new LeaseLostException(this.Message, this);
		}
	}
}