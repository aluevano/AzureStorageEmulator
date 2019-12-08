using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IQueueContainerCollection : IContainerCollection<IQueueContainer>, IEnumerable<IQueueContainer>, IEnumerable, IDisposable
	{

	}
}