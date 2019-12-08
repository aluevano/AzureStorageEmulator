using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlobContainerCollection : IContainerCollection<IBaseBlobContainer>, IEnumerable<IBaseBlobContainer>, IEnumerable, IDisposable
	{

	}
}