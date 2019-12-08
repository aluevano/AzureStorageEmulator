using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class SecondaryReadDisabledException : StorageManagerException
	{
		public SecondaryReadDisabledException() : base("Read operations are currently disabled.")
		{
		}

		public SecondaryReadDisabledException(string message) : base(message)
		{
		}

		public SecondaryReadDisabledException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected SecondaryReadDisabledException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new SecondaryReadDisabledException(this.Message, this);
		}
	}
}