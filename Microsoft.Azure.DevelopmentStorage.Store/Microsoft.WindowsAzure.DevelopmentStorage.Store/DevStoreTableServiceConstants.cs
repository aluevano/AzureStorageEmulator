using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal static class DevStoreTableServiceConstants
	{
		public const int UTILITY_FIXED_COLUMN_COUNT = 3;

		public const int MAX_UTILITY_COLUMN_COUNT = 255;

		public const int MAX_UTILITY_DICTIONARY_ENTRIES = 252;

		public const int MAX_UTILITY_DICTIONARY_KEY_LENGTH = 255;

		public const int MAX_UTILITY_KEY_COLUMN_SIZE = 2048;

		public const int MAX_UTILITY_STRING_SIZE = 65536;

		public const int MAX_UTILITY_BINARY_SIZE = 65536;

		public const int MAX_UTILITY_ROW_SIZE = 1048576;

		public const string TABLE_NAME_REGEX = "^[A-Za-z][A-Za-z0-9]{2,62}$";

		public static DateTime MIN_DATETIME_VALUE;

		static DevStoreTableServiceConstants()
		{
			DevStoreTableServiceConstants.MIN_DATETIME_VALUE = new DateTime(1601, 1, 1);
		}
	}
}