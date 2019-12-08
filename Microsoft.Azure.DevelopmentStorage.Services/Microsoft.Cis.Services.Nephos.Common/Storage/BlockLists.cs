using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class BlockLists : IBlockLists
	{
		public DateTime BlobLastModificationTime
		{
			get;
			set;
		}

		public long BlobSize
		{
			get;
			set;
		}

		public IBlockCollection CommittedBlockList
		{
			get
			{
				return JustDecompileGenerated_get_CommittedBlockList();
			}
			set
			{
				JustDecompileGenerated_set_CommittedBlockList(value);
			}
		}

		private IBlockCollection JustDecompileGenerated_CommittedBlockList_k__BackingField;

		public IBlockCollection JustDecompileGenerated_get_CommittedBlockList()
		{
			return this.JustDecompileGenerated_CommittedBlockList_k__BackingField;
		}

		public void JustDecompileGenerated_set_CommittedBlockList(IBlockCollection value)
		{
			this.JustDecompileGenerated_CommittedBlockList_k__BackingField = value;
		}

		public IBlockCollection UncommittedBlockList
		{
			get
			{
				return JustDecompileGenerated_get_UncommittedBlockList();
			}
			set
			{
				JustDecompileGenerated_set_UncommittedBlockList(value);
			}
		}

		private IBlockCollection JustDecompileGenerated_UncommittedBlockList_k__BackingField;

		public IBlockCollection JustDecompileGenerated_get_UncommittedBlockList()
		{
			return this.JustDecompileGenerated_UncommittedBlockList_k__BackingField;
		}

		public void JustDecompileGenerated_set_UncommittedBlockList(IBlockCollection value)
		{
			this.JustDecompileGenerated_UncommittedBlockList_k__BackingField = value;
		}

		public BlockLists()
		{
		}
	}
}