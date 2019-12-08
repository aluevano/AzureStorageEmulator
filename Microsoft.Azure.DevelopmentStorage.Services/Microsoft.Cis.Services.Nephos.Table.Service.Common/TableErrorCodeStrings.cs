using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Common
{
	public static class TableErrorCodeStrings
	{
		public const string XMethodNotUsingPost = "XMethodNotUsingPost";

		public const string XMethodIncorrectValue = "XMethodIncorrectValue";

		public const string XMethodIncorrectCount = "XMethodIncorrectCount";

		public const string TableHasNoProperties = "TableHasNoProperties";

		public const string DuplicatePropertiesSpecified = "DuplicatePropertiesSpecified";

		public const string TableHasNoSuchProperty = "TableHasNoSuchProperty";

		public const string DuplicateKeyPropertySpecified = "DuplicateKeyPropertySpecified";

		public const string TableAlreadyExists = "TableAlreadyExists";

		public const string TableNotFound = "TableNotFound";

		public const string EntityNotFound = "EntityNotFound";

		public const string EntityAlreadyExists = "EntityAlreadyExists";

		public const string PartitionKeyNotSpecified = "PartitionKeyNotSpecified";

		public const string OperatorInvalid = "OperatorInvalid";

		public const string UpdateConditionNotSatisfied = "UpdateConditionNotSatisfied";

		public const string PropertiesNeedValue = "PropertiesNeedValue";

		public const string PartitionKeyPropertyCannotBeUpdated = "PartitionKeyPropertyCannotBeUpdated";

		public const string TooManyProperties = "TooManyProperties";

		public const string EntityTooLarge = "EntityTooLarge";

		public const string PropertyValueTooLarge = "PropertyValueTooLarge";

		public const string KeyValueTooLarge = "KeyValueTooLarge";

		public const string InvalidValueType = "InvalidValueType";

		public const string TableBeingDeleted = "TableBeingDeleted";

		public const string PrimaryKeyPropertyIsInvalidType = "PrimaryKeyPropertyIsInvalidType";

		public const string PropertyNameTooLong = "PropertyNameTooLong";

		public const string PropertyNameInvalid = "PropertyNameInvalid";

		public const string InvalidDuplicateRow = "InvalidDuplicateRow";

		public const string CommandsInBatchActOnDifferentPartitions = "CommandsInBatchActOnDifferentPartitions";

		public const string JsonFormatNotSupported = "JsonFormatNotSupported";

		public const string AtomFormatNotSupported = "AtomFormatNotSupported";

		public const string JsonVerboseFormatNotSupported = "JsonVerboseFormatNotSupported";

		public const string MediaTypeNotSupported = "MediaTypeNotSupported";

		public const string MethodNotAllowed = "MethodNotAllowed";

		public const string ContentLengthExceeded = "ContentLengthExceeded";

		public const string AccountIOPSLimitExceeded = "AccountIOPSLimitExceeded";

		public const string CannotCreateTableWithIOPSGreaterThanMaxAllowedPerTable = "CannotCreateTableWithIOPSGreaterThanMaxAllowedPerTable";

		public const string PerTableIOPSIncrementLimitReached = "PerTableIOPSIncrementLimitReached";

		public const string PerTableIOPSDecrementLimitReached = "PerTableIOPSDecrementLimitReached";

		public const string SetttingIOPSForATableInProvisioningNotAllowed = "SetttingIOPSForATableInProvisioningNotAllowed";

		public const string PartitionKeyEqualityComparisonExpected = "PartitionKeyEqualityComparisonExpected";

		public const string PartitionKeySpecifiedMoreThanOnce = "PartitionKeySpecifiedMoreThanOnce";
	}
}