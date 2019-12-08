using System;
using System.Diagnostics;

namespace AsyncHelper
{
	[DebuggerStepThrough]
	public class AsyncResult<ResultDataType> : AsyncResultBase
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

		public AsyncResult(string methodName, AsyncCallback callback, object state) : base(methodName, callback, state)
		{
		}

		public void Complete(Exception exception, bool completedSynchronously)
		{
			base.InternalComplete(exception, completedSynchronously);
		}
	}
}