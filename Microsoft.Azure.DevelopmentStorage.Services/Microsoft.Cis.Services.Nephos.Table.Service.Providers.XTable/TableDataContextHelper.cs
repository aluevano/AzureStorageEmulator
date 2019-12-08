using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Providers.XTable
{
	public class TableDataContextHelper
	{
		private const string s_startCharacterRegex = "_|[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}]";

		private const string s_identifierCharacterRegexFor2009_04_14 = "[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}]";

		private const string s_identifierCharacterRegexFor2008_10_27 = "[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}-]";

		public const int MAX_PROPERTY_NAME_LENGTH = 255;

		public const int MAX_PROPERTIES = 255;

		public const int FIXED_COLUMN_COUNT = 3;

		private static Regex s_propertyNameRegexFor2009_04_14;

		private static Regex s_propertyNameRegexFor2008_10_27;

		public readonly static int MaxRowCount;

		static TableDataContextHelper()
		{
			TableDataContextHelper.s_propertyNameRegexFor2009_04_14 = new Regex("^(_|[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}])([\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}])*$", RegexOptions.Compiled);
			TableDataContextHelper.s_propertyNameRegexFor2008_10_27 = new Regex("^(_|[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}])([\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}-])*$", RegexOptions.Compiled);
			TableDataContextHelper.MaxRowCount = 1000;
		}

		public TableDataContextHelper()
		{
		}

		private static void CheckIsValidKeyCharacter(char c)
		{
			if (c < ' ' || c >= '\u007F' && c < '\u00A0' || c == '#' || c == '/' || c == '?' || c == '\\')
			{
				throw new ArgumentOutOfRangeException("c", (object)c, "Invalid characters in property.");
			}
		}

		public static NameValueCollection GetETagHeaders(bool operationIsConditional)
		{
			string str = "somevalue";
			if (!operationIsConditional)
			{
				str = "*";
			}
			return new NameValueCollection()
			{
				{ "If-Match", str }
			};
		}

		public static bool IsValidPropertyName(string propertyName, string apiVersion)
		{
			string str = apiVersion;
			if (str != null && str == "2008-10-27")
			{
				return TableDataContextHelper.s_propertyNameRegexFor2008_10_27.IsMatch(propertyName);
			}
			return TableDataContextHelper.s_propertyNameRegexFor2009_04_14.IsMatch(propertyName);
		}

		public static void ValidateKeyValue(string value)
		{
			if (value == null)
			{
				return;
			}
			string str = value;
			for (int i = 0; i < str.Length; i++)
			{
				TableDataContextHelper.CheckIsValidKeyCharacter(str[i]);
			}
		}
	}
}