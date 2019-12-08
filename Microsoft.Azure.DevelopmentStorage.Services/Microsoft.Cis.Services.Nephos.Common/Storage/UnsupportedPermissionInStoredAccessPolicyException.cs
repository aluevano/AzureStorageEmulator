using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class UnsupportedPermissionInStoredAccessPolicyException : StorageManagerException
	{
		public UnsupportedPermissionInStoredAccessPolicyException() : base("Stored access policy contains a permission that is not supported by this version.")
		{
		}

		public UnsupportedPermissionInStoredAccessPolicyException(string message) : base(message)
		{
		}

		public UnsupportedPermissionInStoredAccessPolicyException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected UnsupportedPermissionInStoredAccessPolicyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new UnsupportedPermissionInStoredAccessPolicyException(this.Message, this);
		}
	}
}