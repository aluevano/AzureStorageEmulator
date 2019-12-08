using System;

namespace AsyncHelper
{
	public class AsyncResultWaiter<T>
	{
		private readonly AsyncResult<T> asyncResult;

		private readonly DateTime expiryTime;

		private readonly int waiterIndex;

		private bool stillPending;

		public AsyncResult<T> AsyncResult
		{
			get
			{
				return this.asyncResult;
			}
		}

		public DateTime ExpiryTime
		{
			get
			{
				return this.expiryTime;
			}
		}

		public bool StillPending
		{
			get
			{
				return this.stillPending;
			}
			set
			{
				this.stillPending = value;
			}
		}

		public int WaiterIndex
		{
			get
			{
				return this.waiterIndex;
			}
		}

		public AsyncResultWaiter(AsyncResult<T> asyncResult, DateTime expiryTime, int waiterIndex)
		{
			this.stillPending = true;
			this.asyncResult = asyncResult;
			this.expiryTime = expiryTime;
			this.waiterIndex = waiterIndex;
		}
	}
}