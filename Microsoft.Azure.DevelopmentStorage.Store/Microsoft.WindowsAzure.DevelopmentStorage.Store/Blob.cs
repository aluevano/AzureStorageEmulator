using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[InheritanceMapping(Code="1", Type=typeof(BlockBlob), IsDefault=true)]
	[InheritanceMapping(Code="2", Type=typeof(PageBlob))]
	[Table(Name="dbo.Blob")]
	public abstract class Blob : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		protected string _AccountName;

		protected string _ContainerName;

		protected string _BlobName;

		protected DateTime _VersionTimestamp;

		private int _BlobType;

		private DateTime _CreationTime;

		private DateTime? _LastModificationTime;

		private long _ContentLength;

		private string _ContentType;

		private long? _ContentCrc64;

		private byte[] _ContentMD5;

		private byte[] _ServiceMetadata;

		private byte[] _Metadata;

		private Guid? _LeaseId;

		private TimeSpan _LeaseDuration;

		private DateTime? _LeaseEndTime;

		private long? _SequenceNumber;

		private int _LeaseState;

		private bool _IsLeaseOp;

		private int _SnapshotCount;

		private string _GenerationId;

		private EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer> _BlobContainer;

		[Column(Storage="_AccountName", DbType="VarChar(24) NOT NULL", CanBeNull=false, IsPrimaryKey=true, UpdateCheck=UpdateCheck.Never)]
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
					if (this._BlobContainer.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._AccountName = value;
					this.SendPropertyChanged("AccountName");
				}
			}
		}

		[Association(Name="BlobContainer_Blob", Storage="_BlobContainer", ThisKey="AccountName,ContainerName", OtherKey="AccountName,ContainerName", IsForeignKey=true)]
		public Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer BlobContainer
		{
			get
			{
				return this._BlobContainer.Entity;
			}
			set
			{
				Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer entity = this._BlobContainer.Entity;
				if (entity != value || !this._BlobContainer.HasLoadedOrAssignedValue)
				{
					this.SendPropertyChanging();
					if (entity != null)
					{
						this._BlobContainer.Entity = null;
						entity.BaseBlobs.Remove(this);
					}
					this._BlobContainer.Entity = value;
					if (value == null)
					{
						this._AccountName = null;
						this._ContainerName = null;
					}
					else
					{
						value.BaseBlobs.Add(this);
						this._AccountName = value.AccountName;
						this._ContainerName = value.ContainerName;
					}
					this.SendPropertyChanged("BlobContainer");
				}
			}
		}

		[Column(Storage="_BlobName", DbType="NVARCHAR(256) COLLATE Latin1_General_BIN NOT NULL", CanBeNull=false, IsPrimaryKey=true, UpdateCheck=UpdateCheck.Never)]
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
					this.SendPropertyChanging();
					this._BlobName = value;
					this.SendPropertyChanged("BlobName");
				}
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.Storage.BlobType BlobType
		{
			get
			{
				return (Microsoft.Cis.Services.Nephos.Common.Storage.BlobType)this.BlobTypeInt;
			}
			set
			{
				this.BlobTypeInt = (int)value;
			}
		}

		[Column(Name="BlobType", Storage="_BlobType", DbType="INT", UpdateCheck=UpdateCheck.Never, IsDiscriminator=true)]
		public int BlobTypeInt
		{
			get
			{
				return this._BlobType;
			}
			set
			{
				if (this._BlobType != value)
				{
					this.SendPropertyChanging();
					this._BlobType = value;
					this.SendPropertyChanged("BlobTypeInt");
				}
			}
		}

		[Column(Storage="_ContainerName", DbType="VarChar(63) NOT NULL", CanBeNull=false, IsPrimaryKey=true, UpdateCheck=UpdateCheck.Never)]
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
					if (this._BlobContainer.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._ContainerName = value;
					this.SendPropertyChanged("ContainerName");
				}
			}
		}

		[Column(Storage="_ContentCrc64", DbType="BIGINT", UpdateCheck=UpdateCheck.Never)]
		public long? ContentCrc64
		{
			get
			{
				return this._ContentCrc64;
			}
			set
			{
				long? nullable = this._ContentCrc64;
				long? nullable1 = value;
				if ((nullable.GetValueOrDefault() != nullable1.GetValueOrDefault() ? true : nullable.HasValue != nullable1.HasValue))
				{
					this.SendPropertyChanging();
					this._ContentCrc64 = value;
					this.SendPropertyChanged("ContentCrc64");
				}
			}
		}

		[Column(Storage="_ContentLength", DbType="BigInt NOT NULL", UpdateCheck=UpdateCheck.Never)]
		public long ContentLength
		{
			get
			{
				return this._ContentLength;
			}
			set
			{
				if (this._ContentLength != value)
				{
					this.SendPropertyChanging();
					this._ContentLength = value;
					this.SendPropertyChanged("ContentLength");
				}
			}
		}

		[Column(Storage="_ContentMD5", DbType="Binary(16)", UpdateCheck=UpdateCheck.Never)]
		public byte[] ContentMD5
		{
			get
			{
				return this._ContentMD5;
			}
			set
			{
				if (this._ContentMD5 != value)
				{
					this.SendPropertyChanging();
					this._ContentMD5 = value;
					this.SendPropertyChanged("ContentMD5");
				}
			}
		}

		[Column(Storage="_ContentType", DbType="VarChar(128)", UpdateCheck=UpdateCheck.Never)]
		public string ContentType
		{
			get
			{
				return this._ContentType;
			}
			set
			{
				if (this._ContentType != value)
				{
					this.SendPropertyChanging();
					this._ContentType = value;
					this.SendPropertyChanged("ContentType");
				}
			}
		}

		[Column(Storage="_CreationTime", DbType="DATETIME NOT NULL DEFAULT GETUTCDATE()", IsDbGenerated=true, UpdateCheck=UpdateCheck.Never)]
		public DateTime CreationTime
		{
			get
			{
				return this._CreationTime;
			}
			set
			{
				if (this._CreationTime != value)
				{
					this.SendPropertyChanging();
					this._CreationTime = value;
					this.SendPropertyChanged("CreationTime");
				}
			}
		}

		[Column(Storage="_GenerationId", CanBeNull=false)]
		public string GenerationId
		{
			get
			{
				return this._GenerationId;
			}
			set
			{
				if (this._GenerationId != value)
				{
					this.SendPropertyChanging();
					this._GenerationId = value;
					this.SendPropertyChanged("GenerationId");
				}
			}
		}

		[Column(Storage="_IsLeaseOp", AutoSync=AutoSync.Always, DbType="BIT DEFAULT 0", UpdateCheck=UpdateCheck.Never)]
		public bool IsLeaseOp
		{
			get
			{
				return this._IsLeaseOp;
			}
			set
			{
				if (this._IsLeaseOp != value)
				{
					this.SendPropertyChanging();
					this._IsLeaseOp = value;
					this.SendPropertyChanged("IsLeaseOp");
				}
			}
		}

		[Column(Storage="_LastModificationTime", AutoSync=AutoSync.Always, DbType="DATETIME DEFAULT GETUTCDATE()", IsDbGenerated=true)]
		public DateTime? LastModificationTime
		{
			get
			{
				return this._LastModificationTime;
			}
			set
			{
				bool flag;
				DateTime? nullable = this._LastModificationTime;
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
					this._LastModificationTime = value;
					this.SendPropertyChanged("LastModificationTime");
					this.OnLastModificationTimeChanged();
				}
			}
		}

		[Column(Storage="_LeaseDuration", DbType="BIGINT DEFAULT 0", UpdateCheck=UpdateCheck.Never)]
		public TimeSpan LeaseDuration
		{
			get
			{
				return this._LeaseDuration;
			}
			set
			{
				if (this._LeaseDuration != value)
				{
					this.SendPropertyChanging();
					this._LeaseDuration = value;
					this.SendPropertyChanged("LeaseDuration");
				}
			}
		}

		[Column(Storage="_LeaseEndTime", DbType="DATETIME")]
		public DateTime? LeaseEndTime
		{
			get
			{
				return this._LeaseEndTime;
			}
			set
			{
				bool flag;
				DateTime? nullable = this._LeaseEndTime;
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
					this._LeaseEndTime = value;
					this.SendPropertyChanged("LeaseEndTime");
					this.OnLeaseEndTimeChanged();
				}
			}
		}

		[Column(Storage="_LeaseId", DbType="UNIQUEIDENTIFIER")]
		public Guid? LeaseId
		{
			get
			{
				return this._LeaseId;
			}
			set
			{
				bool flag;
				Guid? nullable = this._LeaseId;
				Guid? nullable1 = value;
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
					this._LeaseId = value;
					this.SendPropertyChanged("LeaseId");
				}
			}
		}

		[Column(Storage="_LeaseState", DbType="INT DEFAULT 0")]
		public int LeaseState
		{
			get
			{
				return this._LeaseState;
			}
			set
			{
				if (this._LeaseState != value)
				{
					this.SendPropertyChanging();
					this._LeaseState = value;
					this.SendPropertyChanged("LeaseState");
				}
			}
		}

		[Column(Storage="_Metadata", DbType="VarBinary(MAX)", UpdateCheck=UpdateCheck.Never)]
		public byte[] Metadata
		{
			get
			{
				return this._Metadata;
			}
			set
			{
				if (this._Metadata != value)
				{
					this.SendPropertyChanging();
					this._Metadata = value;
					this.SendPropertyChanged("Metadata");
				}
			}
		}

		[Column(Storage="_SequenceNumber", DbType="BIGINT", UpdateCheck=UpdateCheck.Never)]
		public long? SequenceNumber
		{
			get
			{
				return this._SequenceNumber;
			}
			set
			{
				long? nullable = this._SequenceNumber;
				long? nullable1 = value;
				if ((nullable.GetValueOrDefault() != nullable1.GetValueOrDefault() ? true : nullable.HasValue != nullable1.HasValue))
				{
					this.SendPropertyChanging();
					this._SequenceNumber = value;
					this.SendPropertyChanged("SequenceNumber");
				}
			}
		}

		[Column(Storage="_ServiceMetadata", DbType="VARBINARY(MAX)", UpdateCheck=UpdateCheck.Never)]
		public byte[] ServiceMetadata
		{
			get
			{
				return this._ServiceMetadata;
			}
			set
			{
				if (this._ServiceMetadata != value)
				{
					this.SendPropertyChanging();
					this._ServiceMetadata = value;
					this.SendPropertyChanged("ServiceMetadata");
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

		[Column(Storage="_VersionTimestamp", DbType="DATETIME NOT NULL DEFAULT '9999-12-31T23:59:59.997'", IsPrimaryKey=true, UpdateCheck=UpdateCheck.Never)]
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
					this.SendPropertyChanging();
					this._VersionTimestamp = value;
					this.SendPropertyChanged("VersionTimestamp");
				}
			}
		}

		static Blob()
		{
			Blob.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public Blob()
		{
			this._BlobContainer = new EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer>();
		}

		private void OnLastModificationTimeChanged()
		{
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._LastModificationTime);
		}

		private void OnLeaseEndTimeChanged()
		{
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._LeaseEndTime);
		}

		private void OnLoaded()
		{
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._LastModificationTime);
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._CreationTime);
			this._VersionTimestamp = DateTime.SpecifyKind(this._VersionTimestamp, DateTimeKind.Utc);
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._LeaseEndTime);
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
				this.PropertyChanging(this, Blob.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}