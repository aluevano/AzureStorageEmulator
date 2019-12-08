using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class VersionNotSupportedOnStorageStampException : StorageManagerException
	{
		public VersionNotSupportedOnStorageStampException() : base("The specified account is in a region which does not support requests for this version.")
		{
		}

		public VersionNotSupportedOnStorageStampException(string message) : base(message)
		{
		}

		public VersionNotSupportedOnStorageStampException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected VersionNotSupportedOnStorageStampException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new VersionNotSupportedOnStorageStampException(this.Message, this);
		}
	}
}