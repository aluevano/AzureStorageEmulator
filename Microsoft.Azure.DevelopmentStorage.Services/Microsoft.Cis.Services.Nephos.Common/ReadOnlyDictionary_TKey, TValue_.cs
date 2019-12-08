using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
	{
		private readonly IDictionary<TKey, TValue> wrappedDictionary;

		public int Count
		{
			get
			{
				return this.wrappedDictionary.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				return this.wrappedDictionary[key];
			}
			set
			{
				throw ReadOnlyDictionary<TKey, TValue>.GetReadOnlyException();
			}
		}

		public ICollection<TKey> Keys
		{
			get
			{
				return new ReadOnlyCollection<TKey>(this.wrappedDictionary.Keys);
			}
		}

		public ICollection<TValue> Values
		{
			get
			{
				return new ReadOnlyCollection<TValue>(this.wrappedDictionary.Values);
			}
		}

		public ReadOnlyDictionary(IDictionary<TKey, TValue> wrappedDictionary)
		{
			if (wrappedDictionary == null)
			{
				throw new ArgumentNullException("wrappedDictionary");
			}
			this.wrappedDictionary = wrappedDictionary;
		}

		public void Add(TKey key, TValue value)
		{
			throw ReadOnlyDictionary<TKey, TValue>.GetReadOnlyException();
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw ReadOnlyDictionary<TKey, TValue>.GetReadOnlyException();
		}

		public void Clear()
		{
			throw ReadOnlyDictionary<TKey, TValue>.GetReadOnlyException();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return this.wrappedDictionary.Contains(item);
		}

		public bool ContainsKey(TKey key)
		{
			return this.wrappedDictionary.ContainsKey(key);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			this.wrappedDictionary.CopyTo(array, arrayIndex);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this.wrappedDictionary.GetEnumerator();
		}

		private static Exception GetReadOnlyException()
		{
			return new InvalidOperationException("The dictionary is read-only");
		}

		public bool Remove(TKey key)
		{
			throw ReadOnlyDictionary<TKey, TValue>.GetReadOnlyException();
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw ReadOnlyDictionary<TKey, TValue>.GetReadOnlyException();
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return this.wrappedDictionary.TryGetValue(key, out value);
		}
	}
}