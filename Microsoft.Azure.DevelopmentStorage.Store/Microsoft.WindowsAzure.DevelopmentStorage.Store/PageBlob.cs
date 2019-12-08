using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Data.Linq.Mapping;
using System.Data.SqlTypes;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public class PageBlob : Blob
	{
		private long? _MaxSize;

		private string _FileName;

		private bool? _IsIncrementalCopy;

		[Column(Storage="_FileName", DbType="nvarchar(260)", UpdateCheck=UpdateCheck.Never)]
		public string FileName
		{
			get
			{
				return this._FileName;
			}
			set
			{
				if (this._FileName != value)
				{
					this.SendPropertyChanging();
					this._FileName = value;
					this.SendPropertyChanged("FileName");
				}
			}
		}

		[Column(Storage="_IsIncrementalCopy", DbType="Bit", UpdateCheck=UpdateCheck.Never)]
		public bool? IsIncrementalCopy
		{
			get
			{
				return this._IsIncrementalCopy;
			}
			set
			{
				bool? nullable = this._IsIncrementalCopy;
				bool? nullable1 = value;
				if ((nullable.GetValueOrDefault() != nullable1.GetValueOrDefault() ? true : nullable.HasValue != nullable1.HasValue))
				{
					this.SendPropertyChanging();
					this._IsIncrementalCopy = value;
					this.SendPropertyChanged("IsIncrementalCopy");
				}
			}
		}

		public bool IsIncrementalRoot
		{
			get
			{
				bool? isIncrementalCopy = this.IsIncrementalCopy;
				if ((!isIncrementalCopy.GetValueOrDefault() ? true : !isIncrementalCopy.HasValue))
				{
					return false;
				}
				return base.VersionTimestamp >= SqlDateTime.MaxValue.Value;
			}
		}

		[Column(Storage="_MaxSize", DbType="BIGINT", UpdateCheck=UpdateCheck.Never)]
		public long? MaxSize
		{
			get
			{
				return this._MaxSize;
			}
			set
			{
				long? nullable = this._MaxSize;
				long? nullable1 = value;
				if ((nullable.GetValueOrDefault() != nullable1.GetValueOrDefault() ? true : nullable.HasValue != nullable1.HasValue))
				{
					this.SendPropertyChanging();
					this._MaxSize = value;
					this.SendPropertyChanged("MaxSize");
				}
			}
		}

		public PageBlob()
		{
			this.OnCreated();
		}

		private void OnCreated()
		{
			base.BlobType = Microsoft.Cis.Services.Nephos.Common.Storage.BlobType.IndexBlob;
		}
	}
}