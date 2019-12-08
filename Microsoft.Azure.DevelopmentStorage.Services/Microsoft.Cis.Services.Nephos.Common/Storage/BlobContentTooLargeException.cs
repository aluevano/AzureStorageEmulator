using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default construcor taking no arguments is explicitly omitted since the maximum limit must be provided.")]
	public class BlobContentTooLargeException : StorageManagerException
	{
		private long? maxAllowedSize;

		public long? MaxAllowedSize
		{
			get
			{
				return this.maxAllowedSize;
			}
		}

		public BlobContentTooLargeException(long? maxAllowedSize) : base("Content specified for the blob is too large.")
		{
			this.maxAllowedSize = maxAllowedSize;
		}

		public BlobContentTooLargeException(long? maxAllowedSize, string message, Exception innerException) : base(message, innerException)
		{
			this.maxAllowedSize = maxAllowedSize;
		}

		protected BlobContentTooLargeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.maxAllowedSize = (long?)info.GetValue("this.maxAllowedSize", typeof(long?));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.maxAllowedSize", this.maxAllowedSize);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobContentTooLargeException(this.maxAllowedSize, this.Message, this);
		}
	}
}