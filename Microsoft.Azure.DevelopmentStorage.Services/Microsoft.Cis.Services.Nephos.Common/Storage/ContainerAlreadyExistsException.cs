using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class ContainerAlreadyExistsException : StorageManagerException
	{
		public ContainerAlreadyExistsException() : base("The container already exists.")
		{
		}

		public ContainerAlreadyExistsException(string message) : base(message)
		{
		}

		public ContainerAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ContainerAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new ContainerAlreadyExistsException(this.Message, this);
		}
	}
}