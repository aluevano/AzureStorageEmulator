using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class HostInformationNotPresentProtocolException : ProtocolException
	{
		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.HostInformationNotPresent;
			}
		}

		public HostInformationNotPresentProtocolException() : base(CommonStatusEntries.HostInformationNotPresent.UserMessage)
		{
		}

		public HostInformationNotPresentProtocolException(string message) : base(message)
		{
		}

		protected HostInformationNotPresentProtocolException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected HostInformationNotPresentProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new HostInformationNotPresentProtocolException(this.Message, this);
		}
	}
}