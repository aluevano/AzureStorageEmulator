using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public enum UpdateKind
	{
		None,
		Insert,
		Delete,
		Replace,
		Merge,
		InsertOrMerge,
		InsertOrReplace
	}
}