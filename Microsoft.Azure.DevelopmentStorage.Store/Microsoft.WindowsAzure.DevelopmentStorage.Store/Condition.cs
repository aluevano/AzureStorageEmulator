using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal enum Condition
	{
		IfLastModificationTimeMatch,
		IfLastModificationTimeMismatch,
		IfModifiedSinceTime,
		IfNotModifiedSinceTime
	}
}