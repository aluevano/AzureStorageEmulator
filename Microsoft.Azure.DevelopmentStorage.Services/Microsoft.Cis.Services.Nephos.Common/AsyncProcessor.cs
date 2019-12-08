using AsyncHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class AsyncProcessor : IDisposable
	{
		private readonly Queue<AsyncProcessor.WorkItemJob> workItemQueue;

		private readonly object syncObj = new object();

		private readonly List<Thread> workerThreads = new List<Thread>();

		private readonly int maxConcurrent;

		private readonly int maxPending;

		private AsyncProcessor.State state;

		private bool disposed;

		private Semaphore semaphore;

		private AsyncSemaphore asyncSemaphore;

		public AsyncProcessor(int maxConcurrent, int maxPending)
		{
			if (maxConcurrent < 1)
			{
				throw new ArgumentOutOfRangeException("maxConcurrent", "must be at least 1");
			}
			if (maxPending < 1)
			{
				throw new ArgumentOutOfRangeException("maxPending", "must be at least 1");
			}
			this.maxConcurrent = maxConcurrent;
			this.maxPending = maxPending;
			this.asyncSemaphore = new AsyncSemaphore(maxConcurrent, maxConcurrent, maxPending);
			this.workItemQueue = new Queue<AsyncProcessor.WorkItemJob>();
			this.semaphore = new Semaphore(0, 2 * maxConcurrent);
		}

		public IAsyncResult BeginExecute(AsyncProcessor.WorkItem workItem, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return this.BeginExecute<NoResults>((TimeSpan param0) => {
				workItem(timeout);
				return new NoResults();
			}, timeout, callback, state);
		}

		public IAsyncResult BeginExecute<T>(AsyncProcessor.WorkItem<T> workItem, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<T> asyncIteratorContext = new AsyncIteratorContext<T>("AsyncProcessor.Execute", callback, state);
			asyncIteratorContext.Begin(this.ExecuteImpl<T>(workItem, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			if (disposing)
			{
				if (this.semaphore != null)
				{
					this.semaphore.Close();
					this.semaphore = null;
				}
				if (this.asyncSemaphore != null)
				{
					this.asyncSemaphore.Dispose();
					this.asyncSemaphore = null;
				}
			}
			this.disposed = true;
		}

		public void EndExecute(IAsyncResult asyncResult)
		{
			this.EndExecute<NoResults>(asyncResult);
		}

		public T EndExecute<T>(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<T> asyncIteratorContext = (AsyncIteratorContext<T>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		private IEnumerator<IAsyncResult> ExecuteImpl<T>(AsyncProcessor.WorkItem<T> workItem, TimeSpan timeout, AsyncIteratorContext<T> context)
		{
			object obj = null;
			object obj1 = null;
			Duration startingNow = Duration.StartingNow;
			if (timeout == TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in AsyncSemaphore");
			}
			if (timeout < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("timeout", (object)timeout, "Must be >= 0");
			}
			bool flag = false;
			try
			{
				object obj2 = this.syncObj;
				object obj3 = obj2;
				obj = obj2;
				Monitor.Enter(obj3, ref flag);
				if (this.state != AsyncProcessor.State.Running)
				{
					throw new InvalidOperationException("AsyncProcessor must be running first");
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(obj);
				}
			}
			IAsyncResult asyncResult = this.asyncSemaphore.BeginAcquire(timeout, context.GetResumeCallback(), context.GetResumeState("AsyncProcessor.ExecuteImpl"));
			yield return asyncResult;
			this.asyncSemaphore.EndAcquire(asyncResult);
			using (AsyncSignal<T> asyncSignal = new AsyncSignal<T>())
			{
				asyncResult = asyncSignal.BeginWait(startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("AsyncProcessor.ExecuteImpl"));
				Guid activityId = AsyncHelper.Trace.ActivityId;
				bool flag1 = false;
				try
				{
					object obj4 = this.syncObj;
					object obj5 = obj4;
					obj1 = obj4;
					Monitor.Enter(obj5, ref flag1);
					Queue<AsyncProcessor.WorkItemJob> workItemJobs = this.workItemQueue;
					workItemJobs.Enqueue(new AsyncProcessor.WorkItemJob(() => {
						Exception exception = null;
						T cSu0024u003cu003e8_localsa = default(T);
						try
						{
							AsyncHelper.Trace.ActivityId = activityId;
							cSu0024u003cu003e8_localsa = workItem(startingNow.Remaining(timeout));
						}
						catch (Exception exception1)
						{
							exception = exception1;
						}
						if (exception != null)
						{
							asyncSignal.Abort(exception);
						}
						else
						{
							asyncSignal.Set(cSu0024u003cu003e8_localsa);
						}
						this.asyncSemaphore.Release(1);
					}));
				}
				finally
				{
					if (flag1)
					{
						Monitor.Exit(obj1);
					}
				}
				this.semaphore.Release();
				yield return asyncResult;
				context.ResultData = asyncSignal.EndWait(asyncResult);
			}
		}

		public void Initialize()
		{
			lock (this.syncObj)
			{
				if (this.state != AsyncProcessor.State.Shutdown)
				{
					throw new InvalidOperationException("AsyncProcessor must be shutdown before it can be started");
				}
				for (int i = 0; i < this.maxConcurrent; i++)
				{
					Thread thread = new Thread(new ThreadStart(this.WorkerThreadMainLoop));
					thread.Start();
					this.workerThreads.Add(thread);
				}
				this.state = AsyncProcessor.State.Running;
			}
		}

		public void Shutdown()
		{
			List<Thread> threads = null;
			lock (this.syncObj)
			{
				if (this.state != AsyncProcessor.State.Running)
				{
					throw new InvalidOperationException("AsyncProcessor must be running before it can be stopped");
				}
				this.state = AsyncProcessor.State.ShutdownPending;
				threads = new List<Thread>(this.workerThreads);
				this.workerThreads.Clear();
			}
			for (int i = 0; i < this.maxConcurrent; i++)
			{
				this.semaphore.Release();
			}
			foreach (Thread thread in threads)
			{
				thread.Join();
			}
			lock (this.syncObj)
			{
				this.state = AsyncProcessor.State.Shutdown;
			}
		}

		private void WorkerThreadMainLoop()
		{
			while (true)
			{
				this.semaphore.WaitOne();
				AsyncProcessor.WorkItemJob workItemJob = null;
				lock (this.syncObj)
				{
					if (this.state != AsyncProcessor.State.ShutdownPending)
					{
						workItemJob = this.workItemQueue.Dequeue();
					}
					else
					{
						break;
					}
				}
				workItemJob();
			}
		}

		private enum State
		{
			Shutdown,
			Running,
			ShutdownPending
		}

		public delegate void WorkItem(TimeSpan timeout);

		public delegate T WorkItem<T>(TimeSpan timeout);

		public delegate void WorkItemJob();
	}
}