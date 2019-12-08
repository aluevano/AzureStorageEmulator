using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public class BlockBlob : Blob
	{
		private bool? _IsCommitted;

		private bool? _HasBlock;

		private int? _UncommittedBlockIdLength;

		private string _DirectoryPath;

		private EntitySet<BlockData> _BlocksData;

		private EntitySet<CommittedBlock> _CommittedBlocks;

		[Association(Name="BlockBlob_BlockData", Storage="_BlocksData", ThisKey="AccountName,ContainerName,BlobName,VersionTimestamp", OtherKey="AccountName,ContainerName,BlobName,VersionTimestamp")]
		public EntitySet<BlockData> BlocksData
		{
			get
			{
				return this._BlocksData;
			}
			set
			{
				this._BlocksData.Assign(value);
			}
		}

		[Association(Name="BlockBlob_CommittedBlock", Storage="_CommittedBlocks", ThisKey="AccountName,ContainerName,BlobName,VersionTimestamp", OtherKey="AccountName,ContainerName,BlobName,VersionTimestamp")]
		public EntitySet<CommittedBlock> CommittedBlocks
		{
			get
			{
				return this._CommittedBlocks;
			}
			set
			{
				this._CommittedBlocks.Assign(value);
			}
		}

		[Column(Storage="_DirectoryPath", DbType="nvarchar(260)", CanBeNull=false, UpdateCheck=UpdateCheck.Never)]
		public string DirectoryPath
		{
			get
			{
				return this._DirectoryPath;
			}
			set
			{
				if (this._DirectoryPath != value)
				{
					this.SendPropertyChanging();
					this._DirectoryPath = value;
					this.SendPropertyChanged("DirectoryPath");
				}
			}
		}

		[Column(Storage="_HasBlock", DbType="Bit", UpdateCheck=UpdateCheck.Never)]
		public bool? HasBlock
		{
			get
			{
				return this._HasBlock;
			}
			set
			{
				bool? nullable = this._HasBlock;
				bool? nullable1 = value;
				if ((nullable.GetValueOrDefault() != nullable1.GetValueOrDefault() ? true : nullable.HasValue != nullable1.HasValue))
				{
					this.SendPropertyChanging();
					this._HasBlock = value;
					this.SendPropertyChanged("HasBlock");
				}
			}
		}

		[Column(Storage="_IsCommitted", DbType="Bit", UpdateCheck=UpdateCheck.Never)]
		public bool? IsCommitted
		{
			get
			{
				return this._IsCommitted;
			}
			set
			{
				bool? nullable = this._IsCommitted;
				bool? nullable1 = value;
				if ((nullable.GetValueOrDefault() != nullable1.GetValueOrDefault() ? true : nullable.HasValue != nullable1.HasValue))
				{
					this.SendPropertyChanging();
					this._IsCommitted = value;
					this.SendPropertyChanged("IsCommitted");
				}
			}
		}

		[Column(Storage="_UncommittedBlockIdLength", DbType="INT DEFAULT 0", UpdateCheck=UpdateCheck.Never)]
		public int? UncommittedBlockIdLength
		{
			get
			{
				return this._UncommittedBlockIdLength;
			}
			set
			{
				int? nullable = this._UncommittedBlockIdLength;
				int? nullable1 = value;
				if ((nullable.GetValueOrDefault() != nullable1.GetValueOrDefault() ? true : nullable.HasValue != nullable1.HasValue))
				{
					this.SendPropertyChanging();
					this._UncommittedBlockIdLength = value;
					this.SendPropertyChanged("UncommittedBlockIdLength");
				}
			}
		}

		public BlockBlob()
		{
			this._BlocksData = new EntitySet<BlockData>(new Action<BlockData>(this.attach_BlocksData), new Action<BlockData>(this.detach_BlocksData));
			this._CommittedBlocks = new EntitySet<CommittedBlock>(new Action<CommittedBlock>(this.attach_CommittedBlocks), new Action<CommittedBlock>(this.detach_CommittedBlocks));
			this.OnCreated();
		}

		private void attach_BlocksData(BlockData entity)
		{
			this.SendPropertyChanging();
			entity.BlockBlob = this;
		}

		private void attach_CommittedBlocks(CommittedBlock entity)
		{
			this.SendPropertyChanging();
			entity.BlockBlob = this;
		}

		private void detach_BlocksData(BlockData entity)
		{
			this.SendPropertyChanging();
			entity.BlockBlob = null;
		}

		private void detach_CommittedBlocks(CommittedBlock entity)
		{
			this.SendPropertyChanging();
			entity.BlockBlob = null;
		}

		private void OnCreated()
		{
			base.BlobType = Microsoft.Cis.Services.Nephos.Common.Storage.BlobType.ListBlob;
		}
	}
}