using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Table(Name="dbo.Page")]
	public class Page : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		private string _AccountName;

		private string _ContainerName;

		private string _BlobName;

		private DateTime _VersionTimestamp;

		private long _StartOffset;

		private long _EndOffset;

		private long? _FileOffset;

		private int _SnapshotCount;

		private EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.PageBlob> _PageBlob;

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
					if (this._PageBlob.HasLoadedOrAssignedValue)
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
					if (this._PageBlob.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._BlobName = value;
					this.SendPropertyChanged("BlobName");
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
					if (this._PageBlob.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._ContainerName = value;
					this.SendPropertyChanged("ContainerName");
				}
			}
		}

		[Column(Storage="_EndOffset", DbType="BIGINT", UpdateCheck=UpdateCheck.Never)]
		public long EndOffset
		{
			get
			{
				return this._EndOffset;
			}
			set
			{
				if (this._EndOffset != value)
				{
					this.SendPropertyChanging();
					this._EndOffset = value;
					this.SendPropertyChanged("EndOffset");
				}
			}
		}

		[Column(Storage="_FileOffset", DbType="BigInt", UpdateCheck=UpdateCheck.Never)]
		public long? FileOffset
		{
			get
			{
				return this._FileOffset;
			}
			set
			{
				long? nullable = this._FileOffset;
				long? nullable1 = value;
				if ((nullable.GetValueOrDefault() != nullable1.GetValueOrDefault() ? true : nullable.HasValue != nullable1.HasValue))
				{
					this.SendPropertyChanging();
					this._FileOffset = value;
					this.SendPropertyChanged("FileOffset");
				}
			}
		}

		[Association(Name="PageBlob_Page", Storage="_PageBlob", ThisKey="AccountName,ContainerName,BlobName,VersionTimestamp", OtherKey="AccountName,ContainerName,BlobName,VersionTimestamp", IsForeignKey=true)]
		public Microsoft.WindowsAzure.DevelopmentStorage.Store.PageBlob PageBlob
		{
			get
			{
				return this._PageBlob.Entity;
			}
			set
			{
				if (this._PageBlob.Entity != value)
				{
					this.SendPropertyChanging();
					this._PageBlob.Entity = value;
					this.SendPropertyChanged("PageBlob");
				}
			}
		}

		[Column(Storage="_SnapshotCount")]
		public int SnapshotCount
		{
			get
			{
				return this._SnapshotCount;
			}
			set
			{
				if (this._SnapshotCount != value)
				{
					this.SendPropertyChanging();
					this._SnapshotCount = value;
					this.SendPropertyChanged("SnapshotCount");
				}
			}
		}

		[Column(Storage="_StartOffset", DbType="BIGINT NOT NULL", IsPrimaryKey=true)]
		public long StartOffset
		{
			get
			{
				return this._StartOffset;
			}
			set
			{
				if (this._StartOffset != value)
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
					if (this._PageBlob.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._VersionTimestamp = value;
					this.SendPropertyChanged("VersionTimestamp");
				}
			}
		}

		static Page()
		{
			Page.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public Page()
		{
			this._PageBlob = new EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.PageBlob>();
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
				this.PropertyChanging(this, Page.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}