using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class HttpRequestAccessorCommon
	{
		internal readonly static string[] uriDelimiter;

		internal readonly static string[] headerValueDelimiter;

		internal readonly static string[] spaceDelimiter;

		internal readonly static string[] colonDelimiter;

		internal readonly static char[] charsToTrim;

		internal readonly static char subDomainDelimiterChar;

		static HttpRequestAccessorCommon()
		{
			HttpRequestAccessorCommon.uriDelimiter = new string[] { "/" };
			HttpRequestAccessorCommon.headerValueDelimiter = new string[] { "," };
			HttpRequestAccessorCommon.spaceDelimiter = new string[] { " " };
			HttpRequestAccessorCommon.colonDelimiter = new string[] { ":" };
			HttpRequestAccessorCommon.charsToTrim = new char[] { '/' };
			HttpRequestAccessorCommon.subDomainDelimiterChar = '.';
		}

		public static void AddRange(HttpWebRequest request, long startOffset, long? endOffset, bool useStandardRangeHeader)
		{
			if (useStandardRangeHeader)
			{
				if (!endOffset.HasValue)
				{
					request.AddRange((int)startOffset);
					return;
				}
				request.AddRange((int)startOffset, (int)endOffset.Value);
				return;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { startOffset, null };
			objArray[1] = (endOffset.HasValue ? endOffset.ToString() : string.Empty);
			string str = string.Format(invariantCulture, "bytes={0}-{1}", objArray);
			request.Headers.Add("x-ms-range", str);
		}

		public static Uri AppendBlobNameToContainerUrl(Uri containerUrl, string blobName)
		{
			StringBuilder stringBuilder = new StringBuilder(containerUrl.AbsoluteUri);
			stringBuilder.Append("/");
			stringBuilder.Append(HttpUtilities.PathEncode(blobName));
			return new Uri(stringBuilder.ToString());
		}

		private static DateTime? ExtractAndVerifyDate(string dateStr)
		{
			DateTime? nullable = null;
			if (!string.IsNullOrEmpty(dateStr) && !HttpUtilities.TryGetDateTimeFromHttpString(dateStr, out nullable))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { dateStr };
				throw new ArgumentException(string.Format(invariantCulture, "Invalid date header value - '{0}'", objArray));
			}
			return nullable;
		}

		public static bool GetApplicationMetadataFromHeaders(NameValueCollection headers, string requestVersion, NameValueCollection metadata)
		{
			int num;
			int num1;
			return HttpRequestAccessorCommon.GetApplicationMetadataFromHeaders(headers, requestVersion, metadata, out num, out num1);
		}

		public static bool GetApplicationMetadataFromHeaders(NameValueCollection headers, string requestVersion, NameValueCollection metadata, out int metadataKeyLength, out int metadataValueLength)
		{
			bool flag = false;
			metadataKeyLength = 0;
			metadataValueLength = 0;
			metadata.Clear();
			string[] allKeys = headers.AllKeys;
			for (int i = 0; i < (int)allKeys.Length; i++)
			{
				string str = allKeys[i];
				if (str.StartsWith("x-ms-meta-", StringComparison.OrdinalIgnoreCase))
				{
					string str1 = str.Substring("x-ms-meta-".Length);
					if (str1.Length != 0)
					{
						string item = headers[str];
						MetadataEncoding.EnsureMetadataEntryIsValid(str1, item, requestVersion);
						metadata.Add(str1, item);
						metadataKeyLength += str1.Length;
						metadataValueLength += item.Length;
					}
					else
					{
						flag = true;
						break;
					}
				}
			}
			return flag;
		}

		public static void GetAuthorizationFieldValues(NameValueCollection headers, out string authScheme, out string authSchemeParam)
		{
			HttpRequestAccessorCommon.GetAuthorizationFieldValues(headers, "Authorization", out authScheme, out authSchemeParam);
		}

		public static void GetAuthorizationFieldValues(NameValueCollection headers, string authHeaderName, out string authScheme, out string authSchemeParam)
		{
			ArrayList headerValues = HttpRequestAccessorCommon.GetHeaderValues(headers, authHeaderName);
			if (headerValues.Count == 0)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { "Authorization" };
				throw new HttpRequestHeaderNotFoundException(string.Format(invariantCulture, "No '{0}' header found.", objArray));
			}
			if (headerValues.Count > 1)
			{
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				object[] objArray1 = new object[] { "Authorization" };
				StringBuilder stringBuilder = new StringBuilder(string.Format(cultureInfo, "More than one '{0}' header values found ", objArray1));
				string str = ":";
				foreach (string headerValue in headerValues)
				{
					stringBuilder.Append(str);
					stringBuilder.Append(headerValue);
					str = ",";
				}
				throw new HttpRequestDuplicateHeaderException(stringBuilder.ToString());
			}
			string item = (string)headerValues[0];
			string[] strArrays = item.Split(HttpRequestAccessorCommon.spaceDelimiter, StringSplitOptions.RemoveEmptyEntries);
			if ((int)strArrays.Length != 2)
			{
				CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
				object[] objArray2 = new object[] { "Authorization", item };
				throw new HttpRequestInvalidHeaderException(string.Format(invariantCulture1, "{0} value '{1}' is invalid.", objArray2));
			}
			authScheme = strArrays[0];
			authSchemeParam = strArrays[1];
		}

		public static string GetHeaderValue(NameValueCollection headers, string key, bool multipleConditionalHeadersEnabled)
		{
			string[] values = headers.GetValues(key);
			if (values == null)
			{
				return string.Empty;
			}
			if (multipleConditionalHeadersEnabled && (key.Equals("If-Match", StringComparison.OrdinalIgnoreCase) || key.Equals("If-None-Match", StringComparison.OrdinalIgnoreCase)))
			{
				return headers[key];
			}
			if ((int)values.Length > 1)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { key, (int)values.Length, headers[key] };
				throw new HttpRequestDuplicateHeaderException(string.Format(invariantCulture, "The given HTTP request header {0} contains more than one ({1}) values. Values: {2}", objArray));
			}
			return values[0];
		}

		public static ArrayList GetHeaderValues(HttpListenerRequest request, string headerName)
		{
			return HttpRequestAccessorCommon.GetHeaderValues(request.Headers, headerName);
		}

		public static ArrayList GetHeaderValues(WebRequest request, string headerName)
		{
			return HttpRequestAccessorCommon.GetHeaderValues(request.Headers, headerName);
		}

		public static ArrayList GetHeaderValues(NameValueCollection headers, string headerName)
		{
			ArrayList arrayLists = new ArrayList();
			string[] values = headers.GetValues(headerName);
			if (values != null)
			{
				string[] strArrays = values;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					arrayLists.Add(str.TrimStart(new char[0]));
				}
			}
			return arrayLists;
		}

		public static DateTime? GetNephosOrStandardDateHeader(NameValueCollection requestHeaders)
		{
			return HttpRequestAccessorCommon.ExtractAndVerifyDate(requestHeaders["x-ms-date"] ?? requestHeaders["Date"]);
		}

		public static string GetRangeHeaderValue(NameValueCollection headers, out string rangeHeaderName)
		{
			string item = headers["x-ms-range"];
			if (item != null)
			{
				rangeHeaderName = "x-ms-range";
				return item;
			}
			rangeHeaderName = "Range";
			return headers["Range"];
		}

		public static string GetRangeHeaderValue(NameValueCollection headers)
		{
			string str;
			return HttpRequestAccessorCommon.GetRangeHeaderValue(headers, out str);
		}

		public static void RemoveRangeHeaderValues(NameValueCollection headers)
		{
			headers.Remove("x-ms-range");
			headers.Remove("Range");
		}

		public static string TrimEndSlash(string inputString)
		{
			return inputString.TrimEnd(HttpRequestAccessorCommon.charsToTrim);
		}

		public static string TrimRootContainerNameFromEnd(string inputString, bool isRootContainerNameEncoded)
		{
			string str = "/$root";
			if (isRootContainerNameEncoded)
			{
				str = HttpUtilities.PathEncode(str);
			}
			if (!inputString.EndsWith(str))
			{
				return inputString;
			}
			return inputString.Remove(inputString.Length - str.Length, str.Length);
		}
	}
}