using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IBlobProperties : IPutBlobProperties
	{
		int? CommittedBlockCount
		{
			get;
		}

		long ContentLength
		{
			get;
		}

		DateTime? CopyCompletionTime
		{
			get;
		}

		string CopyId
		{
			get;
		}

		string CopyProgress
		{
			get;
		}

		string CopySource
		{
			get;
		}

		string CopyStatus
		{
			get;
		}

		string CopyStatusDescription
		{
			get;
		}

		bool? IsBlobEncrypted
		{
			get;
		}

		bool IsIncrementalCopy
		{
			get;
		}

		DateTime? LastCopySnapshot
		{
			get;
		}

		long? SequenceNumber
		{
			get;
		}
	}
}