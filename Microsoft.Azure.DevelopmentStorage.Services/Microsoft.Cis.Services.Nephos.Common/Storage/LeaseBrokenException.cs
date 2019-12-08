using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class LeaseBrokenException : StorageManagerException
	{
		public LeaseBrokenException() : base("The lease has been broken.")
		{
		}

		public LeaseBrokenException(string message) : base(message)
		{
		}

		public LeaseBrokenException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected LeaseBrokenException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new LeaseBrokenException(this.Message, this);
		}
	}
}