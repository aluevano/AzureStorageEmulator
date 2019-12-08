using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	[Serializable]
	public class NephosStorageException : Exception, IRethrowableException
	{
		public NephosStorageException()
		{
		}

		public NephosStorageException(string message) : base(message)
		{
		}

		public NephosStorageException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected NephosStorageException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public virtual Exception GetRethrowableClone()
		{
			return new NephosStorageException(this.Message, this);
		}
	}
}