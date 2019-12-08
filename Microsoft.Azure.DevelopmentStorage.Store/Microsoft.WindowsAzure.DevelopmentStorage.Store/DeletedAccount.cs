using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Table(Name="")]
	public class DeletedAccount : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs;

		private string _Name;

		private DateTime _DeletionTime;

		[Column(Storage="_DeletionTime", DbType="DATETIME")]
		public DateTime DeletionTime
		{
			get
			{
				return this._DeletionTime;
			}
			set
			{
				if (this._DeletionTime != value)
				{
					this.SendPropertyChanging();
					this._DeletionTime = value;
					this.SendPropertyChanged("DeletionTime");
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

		static DeletedAccount()
		{
			DeletedAccount.emptyChangingEventArgs = new PropertyChangingEventArgs(string.Empty);
		}

		public DeletedAccount()
		{
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
				this.PropertyChanging(this, DeletedAccount.emptyChangingEventArgs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event PropertyChangingEventHandler PropertyChanging;
	}
}