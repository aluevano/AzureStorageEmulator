using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IPageRange
	{
		bool IsClear
		{
			get;
		}

		long PageEnd
		{
			get;
		}

		long PageStart
		{
			get;
		}
	}
}