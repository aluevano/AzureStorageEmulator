using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class EnumUtilities
	{
		public static T ConvertFromString<T>(string data, char[] separators)
		{
			string[] strArrays = data.Split(separators);
			NephosAssertionException.Assert((int)strArrays.Length > 0, "Enum value cannot be empty");
			int num = 0;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				num |= (int)Enum.Parse(typeof(T), strArrays[i]);
			}
			return (T)Enum.ToObject(typeof(T), num);
		}

		public static T ConvertFromString<T>(string data)
		{
			return EnumUtilities.ConvertFromString<T>(data, new char[] { ' ' });
		}
	}
}