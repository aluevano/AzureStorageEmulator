using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface ITableContainerCollection : IContainerCollection<ITableContainer>, IEnumerable<ITableContainer>, IEnumerable, IDisposable
	{

	}
}