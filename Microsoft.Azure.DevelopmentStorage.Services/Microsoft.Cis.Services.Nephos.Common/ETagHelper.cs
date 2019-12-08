using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class ETagHelper
	{
		public const string ETagPrefix = "0x";

		public const char EtagCover = '\"';

		public const string AstoriaEtagPrefix = "W/\"datetime";

		public const char AstoriaEtagDatetimeCover = '\'';

		public ETagHelper()
		{
		}

		public static string GetAstoriaETag(DateTime? lastModifiedTime)
		{
			if (!lastModifiedTime.HasValue)
			{
				return null;
			}
			string str = string.Format("datetime'{0}'", XmlConvert.ToString(lastModifiedTime.Value, XmlDateTimeSerializationMode.RoundtripKind));
			return string.Format("W/\"{0}\"", Uri.EscapeDataString(str));
		}

		public static string GetAstoriaETagCondition(DateTime? lastModifiedTime)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { (lastModifiedTime.HasValue ? ETagHelper.GetAstoriaETag(lastModifiedTime) : "*") };
			return string.Format(invariantCulture, "If-Match: {0}", objArray);
		}

		public static string GetETagFromDateTime(DateTime lastModifiedTime, bool addQuotes)
		{
			if (!addQuotes)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] ticks = new object[] { "0x", lastModifiedTime.Ticks };
				return string.Format(invariantCulture, "{0}{1:X}", ticks);
			}
			CultureInfo cultureInfo = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { '\"', "0x", lastModifiedTime.Ticks, '\"' };
			return string.Format(cultureInfo, "{0}{1}{2:X}{3}", objArray);
		}

		public static DateTime GetLastModifiedTimeFromAstoriaEtag(string eTag)
		{
			if (string.IsNullOrEmpty(eTag))
			{
				throw new FormatException("ETag is null or empty.");
			}
			if (!eTag.StartsWith("W/\"datetime"))
			{
				throw new FormatException("Astoria ETag did not start with W/\"datetime");
			}
			if (eTag[eTag.Length - 1] != '\"')
			{
				throw new FormatException(string.Concat("ETag must be enclosed between ", '\"'));
			}
			if (eTag.Length < "W/\"datetime".Length + 3 || eTag["W/\"datetime".Length] != '\'' || eTag[eTag.Length - 2] != '\'')
			{
				throw new FormatException(string.Concat("ETag datetime value must be enclosed between ", '\''));
			}
			string str = eTag.Substring("W/\"datetime".Length + 1, eTag.Length - "W/\"datetime".Length - 3);
			return DateTime.Parse(HttpUtility.UrlDecode(str), null, DateTimeStyles.AdjustToUniversal);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="0#e", Justification="eTag is an accepted capitalization of the word ETag (added to the custom dictionary for Nephos) for parameter names.")]
		public static DateTime GetLastModifiedTimeFromETag(string eTag)
		{
			long num = (long)0;
			if (!string.IsNullOrEmpty(eTag) && eTag.Length > 2 && eTag[0] == '\"' && eTag[eTag.Length - 1] == '\"')
			{
				int length = eTag.Length - 2;
				eTag = eTag.Substring(1, (length < 0 ? 0 : length));
			}
			if (!eTag.StartsWith("0x", StringComparison.Ordinal))
			{
				throw new FormatException("eTag did not start with 0x");
			}
			num = long.Parse(eTag.Substring("0x".Length), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			return new DateTime(num, DateTimeKind.Utc);
		}
	}
}