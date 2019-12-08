using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class ContinuationTokenParser
	{
		private const char exclamationDelimiter = '!';

		private const string headerFormat = "^(\\d+)!(\\d+)!";

		private const int SingleTokenLengthDigitsV2 = 6;

		private const string SingleTokenLengthFormat = "{0:D6}";

		public static string DecodeContinuationToken(string continuationToken)
		{
			string[] strArrays = ContinuationTokenParser.DecodeContinuationToken(continuationToken, new List<ContinuationTokenVersion>()
			{
				ContinuationTokenVersion.VersionOne
			});
			NephosAssertionException.Assert((int)strArrays.Length == 1);
			return strArrays[0];
		}

		public static string[] DecodeContinuationToken(string continuationToken, List<ContinuationTokenVersion> supportedVersions)
		{
			ContinuationTokenVersion continuationTokenVersion;
			return ContinuationTokenParser.DecodeContinuationToken(continuationToken, supportedVersions, out continuationTokenVersion);
		}

		public static string[] DecodeContinuationToken(string continuationToken, List<ContinuationTokenVersion> supportedVersions, out ContinuationTokenVersion continuationTokenVersion)
		{
			return ContinuationTokenParser.DecodeContinuationToken(continuationToken, supportedVersions, true, out continuationTokenVersion);
		}

		public static string[] DecodeContinuationToken(string continuationToken, List<ContinuationTokenVersion> supportedVersions, bool isOriginalVersionSupported, out ContinuationTokenVersion continuationTokenVersion)
		{
			string[] strArrays;
			if (supportedVersions == null || supportedVersions.Count <= 0)
			{
				throw new ArgumentException("The supported versions cannot be null and must contain at least one version.", "supportedVersions");
			}
			if (string.IsNullOrEmpty(continuationToken))
			{
				throw new ArgumentException("The continuation token cannot be null or empty.", "continuationToken");
			}
			continuationTokenVersion = ContinuationTokenVersion.None;
			try
			{
				if (Regex.IsMatch(continuationToken, "^(\\d+)!(\\d+)!"))
				{
					char[] chrArray = new char[] { '!' };
					string[] strArrays1 = continuationToken.Split(chrArray, 3, StringSplitOptions.None);
					if ((int)strArrays1.Length != 3)
					{
						throw new ContinuationTokenParserException(string.Format("Did find expected number of tokens ({0}).", 3), "continuationToken");
					}
					continuationTokenVersion = (ContinuationTokenVersion)Convert.ToInt32(strArrays1[0]);
					if (!supportedVersions.Contains((ContinuationTokenVersion)((int)continuationTokenVersion)))
					{
						string str = "[";
						int num = 0;
						foreach (ContinuationTokenVersion supportedVersion in supportedVersions)
						{
							str = string.Concat(str, supportedVersion.ToString());
							if (num < supportedVersions.Count - 1)
							{
								str = string.Concat(str, ", ");
							}
							num++;
						}
						str = string.Concat(str, "]");
						throw new ContinuationTokenParserException(string.Format("The version found ({0}) is not in the list of supported versions; supported versions = {1}.", (ContinuationTokenVersion)((int)continuationTokenVersion), str));
					}
					switch ((int)continuationTokenVersion)
					{
						case 1:
						{
							strArrays = ContinuationTokenParser.DecodeContinuationTokenV1(strArrays1);
							return strArrays;
						}
						case 2:
						{
							strArrays = ContinuationTokenParser.DecodeContinuationTokenV2(strArrays1);
							return strArrays;
						}
					}
					object[] objArray = new object[] { (ContinuationTokenVersion)((int)continuationTokenVersion) };
					NephosAssertionException.Fail("Internal error: Version {0} is invalid.", objArray);
					strArrays = null;
				}
				else
				{
					if (!isOriginalVersionSupported)
					{
						throw new ContinuationTokenParserException("Proper header format not found.", "continuationToken");
					}
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Received continuation token in original PDC2008 format. Token = '{0}'", new object[] { continuationToken });
					strArrays = new string[] { continuationToken };
				}
			}
			catch (ContinuationTokenParserException continuationTokenParserException1)
			{
				ContinuationTokenParserException continuationTokenParserException = continuationTokenParserException1;
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] objArray1 = new object[] { (ContinuationTokenVersion)((int)continuationTokenVersion), continuationToken };
				error.Log("Error decoding continuation token with version {0} and value {1}", objArray1);
				throw new ContinuationTokenParserException(string.Format("Caught an exception when decoding a {0} continuation token. Continuation token = '{1}'.", (ContinuationTokenVersion)((int)continuationTokenVersion), continuationToken), continuationTokenParserException);
			}
			return strArrays;
		}

		public static string[] DecodeContinuationTokenV1(string[] tokenSplits)
		{
			string[] strArrays;
			int num = Convert.ToInt32(tokenSplits[1]);
			if (num != tokenSplits[2].Length)
			{
				throw new ContinuationTokenParserException(string.Format("Token size ({0}) did not match token length ({1}).", num, tokenSplits[2].Length), "continuationToken");
			}
			string str = tokenSplits[2].Substring(0, num);
			string str1 = ContinuationTokenParser.UrlCustomUnescapeBase64String(str);
			try
			{
				string str2 = (new UTF8Encoding()).GetString(Convert.FromBase64String(str1));
				strArrays = new string[] { str2 };
			}
			catch (FormatException formatException)
			{
				throw new ContinuationTokenParserException("The continuation token can't be decoded from Base-64 string.", formatException);
			}
			return strArrays;
		}

		public static string[] DecodeContinuationTokenV2(string[] tokenSplits)
		{
			int num = Convert.ToInt32(tokenSplits[1]);
			if (num != tokenSplits[2].Length)
			{
				throw new ContinuationTokenParserException(string.Format("Encoded token list string length ({0}) did not match actual encoded token list string length ({1}).", num, tokenSplits[2].Length), "continuationToken");
			}
			string str = ContinuationTokenParser.UrlCustomUnescapeBase64String(tokenSplits[2]);
			string empty = string.Empty;
			try
			{
				empty = (new UTF8Encoding()).GetString(Convert.FromBase64String(str));
			}
			catch (FormatException formatException)
			{
				throw new ContinuationTokenParserException("The continuation token can't be decoded from Base-64 string.", formatException);
			}
			List<string> strs = new List<string>();
			int num1 = 0;
			do
			{
				int num2 = -1;
				if (empty.Length - num1 < 7)
				{
					object[] length = new object[] { empty.Length, empty.Length - num1, num1, 6, 1 };
					throw new ContinuationTokenParserException(string.Format("The token list string of length {0} only has {1} characters left given current offset {2}, which is not enough to contain a single token length which is of length {3} and the delimiter of length {4}.", length));
				}
				num2 = int.Parse(empty.Substring(num1, 6));
				num1 += 6;
				if (empty[num1] != '!')
				{
					throw new ContinuationTokenParserException(string.Format("The token list string does not contain the expected delimiter {0} at position {1} after extracting a token length.", '!', num1));
				}
				num1++;
				if (empty.Length - num1 < num2 + 1)
				{
					object[] objArray = new object[] { empty.Length, empty.Length - num1, num1, num2, 1 };
					throw new ContinuationTokenParserException(string.Format("The token list string of length {0} only has {1} characters left given current offset {2}, which is not enough to contain a token of length {3} and the delimiter of length {4}.", objArray));
				}
				string str1 = empty.Substring(num1, num2);
				num1 += num2;
				if (empty[num1] != '!')
				{
					throw new ContinuationTokenParserException(string.Format("The token list string does not contain the expected delimiter {0} at position {1} after extracting a token.", '!', num1));
				}
				num1++;
				strs.Add(str1);
			}
			while (num1 != empty.Length);
			return strs.ToArray();
		}

		public static string EncodeContinuationToken(string continuationToken)
		{
			return ContinuationTokenParser.EncodeContinuationTokenV1(continuationToken);
		}

		public static string EncodeContinuationTokenV1(string continuationToken)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(1);
			stringBuilder.Append('!');
			UTF8Encoding uTF8Encoding = new UTF8Encoding();
			string base64String = Convert.ToBase64String(uTF8Encoding.GetBytes(continuationToken.ToString()));
			string str = ContinuationTokenParser.UrlCustomEscapeBase64String(base64String);
			stringBuilder.Append(str.Length);
			stringBuilder.Append('!');
			stringBuilder.Append(str);
			return stringBuilder.ToString();
		}

		public static string EncodeContinuationTokenV2(string[] continuationToken)
		{
			if (continuationToken == null || (int)continuationToken.Length <= 0)
			{
				throw new ArgumentException("The continuation token is null or has no parts.", "continuationToken");
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(2);
			stringBuilder.Append('!');
			UTF8Encoding uTF8Encoding = new UTF8Encoding();
			string empty = string.Empty;
			string[] strArrays = continuationToken;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				empty = string.Concat(empty, string.Format("{0:D6}!{1}!", str.Length, str));
			}
			string base64String = Convert.ToBase64String(uTF8Encoding.GetBytes(empty));
			string str1 = ContinuationTokenParser.UrlCustomEscapeBase64String(base64String);
			stringBuilder.Append(str1.Length);
			stringBuilder.Append('!');
			stringBuilder.Append(str1);
			return stringBuilder.ToString();
		}

		private static char TranslateChar(char c)
		{
			char chr = c;
			if (chr == '+')
			{
				return '*';
			}
			if (chr == '/')
			{
				return '\u005F';
			}
			if (chr == '=')
			{
				return '-';
			}
			return c;
		}

		private static char UntranslateChar(char c)
		{
			char chr = c;
			if (chr == '*')
			{
				return '+';
			}
			if (chr == '-')
			{
				return '=';
			}
			if (chr == '\u005F')
			{
				return '/';
			}
			return c;
		}

		public static string UrlCustomEscapeBase64String(string token)
		{
			StringBuilder stringBuilder = new StringBuilder();
			char[] charArray = token.ToCharArray();
			for (int i = 0; i < (int)charArray.Length; i++)
			{
				char chr = charArray[i];
				stringBuilder.Append(ContinuationTokenParser.TranslateChar(chr));
			}
			return stringBuilder.ToString();
		}

		public static string UrlCustomUnescapeBase64String(string token)
		{
			StringBuilder stringBuilder = new StringBuilder();
			char[] charArray = token.ToCharArray();
			for (int i = 0; i < (int)charArray.Length; i++)
			{
				char chr = charArray[i];
				stringBuilder.Append(ContinuationTokenParser.UntranslateChar(chr));
			}
			return stringBuilder.ToString();
		}
	}
}