using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Table(Name="dbo.Account")]
	public class Account : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		private string _Name;

		private byte[] _SecretKey;

		private byte[] _QueueServiceSettings;

		private byte[] _BlobServiceSettings;

		private byte[] _TableServiceSettings;

		private byte[] _SecondaryKey;

		private bool _SecondaryReadEnabled;

		private EntitySet<BlobContainer> _BlobContainers;

		private EntitySet<QueueContainer> _QueueContainers;

		private EntitySet<TableContainer> _TableContainers;

		[Association(Name="Account_BlobContainer", Storage="_BlobContainers", ThisKey="Name", OtherKey="AccountName")]
		public EntitySet<BlobContainer> BlobContainers
		{
			get
			{
				return this._BlobContainers;
			}
			set
			{
				this._BlobContainers.Assign(value);
			}
		}

		[Column(Storage="_BlobServiceSettings", DbType="varbinary(max)", UpdateCheck=UpdateCheck.Never)]
		public byte[] BlobServiceSettings
		{
			get
			{
				return this._BlobServiceSettings;
			}
			set
			{
				if (this._BlobServiceSettings != value)
				{
					this.SendPropertyChanging();
					this._BlobServiceSettings = value;
					this.SendPropertyChanged("BlobServiceSettings");
				}
			}
		}

		[Column(Storage="_Name", DbType="VarChar(24) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if (this._Name != value)
				{
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
				}
			}
		}

		[Association(Name="Account_QueueContainer", Storage="_QueueContainers", ThisKey="Name", OtherKey="AccountName")]
		public EntitySet<QueueContainer> QueueContainers
		{
			get
			{
				return this._QueueContainers;
			}
			set
			{
				this._QueueContainers.Assign(value);
			}
		}

		[Column(Storage="_QueueServiceSettings", DbType="varbinary(max)", UpdateCheck=UpdateCheck.Never)]
		public byte[] QueueServiceSettings
		{
			get
			{
				return this._QueueServiceSettings;
			}
			set
			{
				if (this._QueueServiceSettings != value)
				{
					this.SendPropertyChanging();
					this._QueueServiceSettings = value;
					this.SendPropertyChanged("QueueServiceSettings");
				}
			}
		}

		[Column(Storage="_SecondaryKey", DbType="VarBinary(256)")]
		public byte[] SecondaryKey
		{
			get
			{
				return this._SecondaryKey;
			}
			set
			{
				if (this._SecondaryKey != value)
				{
					this.SendPropertyChanging();
					this._SecondaryKey = value;
					this.SendPropertyChanged("SecondaryKey");
				}
			}
		}

		[Column(Storage="_SecondaryReadEnabled", DbType="BIT DEFAULT 1")]
		public bool SecondaryReadEnabled
		{
			get
			{
				return this._SecondaryReadEnabled;
			}
			set
			{
				if (this._SecondaryReadEnabled != value)
				{
					this.SendPropertyChanging();
					this._SecondaryReadEnabled = value;
					this.SendPropertyChanged("SecondaryReadEnabled");
				}
			}
		}

		[Column(Storage="_SecretKey", DbType="VarBinary(256)", UpdateCheck=UpdateCheck.Never)]
		public byte[] SecretKey
		{
			get
			{
				return this._SecretKey;
			}
			set
			{
				if (this._SecretKey != value)
				{
					this.SendPropertyChanging();
					this._SecretKey = value;
					this.SendPropertyChanged("SecretKey");
				}
			}
		}

		[Association(Name="Account_TableContainer", Storage="_TableContainers", ThisKey="Name", OtherKey="AccountName")]
		public EntitySet<TableContainer> TableContainers
		{
			get
			{
				return this._TableContainers;
			}
			set
			{
				this._TableContainers.Assign(value);
			}
		}

		[Column(Storage="_TableServiceSettings", DbType="varbinary(max)", UpdateCheck=UpdateCheck.Never)]
		public byte[] TableServiceSettings
		{
			get
			{
				return this._TableServiceSettings;
			}
			set
			{
				if (this._TableServiceSettings != value)
				{
					this.SendPropertyChanging();
					this._TableServiceSettings = value;
					this.SendPropertyChanged("TableServiceSettings");
				}
			}
		}

		static Account()
		{
			Account.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public Account()
		{
			this._BlobContainers = new EntitySet<BlobContainer>(new Action<BlobContainer>(this.attach_BlobContainers), new Action<BlobContainer>(this.detach_BlobContainers));
			this._QueueContainers = new EntitySet<QueueContainer>(new Action<QueueContainer>(this.attach_QueueContainers), new Action<QueueContainer>(this.detach_QueueContainers));
			this._TableContainers = new EntitySet<TableContainer>(new Action<TableContainer>(this.attach_TableContainers), new Action<TableContainer>(this.detach_TableContainers));
		}

		private void attach_BlobContainers(BlobContainer entity)
		{
			this.SendPropertyChanging();
			entity.Account = this;
		}

		private void attach_QueueContainers(QueueContainer entity)
		{
			this.SendPropertyChanging();
			entity.Account = this;
		}

		private void attach_TableContainers(TableContainer entity)
		{
			this.SendPropertyChanging();
			entity.Account = this;
		}

		private void detach_BlobContainers(BlobContainer entity)
		{
			this.SendPropertyChanging();
			entity.Account = null;
		}

		private void detach_QueueContainers(QueueContainer entity)
		{
			this.SendPropertyChanging();
			entity.Account = null;
		}

		private void detach_TableContainers(TableContainer entity)
		{
			this.SendPropertyChanging();
			entity.Account = null;
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
				this.PropertyChanging(this, Account.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}