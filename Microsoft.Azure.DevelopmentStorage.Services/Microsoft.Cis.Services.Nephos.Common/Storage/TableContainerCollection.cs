using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class TableContainerCollection : ITableContainerCollection, IContainerCollection<ITableContainer>, IEnumerable<ITableContainer>, IEnumerable, IDisposable
	{
		private ITableContainerCollection tableContainerCollection;

		public bool HasMoreRows
		{
			get
			{
				bool hasMoreRows;
				try
				{
					hasMoreRows = this.tableContainerCollection.HasMoreRows;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return hasMoreRows;
			}
		}

		public string NextContainerStart
		{
			get
			{
				string nextContainerStart;
				try
				{
					nextContainerStart = this.tableContainerCollection.NextContainerStart;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return nextContainerStart;
			}
		}

		public TableContainerCollection(ITableContainerCollection tableContainerCollection)
		{
			this.tableContainerCollection = tableContainerCollection;
		}

		public void Dispose()
		{
			this.tableContainerCollection.Dispose();
			GC.SuppressFinalize(this);
		}

		public IEnumerator<ITableContainer> GetEnumerator()
		{
			IEnumerator<ITableContainer> enumerator = null;
			try
			{
				enumerator = this.tableContainerCollection.GetEnumerator();
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			bool flag = true;
			while (flag)
			{
				ITableContainer current = null;
				try
				{
					flag = enumerator.MoveNext();
					if (flag)
					{
						current = enumerator.Current;
					}
				}
				catch (Exception exception1)
				{
					StorageStamp.TranslateException(exception1);
					throw;
				}
				if (!flag)
				{
					continue;
				}
				yield return new TableContainer(current);
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}