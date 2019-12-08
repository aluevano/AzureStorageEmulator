using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class BothCrc64AndMsMd5HeaderPresentException : ProtocolException
	{
		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.BothCrc64AndMsMd5HeaderPresent;
			}
		}

		public BothCrc64AndMsMd5HeaderPresentException() : base(null)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BothCrc64AndMsMd5HeaderPresentException();
		}
	}
}