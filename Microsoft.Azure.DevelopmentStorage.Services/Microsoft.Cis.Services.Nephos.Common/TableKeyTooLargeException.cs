using AsyncHelper;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class TableKeyTooLargeException : Exception, IRethrowableException
	{
		public Microsoft.Cis.Services.Nephos.Common.KeyType KeyType
		{
			get;
			private set;
		}

		public TableKeyTooLargeException(Microsoft.Cis.Services.Nephos.Common.KeyType keyType)
		{
			this.KeyType = keyType;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("KeyType", this.KeyType);
		}

		public Exception GetRethrowableClone()
		{
			return new TableKeyTooLargeException(this.KeyType);
		}
	}
}