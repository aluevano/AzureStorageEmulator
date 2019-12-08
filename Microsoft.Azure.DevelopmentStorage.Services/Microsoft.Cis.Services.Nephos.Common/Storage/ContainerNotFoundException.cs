using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class ContainerNotFoundException : StorageManagerException
	{
		public ContainerNotFoundException() : base("The container does not exist.")
		{
		}

		public ContainerNotFoundException(string message) : base(message)
		{
		}

		public ContainerNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ContainerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new ContainerNotFoundException(this.Message, this);
		}
	}
}