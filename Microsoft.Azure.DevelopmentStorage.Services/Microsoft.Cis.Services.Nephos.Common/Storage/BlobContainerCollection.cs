using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class BlobContainerCollection : IBlobContainerCollection, IContainerCollection<IBaseBlobContainer>, IEnumerable<IBaseBlobContainer>, IEnumerable, IDisposable
	{
		private IBlobContainerCollection blobContainerCollection;

		public bool HasMoreRows
		{
			get
			{
				bool hasMoreRows;
				try
				{
					hasMoreRows = this.blobContainerCollection.HasMoreRows;
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
					nextContainerStart = this.blobContainerCollection.NextContainerStart;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return nextContainerStart;
			}
		}

		public BlobContainerCollection(IBlobContainerCollection blobContainerCollection)
		{
			this.blobContainerCollection = blobContainerCollection;
		}

		public void Dispose()
		{
			this.blobContainerCollection.Dispose();
			GC.SuppressFinalize(this);
		}

		public IEnumerator<IBaseBlobContainer> GetEnumerator()
		{
			IEnumerator<IBaseBlobContainer> enumerator = null;
			try
			{
				enumerator = this.blobContainerCollection.GetEnumerator();
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			bool flag = true;
			while (flag)
			{
				IBaseBlobContainer current = null;
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
				yield return new BaseBlobContainer(current);
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}