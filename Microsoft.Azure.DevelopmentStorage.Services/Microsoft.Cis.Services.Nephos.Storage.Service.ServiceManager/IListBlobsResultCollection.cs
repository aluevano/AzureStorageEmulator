using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IListBlobsResultCollection : IEnumerable<IListBlobResultsBlobProperties>, IEnumerable, IDisposable
	{
		string NextMarker
		{
			get;
		}
	}
}