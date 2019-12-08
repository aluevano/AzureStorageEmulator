using AsyncHelper;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class StorageManagerException : Exception, IRethrowableException
	{
		private static string message;

		public int XStoreErrorCode
		{
			get;
			private set;
		}

		static StorageManagerException()
		{
			StorageManagerException.message = "The storage manager threw an exception.";
		}

		public StorageManagerException() : base(StorageManagerException.message)
		{
		}

		public StorageManagerException(int xstoreErrorCode) : base(string.Concat(StorageManagerException.message, " Error code: 0x", xstoreErrorCode.ToString("x")))
		{
			this.XStoreErrorCode = xstoreErrorCode;
		}

		public StorageManagerException(string message) : base(message)
		{
		}

		public StorageManagerException(string message, int xstoreErrorCode) : base(message)
		{
			this.XStoreErrorCode = xstoreErrorCode;
		}

		public StorageManagerException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public StorageManagerException(int xstoreErrorCode, Exception innerException) : base(string.Concat(StorageManagerException.message, " Error code: 0x", xstoreErrorCode.ToString("x")), innerException)
		{
			this.XStoreErrorCode = xstoreErrorCode;
		}

		public StorageManagerException(string message, int xstoreErrorCode, Exception innerException) : base(message, innerException)
		{
			this.XStoreErrorCode = xstoreErrorCode;
		}

		protected StorageManagerException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.XStoreErrorCode = (int)info.GetValue("XStoreErrorCode", typeof(int));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("XStoreErrorCode", this.XStoreErrorCode);
		}

		public virtual Exception GetRethrowableClone()
		{
			return new StorageManagerException(this.Message, this.XStoreErrorCode, this);
		}
	}
}