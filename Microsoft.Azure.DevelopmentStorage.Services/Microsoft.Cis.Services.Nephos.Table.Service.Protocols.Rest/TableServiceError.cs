using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public enum TableServiceError
	{
		None,
		TableHasNoProperties,
		CommandsInBatchActOnDifferentPartitions,
		DuplicatePropertiesSpecified,
		TableHasNoSuchProperty,
		DuplicateKeyPropertySpecified,
		EntityNotFound,
		EntityAlreadyExists,
		PartitionKeyNotSpecified,
		OperatorInvalid,
		OperationTimedOut,
		UpdateConditionNotSatisfied,
		PropertiesNeedValue,
		PartitionKeyPropertyCannotBeUpdated,
		EntityTooLarge,
		PropertyValueTooLarge,
		InvalidValueType,
		PropertyNameTooLong,
		TooManyProperties,
		PropertyNameInvalid,
		ContentLengthExceeded
	}
}