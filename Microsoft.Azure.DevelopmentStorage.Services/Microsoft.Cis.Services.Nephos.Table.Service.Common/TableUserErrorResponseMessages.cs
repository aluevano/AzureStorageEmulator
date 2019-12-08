using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Common
{
	public static class TableUserErrorResponseMessages
	{
		public const string BatchRequestContainsDuplicateRow = "The batch request contains multiple changes with same row key. An entity can appear only once in a batch request.";
	}
}