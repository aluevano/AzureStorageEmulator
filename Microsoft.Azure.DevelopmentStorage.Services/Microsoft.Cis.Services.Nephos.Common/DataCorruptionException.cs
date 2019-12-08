using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class DataCorruptionException : Exception, IRethrowableException
	{
		public DataCorruptionException()
		{
		}

		public DataCorruptionException(string message) : base(message)
		{
		}

		public DataCorruptionException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected DataCorruptionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public virtual Exception GetRethrowableClone()
		{
			return new DataCorruptionException(base.Message, this);
		}
	}
}