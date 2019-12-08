using System;
using System.Data.Services.Common;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public static class TableProtocolHeadHelper
	{
		private static string GetCleanedMediaType(string mediaType)
		{
			string str = mediaType;
			int num = mediaType.IndexOf(';');
			if (num != -1)
			{
				str = mediaType.Substring(0, num);
			}
			char[] charArray = " ".ToCharArray();
			str = str.TrimEnd(charArray).TrimStart(charArray);
			return str;
		}

		public static string GetHeaderValue(this DataServiceProtocolVersion dataServiceProtocolVersion)
		{
			string str;
			switch (dataServiceProtocolVersion)
			{
				case DataServiceProtocolVersion.V1:
				{
					str = "1.0";
					break;
				}
				case DataServiceProtocolVersion.V2:
				{
					str = "2.0";
					break;
				}
				case DataServiceProtocolVersion.V3:
				{
					str = "3.0";
					break;
				}
				default:
				{
					throw new ArgumentException(string.Format("DataServiceProtocolVersion [{0}] cannot be converted to a header value.", dataServiceProtocolVersion), "dataServiceProtocolVersion");
				}
			}
			return str;
		}

		public static bool IsBatchRequest(Uri requestUri, Uri serviceUri)
		{
			if (requestUri == null || !requestUri.IsAbsoluteUri)
			{
				throw new ArgumentException("requestUri");
			}
			if (serviceUri == null || !serviceUri.IsAbsoluteUri)
			{
				throw new ArgumentException("serviceUri");
			}
			if (requestUri.AbsolutePath.ToString().TrimEnd("/".ToCharArray()).EndsWith("$batch", StringComparison.Ordinal))
			{
				int length = (int)serviceUri.Segments.Length;
				string[] segments = requestUri.Segments;
				int num = 0;
				string str = null;
				for (int i = length; i < (int)segments.Length; i++)
				{
					string str1 = segments[i];
					if (str1.Length > 0 && str1 != "/")
					{
						if (str == null)
						{
							str = (str1[str1.Length - 1].ToString() != "/" ? str1 : str1.Substring(0, str1.Length - 1));
						}
						num++;
					}
				}
				if (num == 1 && str == "$batch")
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsMediaTypeEqual(string mediaType1, string mediaType2)
		{
			if (string.IsNullOrEmpty(mediaType1))
			{
				throw new ArgumentException("mediaType1");
			}
			if (string.IsNullOrEmpty(mediaType2))
			{
				throw new ArgumentException("mediaType2");
			}
			string cleanedMediaType = TableProtocolHeadHelper.GetCleanedMediaType(mediaType1);
			return cleanedMediaType.Equals(TableProtocolHeadHelper.GetCleanedMediaType(mediaType2), StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsMediaTypeJSON(string mediaType)
		{
			return TableProtocolHeadHelper.IsMediaTypeEqual(mediaType, "application/json");
		}
	}
}