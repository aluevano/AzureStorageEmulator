using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Table(Name="dbo.CommittedBlock")]
	public class CommittedBlock : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		private string _AccountName;

		private string _ContainerName;

		private string _BlobName;

		private DateTime _VersionTimestamp;

		private long _Offset;

		private string _BlockId;

		private long? _Length;

		private DateTime? _BlockVersion;

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

		[Association(Name="BlockBlob_CommittedBlock", Storage="_BlockBlob", ThisKey="AccountName,ContainerName,BlobName,VersionTimestamp", OtherKey="AccountName,ContainerName,BlobName,VersionTimestamp", IsForeignKey=true)]
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
						entity.CommittedBlocks.Remove(this);
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
						value.CommittedBlocks.Add(this);
						this._AccountName = value.AccountName;
						this._ContainerName = value.ContainerName;
						this._BlobName = value.BlobName;
						this._VersionTimestamp = value.VersionTimestamp;
					}
					this.SendPropertyChanged("BlockBlob");
				}
			}
		}

		[Column(Storage="_BlockId", DbType="VarChar(128) NOT NULL", CanBeNull=false, UpdateCheck=UpdateCheck.Never)]
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

		[Column(Storage="_BlockVersion", DbType="DATETIME", UpdateCheck=UpdateCheck.Never)]
		public DateTime? BlockVersion
		{
			get
			{
				return this._BlockVersion;
			}
			set
			{
				bool flag;
				DateTime? nullable = this._BlockVersion;
				DateTime? nullable1 = value;
				if (nullable.HasValue != nullable1.HasValue)
				{
					flag = true;
				}
				else
				{
					flag = (!nullable.HasValue ? false : nullable.GetValueOrDefault() != nullable1.GetValueOrDefault());
				}
				if (flag)
				{
					this.SendPropertyChanging();
					this._BlockVersion = value;
					this.SendPropertyChanged("BlockVersion");
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

		[Column(Storage="_Offset", DbType="BIGINT NOT NULL", IsPrimaryKey=true)]
		public long Offset
		{
			get
			{
				return this._Offset;
			}
			set
			{
				if (this._Offset != value)
				{
					this.SendPropertyChanging();
					this._Offset = value;
					this.SendPropertyChanged("Offset");
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

		static CommittedBlock()
		{
			CommittedBlock.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public CommittedBlock()
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
				this.PropertyChanging(this, CommittedBlock.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}