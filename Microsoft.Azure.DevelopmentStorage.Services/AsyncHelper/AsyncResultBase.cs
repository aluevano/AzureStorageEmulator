using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace AsyncHelper
{
	[DebuggerStepThrough]
	public class AsyncResultBase : IAsyncResult
	{
		private string methodName;

		private AsyncCallback callback;

		private object state;

		private Exception exception;

		private volatile EventWaitHandle waitHandle;

		private readonly static EventWaitHandle completeWaitHandleSentinel;

		private bool completedSynchronously;

		public object AsyncState
		{
			get
			{
				return this.state;
			}
		}

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				EventWaitHandle eventWaitHandle = this.waitHandle;
				if (eventWaitHandle != null && AsyncResultBase.completeWaitHandleSentinel != eventWaitHandle)
				{
					return eventWaitHandle;
				}
				EventWaitHandle eventWaitHandle1 = null;
				if (eventWaitHandle == null)
				{
					eventWaitHandle1 = new EventWaitHandle(false, EventResetMode.ManualReset);
					eventWaitHandle = Interlocked.CompareExchange<EventWaitHandle>(ref this.waitHandle, eventWaitHandle1, null);
				}
				if (AsyncResultBase.completeWaitHandleSentinel == eventWaitHandle)
				{
					if (eventWaitHandle1 != null)
					{
						eventWaitHandle1.Set();
					}
					else
					{
						eventWaitHandle1 = new EventWaitHandle(true, EventResetMode.ManualReset);
					}
					eventWaitHandle = Interlocked.CompareExchange<EventWaitHandle>(ref this.waitHandle, eventWaitHandle1, AsyncResultBase.completeWaitHandleSentinel);
				}
				if (eventWaitHandle == null || AsyncResultBase.completeWaitHandleSentinel == eventWaitHandle)
				{
					return eventWaitHandle1;
				}
				if (eventWaitHandle1 != null)
				{
					eventWaitHandle1.Close();
				}
				return eventWaitHandle;
			}
		}

		public bool CompletedSynchronously
		{
			get
			{
				return this.completedSynchronously;
			}
		}

		public bool IsCompleted
		{
			get
			{
				EventWaitHandle eventWaitHandle = this.waitHandle;
				if (eventWaitHandle == null)
				{
					return false;
				}
				if (eventWaitHandle == AsyncResultBase.completeWaitHandleSentinel)
				{
					return true;
				}
				return eventWaitHandle.WaitOne(0, false);
			}
		}

		internal string MethodName
		{
			get
			{
				return this.methodName;
			}
		}

		static AsyncResultBase()
		{
			AsyncResultBase.completeWaitHandleSentinel = new EventWaitHandle(true, EventResetMode.ManualReset);
		}

		protected AsyncResultBase(string methodName, AsyncCallback callback, object state)
		{
			this.methodName = methodName;
			this.callback = callback;
			this.state = state;
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId="0#")]
		public void End(out Exception exceptionToThrow)
		{
			this.End(out exceptionToThrow, RethrowableWrapperBehavior.NoWrap);
		}

		[SuppressMessage("Anvil.NullPtr", "26501", Justification="AsyncWaitHandle always returns a non-null value")]
		public void End(out Exception exceptionToThrow, RethrowableWrapperBehavior wrapperBehavior)
		{
			if (AsyncResultBase.completeWaitHandleSentinel != this.waitHandle)
			{
				this.AsyncWaitHandle.WaitOne();
				this.waitHandle.Close();
				this.waitHandle = null;
			}
			exceptionToThrow = null;
			try
			{
				exceptionToThrow = ExceptionCloner.AttemptClone(this.exception, wrapperBehavior);
			}
			catch (TypeInitializationException typeInitializationException)
			{
			}
			this.exception = null;
		}

		internal void InternalComplete(Exception exception, bool completedSynchronously)
		{
			this.exception = exception;
			this.completedSynchronously = completedSynchronously;
			EventWaitHandle eventWaitHandle = Interlocked.CompareExchange<EventWaitHandle>(ref this.waitHandle, AsyncResultBase.completeWaitHandleSentinel, null);
			if (AsyncResultBase.completeWaitHandleSentinel == eventWaitHandle)
			{
				AsyncCallTracker asyncState = this.AsyncState as AsyncCallTracker;
				string str = (asyncState == null ? "" : string.Concat("Trace: ", asyncState.GetCallStack()));
				throw new InvalidOperationException(string.Concat("Async operation ", this.MethodName, " completed twice. ", str));
			}
			if (eventWaitHandle != null)
			{
				if (eventWaitHandle.WaitOne(0, false))
				{
					AsyncCallTracker asyncCallTracker = this.AsyncState as AsyncCallTracker;
					string str1 = (asyncCallTracker == null ? "" : string.Concat("Trace: ", asyncCallTracker.GetCallStack()));
					throw new InvalidOperationException(string.Concat("Async operation ", this.MethodName, " completed twice. ", str1));
				}
				eventWaitHandle.Set();
			}
			AsyncCallback asyncCallback = this.callback;
			if (asyncCallback != null)
			{
				this.callback = null;
				asyncCallback(this);
			}
		}
	}
}