using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class BlobObjectCollection : IBlobObjectCollection, IEnumerable<IBlobObject>, IEnumerable, IDisposable
	{
		private IBlobObjectCollection blobs;

		public bool HasMoreRows
		{
			get
			{
				bool hasMoreRows;
				try
				{
					hasMoreRows = this.blobs.HasMoreRows;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return hasMoreRows;
			}
		}

		public bool IsListingByAccount
		{
			get
			{
				bool isListingByAccount;
				try
				{
					isListingByAccount = this.blobs.IsListingByAccount;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return isListingByAccount;
			}
		}

		public string NextBlobStart
		{
			get
			{
				string nextBlobStart;
				try
				{
					nextBlobStart = this.blobs.NextBlobStart;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return nextBlobStart;
			}
		}

		public string NextContainerStart
		{
			get
			{
				string nextContainerStart;
				try
				{
					nextContainerStart = this.blobs.NextContainerStart;
				}
				catch (InvalidOperationException invalidOperationException)
				{
					nextContainerStart = null;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return nextContainerStart;
			}
		}

		public DateTime? NextSnapshotStart
		{
			get
			{
				DateTime? nextSnapshotStart;
				try
				{
					nextSnapshotStart = this.blobs.NextSnapshotStart;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return nextSnapshotStart;
			}
		}

		public BlobObjectCollection(IBlobObjectCollection blobs)
		{
			this.blobs = blobs;
		}

		public void Dispose()
		{
			this.blobs.Dispose();
			GC.SuppressFinalize(this);
		}

		public IEnumerator<IBlobObject> GetEnumerator()
		{
			IEnumerator<IBlobObject> enumerator = null;
			try
			{
				enumerator = this.blobs.GetEnumerator();
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			bool flag = true;
			while (flag)
			{
				IBlobObject current = null;
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
				yield return new BaseBlobObject(current);
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}