using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncHelper
{
	public class AsyncSemaphore : IDisposable
	{
		private Queue<AsyncResultWaiter<int>> pending;

		private readonly SortedDictionary<AsyncResultWaiter<int>, NoResults> pendingWaitersToExpire;

		private readonly static TimeSpan FutureExpiryThreshold;

		private readonly int maxCount;

		private Timer timer;

		private int numExpiredWaitersInQueue;

		private readonly int? maxPending;

		private int waiterIndex;

		private int count;

		private bool disposed;

		private readonly object syncObj = new object();

		public int Count
		{
			get
			{
				this.CheckDisposed();
				return Thread.VolatileRead(ref this.count);
			}
		}

		public int MaxCount
		{
			get
			{
				this.CheckDisposed();
				return this.maxCount;
			}
		}

		public int PendingCount
		{
			get
			{
				int count;
				this.CheckDisposed();
				lock (this.syncObj)
				{
					count = this.pending.Count;
				}
				return count;
			}
		}

		static AsyncSemaphore()
		{
			AsyncSemaphore.FutureExpiryThreshold = TimeSpan.FromMilliseconds(50);
		}

		public AsyncSemaphore(int initialCount, int maxCount)
		{
			if (initialCount > maxCount)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { initialCount, maxCount };
				throw new ArgumentException(string.Format(invariantCulture, "initialCount ({0}) greater than maxCount ({1})", objArray));
			}
			if (initialCount < 0)
			{
				throw new ArgumentOutOfRangeException("initialCount", (object)initialCount, "initialCount must be >= 0");
			}
			if (maxCount < 1)
			{
				throw new ArgumentOutOfRangeException("maxCount", (object)maxCount, "maxCount must be >= 1");
			}
			this.count = initialCount;
			this.maxCount = maxCount;
			this.pending = new Queue<AsyncResultWaiter<int>>();
			this.pendingWaitersToExpire = new SortedDictionary<AsyncResultWaiter<int>, NoResults>((object)(new AsyncResultWaiterComparer<int>()));
		}

		public AsyncSemaphore(int initialCount, int maxCount, int maxPending) : this(initialCount, maxCount)
		{
			if (maxPending < 1)
			{
				throw new ArgumentOutOfRangeException("maxPending", (object)maxPending, "maxPending must be >= 1");
			}
			this.maxPending = new int?(maxPending);
		}

		public IAsyncResult BeginAcquire(AsyncCallback callback, object state)
		{
			return this.BeginAcquire(1, TimeSpan.MaxValue, callback, state);
		}

		public IAsyncResult BeginAcquire(int acquireCount, AsyncCallback callback, object state)
		{
			return this.BeginAcquire(acquireCount, TimeSpan.MaxValue, callback, state);
		}

		public IAsyncResult BeginAcquire(TimeSpan timeout, AsyncCallback callback, object state)
		{
			return this.BeginAcquire(1, timeout, callback, state);
		}

		public IAsyncResult BeginAcquire(int acquireCount, TimeSpan timeout, AsyncCallback callback, object state)
		{
			IAsyncResult asyncResult;
			this.CheckDisposed();
			if (acquireCount <= 0 || acquireCount > this.maxCount)
			{
				throw new ArgumentOutOfRangeException("acquireCount", (object)acquireCount, "acquireCount must be > 0 and <= MaxCount");
			}
			if (timeout < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("timeout", (object)timeout, "must be >= 0");
			}
			AsyncResult<int> asyncResult1 = new AsyncResult<int>("AsyncSemaphore.Acquire", callback, state)
			{
				ResultData = acquireCount
			};
			if (timeout == TimeSpan.Zero)
			{
				asyncResult1.Complete(new TimeoutException("The resource(s) could not be acquired within the specified timeout period."), true);
				return asyncResult1;
			}
			DateTime dateTime = (timeout == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.UtcNow.Add(timeout));
			lock (this.syncObj)
			{
				if (acquireCount > this.count || this.pending.Count > 0)
				{
					if (this.maxPending.HasValue && this.pending.Count == this.maxPending.Value)
					{
						if (this.numExpiredWaitersInQueue <= 0)
						{
							asyncResult1.Complete(new SemaphoreQueueExhaustedException("Too many pending waiters"), true);
							asyncResult = asyncResult1;
							return asyncResult;
						}
						else
						{
							this.CleanupExpiredWaiters(this.syncObj);
						}
					}
					AsyncSemaphore asyncSemaphore = this;
					int num = asyncSemaphore.waiterIndex;
					int num1 = num;
					asyncSemaphore.waiterIndex = num + 1;
					AsyncResultWaiter<int> asyncResultWaiter = new AsyncResultWaiter<int>(asyncResult1, dateTime, num1);
					this.pending.Enqueue(asyncResultWaiter);
					if (timeout != TimeSpan.MaxValue)
					{
						this.pendingWaitersToExpire.Add(asyncResultWaiter, new NoResults());
						if (this.timer == null)
						{
							this.timer = new Timer(new TimerCallback(this.TimeoutExpiredWaiters), null, timeout, TimeSpan.FromMilliseconds(-1));
						}
						else if (asyncResultWaiter.AsyncResult == this.First<AsyncResultWaiter<int>>(this.pendingWaitersToExpire.Keys).AsyncResult)
						{
							this.timer.Change(timeout, TimeSpan.FromMilliseconds(-1));
						}
					}
				}
				else
				{
					this.count -= acquireCount;
					asyncResult1.Complete(null, true);
				}
				return asyncResult1;
			}
			return asyncResult;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("AsyncSemaphore");
			}
		}

		private void CleanupExpiredWaiters(object syncObj)
		{
			Queue<AsyncResultWaiter<int>> asyncResultWaiters = new Queue<AsyncResultWaiter<int>>();
			while (this.pending.Count > 0)
			{
				AsyncResultWaiter<int> asyncResultWaiter = this.pending.Dequeue();
				if (!asyncResultWaiter.StillPending)
				{
					continue;
				}
				asyncResultWaiters.Enqueue(asyncResultWaiter);
			}
			this.pending = asyncResultWaiters;
			this.numExpiredWaitersInQueue = 0;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			if (disposing)
			{
				lock (this.syncObj)
				{
					while (this.pending.Count > 0)
					{
						AsyncResultWaiter<int> asyncResultWaiter = this.pending.Dequeue();
						if (!asyncResultWaiter.StillPending)
						{
							continue;
						}
						this.pendingWaitersToExpire.Remove(asyncResultWaiter);
						ThreadPool.QueueUserWorkItem((object o) => ((AsyncResult<int>)o).Complete(new TimeoutException("The asynchronous semaphore was disposed."), false), asyncResultWaiter.AsyncResult);
					}
					if (this.timer != null)
					{
						this.timer.Dispose();
						this.timer = null;
					}
				}
			}
			this.disposed = true;
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="End call is non-static to match Begin call")]
		public void EndAcquire(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			((AsyncResult<int>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private T First<T>(IEnumerable<T> enumerable)
		{
			T current = default(T);
			bool flag = false;
			using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					current = enumerator.Current;
					flag = true;
				}
			}
			if (!flag)
			{
				throw new InvalidOperationException("list was empty");
			}
			return current;
		}

		public void Release()
		{
			this.Release(1);
		}

		public void Release(int releaseCount)
		{
			this.CheckDisposed();
			if (releaseCount < 1)
			{
				throw new ArgumentOutOfRangeException("releaseCount", (object)releaseCount, "releaseCount must be >= 1");
			}
			lock (this.syncObj)
			{
				if (releaseCount > this.maxCount - this.count)
				{
					throw new SemaphoreFullException();
				}
				this.count += releaseCount;
				while (this.pending.Count > 0)
				{
					AsyncResultWaiter<int> asyncResultWaiter = this.pending.Peek();
					if (asyncResultWaiter.StillPending)
					{
						if (asyncResultWaiter.AsyncResult.ResultData > this.count)
						{
							break;
						}
						this.pending.Dequeue();
						this.count -= asyncResultWaiter.AsyncResult.ResultData;
						if (asyncResultWaiter.ExpiryTime != DateTime.MaxValue)
						{
							this.pendingWaitersToExpire.Remove(asyncResultWaiter);
						}
						ThreadPool.QueueUserWorkItem((object o) => ((AsyncResult<int>)o).Complete(null, false), asyncResultWaiter.AsyncResult);
					}
					else
					{
						this.pending.Dequeue();
						this.numExpiredWaitersInQueue--;
					}
				}
			}
		}

		private void TimeoutExpiredWaiters(object state)
		{
			DateTime dateTime = DateTime.UtcNow.Add(AsyncSemaphore.FutureExpiryThreshold);
			List<AsyncResultWaiter<int>> asyncResultWaiters = new List<AsyncResultWaiter<int>>(1);
			lock (this.syncObj)
			{
				while (this.pendingWaitersToExpire.Count > 0)
				{
					AsyncResultWaiter<int> asyncResultWaiter = this.First<AsyncResultWaiter<int>>(this.pendingWaitersToExpire.Keys);
					if (asyncResultWaiter.ExpiryTime >= dateTime)
					{
						TimeSpan zero = asyncResultWaiter.ExpiryTime.Subtract(DateTime.UtcNow);
						if (zero < TimeSpan.Zero)
						{
							zero = TimeSpan.Zero;
						}
						this.timer.Change(zero, TimeSpan.FromMilliseconds(-1));
						break;
					}
					else
					{
						asyncResultWaiters.Add(asyncResultWaiter);
						this.pendingWaitersToExpire.Remove(asyncResultWaiter);
						asyncResultWaiter.StillPending = false;
						this.numExpiredWaitersInQueue++;
					}
				}
			}
			foreach (AsyncResultWaiter<int> asyncResultWaiter1 in asyncResultWaiters)
			{
				ThreadPool.QueueUserWorkItem((object delegateState) => ((AsyncResult<int>)delegateState).Complete(new TimeoutException("The resource(s) could not be acquired within the specified timeout period."), false), asyncResultWaiter1.AsyncResult);
			}
		}
	}
}