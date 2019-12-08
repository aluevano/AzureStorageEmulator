using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class StorageOperationContext
	{
		public TimeSpan SmbOpLockBreakLatency
		{
			get;
			set;
		}

		public StorageOperationContext()
		{
		}
	}
}