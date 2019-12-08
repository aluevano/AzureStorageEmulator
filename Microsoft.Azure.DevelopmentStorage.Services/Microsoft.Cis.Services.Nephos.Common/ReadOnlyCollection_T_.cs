using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class ReadOnlyCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable
	{
		private readonly ICollection<T> wrappedCollection;

		public int Count
		{
			get
			{
				return this.wrappedCollection.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public ReadOnlyCollection(ICollection<T> wrappedCollection)
		{
			if (wrappedCollection == null)
			{
				throw new ArgumentNullException("wrappedCollection");
			}
			this.wrappedCollection = wrappedCollection;
		}

		public void Add(T item)
		{
			throw ReadOnlyCollection<T>.GetReadOnlyException();
		}

		public void Clear()
		{
			throw ReadOnlyCollection<T>.GetReadOnlyException();
		}

		public bool Contains(T item)
		{
			return this.wrappedCollection.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			this.wrappedCollection.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.wrappedCollection.GetEnumerator();
		}

		private static Exception GetReadOnlyException()
		{
			return new InvalidOperationException("The collection is read-only");
		}

		public bool Remove(T item)
		{
			throw ReadOnlyCollection<T>.GetReadOnlyException();
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}