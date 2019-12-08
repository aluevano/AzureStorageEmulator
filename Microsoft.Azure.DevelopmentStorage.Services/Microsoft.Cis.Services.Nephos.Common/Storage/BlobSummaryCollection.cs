using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class BlobSummaryCollection : IBlobSummaryCollection, IEnumerable<IBlobSummary>, IEnumerable, IDisposable
	{
		private IBlobSummaryCollection blobSummaryCollection;

		public bool HasMoreRows
		{
			get
			{
				bool hasMoreRows;
				try
				{
					hasMoreRows = this.blobSummaryCollection.HasMoreRows;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return hasMoreRows;
			}
		}

		public string NextBlobStart
		{
			get
			{
				string nextBlobStart;
				try
				{
					nextBlobStart = this.blobSummaryCollection.NextBlobStart;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return nextBlobStart;
			}
		}

		public BlobSummaryCollection(IBlobSummaryCollection blobSummaryCollection)
		{
			this.blobSummaryCollection = blobSummaryCollection;
		}

		public void Dispose()
		{
			this.blobSummaryCollection.Dispose();
			GC.SuppressFinalize(this);
		}

		public IEnumerator<IBlobSummary> GetEnumerator()
		{
			IEnumerator<IBlobSummary> enumerator = null;
			try
			{
				enumerator = this.blobSummaryCollection.GetEnumerator();
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			bool flag = true;
			while (flag)
			{
				IBlobSummary current = null;
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
				yield return new BlobSummary(current);
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}