using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class HeaderValues
	{
		public const string ContentTypeXml = "application/xml";

		public const string ContentTypeAtomXml = "application/atom+xml";

		public const string ContentTypeBatch = "multipart/mixed";

		public const string ApplicationJson = "application/json";

		public const string ApplicationJsonUtf8 = "application/json; charset=utf-8";

		public const string TRUE = "true";

		public const string FALSE = "false";

		public const string MimeApplicationJson = "application/json";

		public const string DefaultContentType = "application/octet-stream";

		public const string ContentEncodingGZip = "gzip";

		public const string HeaderUnitForByteRanges = "bytes";

		public const string HeaderSeparatorForStartAndEndInRanges = "-";

		public const string RangeHeaderSeparatorForUnitAndRange = "=";

		public const string RangeHeaderFormat = "bytes={0}-{1}";

		public const string ContentRangeHeaderSeparatorForUnitAndRange = " ";

		public const string ContentRangeHeaderFormat = "bytes {0}-{1}/{2}";

		public const string ContentRangeHeaderFormatWithoutRange = "bytes */{0}";

		public const string WildCardETag = "*";

		[SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly", Justification="This property is used as a parameter to String.Split")]
		public readonly static string[] HeaderSeparatorForStartAndEndInRangesSplitter;

		[SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly", Justification="This property is used as a parameter to String.Split")]
		public readonly static string[] RangeHeaderSeparatorForUnitAndRangeSplitter;

		static HeaderValues()
		{
			HeaderValues.HeaderSeparatorForStartAndEndInRangesSplitter = new string[] { "-" };
			HeaderValues.RangeHeaderSeparatorForUnitAndRangeSplitter = new string[] { "=" };
		}
	}
}