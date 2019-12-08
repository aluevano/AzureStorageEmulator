using System;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public abstract class ObjectCache<T>
	where T : IDisposable
	{
		private readonly int maxSize;

		private readonly T[] cacheArray;

		private int cacheSize;

		private readonly object cacheSync;

		public ObjectCache(int maxSize)
		{
			this.maxSize = maxSize;
			this.cacheArray = new T[maxSize];
			this.cacheSize = 0;
		}

		public virtual T Acquire()
		{
			T t = default(T);
			Monitor.Enter(this.cacheSync);
			if (this.cacheSize != 0)
			{
				ObjectCache<T> objectCache = this;
				int num = objectCache.cacheSize - 1;
				int num1 = num;
				objectCache.cacheSize = num;
				t = this.cacheArray[num1];
				this.cacheArray[this.cacheSize] = default(T);
			}
			Monitor.Exit(this.cacheSync);
			return t;
		}

		public virtual void Release(T obj)
		{
			Monitor.Enter(this.cacheSync);
			if (this.cacheSize < this.maxSize)
			{
				T[] tArray = this.cacheArray;
				ObjectCache<T> objectCache = this;
				int num = objectCache.cacheSize;
				int num1 = num;
				objectCache.cacheSize = num + 1;
				tArray[num1] = obj;
				obj = default(T);
			}
			Monitor.Exit(this.cacheSync);
			if (obj != null)
			{
				obj.Dispose();
			}
		}
	}
}