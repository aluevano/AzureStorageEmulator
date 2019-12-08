using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class ListContainersOperationContext : ListBaseObjectsOperationContext
	{
		public bool IncludeSnapshots
		{
			get;
			set;
		}

		public bool IsIncludingShareQuotaInResponse
		{
			get;
			set;
		}

		public ListContainersOperationContext(string version) : base(version)
		{
		}
	}
}