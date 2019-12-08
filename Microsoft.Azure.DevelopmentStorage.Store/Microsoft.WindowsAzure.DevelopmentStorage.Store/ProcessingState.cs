using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal enum ProcessingState
	{
		Projection,
		Where,
		Aggregation,
		OrderBy,
		Select
	}
}