using AsyncHelper;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	[Serializable]
	public class CannotCopyAcrossAccountsException : Exception, IRethrowableException
	{
		public string DestinationAccountName
		{
			get;
			private set;
		}

		public string SourceAccountName
		{
			get;
			private set;
		}

		public CannotCopyAcrossAccountsException(string sourceAccountName, string destinationAccountName) : base("Copying blobs across accounts are not supported")
		{
			this.SourceAccountName = sourceAccountName;
			this.DestinationAccountName = destinationAccountName;
		}

		public CannotCopyAcrossAccountsException(string message, CannotCopyAcrossAccountsException innerException) : base(message, innerException)
		{
			if (innerException == null)
			{
				throw new ArgumentNullException("innerException");
			}
			this.SourceAccountName = innerException.SourceAccountName;
			this.DestinationAccountName = innerException.DestinationAccountName;
		}

		protected CannotCopyAcrossAccountsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.SourceAccountName = info.GetString("this.SourceAccountName");
			this.DestinationAccountName = info.GetString("this.DestinationAccountName");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.SourceAccountName", this.SourceAccountName);
			info.AddValue("this.DestinationAccountName", this.DestinationAccountName);
			base.GetObjectData(info, context);
		}

		public virtual Exception GetRethrowableClone()
		{
			return new CannotCopyAcrossAccountsException(this.Message, this);
		}
	}
}