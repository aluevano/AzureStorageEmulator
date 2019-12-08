using System;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class XmlUtilities
	{
		public const string StringHasInvalidXmlCharacters = "The string includes invalid Xml Characters";

		public static string EscapeInvalidXmlCharacters(string source, bool supportValidSurrogateCharacters = false)
		{
			if (source == null)
			{
				return null;
			}
			StringBuilder stringBuilder = null;
			for (int i = 0; i < source.Length; i++)
			{
				if (!XmlUtilities.IsValidCharacter(source[i]))
				{
					if (!supportValidSurrogateCharacters || i + 1 >= source.Length || !XmlUtilities.IsValidSurrogateCharacter(source[i], source[i + 1]))
					{
						if (stringBuilder == null)
						{
							stringBuilder = new StringBuilder();
							if (i > 0)
							{
								stringBuilder.Append(source.Substring(0, i));
							}
						}
						int num = source[i];
						stringBuilder.Append(string.Concat("#x", num.ToString("x4")));
					}
					else
					{
						i++;
					}
				}
				else if (stringBuilder != null)
				{
					stringBuilder.Append(source[i]);
				}
			}
			if (stringBuilder == null)
			{
				return source;
			}
			return stringBuilder.ToString();
		}

		public static bool HasInvalidXmlCharacters(string stringToCheck, bool supportValidSurrogateCharacters = false)
		{
			if (stringToCheck == null)
			{
				return false;
			}
			for (int i = 0; i < stringToCheck.Length; i++)
			{
				if (!XmlUtilities.IsValidCharacter(stringToCheck[i]))
				{
					if (!supportValidSurrogateCharacters || i + 1 >= stringToCheck.Length || !XmlUtilities.IsValidSurrogateCharacter(stringToCheck[i], stringToCheck[i + 1]))
					{
						return true;
					}
					i++;
				}
			}
			return false;
		}

		private static bool IsValidCharacter(char c)
		{
			if (c >= ' ' && c <= '\uD7FF')
			{
				return true;
			}
			if (c == '\n' || c == '\r' || c == '\t')
			{
				return true;
			}
			if (c >= '\uE000' && c <= '\uFFFD')
			{
				return true;
			}
			return false;
		}

		private static bool IsValidSurrogateCharacter(char high, char low)
		{
			if (char.IsHighSurrogate(high) && char.IsLowSurrogate(low))
			{
				return true;
			}
			return false;
		}
	}
}