using System;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Threading;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Table(Name="dbo.TableRow")]
	public class TableRow : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		private string _AccountName;

		private string _TableName;

		private string _PartitionKey;

		private string _RowKey;

		private DateTime _Timestamp;

		private XElement _Data;

		private EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer> _TableContainer;

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
					if (this._TableContainer.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._AccountName = value;
					this.SendPropertyChanged("AccountName");
				}
			}
		}

		[Column(Storage="_Data", DbType="XML", CanBeNull=false, UpdateCheck=UpdateCheck.Never)]
		public XElement Data
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

		[Column(Storage="_PartitionKey", DbType="NVARCHAR(256) COLLATE Latin1_General_BIN", CanBeNull=false, IsPrimaryKey=true, UpdateCheck=UpdateCheck.Never)]
		public string PartitionKey
		{
			get
			{
				return this._PartitionKey;
			}
			set
			{
				if (this._PartitionKey != value)
				{
					this.SendPropertyChanging();
					this._PartitionKey = value;
					this.SendPropertyChanged("PartitionKey");
				}
			}
		}

		[Column(Storage="_RowKey", DbType="NVARCHAR(256) COLLATE Latin1_General_BIN", CanBeNull=false, IsPrimaryKey=true, UpdateCheck=UpdateCheck.Never)]
		public string RowKey
		{
			get
			{
				return this._RowKey;
			}
			set
			{
				if (this._RowKey != value)
				{
					this.SendPropertyChanging();
					this._RowKey = value;
					this.SendPropertyChanged("RowKey");
				}
			}
		}

		[Association(Name="TableContainer_TableRow", Storage="_TableContainer", ThisKey="AccountName,TableName", OtherKey="AccountName,TableName", IsForeignKey=true)]
		public Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer TableContainer
		{
			get
			{
				return this._TableContainer.Entity;
			}
			set
			{
				Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer entity = this._TableContainer.Entity;
				if (entity != value || !this._TableContainer.HasLoadedOrAssignedValue)
				{
					this.SendPropertyChanging();
					if (entity != null)
					{
						this._TableContainer.Entity = null;
						entity.TableRows.Remove(this);
					}
					this._TableContainer.Entity = value;
					if (value == null)
					{
						this._AccountName = null;
						this._TableName = null;
					}
					else
					{
						value.TableRows.Add(this);
						this._AccountName = value.AccountName;
						this._TableName = value.TableName;
					}
					this.SendPropertyChanged("TableContainer");
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
					if (this._TableContainer.HasLoadedOrAssignedValue)
					{
						throw new ForeignKeyReferenceAlreadyHasValueException();
					}
					this.SendPropertyChanging();
					this._TableName = value;
					this.SendPropertyChanged("TableName");
				}
			}
		}

		[Column(Storage="_Timestamp", AutoSync=AutoSync.Always, DbType="DATETIME NOT NULL DEFAULT GETUTCDATE()", IsDbGenerated=true, IsVersion=true, UpdateCheck=UpdateCheck.Never)]
		public DateTime Timestamp
		{
			get
			{
				return this._Timestamp;
			}
			set
			{
				if (this._Timestamp != value)
				{
					this.SendPropertyChanging();
					this._Timestamp = value;
					this.SendPropertyChanged("Timestamp");
					this.OnTimestampChanged();
				}
			}
		}

		static TableRow()
		{
			TableRow.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public TableRow()
		{
			this._TableContainer = new EntityRef<Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer>();
		}

		private void OnLoaded()
		{
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._Timestamp);
		}

		private void OnTimestampChanged()
		{
			DevelopmentStorageDbDataContext.AdjustSqlDateTime(ref this._Timestamp);
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
				this.PropertyChanging(this, TableRow.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}