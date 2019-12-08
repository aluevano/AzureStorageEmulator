using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class ConditionHeaderPresentProtocolException : ProtocolException
	{
		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.ConditionHeadersNotSupported;
			}
		}

		public ConditionHeaderPresentProtocolException() : base(null)
		{
		}

		public ConditionHeaderPresentProtocolException(Exception innerException) : base("Condition headers are not supported", innerException)
		{
		}

		protected ConditionHeaderPresentProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new ConditionHeaderPresentProtocolException(this);
		}
	}
}