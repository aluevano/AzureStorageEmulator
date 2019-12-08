using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncHelper
{
	public class AsyncSignal<SignalDataType> : IDisposable
	{
		private readonly static TimeSpan FutureExpiryThreshold;

		protected SortedList<AsyncResultWaiter<SignalDataType>, AsyncSignal<SignalDataType>.NoAdditionalData> waiters;

		protected Timer timer;

		protected int waiterCounter;

		protected object syncObj;

		protected readonly static AsyncResultWaiterComparer<SignalDataType> WaiterComparer;

		protected readonly static AsyncSignal<SignalDataType>.NoAdditionalData NoData;

		static AsyncSignal()
		{
			AsyncSignal<SignalDataType>.FutureExpiryThreshold = TimeSpan.FromMilliseconds(50);
			AsyncSignal<SignalDataType>.WaiterComparer = new AsyncResultWaiterComparer<SignalDataType>();
			AsyncSignal<SignalDataType>.NoData = new AsyncSignal<SignalDataType>.NoAdditionalData();
		}

		public AsyncSignal()
		{
		}

		public virtual void Abort(Exception exception)
		{
			List<AsyncResultWaiter<SignalDataType>> asyncResultWaiters = null;
			lock (this.syncObj)
			{
				if (this.waiters != null)
				{
					asyncResultWaiters = new List<AsyncResultWaiter<SignalDataType>>(this.waiters.Keys);
					this.waiters.Clear();
				}
			}
			if (asyncResultWaiters != null)
			{
				foreach (AsyncResultWaiter<SignalDataType> asyncResultWaiter in asyncResultWaiters)
				{
					ThreadPool.QueueUserWorkItem((object state) => ((AsyncResult<SignalDataType>)state).Complete(exception, false), asyncResultWaiter.AsyncResult);
				}
			}
		}

		public virtual IAsyncResult BeginWait(TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncResult<SignalDataType> asyncResult = new AsyncResult<SignalDataType>("AsyncSignal.Wait", callback, state);
			DateTime dateTime = (timeout == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.UtcNow.Add(timeout));
			lock (this.syncObj)
			{
				if (this.waiters == null)
				{
					this.waiters = new SortedList<AsyncResultWaiter<SignalDataType>, AsyncSignal<SignalDataType>.NoAdditionalData>(1, (object)AsyncSignal<SignalDataType>.WaiterComparer);
				}
				SortedList<AsyncResultWaiter<SignalDataType>, AsyncSignal<SignalDataType>.NoAdditionalData> asyncResultWaiters = this.waiters;
				AsyncSignal<SignalDataType> asyncSignal = this;
				int num = asyncSignal.waiterCounter;
				int num1 = num;
				asyncSignal.waiterCounter = num + 1;
				asyncResultWaiters.Add(new AsyncResultWaiter<SignalDataType>(asyncResult, dateTime, num1), AsyncSignal<SignalDataType>.NoData);
				if (timeout != TimeSpan.MaxValue)
				{
					if (this.timer == null)
					{
						this.timer = new Timer(new TimerCallback(this.TimeoutExpiredWaiters), null, timeout, TimeSpan.FromMilliseconds(-1));
					}
					else if (asyncResult == this.waiters.Keys[0].AsyncResult)
					{
						this.timer.Change(timeout, TimeSpan.FromMilliseconds(-1));
					}
				}
			}
			return asyncResult;
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification="Exception is not being raised during Dispose, it is being propagated to the thread waiting for the async operation completion")]
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (this.syncObj)
				{
					if (this.waiters != null && this.waiters.Count > 0)
					{
						this.Abort(new TimeoutException("The asynchronous signal was disposed."));
					}
					if (this.timer != null)
					{
						this.timer.Dispose();
						this.timer = null;
					}
				}
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual SignalDataType EndWait(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncResult<SignalDataType> asyncResult1 = (AsyncResult<SignalDataType>)asyncResult;
			asyncResult1.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncResult1.ResultData;
		}

		public virtual void Set(SignalDataType data)
		{
			List<AsyncResultWaiter<SignalDataType>> asyncResultWaiters = null;
			lock (this.syncObj)
			{
				if (this.waiters != null && this.waiters.Count > 0)
				{
					asyncResultWaiters = new List<AsyncResultWaiter<SignalDataType>>(this.waiters.Keys);
					this.waiters.Clear();
				}
			}
			if (asyncResultWaiters != null)
			{
				foreach (AsyncResultWaiter<SignalDataType> asyncResultWaiter in asyncResultWaiters)
				{
					AsyncResult<SignalDataType> asyncResult = asyncResultWaiter.AsyncResult;
					asyncResult.ResultData = data;
					ThreadPool.QueueUserWorkItem((object state) => ((AsyncResult<SignalDataType>)state).Complete(null, false), asyncResult);
				}
			}
		}

		protected void TimeoutExpiredWaiters(object state_ignored)
		{
			DateTime dateTime = DateTime.UtcNow.Add(AsyncSignal<SignalDataType>.FutureExpiryThreshold);
			List<AsyncResultWaiter<SignalDataType>> asyncResultWaiters = new List<AsyncResultWaiter<SignalDataType>>(1);
			lock (this.syncObj)
			{
				while (this.waiters.Count > 0)
				{
					AsyncResultWaiter<SignalDataType> item = this.waiters.Keys[0];
					if (item.ExpiryTime >= dateTime)
					{
						TimeSpan zero = item.ExpiryTime.Subtract(DateTime.UtcNow);
						if (zero < TimeSpan.Zero)
						{
							zero = TimeSpan.Zero;
						}
						this.timer.Change(zero, TimeSpan.FromMilliseconds(-1));
						break;
					}
					else
					{
						asyncResultWaiters.Add(item);
						this.waiters.RemoveAt(0);
					}
				}
			}
			foreach (AsyncResultWaiter<SignalDataType> asyncResultWaiter in asyncResultWaiters)
			{
				ThreadPool.QueueUserWorkItem((object state) => ((AsyncResult<SignalDataType>)state).Complete(new TimeoutException("The asynchronous signal was not set within the specified timeout period."), false), asyncResultWaiter.AsyncResult);
			}
		}

		protected internal struct NoAdditionalData
		{

		}
	}
}