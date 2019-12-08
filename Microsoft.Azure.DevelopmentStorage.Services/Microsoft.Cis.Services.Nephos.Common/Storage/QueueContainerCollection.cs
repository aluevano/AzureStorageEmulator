using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	internal class QueueContainerCollection : IQueueContainerCollection, IContainerCollection<IQueueContainer>, IEnumerable<IQueueContainer>, IEnumerable, IDisposable
	{
		private IQueueContainerCollection queueContainerCollection;

		public bool HasMoreRows
		{
			get
			{
				bool hasMoreRows;
				try
				{
					hasMoreRows = this.queueContainerCollection.HasMoreRows;
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
					nextContainerStart = this.queueContainerCollection.NextContainerStart;
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
				return nextContainerStart;
			}
		}

		public QueueContainerCollection(IQueueContainerCollection queueContainerCollection)
		{
			this.queueContainerCollection = queueContainerCollection;
		}

		public void Dispose()
		{
			this.queueContainerCollection.Dispose();
			GC.SuppressFinalize(this);
		}

		public IEnumerator<IQueueContainer> GetEnumerator()
		{
			IEnumerator<IQueueContainer> enumerator = null;
			try
			{
				enumerator = this.queueContainerCollection.GetEnumerator();
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			bool flag = true;
			while (flag)
			{
				IQueueContainer current = null;
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
				yield return new QueueContainer(current);
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}