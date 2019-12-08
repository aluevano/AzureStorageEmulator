using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlockLists
	{
		DateTime BlobLastModificationTime
		{
			get;
			set;
		}

		long BlobSize
		{
			get;
			set;
		}

		IBlockCollection CommittedBlockList
		{
			get;
		}

		IBlockCollection UncommittedBlockList
		{
			get;
		}
	}
}