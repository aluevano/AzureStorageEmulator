using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Table(Name="dbo.QueueMessage")]
	public class QueueMessage : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		private string _AccountName;

		private string _QueueName;

		private DateTime _VisibilityStartTime;

		private Guid _MessageId;

		private DateTime _ExpiryTime;

		private DateTime _InsertionTime;

		private int _DequeueCount;

		private byte[] _Data;

		private EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer> _QueueContainer;

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
					if (this._QueueContainer.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._AccountName = value;
					this.SendPropertyChanged("AccountName");
				}
			}
		}

		[Column(Storage="_Data", DbType="VarBinary(MAX) NOT NULL", CanBeNull=false, UpdateCheck=UpdateCheck.Never)]
		public byte[] Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				if (this._Data != value)
				{
					this.SendPropertyChanging();
					this._Data = value;
					this.SendPropertyChanged("Data");
				}
			}
		}

		[Column(Storage="_DequeueCount", DbType="INT", UpdateCheck=UpdateCheck.Never)]
		public int DequeueCount
		{
			get
			{
				return this._DequeueCount;
			}
			set
			{
				if (this._DequeueCount != value)
				{
					this.SendPropertyChanging();
					this._DequeueCount = value;
					this.SendPropertyChanged("DequeueCount");
				}
			}
		}

		[Column(Storage="_ExpiryTime", DbType="DATETIME NOT NULL", UpdateCheck=UpdateCheck.Never)]
		public DateTime ExpiryTime
		{
			get
			{
				return this._ExpiryTime;
			}
			set
			{
				if (this._ExpiryTime != value)
				{
					this.SendPropertyChanging();
					this._ExpiryTime = value;
					this.SendPropertyChanged("ExpiryTime");
					this.OnExpiryTimeChanged();
				}
			}
		}

		[Column(Storage="_InsertionTime", DbType="DATETIME NOT NULL DEFAULT GETUTCDATE()", UpdateCheck=UpdateCheck.Never)]
		public DateTime InsertionTime
		{
			get
			{
				return this._InsertionTime;
			}
			set
			{
				if (this._InsertionTime != value)
				{
					this.SendPropertyChanging();
					this._InsertionTime = value;
					this.SendPropertyChanged("InsertionTime");
					this.OnInsertionTimeChanged();
				}
			}
		}

		[Column(Storage="_MessageId", DbType="UNIQUEIDENTIFIER", IsPrimaryKey=true, UpdateCheck=UpdateCheck.Never)]
		public Guid MessageId
		{
			get
			{
				return this._MessageId;
			}
			set
			{
				if (this._MessageId != value)
				{
					this.SendPropertyChanging();
					this._MessageId = value;
					this.SendPropertyChanged("MessageId");
				}
			}
		}

		[Association(Name="QueueContainer_QueueMessage", Storage="_QueueContainer", ThisKey="AccountName,QueueName", OtherKey="AccountName,QueueName", IsForeignKey=true)]
		public Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer QueueContainer
		{
			get
			{
				return this._QueueContainer.Entity;
			}
			set
			{
				Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer entity = this._QueueContainer.Entity;
				if (entity != value || !this._QueueContainer.HasLoadedOrAssignedValue)
				{
					this.SendPropertyChanging();
					if (entity != null)
					{
						this._QueueContainer.Entity = null;
						entity.QueueMessages.Remove(this);
					}
					this._QueueContainer.Entity = value;
					if (value == null)
					{
						this._AccountName = null;
						this._QueueName = null;
					}
					else
					{
						value.QueueMessages.Add(this);
						this._AccountName = value.AccountName;
						this._QueueName = value.QueueName;
					}
					this.SendPropertyChanged("QueueContainer");
				}
			}
		}

		[Column(Storage="_QueueName", DbType="VarChar(63) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
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
					if (this._QueueContainer.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._QueueName = value;
					this.SendPropertyChanged("QueueName");
				}
			}
		}

		[Column(Storage="_VisibilityStartTime", DbType="DATETIME NOT NULL DEFAULT GETUTCDATE()", IsPrimaryKey=true)]
		public DateTime VisibilityStartTime
		{
			get
			{
				return this._VisibilityStartTime;
			}
			set
			{
				if (this._VisibilityStartTime != value)
				{
					this.SendPropertyChanging();
					this._VisibilityStartTime = value;
					this.SendPropertyChanged("VisibilityStartTime");
					this.OnVisibilityStartTimeChanged();
				}
			}
		}

		static QueueMessage()
		{
			QueueMessage.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public QueueMessage()
		{
			this._QueueContainer = new EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.QueueContainer>();
		}

		private void OnExpiryTimeChanged()
		{
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._ExpiryTime);
		}

		private void OnInsertionTimeChanged()
		{
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._InsertionTime);
		}

		private void OnLoaded()
		{
			this._VisibilityStartTime = DateTime.SpecifyKind(this._VisibilityStartTime, DateTimeKind.Utc);
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._ExpiryTime);
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._InsertionTime);
		}

		private void OnVisibilityStartTimeChanged()
		{
			this._VisibilityStartTime = DateTime.SpecifyKind(this._VisibilityStartTime, DateTimeKind.Utc);
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
				this.PropertyChanging(this, QueueMessage.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}