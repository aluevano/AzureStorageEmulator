using System;
using System.Diagnostics;

namespace AsyncHelper
{
	[DebuggerStepThrough]
	public sealed class AsyncIteratorContext : AsyncIteratorContextBase
	{
		public AsyncIteratorContext(string methodName, AsyncCallback callback, object state) : base(methodName, callback, state)
		{
		}
	}
}