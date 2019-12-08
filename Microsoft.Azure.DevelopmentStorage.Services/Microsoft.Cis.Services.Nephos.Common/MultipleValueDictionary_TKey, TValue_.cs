using AsyncHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[ComVisible(false)]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification="Erm, no, this class should not end in the suffix Container.")]
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification="It's exactly what it says it is, so the suffix is appropriate.")]
	public class MultipleValueDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, Set<TValue>>>, IEnumerable, ISerializable, IDeserializationCallback
	{
		private readonly Dictionary<TKey, Set<TValue>> dictionary;

		private readonly IEqualityComparer<TValue> valueComparer;

		private int valueCount;

		public Set<TValue> this[TKey key]
		{
			get
			{
				return this.dictionary[key];
			}
		}

		public IEqualityComparer<TKey> KeyComparer
		{
			get
			{
				return this.dictionary.Comparer;
			}
		}

		public int KeyCount
		{
			get
			{
				return this.dictionary.Count;
			}
		}

		public ICollection<TKey> Keys
		{
			get
			{
				return this.dictionary.Keys;
			}
		}

		public IEqualityComparer<TValue> ValueComparer
		{
			get
			{
				return this.valueComparer;
			}
		}

		public int ValueCount
		{
			get
			{
				return this.valueCount;
			}
		}

		public MultipleValueDictionary()
		{
			this.dictionary = new Dictionary<TKey, Set<TValue>>();
		}

		public MultipleValueDictionary(MultipleValueDictionary<TKey, TValue> multipleValueDictionary) : this(multipleValueDictionary, null, null)
		{
		}

		public MultipleValueDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			this.dictionary = new Dictionary<TKey, Set<TValue>>(keyComparer);
			this.valueComparer = valueComparer;
		}

		public MultipleValueDictionary(int capacity)
		{
			this.dictionary = new Dictionary<TKey, Set<TValue>>(capacity);
		}

		public MultipleValueDictionary(MultipleValueDictionary<TKey, TValue> multipleValueDictionary, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			if (multipleValueDictionary == null)
			{
				throw new ArgumentNullException("multipleValueDictionary");
			}
			this.dictionary = new Dictionary<TKey, Set<TValue>>(keyComparer);
			foreach (KeyValuePair<TKey, Set<TValue>> keyValuePair in multipleValueDictionary)
			{
				this.dictionary.Add(keyValuePair.Key, new Set<TValue>(keyValuePair.Value));
			}
			this.valueCount = multipleValueDictionary.ValueCount;
			this.valueComparer = valueComparer;
		}

		public MultipleValueDictionary(int capacity, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
		{
			this.dictionary = new Dictionary<TKey, Set<TValue>>(capacity, keyComparer);
			this.valueComparer = valueComparer;
		}

		protected MultipleValueDictionary(SerializationInfo info, StreamingContext context)
		{
			this.dictionary = (Dictionary<TKey, Set<TValue>>)info.GetValue("dictionary", typeof(Dictionary<TKey, Set<TValue>>));
			this.valueCount = info.GetInt32("valueCount");
			this.valueComparer = (IEqualityComparer<TValue>)info.GetValue("valueComparer", typeof(IEqualityComparer<TValue>));
		}

		public bool Add(TKey key, TValue value)
		{
			Set<TValue> tValues;
			if (!this.dictionary.TryGetValue(key, out tValues))
			{
				tValues = new Set<TValue>(this.valueComparer);
				this.dictionary.Add(key, tValues);
			}
			bool flag = tValues.Add(value);
			if (flag)
			{
				this.valueCount++;
			}
			return flag;
		}

		public bool AddRange(TKey key, IEnumerable<TValue> values)
		{
			Set<TValue> tValues;
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			bool flag = !this.dictionary.TryGetValue(key, out tValues);
			if (flag)
			{
				tValues = new Set<TValue>(this.valueComparer);
				this.dictionary.Add(key, tValues);
			}
			bool flag1 = true;
			bool flag2 = false;
			foreach (TValue value in values)
			{
				bool flag3 = tValues.Add(value);
				if (flag3)
				{
					this.valueCount++;
				}
				flag1 &= flag3;
				flag2 |= flag3;
			}
			if (flag && !flag2)
			{
				this.dictionary.Remove(key);
			}
			return flag1;
		}

		public void Clear()
		{
			this.dictionary.Clear();
			this.valueCount = 0;
		}

		public bool Contains(TKey key, TValue value)
		{
			Set<TValue> tValues;
			if (!this.dictionary.TryGetValue(key, out tValues))
			{
				return false;
			}
			return tValues.Contains(value);
		}

		public bool ContainsKey(TKey key)
		{
			return this.dictionary.ContainsKey(key);
		}

		public IEnumerator<KeyValuePair<TKey, Set<TValue>>> GetEnumerator()
		{
			return this.dictionary.GetEnumerator();
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("dictionary", this.dictionary, this.dictionary.GetType());
			info.AddValue("valueCount", this.valueCount);
			info.AddValue("valueComparer", this.valueComparer, this.valueComparer.GetType());
		}

		public virtual void OnDeserialization(object sender)
		{
			this.dictionary.OnDeserialization(sender);
		}

		public bool Remove(TKey key, TValue value)
		{
			Set<TValue> tValues;
			if (!this.dictionary.TryGetValue(key, out tValues))
			{
				return false;
			}
			bool flag = tValues.Remove(value);
			if (flag)
			{
				this.valueCount--;
			}
			return flag;
		}

		public bool RemoveKey(TKey key)
		{
			Set<TValue> tValues;
			if (!this.dictionary.TryGetValue(key, out tValues))
			{
				return false;
			}
			this.valueCount -= tValues.Count;
			return this.dictionary.Remove(key);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.dictionary.GetEnumerator();
		}
	}
}