using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class ComputeMd5HeaderUsedWithOtherCrc64Md5HeaderException : ProtocolException
	{
		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.ComputeMd5HeaderUsedWithOtherCrc64Md5Header;
			}
		}

		public ComputeMd5HeaderUsedWithOtherCrc64Md5HeaderException() : base(null)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new ComputeMd5HeaderUsedWithOtherCrc64Md5HeaderException();
		}
	}
}