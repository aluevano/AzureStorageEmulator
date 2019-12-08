using System;

namespace AsyncHelper
{
	public class Tuple<Type1, Type2>
	{
		private AsyncHelper.Type1 first;

		private AsyncHelper.Type2 second;

		public AsyncHelper.Type1 First
		{
			get
			{
				return this.first;
			}
			set
			{
				this.first = value;
			}
		}

		public AsyncHelper.Type2 Second
		{
			get
			{
				return this.second;
			}
			set
			{
				this.second = value;
			}
		}

		public Tuple()
		{
		}

		public Tuple(AsyncHelper.Type1 first, AsyncHelper.Type2 second)
		{
			this.first = first;
			this.second = second;
		}
	}
}