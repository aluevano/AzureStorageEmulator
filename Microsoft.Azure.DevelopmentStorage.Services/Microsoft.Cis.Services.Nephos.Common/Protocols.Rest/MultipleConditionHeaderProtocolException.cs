using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class MultipleConditionHeaderProtocolException : ProtocolException
	{
		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.MultipleConditionHeadersNotSupported;
			}
		}

		public MultipleConditionHeaderProtocolException() : base(null)
		{
		}

		public MultipleConditionHeaderProtocolException(Exception innerException) : base("Multiple condition headers are not supported", innerException)
		{
		}

		protected MultipleConditionHeaderProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new MultipleConditionHeaderProtocolException(this);
		}
	}
}