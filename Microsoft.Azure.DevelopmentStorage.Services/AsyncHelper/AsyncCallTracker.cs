using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace AsyncHelper
{
	[DebuggerStepThrough]
	internal class AsyncCallTracker
	{
		private readonly string calleeName;

		private readonly AsyncIteratorContextBase caller;

		private readonly Guid activityId;

		private readonly Dictionary<string, object> metadata;

		private int state;

		public Guid ActivityId
		{
			get
			{
				return this.activityId;
			}
		}

		public string CalleeName
		{
			get
			{
				return this.calleeName;
			}
		}

		public AsyncIteratorContextBase Caller
		{
			get
			{
				return this.caller;
			}
		}

		[SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
		internal AsyncCallTracker(string calleeName, AsyncIteratorContextBase caller)
		{
			this.calleeName = calleeName;
			this.caller = caller;
			this.activityId = AsyncHelper.Trace.ActivityId;
			this.metadata = AsyncHelper.Trace.Metadata;
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification="As designed")]
		~AsyncCallTracker()
		{
			if (!Environment.HasShutdownStarted && 1 == Thread.VolatileRead(ref this.state))
			{
				throw new InvalidOperationException(string.Concat("Async call never completed.\n", this.GetCallStack()));
			}
		}

		internal string GetCallStack()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (AsyncCallTracker i = this; i != null; i = i.caller.Caller)
			{
				stringBuilder.AppendFormat("from {0} to {1}\n", i.caller.MethodName, i.calleeName);
			}
			return stringBuilder.ToString();
		}

		internal void IndicateCallComplete()
		{
			GC.SuppressFinalize(this);
			AsyncCallTracker.State state = (AsyncCallTracker.State)Interlocked.Exchange(ref this.state, 2);
			if (AsyncCallTracker.State.Complete == state)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] methodName = new object[] { this.caller.MethodName, this.calleeName, this.activityId };
				throw new InvalidOperationException(string.Format(invariantCulture, "AsyncCallTracker.IndicateCallComplete called twice.  Caller={0}, Callee={1}, this.activityId={2}", methodName));
			}
			if (AsyncCallTracker.State.CallerWaiting == state)
			{
				AsyncHelper.Trace.ActivityId = this.activityId;
				AsyncHelper.Trace.Metadata = this.metadata;
				this.caller.ResumeExecution();
			}
		}

		public override string ToString()
		{
			return string.Concat(this.caller.MethodName, "-->", this.calleeName);
		}

		internal bool Wait()
		{
			AsyncCallTracker.State state = (AsyncCallTracker.State)Interlocked.CompareExchange(ref this.state, 1, 0);
			if (AsyncCallTracker.State.CallerWaiting == state)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] methodName = new object[] { this.caller.MethodName, this.calleeName };
				throw new InvalidOperationException(string.Format(invariantCulture, "AsyncCallTracker.Wait called twice.  Caller={0}, Callee={1}", methodName));
			}
			if (state == AsyncCallTracker.State.Begun)
			{
				return false;
			}
			return true;
		}

		private enum State
		{
			Begun,
			CallerWaiting,
			Complete
		}
	}
}