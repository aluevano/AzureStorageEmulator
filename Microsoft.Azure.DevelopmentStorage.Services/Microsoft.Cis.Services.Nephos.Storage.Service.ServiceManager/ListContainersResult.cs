using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ListContainersResult : IListContainersResultCollection, IEnumerable<IListContainersResultContainerProperties>, IEnumerable
	{
		private IBlobContainerCollection containers;

		public string NextMarker
		{
			get
			{
				return this.containers.NextContainerStart;
			}
		}

		public ListContainersResult(IBlobContainerCollection containers)
		{
			this.containers = containers;
		}

		public IEnumerator<IListContainersResultContainerProperties> GetEnumerator()
		{
			foreach (IBaseBlobContainer container in this.containers)
			{
				yield return new ListContainersResultContainerProperties(container);
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}