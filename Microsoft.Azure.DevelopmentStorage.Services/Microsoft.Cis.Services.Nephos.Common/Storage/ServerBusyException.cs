using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class ServerBusyException : StorageManagerException
	{
		public bool ExpectedFailure
		{
			get;
			private set;
		}

		public ServerBusyException(bool expectedFailure) : base("The server is overloaded.")
		{
			this.ExpectedFailure = expectedFailure;
		}

		public ServerBusyException(string message, bool expectedFailure) : base(message)
		{
			this.ExpectedFailure = expectedFailure;
		}

		public ServerBusyException(string message, bool expectedFailure, Exception innerException) : base(message, innerException)
		{
			this.ExpectedFailure = expectedFailure;
		}

		protected ServerBusyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.ExpectedFailure = info.GetBoolean("this.ExpectedFailure");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.ExpectedFailure", this.ExpectedFailure);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new ServerBusyException(this.Message, this.ExpectedFailure, this);
		}
	}
}