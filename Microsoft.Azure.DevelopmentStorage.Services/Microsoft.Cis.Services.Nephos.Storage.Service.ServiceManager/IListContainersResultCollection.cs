using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IListContainersResultCollection : IEnumerable<IListContainersResultContainerProperties>, IEnumerable
	{
		string NextMarker
		{
			get;
		}
	}
}