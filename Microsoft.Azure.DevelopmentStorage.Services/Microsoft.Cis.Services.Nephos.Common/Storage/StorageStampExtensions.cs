using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public static class StorageStampExtensions
	{
		public static IStorageAccount CreateAccountInstance(this IStorageStamp storageStamp, string accountName)
		{
			return storageStamp.CreateAccountInstance(accountName, null);
		}
	}
}