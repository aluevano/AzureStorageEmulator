using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class BothCrc64AndMd5HeaderPresentException : ProtocolException
	{
		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.BothCrc64AndMd5HeaderPresent;
			}
		}

		public BothCrc64AndMd5HeaderPresentException() : base(null)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BothCrc64AndMd5HeaderPresentException();
		}
	}
}