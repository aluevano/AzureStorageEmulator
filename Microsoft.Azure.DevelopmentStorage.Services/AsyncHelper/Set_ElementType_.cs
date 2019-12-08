using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace AsyncHelper
{
	[ComVisible(false)]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification="No, this class should not end in the suffix Container.")]
	public class Set<ElementType> : IEnumerable<ElementType>, IEnumerable, ISerializable, IDeserializationCallback
	{
		private readonly Dictionary<ElementType, bool> dictionary;

		public IEqualityComparer<ElementType> Comparer
		{
			get
			{
				return this.dictionary.Comparer;
			}
		}

		public int Count
		{
			get
			{
				return this.dictionary.Count;
			}
		}

		public Set()
		{
			this.dictionary = new Dictionary<ElementType, bool>();
		}

		public Set(Set<ElementType> set)
		{
			this.dictionary = new Dictionary<ElementType, bool>(set.dictionary);
		}

		public Set(IEqualityComparer<ElementType> comparer)
		{
			this.dictionary = new Dictionary<ElementType, bool>(comparer);
		}

		public Set(int capacity)
		{
			this.dictionary = new Dictionary<ElementType, bool>(capacity);
		}

		public Set(Set<ElementType> set, IEqualityComparer<ElementType> comparer)
		{
			this.dictionary = new Dictionary<ElementType, bool>(set.dictionary, comparer);
		}

		public Set(int capacity, IEqualityComparer<ElementType> comparer)
		{
			this.dictionary = new Dictionary<ElementType, bool>(capacity, comparer);
		}

		protected Set(SerializationInfo info, StreamingContext context)
		{
			this.dictionary = (Dictionary<ElementType, bool>)info.GetValue("dictionary", typeof(Dictionary<ElementType, bool>));
		}

		public bool Add(ElementType element)
		{
			bool flag;
			try
			{
				this.dictionary.Add(element, true);
				flag = true;
			}
			catch (ArgumentException argumentException)
			{
				flag = false;
			}
			return flag;
		}

		public bool AddRange(IEnumerable<ElementType> elements)
		{
			if (elements == null)
			{
				throw new ArgumentNullException("elements");
			}
			bool flag = true;
			foreach (ElementType element in elements)
			{
				flag &= this.Add(element);
			}
			return flag;
		}

		public void Clear()
		{
			this.dictionary.Clear();
		}

		public bool Contains(ElementType element)
		{
			return this.dictionary.ContainsKey(element);
		}

		public IEnumerator<ElementType> GetEnumerator()
		{
			return this.dictionary.Keys.GetEnumerator();
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("dictionary", this.dictionary, this.dictionary.GetType());
		}

		public virtual void OnDeserialization(object sender)
		{
			this.dictionary.OnDeserialization(sender);
		}

		public bool Remove(ElementType element)
		{
			return this.dictionary.Remove(element);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.dictionary.Keys.GetEnumerator();
		}
	}
}