using System;
using System.Diagnostics;

namespace AsyncHelper
{
	[DebuggerStepThrough]
	public sealed class AsyncIteratorContext<ResultDataType> : AsyncIteratorContextBase
	{
		private ResultDataType resultData;

		public ResultDataType ResultData
		{
			get
			{
				return this.resultData;
			}
			set
			{
				this.resultData = value;
			}
		}

		public AsyncIteratorContext(string methodName, AsyncCallback callback, object state) : base(methodName, callback, state)
		{
		}
	}
}