using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IContainerCollection<TContainer> : IEnumerable<TContainer>, IEnumerable, IDisposable
	where TContainer : IContainer
	{
		bool HasMoreRows
		{
			get;
		}

		string NextContainerStart
		{
			get;
		}
	}
}