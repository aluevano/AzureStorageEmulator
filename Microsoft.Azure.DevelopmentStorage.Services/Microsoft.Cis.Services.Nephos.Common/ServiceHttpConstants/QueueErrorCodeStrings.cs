using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class QueueErrorCodeStrings
	{
		public const string QueueNotFound = "QueueNotFound";

		public const string QueueDisabled = "QueueDisabled";

		public const string QueueAlreadyExists = "QueueAlreadyExists";

		public const string QueueNotEmpty = "QueueNotEmpty";

		public const string QueueBeingDeleted = "QueueBeingDeleted";

		public const string PopReceiptMismatch = "PopReceiptMismatch";

		public const string InvalidParameter = "InvalidParameter";

		public const string MessageNotFound = "MessageNotFound";

		public const string MessageTooLarge = "MessageTooLarge";

		public const string InvalidMarker = "InvalidMarker";
	}
}