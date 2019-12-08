using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbBlobContainerCollection : IBlobContainerCollection, IContainerCollection<IBaseBlobContainer>, IEnumerable<IBaseBlobContainer>, IEnumerable, IDisposable
	{
		private IEnumerable<IBaseBlobContainer> _containers;

		private readonly string _nextContainerStart;

		private bool _disposed;

		public bool HasMoreRows
		{
			get
			{
				return this._nextContainerStart != null;
			}
		}

		public string NextContainerStart
		{
			get
			{
				return this._nextContainerStart;
			}
		}

		public DbBlobContainerCollection(IEnumerable<IBaseBlobContainer> containers, string nextContainerStart)
		{
			this._containers = containers;
			this._nextContainerStart = nextContainerStart;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (this._disposed)
			{
				return;
			}
			if (disposing)
			{
				foreach (IBaseBlobContainer _container in this._containers)
				{
					_container.Dispose();
				}
				this._containers = null;
			}
			this._disposed = true;
		}

		~DbBlobContainerCollection()
		{
			this.Dispose(false);
		}

		public IEnumerator<IBaseBlobContainer> GetEnumerator()
		{
			foreach (IBaseBlobContainer _container in this._containers)
			{
				yield return _container;
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}