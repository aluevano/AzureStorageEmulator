using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default construcor taking no arguments is explicitly omitted since the content length must be provided.")]
	public class InvalidBlobRegionException : StorageManagerException
	{
		private long? blobContentLength;

		public long? BlobContentLength
		{
			get
			{
				return this.blobContentLength;
			}
		}

		public InvalidBlobRegionException(long? contentLength) : this(contentLength, "Specified blob region is invalid.")
		{
		}

		public InvalidBlobRegionException(long? contentLength, string message) : this(contentLength, message, null)
		{
		}

		public InvalidBlobRegionException(long? contentLength, string message, Exception innerException) : base(message, innerException)
		{
			this.blobContentLength = contentLength;
		}

		protected InvalidBlobRegionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.blobContentLength = (long?)info.GetValue("this.blobContentLength", typeof(long?));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.blobContentLength", this.blobContentLength);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidBlobRegionException(this.BlobContentLength, this.Message, this);
		}
	}
}