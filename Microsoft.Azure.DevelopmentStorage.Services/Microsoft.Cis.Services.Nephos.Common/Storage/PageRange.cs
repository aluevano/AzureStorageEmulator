using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class PageRange : IPageRange
	{
		private long pageStart;

		private long pageEnd;

		private bool isClear;

		public bool IsClear
		{
			get
			{
				return this.isClear;
			}
		}

		public long PageEnd
		{
			get
			{
				return this.pageEnd;
			}
		}

		public long PageStart
		{
			get
			{
				return this.pageStart;
			}
		}

		public PageRange(long pageStart, long pageEnd)
		{
			this.pageStart = pageStart;
			this.pageEnd = pageEnd;
			this.isClear = false;
		}

		public PageRange(long pageStart, long pageEnd, bool isClear)
		{
			this.pageStart = pageStart;
			this.pageEnd = pageEnd;
			this.isClear = isClear;
		}
	}
}