using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Table(Name="dbo.BlobContainer")]
	public class BlobContainer : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		private string _AccountName;

		private string _ContainerName;

		private DateTime _LastModificationTime;

		private byte[] _ServiceMetadata;

		private byte[] _Metadata;

		private Guid? _LeaseId;

		private int _LeaseState;

		private TimeSpan _LeaseDuration;

		private DateTime? _LeaseEndTime;

		private bool _IsLeaseOp;

		private EntitySet<Blob> _BaseBlobs;

		private EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.Account> _Account;

		[Association(Name="Account_BlobContainer", Storage="_Account", ThisKey="AccountName", OtherKey="Name", IsForeignKey=true)]
		public Microsoft.WindowsAzure.DevelopmentStorage.Store.Account Account
		{
			get
			{
				return this._Account.Entity;
			}
			set
			{
				Microsoft.WindowsAzure.DevelopmentStorage.Store.Account entity = this._Account.Entity;
				if (entity != value || !this._Account.HasLoadedOrAssignedValue)
				{
					this.SendPropertyChanging();
					if (entity != null)
					{
						this._Account.Entity = null;
						entity.BlobContainers.Remove(this);
					}
					this._Account.Entity = value;
					if (value == null)
					{
						this._AccountName = null;
					}
					else
					{
						value.BlobContainers.Add(this);
						this._AccountName = value.Name;
					}
					this.SendPropertyChanged("Account");
				}
			}
		}

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
					if (this._Account.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._AccountName = value;
					this.SendPropertyChanged("AccountName");
				}
			}
		}

		[Association(Name="BlobContainer_Blob", Storage="_BaseBlobs", ThisKey="AccountName,ContainerName", OtherKey="AccountName,ContainerName")]
		public EntitySet<Blob> BaseBlobs
		{
			get
			{
				return this._BaseBlobs;
			}
			set
			{
				this._BaseBlobs.Assign(value);
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
					this.SendPropertyChanging();
					this._ContainerName = value;
					this.SendPropertyChanged("ContainerName");
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

		[Column(Storage="_LastModificationTime", AutoSync=AutoSync.Always, DbType="DATETIME NOT NULL DEFAULT GETUTCDATE()", IsDbGenerated=true)]
		public DateTime LastModificationTime
		{
			get
			{
				return this._LastModificationTime;
			}
			set
			{
				if (this._LastModificationTime != value)
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

		[Column(Storage="_LeaseState", DbType="INT DEFAULT 0", UpdateCheck=UpdateCheck.Never)]
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

		static BlobContainer()
		{
			BlobContainer.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public BlobContainer()
		{
			this._BaseBlobs = new EntitySet<Blob>(new Action<Blob>(this.attach_BaseBlobs), new Action<Blob>(this.detach_BaseBlobs));
			this._Account = new EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.Account>();
		}

		private void attach_BaseBlobs(Blob entity)
		{
			this.SendPropertyChanging();
			entity.BlobContainer = this;
		}

		private void detach_BaseBlobs(Blob entity)
		{
			this.SendPropertyChanging();
			entity.BlobContainer = null;
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
				this.PropertyChanging(this, BlobContainer.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}