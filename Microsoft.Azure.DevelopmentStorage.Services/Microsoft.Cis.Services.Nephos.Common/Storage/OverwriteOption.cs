using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public enum OverwriteOption
	{
		None,
		CreateNewOnly,
		UpdateExistingOnly,
		CreateNewOrUpdateExisting
	}
}