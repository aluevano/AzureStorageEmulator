using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InvalidGeoInformationException : StorageManagerException
	{
		public InvalidGeoInformationException() : base("The specified geo region and/or geo domain information is not valid.")
		{
		}

		public InvalidGeoInformationException(string message) : base(message)
		{
		}

		public InvalidGeoInformationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidGeoInformationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidGeoInformationException(this.Message, this);
		}
	}
}