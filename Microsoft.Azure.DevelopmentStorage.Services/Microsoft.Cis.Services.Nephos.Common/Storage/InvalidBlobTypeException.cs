using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InvalidBlobTypeException : StorageManagerException
	{
		private bool isPageBlobInvalidVersion;

		public bool IsPageBlobInvalidVersion
		{
			get
			{
				return this.isPageBlobInvalidVersion;
			}
		}

		public InvalidBlobTypeException() : base("The blob type is invalid for this operation.")
		{
		}

		public InvalidBlobTypeException(string message) : base(message)
		{
		}

		public InvalidBlobTypeException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public InvalidBlobTypeException(string message, Exception innerException, bool isPageBlobInvalidVersion) : base(message, innerException)
		{
			this.isPageBlobInvalidVersion = isPageBlobInvalidVersion;
		}

		protected InvalidBlobTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.isPageBlobInvalidVersion = (bool)info.GetValue("IsPageBlobInvalidVersion", typeof(bool));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("IsPageBlobInvalidVersion", this.isPageBlobInvalidVersion);
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidBlobTypeException(this.Message, this, this.IsPageBlobInvalidVersion);
		}
	}
}