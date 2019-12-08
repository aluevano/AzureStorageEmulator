using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class MetadataFormatException : Exception, IRethrowableException
	{
		public MetadataFormatException() : base("There was an error encoding or decoding the metadata due to improper formatting.")
		{
		}

		public MetadataFormatException(string message) : base(message)
		{
		}

		public MetadataFormatException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected MetadataFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new MetadataFormatException(this.Message, base.InnerException);
		}
	}
}