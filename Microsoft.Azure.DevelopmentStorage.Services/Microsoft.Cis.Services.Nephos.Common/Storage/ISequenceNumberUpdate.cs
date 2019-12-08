using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface ISequenceNumberUpdate
	{
		long SequenceNumber
		{
			get;
		}

		SequenceNumberUpdateType UpdateType
		{
			get;
		}
	}
}