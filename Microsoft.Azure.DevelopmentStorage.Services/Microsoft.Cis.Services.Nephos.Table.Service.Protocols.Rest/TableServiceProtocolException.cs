using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="This exception is required to be constructed with a status entry passed in. The only construcor allowed is one that takes the message and an inner exception is provided.")]
	public class TableServiceProtocolException : Exception, IRethrowableException
	{
		public NephosStatusEntry StatusEntry
		{
			get;
			private set;
		}

		public TableServiceProtocolException(NephosStatusEntry statusEntry) : base((statusEntry != null ? statusEntry.UserMessage : string.Empty))
		{
			if (statusEntry == null)
			{
				throw new ArgumentNullException("statusEntry");
			}
			this.StatusEntry = statusEntry;
		}

		public TableServiceProtocolException(NephosStatusEntry statusEntry, Exception innerException) : base((statusEntry != null ? statusEntry.UserMessage : string.Empty), innerException)
		{
			if (statusEntry == null)
			{
				throw new ArgumentNullException("statusEntry");
			}
			this.StatusEntry = statusEntry;
		}

		protected TableServiceProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.StatusEntry = (NephosStatusEntry)info.GetValue("this.StatusEntry", typeof(NephosStatusEntry));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.StatusEntry", this.StatusEntry);
			base.GetObjectData(info, context);
		}

		public Exception GetRethrowableClone()
		{
			return new TableServiceProtocolException(this.StatusEntry, this);
		}
	}
}