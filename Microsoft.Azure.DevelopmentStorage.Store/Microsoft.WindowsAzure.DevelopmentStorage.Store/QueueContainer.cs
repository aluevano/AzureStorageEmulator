using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Table(Name="dbo.QueueContainer")]
	public class QueueContainer : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		private string _AccountName;

		private string _QueueName;

		private DateTime _LastModificationTime;

		private byte[] _ServiceMetadata;

		private byte[] _Metadata;

		private EntitySet<QueueMessage> _QueueMessages;

		private EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.Account> _Account;

		[Association(Name="Account_QueueContainer", Storage="_Account", ThisKey="AccountName", OtherKey="Name", IsForeignKey=true)]
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
						entity.QueueContainers.Remove(this);
					}
					this._Account.Entity = value;
					if (value == null)
					{
						this._AccountName = null;
					}
					else
					{
						value.QueueContainers.Add(this);
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

		[Column(Storage="_LastModificationTime", AutoSync=AutoSync.Always, DbType="DATETIME NOT NULL DEFAULT GETUTCDATE()", IsDbGenerated=true, IsVersion=true, UpdateCheck=UpdateCheck.Never)]
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

		[Association(Name="QueueContainer_QueueMessage", Storage="_QueueMessages", ThisKey="AccountName,QueueName", OtherKey="AccountName,QueueName")]
		public EntitySet<QueueMessage> QueueMessages
		{
			get
			{
				return this._QueueMessages;
			}
			set
			{
				this._QueueMessages.Assign(value);
			}
		}

		[Column(Storage="_QueueName", DbType="VarChar(63) NOT NULL", CanBeNull=false, IsPrimaryKey=true, UpdateCheck=UpdateCheck.Never)]
		public string QueueName
		{
			get
			{
				return this._QueueName;
			}
			set
			{
				if (this._QueueName != value)
				{
					this.SendPropertyChanging();
					this._QueueName = value;
					this.SendPropertyChanged("QueueName");
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

		static QueueContainer()
		{
			QueueContainer.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public QueueContainer()
		{
			this._QueueMessages = new EntitySet<QueueMessage>(new Action<QueueMessage>(this.attach_QueueMessages), new Action<QueueMessage>(this.detach_QueueMessages));
			this._Account = new EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.Account>();
		}

		private void attach_QueueMessages(QueueMessage entity)
		{
			this.SendPropertyChanging();
			entity.QueueContainer = this;
		}

		private void detach_QueueMessages(QueueMessage entity)
		{
			this.SendPropertyChanging();
			entity.QueueContainer = null;
		}

		private void OnLastModificationTimeChanged()
		{
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._LastModificationTime);
		}

		private void OnLoaded()
		{
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._LastModificationTime);
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
				this.PropertyChanging(this, QueueContainer.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}