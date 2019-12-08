using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Table(Name="dbo.TableContainer")]
	public class TableContainer : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		private string _AccountName;

		private string _TableName;

		private DateTime _LastModificationTime;

		private byte[] _ServiceMetadata;

		private byte[] _Metadata;

		private string _CasePreservedTableName;

		private EntitySet<TableRow> _TableRows;

		private EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.Account> _Account;

		[Association(Name="Account_TableContainer", Storage="_Account", ThisKey="AccountName", OtherKey="Name", IsForeignKey=true)]
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
						entity.TableContainers.Remove(this);
					}
					this._Account.Entity = value;
					if (value == null)
					{
						this._AccountName = null;
					}
					else
					{
						value.TableContainers.Add(this);
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

		[Column(Storage="_CasePreservedTableName", DbType="varchar(63) COLLATE Latin1_General_BIN NOT NULL", CanBeNull=false, UpdateCheck=UpdateCheck.Never)]
		public string CasePreservedTableName
		{
			get
			{
				return this._CasePreservedTableName;
			}
			set
			{
				if (this._CasePreservedTableName != value)
				{
					this.SendPropertyChanging();
					this._CasePreservedTableName = value;
					this.SendPropertyChanged("CasePreservedTableName");
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

		[Column(Storage="_ServiceMetadata", DbType="VARBINARY(MAX)", CanBeNull=false, UpdateCheck=UpdateCheck.Never)]
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

		[Column(Storage="_TableName", DbType="varchar(63) COLLATE Latin1_General_BIN NOT NULL", CanBeNull=false, IsPrimaryKey=true, UpdateCheck=UpdateCheck.Never)]
		public string TableName
		{
			get
			{
				return this._TableName;
			}
			set
			{
				if (this._TableName != value)
				{
					this.SendPropertyChanging();
					this._TableName = value;
					this.SendPropertyChanged("TableName");
				}
			}
		}

		[Association(Name="TableContainer_TableRow", Storage="_TableRows", ThisKey="AccountName,TableName", OtherKey="AccountName,TableName")]
		public EntitySet<TableRow> TableRows
		{
			get
			{
				return this._TableRows;
			}
			set
			{
				this._TableRows.Assign(value);
			}
		}

		static TableContainer()
		{
			TableContainer.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public TableContainer()
		{
			this._TableRows = new EntitySet<TableRow>(new Action<TableRow>(this.attach_TableRows), new Action<TableRow>(this.detach_TableRows));
			this._Account = new EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.Account>();
		}

		private void attach_TableRows(TableRow entity)
		{
			this.SendPropertyChanging();
			entity.TableContainer = this;
		}

		private void detach_TableRows(TableRow entity)
		{
			this.SendPropertyChanging();
			entity.TableContainer = null;
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
				this.PropertyChanging(this, TableContainer.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}