using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Common
{
	public static class TableStatusEntries
	{
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry XMethodNotUsingPost;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry XMethodIncorrectValue;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry XMethodIncorrectCount;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry TableHasNoProperties;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry DuplicatePropertiesSpecified;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry TableHasNoSuchProperty;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry DuplicateKeyPropertySpecified;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry TableAlreadyExists;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry TableNotFound;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry EntityNotFound;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry EntityAlreadyExists;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry PartitionKeyNotSpecified;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry OperatorInvalid;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry UpdateConditionNotSatisfied;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry PropertiesNeedValue;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry PartitionKeyPropertyCannotBeUpdated;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry TooManyProperties;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry EntityTooLarge;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry PropertyValueTooLarge;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry PartitionKeyValueTooLarge;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry RowKeyValueTooLarge;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry InvalidValueType;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry TableBeingDeleted;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry PropertyNameTooLong;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry PropertyNameInvalid;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry CommandsInBatchActOnDifferentPartitions;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry InvalidDuplicateRow;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry JsonFormatNotSupported;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry AtomFormatNotSupported;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry JsonVerboseFormatNotSupported;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry MediaTypeNotSupported;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry MethodNotAllowed;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry ContentLengthExceeded;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry AccountIOPSLimitExceeded;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry CannotCreateTableWithIOPSGreaterThanMaxAllowedPerTable;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry TableExceedsPerTableIOPSIncrementLimit;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry TableExceedsPerTableIOPSDecrementLimit;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry TableInProvisioningState;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry PartitionKeyEqualityComparisonExpectedForPremiumTable;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="NephosStatusEntry is an immutable type")]
		public readonly static NephosStatusEntry PartitionKeySpecifiedMoreThanOnceForPremiumTable;

		static TableStatusEntries()
		{
			TableStatusEntries.XMethodNotUsingPost = new NephosStatusEntry("XMethodNotUsingPost", HttpStatusCode.BadRequest, "The request uses X-HTTP-Method with a Http Verb that is not POST.");
			TableStatusEntries.XMethodIncorrectValue = new NephosStatusEntry("XMethodIncorrectValue", HttpStatusCode.BadRequest, "The specified X-HTTP-Method is invalid.");
			TableStatusEntries.XMethodIncorrectCount = new NephosStatusEntry("XMethodIncorrectCount", HttpStatusCode.BadRequest, "More than one X-HTTP-Method is specified.");
			TableStatusEntries.TableHasNoProperties = new NephosStatusEntry("TableHasNoProperties", HttpStatusCode.NotFound, "The table has no properties.");
			TableStatusEntries.DuplicatePropertiesSpecified = new NephosStatusEntry("DuplicatePropertiesSpecified", HttpStatusCode.BadRequest, "A property is specified more than once.");
			TableStatusEntries.TableHasNoSuchProperty = new NephosStatusEntry("TableHasNoSuchProperty", HttpStatusCode.NotFound, "The property specified is not in the table.");
			TableStatusEntries.DuplicateKeyPropertySpecified = new NephosStatusEntry("DuplicateKeyPropertySpecified", HttpStatusCode.BadRequest, "A key property is specified more than once.");
			TableStatusEntries.TableAlreadyExists = new NephosStatusEntry("TableAlreadyExists", HttpStatusCode.Conflict, "The table specified already exists.");
			TableStatusEntries.TableNotFound = new NephosStatusEntry("TableNotFound", HttpStatusCode.NotFound, "The table specified does not exist.");
			TableStatusEntries.EntityNotFound = new NephosStatusEntry("EntityNotFound", HttpStatusCode.NotFound, "The specified entity does not exist.");
			TableStatusEntries.EntityAlreadyExists = new NephosStatusEntry("EntityAlreadyExists", HttpStatusCode.Conflict, "The specified entity already exists.");
			TableStatusEntries.PartitionKeyNotSpecified = new NephosStatusEntry("PartitionKeyNotSpecified", HttpStatusCode.BadRequest, "The required property 'PartitionKey' is not specified.");
			TableStatusEntries.OperatorInvalid = new NephosStatusEntry("OperatorInvalid", HttpStatusCode.BadRequest, "The operator specified is invalid.");
			TableStatusEntries.UpdateConditionNotSatisfied = new NephosStatusEntry("UpdateConditionNotSatisfied", HttpStatusCode.PreconditionFailed, "The update condition specified in the request was not satisfied.");
			TableStatusEntries.PropertiesNeedValue = new NephosStatusEntry("PropertiesNeedValue", HttpStatusCode.BadRequest, "The values are not specified for all properties in the entity.");
			TableStatusEntries.PartitionKeyPropertyCannotBeUpdated = new NephosStatusEntry("PartitionKeyPropertyCannotBeUpdated", HttpStatusCode.BadRequest, "The 'PartitionKey' property cannot be udpated.");
			TableStatusEntries.TooManyProperties = new NephosStatusEntry("TooManyProperties", HttpStatusCode.BadRequest, "The entity contains more properties than allowed. Each entity can include up to 252 properties to store data. Each entity also has 3 system properties.");
			TableStatusEntries.EntityTooLarge = new NephosStatusEntry("EntityTooLarge", HttpStatusCode.BadRequest, "The entity is larger than the maximum allowed size (1MB).");
			TableStatusEntries.PropertyValueTooLarge = new NephosStatusEntry("PropertyValueTooLarge", HttpStatusCode.BadRequest, "The property value exceeds the maximum allowed size (64KB). If the property value is a string, it is UTF-16 encoded and the maximum number of characters should be 32K or less.");
			TableStatusEntries.PartitionKeyValueTooLarge = new NephosStatusEntry("KeyValueTooLarge", HttpStatusCode.BadRequest, "The 'PartitionKey' property exceeds the maximum allowed key size (2KB). It is a UTF-16 encoded string and the maximum number of characters should be 1K or less.");
			TableStatusEntries.RowKeyValueTooLarge = new NephosStatusEntry("KeyValueTooLarge", HttpStatusCode.BadRequest, "The 'RowKey' property exceeds the maximum allowed key size (2KB). It is a UTF-16 encoded string and the maximum number of characters should be 1K or less.");
			TableStatusEntries.InvalidValueType = new NephosStatusEntry("InvalidValueType", HttpStatusCode.BadRequest, "The value specified is invalid.");
			TableStatusEntries.TableBeingDeleted = new NephosStatusEntry("TableBeingDeleted", HttpStatusCode.Conflict, "The specified table is being deleted. Try operation later.");
			TableStatusEntries.PropertyNameTooLong = new NephosStatusEntry("PropertyNameTooLong", HttpStatusCode.BadRequest, "The property name exceeds the maximum allowed length (255).");
			TableStatusEntries.PropertyNameInvalid = new NephosStatusEntry("PropertyNameInvalid", HttpStatusCode.BadRequest, "The property name is invalid.");
			TableStatusEntries.CommandsInBatchActOnDifferentPartitions = new NephosStatusEntry("CommandsInBatchActOnDifferentPartitions", HttpStatusCode.BadRequest, "All commands in a batch must operate on same entity group.");
			TableStatusEntries.InvalidDuplicateRow = new NephosStatusEntry("InvalidDuplicateRow", HttpStatusCode.BadRequest, "The batch request contains multiple changes with same row key. An entity can appear only once in a batch request.");
			TableStatusEntries.JsonFormatNotSupported = new NephosStatusEntry("JsonFormatNotSupported", HttpStatusCode.UnsupportedMediaType, "JSON format is not supported.");
			TableStatusEntries.AtomFormatNotSupported = new NephosStatusEntry("AtomFormatNotSupported", HttpStatusCode.UnsupportedMediaType, "Atom format is not supported.");
			TableStatusEntries.JsonVerboseFormatNotSupported = new NephosStatusEntry("JsonVerboseFormatNotSupported", HttpStatusCode.UnsupportedMediaType, "Verbose JSON format is not supported.");
			TableStatusEntries.MediaTypeNotSupported = new NephosStatusEntry("MediaTypeNotSupported", HttpStatusCode.UnsupportedMediaType, "None of the provided media types are supported");
			TableStatusEntries.MethodNotAllowed = new NephosStatusEntry("MethodNotAllowed", HttpStatusCode.MethodNotAllowed, "The requested method is not allowed on the specified resource.");
			TableStatusEntries.ContentLengthExceeded = new NephosStatusEntry("ContentLengthExceeded", HttpStatusCode.BadRequest, "The content length for the requested operation has exceeded the limit (4MB).");
			TableStatusEntries.AccountIOPSLimitExceeded = new NephosStatusEntry("AccountIOPSLimitExceeded", HttpStatusCode.BadRequest, "The requested IOPS value causes the account IOPS limit to be exceeded");
			TableStatusEntries.CannotCreateTableWithIOPSGreaterThanMaxAllowedPerTable = new NephosStatusEntry("CannotCreateTableWithIOPSGreaterThanMaxAllowedPerTable", HttpStatusCode.BadRequest, "The specified IOPS value is more than the allowed limit per table");
			TableStatusEntries.TableExceedsPerTableIOPSIncrementLimit = new NephosStatusEntry("PerTableIOPSIncrementLimitReached", HttpStatusCode.Conflict, "The number of IOPS increments for this table has reached the limit");
			TableStatusEntries.TableExceedsPerTableIOPSDecrementLimit = new NephosStatusEntry("PerTableIOPSDecrementLimitReached", HttpStatusCode.Conflict, "The number of IOPS decrements for this table has reached the limit");
			TableStatusEntries.TableInProvisioningState = new NephosStatusEntry("SetttingIOPSForATableInProvisioningNotAllowed", HttpStatusCode.Conflict, "Cannot change IOPS for a table which is in provisioning state");
			TableStatusEntries.PartitionKeyEqualityComparisonExpectedForPremiumTable = new NephosStatusEntry("PartitionKeyEqualityComparisonExpected", HttpStatusCode.BadRequest, "For a premium table, an ordered query filter must be an equality comparison on PartitionKey, optionally combined with an expression using the AND operator");
			TableStatusEntries.PartitionKeySpecifiedMoreThanOnceForPremiumTable = new NephosStatusEntry("PartitionKeySpecifiedMoreThanOnce", HttpStatusCode.BadRequest, "For a premium table, an ordered query filter must not refer PartitionKey more than once");
		}
	}
}