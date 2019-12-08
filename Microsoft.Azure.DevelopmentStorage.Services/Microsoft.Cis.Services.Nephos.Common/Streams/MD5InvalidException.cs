using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	[Serializable]
	public class MD5InvalidException : Exception, IRethrowableException
	{
		public MD5InvalidException() : base("The Md5 hash value is invalid.")
		{
		}

		public MD5InvalidException(string message) : base(message)
		{
		}

		public MD5InvalidException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected MD5InvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new MD5InvalidException(this.Message, this);
		}
	}
}