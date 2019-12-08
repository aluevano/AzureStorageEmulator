using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbBlobObjectCollection : IBlobObjectCollection, IEnumerable<IBlobObject>, IEnumerable, IDisposable
	{
		private readonly string _nextBlobToStart;

		private readonly string _nextContainerStart;

		private readonly DateTime? _nextSnapshotStart;

		private IEnumerable<IBlobObject> _blobs;

		private bool _disposed;

		public bool HasMoreRows
		{
			get
			{
				return this._nextBlobToStart != null;
			}
		}

		bool Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObjectCollection.IsListingByAccount
		{
			get
			{
				return false;
			}
		}

		public string NextBlobStart
		{
			get
			{
				return this._nextBlobToStart;
			}
		}

		public string NextContainerStart
		{
			get
			{
				return this._nextContainerStart;
			}
		}

		public DateTime? NextSnapshotStart
		{
			get
			{
				return this._nextSnapshotStart;
			}
		}

		internal DbBlobObjectCollection(IEnumerable<IBlobObject> blobs, string nextContainerStart, string nextBlobToStart, DateTime? nextSnapshotStart)
		{
			this._blobs = blobs;
			this._nextBlobToStart = nextBlobToStart;
			this._nextContainerStart = nextContainerStart;
			this._nextSnapshotStart = nextSnapshotStart;
		}

		protected void CheckDisposed()
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException("DbBlobObjectCollection");
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			if (disposing)
			{
				foreach (IBlobObject _blob in this._blobs)
				{
					_blob.Dispose();
				}
				this._blobs = null;
			}
			this._disposed = true;
		}

		public IEnumerator<IBlobObject> GetEnumerator()
		{
			foreach (IBlobObject _blob in this._blobs)
			{
				yield return _blob;
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}