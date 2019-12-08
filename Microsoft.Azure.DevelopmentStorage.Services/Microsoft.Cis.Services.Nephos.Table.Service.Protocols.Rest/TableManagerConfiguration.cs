using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class TableManagerConfiguration
	{
		public int MaxConcurrentSynchronousCalls
		{
			get;
			private set;
		}

		public int MaxPendingSynchronousCalls
		{
			get;
			private set;
		}

		public TableManagerConfiguration(int maxConcurrentSynchronousCalls, int maxPendingSynchronousCalls)
		{
			NephosAssertionException.Assert(maxPendingSynchronousCalls > 0, "maxPendingSynchronousCalls must be greater than 0");
			this.MaxConcurrentSynchronousCalls = maxConcurrentSynchronousCalls;
			this.MaxPendingSynchronousCalls = maxPendingSynchronousCalls;
		}
	}
}