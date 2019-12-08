using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Table(Name="dbo.BlockData")]
	public class BlockData : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		private string _AccountName;

		private string _ContainerName;

		private string _BlobName;

		private DateTime _VersionTimestamp;

		private bool _IsCommitted;

		private string _BlockId;

		private long? _Length;

		private long? _StartOffset;

		private string _FilePath;

		private EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlockBlob> _BlockBlob;

		[Column(Storage="_AccountName", DbType="VarChar(24) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string AccountName
		{
			get
			{
				return this._AccountName;
			}
			set
			{
				if (this._AccountName != value)
				{
					if (this._BlockBlob.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._AccountName = value;
					this.SendPropertyChanged("AccountName");
				}
			}
		}

		[Column(Storage="_BlobName", DbType="NVarChar(256) COLLATE Latin1_General_BIN NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string BlobName
		{
			get
			{
				return this._BlobName;
			}
			set
			{
				if (this._BlobName != value)
				{
					if (this._BlockBlob.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._BlobName = value;
					this.SendPropertyChanged("BlobName");
				}
			}
		}

		[Association(Name="BlockBlob_BlockData", Storage="_BlockBlob", ThisKey="AccountName,ContainerName,BlobName,VersionTimestamp", OtherKey="AccountName,ContainerName,BlobName,VersionTimestamp", IsForeignKey=true)]
		public Microsoft.WindowsAzure.DevelopmentStorage.Store.BlockBlob BlockBlob
		{
			get
			{
				return this._BlockBlob.Entity;
			}
			set
			{
				Microsoft.WindowsAzure.DevelopmentStorage.Store.BlockBlob entity = this._BlockBlob.Entity;
				if (entity != value || !this._BlockBlob.HasLoadedOrAssignedValue)
				{
					this.SendPropertyChanging();
					if (entity != null)
					{
						this._BlockBlob.Entity = null;
						entity.BlocksData.Remove(this);
					}
					this._BlockBlob.Entity = value;
					if (value == null)
					{
						this._AccountName = null;
						this._ContainerName = null;
						this._BlobName = null;
						this._VersionTimestamp = new DateTime();
					}
					else
					{
						value.BlocksData.Add(this);
						this._AccountName = value.AccountName;
						this._ContainerName = value.ContainerName;
						this._BlobName = value.BlobName;
						this._VersionTimestamp = value.VersionTimestamp;
					}
					this.SendPropertyChanged("BlockBlob");
				}
			}
		}

		[Column(Storage="_BlockId", DbType="VarChar(128) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string BlockId
		{
			get
			{
				return this._BlockId;
			}
			set
			{
				if (this._BlockId != value)
				{
					this.SendPropertyChanging();
					this._BlockId = value;
					this.SendPropertyChanged("BlockId");
				}
			}
		}

		[Column(Storage="_ContainerName", DbType="VarChar(63) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string ContainerName
		{
			get
			{
				return this._ContainerName;
			}
			set
			{
				if (this._ContainerName != value)
				{
					if (this._BlockBlob.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._ContainerName = value;
					this.SendPropertyChanged("ContainerName");
				}
			}
		}

		[Column(Storage="_FilePath", DbType="nvarchar(260)")]
		public string FilePath
		{
			get
			{
				return this._FilePath;
			}
			set
			{
				if (this._FilePath != value)
				{
					this.SendPropertyChanging();
					this._FilePath = value;
					this.SendPropertyChanged("FilePath");
				}
			}
		}

		[Column(Storage="_IsCommitted", DbType="BIT NOT NULL", IsPrimaryKey=true)]
		public bool IsCommitted
		{
			get
			{
				return this._IsCommitted;
			}
			set
			{
				if (this._IsCommitted != value)
				{
					this.SendPropertyChanging();
					this._IsCommitted = value;
					this.SendPropertyChanged("IsCommitted");
				}
			}
		}

		[Column(Storage="_Length", DbType="BigInt", UpdateCheck=UpdateCheck.Never)]
		public long? Length
		{
			get
			{
				return this._Length;
			}
			set
			{
				long? nullable = this._Length;
				long? nullable1 = value;
				if ((nullable.GetValueOrDefault() != nullable1.GetValueOrDefault() ? true : nullable.HasValue != nullable1.HasValue))
				{
					this.SendPropertyChanging();
					this._Length = value;
					this.SendPropertyChanged("Length");
				}
			}
		}

		[Column(Storage="_StartOffset", DbType="BIGINT NOT NULL")]
		public long? StartOffset
		{
			get
			{
				return this._StartOffset;
			}
			set
			{
				long? nullable = this._StartOffset;
				long? nullable1 = value;
				if ((nullable.GetValueOrDefault() != nullable1.GetValueOrDefault() ? true : nullable.HasValue != nullable1.HasValue))
				{
					this.SendPropertyChanging();
					this._StartOffset = value;
					this.SendPropertyChanged("StartOffset");
				}
			}
		}

		[Column(Storage="_VersionTimestamp", DbType="DATETIME NOT NULL", IsPrimaryKey=true)]
		public DateTime VersionTimestamp
		{
			get
			{
				return this._VersionTimestamp;
			}
			set
			{
				if (this._VersionTimestamp != value)
				{
					if (this._BlockBlob.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._VersionTimestamp = value;
					this.SendPropertyChanged("VersionTimestamp");
				}
			}
		}

		static BlockData()
		{
			BlockData.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public BlockData()
		{
			this._BlockBlob = new EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlockBlob>();
		}

		protected virtual void SendPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected virtual void SendPropertyChanging()
		{
			if (this.PropertyChanging != null)
			{
				this.PropertyChanging(this, BlockData.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}