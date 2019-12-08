using System;
using System.Collections.Generic;

namespace AsyncHelper
{
	public struct AsyncResultWaiterComparer<T> : IComparer<AsyncResultWaiter<T>>
	{
		public int Compare(AsyncResultWaiter<T> x, AsyncResultWaiter<T> y)
		{
			int num = x.ExpiryTime.CompareTo(y.ExpiryTime);
			if (num == 0)
			{
				num = x.WaiterIndex.CompareTo(y.WaiterIndex);
			}
			return num;
		}
	}
}