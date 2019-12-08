using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Common
{
	public static class TableXmlConstants
	{
		public const string DataWebNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices";

		public const string DataWebMetadataNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

		public const string XmlnsNamespacePrefix = "xmlns";

		public const string XmlNamespacePrefix = "xml";

		public const string XmlErrorElementName = "error";

		public const string XmlErrorCodeElementName = "code";

		public const string XmlErrorInnerElementName = "innererror";

		public const string XmlErrorInternalExceptionElementName = "internalexception";

		public const string XmlErrorTypeElementName = "type";

		public const string XmlErrorStackTraceElementName = "stacktrace";

		public const string XmlErrorMessageElementName = "message";

		public const string XmlLangAttributeName = "lang";

		public const string XmlErrorMessageLanguage = "en-US";

		public const string TablesContainerName = "Tables";

		public const string TableColumnName = "Name";

		public const string PartitionKeyColumnName = "PartitionKey";

		public const string RowKeyColumnName = "RowKey";

		public const string TimestampColumnName = "Timestamp";

		public const string ETagColumnName = "Timestamp";

		public const string HttpDataServiceVersion = "DataServiceVersion";

		public const string HttpMaxDataServiceVersion = "MaxDataServiceVersion";

		public const string HttpMethodMerge = "MERGE";

		public const string HttpXMethod = "X-HTTP-Method";

		public const string UriBatchSegment = "$batch";
	}
}