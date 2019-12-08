using AsyncHelper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Web;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class HttpUtilities
	{
		public static string RFC850DateTimePattern;

		internal readonly static string[] DaysOfWeek;

		internal readonly static string[] MonthsOfYear;

		private readonly static string[] HeadersOfInterest;

		private readonly static string EncodedSpaceByHttpUtilityUrlEncode;

		private readonly static string EncodedSpace;

		private readonly static string EncodedSlash;

		static HttpUtilities()
		{
			HttpUtilities.RFC850DateTimePattern = "dddd, dd'-'MMM'-'yy HH':'mm':'ss 'GMT'";
			string[] strArrays = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
			HttpUtilities.DaysOfWeek = strArrays;
			string[] strArrays1 = new string[] { "0", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
			HttpUtilities.MonthsOfYear = strArrays1;
			HttpUtilities.HeadersOfInterest = new string[] { "x-ms-request-id" };
			HttpUtilities.EncodedSpaceByHttpUtilityUrlEncode = "+";
			HttpUtilities.EncodedSpace = Uri.HexEscape(' ');
			HttpUtilities.EncodedSlash = Uri.HexEscape('/');
		}

		public static void AbortHttpWebRequestOnTimeout(HttpWebRequest request, int timeoutInMilliseconds, IAsyncResult asyncResult)
		{
			WaitOrTimerCallback waitOrTimerCallback = (object state, bool timedOut) => {
				AsyncHelper.Tuple<HttpWebRequest, RegisteredWaitHandle> tuple = (AsyncHelper.Tuple<HttpWebRequest, RegisteredWaitHandle>)state;
				if (timedOut)
				{
					HttpWebRequest first = tuple.First;
					if (first != null)
					{
						first.Abort();
					}
				}
				if (tuple.Second != null)
				{
					tuple.Second.Unregister(null);
				}
			};
			AsyncHelper.Tuple<HttpWebRequest, RegisteredWaitHandle> tuple1 = new AsyncHelper.Tuple<HttpWebRequest, RegisteredWaitHandle>()
			{
				First = request,
				Second = ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, waitOrTimerCallback, tuple1, timeoutInMilliseconds, true)
			};
		}

		private static void AppendDigits(StringBuilder sb, long number, long divisor)
		{
			while (divisor > (long)0)
			{
				char chr = (char)((long)48 + number / divisor % (long)10);
				sb.Append(chr);
				divisor /= (long)10;
			}
		}

		public static string ConvertDateTimeToHttpString(DateTime dt)
		{
			if (dt != DateTime.MaxValue && dt != DateTime.MinValue)
			{
				NephosAssertionException.Assert(dt.Kind == DateTimeKind.Utc);
			}
			StringBuilder stringBuilder = StringBuilderPool.Allocate();
			stringBuilder.Append(HttpUtilities.DaysOfWeek[(int)dt.DayOfWeek]);
			stringBuilder.Append(", ");
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Day, (long)10);
			stringBuilder.Append(' ');
			stringBuilder.Append(HttpUtilities.MonthsOfYear[dt.Month]);
			stringBuilder.Append(' ');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Year, (long)1000);
			stringBuilder.Append(' ');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Hour, (long)10);
			stringBuilder.Append(':');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Minute, (long)10);
			stringBuilder.Append(':');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Second, (long)10);
			stringBuilder.Append(" GMT");
			string str = stringBuilder.ToString();
			StringBuilderPool.Release(stringBuilder);
			return str;
		}

		public static string ConvertDateTimeToRoundTripFormatHttpString(DateTime dt)
		{
			if (dt != DateTime.MaxValue && dt != DateTime.MinValue)
			{
				NephosAssertionException.Assert(dt.Kind == DateTimeKind.Utc);
			}
			StringBuilder stringBuilder = StringBuilderPool.Allocate();
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Year, (long)1000);
			stringBuilder.Append('-');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Month, (long)10);
			stringBuilder.Append('-');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Day, (long)10);
			stringBuilder.Append('T');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Hour, (long)10);
			stringBuilder.Append(':');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Minute, (long)10);
			stringBuilder.Append(':');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Second, (long)10);
			stringBuilder.Append('.');
			HttpUtilities.AppendDigits(stringBuilder, dt.Ticks % (long)10000000, (long)1000000);
			stringBuilder.Append('Z');
			string str = stringBuilder.ToString();
			StringBuilderPool.Release(stringBuilder);
			return str;
		}

		public static string ConvertDateTimeToSortableFormatHttpString(DateTime dt)
		{
			if (dt != DateTime.MaxValue && dt != DateTime.MinValue)
			{
				NephosAssertionException.Assert(dt.Kind == DateTimeKind.Utc);
			}
			StringBuilder stringBuilder = StringBuilderPool.Allocate();
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Year, (long)1000);
			stringBuilder.Append('-');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Month, (long)10);
			stringBuilder.Append('-');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Day, (long)10);
			stringBuilder.Append(' ');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Hour, (long)10);
			stringBuilder.Append(':');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Minute, (long)10);
			stringBuilder.Append(':');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Second, (long)10);
			stringBuilder.Append('Z');
			string str = stringBuilder.ToString();
			StringBuilderPool.Release(stringBuilder);
			return str;
		}

		public static string ConvertSnapshotDateTimeToHttpString(DateTime dt)
		{
			if (dt != DateTime.MaxValue && dt != DateTime.MinValue)
			{
				NephosAssertionException.Assert(dt.Kind == DateTimeKind.Utc);
			}
			StringBuilder stringBuilder = StringBuilderPool.Allocate();
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Year, (long)1000);
			stringBuilder.Append('-');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Month, (long)10);
			stringBuilder.Append('-');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Day, (long)10);
			stringBuilder.Append('T');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Hour, (long)10);
			stringBuilder.Append(':');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Minute, (long)10);
			stringBuilder.Append(':');
			HttpUtilities.AppendDigits(stringBuilder, (long)dt.Second, (long)10);
			stringBuilder.Append('.');
			HttpUtilities.AppendDigits(stringBuilder, dt.Ticks % (long)10000000, (long)1000000);
			stringBuilder.Append('Z');
			string str = stringBuilder.ToString();
			StringBuilderPool.Release(stringBuilder);
			return str;
		}

		public static UriString ConvertUrlFromHttpToHttps(UriString uriString)
		{
			UriString uriString1;
			try
			{
				UriBuilder uriBuilder = new UriBuilder(new Uri(uriString.RawString));
				if (uriBuilder.Scheme == Uri.UriSchemeHttp)
				{
					uriBuilder.Scheme = Uri.UriSchemeHttps;
					if (uriBuilder.Port != 8080)
					{
						uriBuilder.Port = -1;
					}
					else
					{
						uriBuilder.Port = 8443;
					}
				}
				return new UriString(uriBuilder.Uri.OriginalString);
			}
			catch (Exception exception)
			{
				uriString1 = uriString;
			}
			return uriString1;
		}

		public static bool DateTimesEqualsUpToSeconds(DateTime dt1, DateTime dt2)
		{
			bool secondsIgnoringKind = HttpUtilities.DateTimesEqualsUpToSecondsIgnoringKind(dt1, dt2);
			if (!secondsIgnoringKind || HttpUtilities.DateTimesEqualsUpToSecondsIgnoringKind(dt1, DateTime.MaxValue))
			{
				return secondsIgnoringKind;
			}
			return dt1.Kind == dt2.Kind;
		}

		private static bool DateTimesEqualsUpToSecondsIgnoringKind(DateTime dt1, DateTime dt2)
		{
			if (dt1.Second != dt2.Second || dt1.Minute != dt2.Minute || dt1.Hour != dt2.Hour || dt1.Day != dt2.Day)
			{
				return false;
			}
			return dt1.Year == dt2.Year;
		}

		public static NameValueCollection GetAdditionalResponseDetails(HttpWebResponse httpResponse)
		{
			NameValueCollection nameValueCollection = new NameValueCollection();
			string[] headersOfInterest = HttpUtilities.HeadersOfInterest;
			for (int i = 0; i < (int)headersOfInterest.Length; i++)
			{
				string str = headersOfInterest[i];
				if (httpResponse.Headers[str] != null)
				{
					nameValueCollection[str] = httpResponse.Headers[str];
				}
			}
			return nameValueCollection;
		}

		public static NameValueCollection GetAdditionalResponseDetails(IDictionary<string, string> responseHeaders)
		{
			string str;
			NameValueCollection nameValueCollection = new NameValueCollection();
			string[] headersOfInterest = HttpUtilities.HeadersOfInterest;
			for (int i = 0; i < (int)headersOfInterest.Length; i++)
			{
				string str1 = headersOfInterest[i];
				if (responseHeaders.TryGetValue(str1, out str))
				{
					nameValueCollection[str1] = str;
				}
			}
			return nameValueCollection;
		}

		public static NameValueCollection GetHttpHeaderValueCollection(string headerValue)
		{
			if (string.IsNullOrEmpty(headerValue) || string.IsNullOrEmpty(headerValue.Trim()))
			{
				return null;
			}
			char[] chrArray = new char[] { ',' };
			string[] strArrays = headerValue.Split(chrArray, StringSplitOptions.RemoveEmptyEntries);
			NameValueCollection nameValueCollection = new NameValueCollection((int)strArrays.Length);
			string[] strArrays1 = strArrays;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string str = strArrays1[i];
				string[] strArrays2 = str.Split(new char[] { '=' });
				if ((int)strArrays2.Length > 2)
				{
					throw new ArgumentOutOfRangeException("headerValue", string.Format("Unrecognized format for HeaderValue '{0}'", headerValue));
				}
				nameValueCollection.Add(strArrays2[0].Trim(), ((int)strArrays2.Length > 1 ? strArrays2[1].Trim() : string.Empty));
			}
			return nameValueCollection;
		}

		internal static Uri GetOriginalUriFromRedirectedRequest(Uri redirectedRequestUrl, string originalRequestHostName)
		{
			string str = redirectedRequestUrl.OriginalString.Trim();
			int num = str.IndexOf('/', 8);
			if (num == -1)
			{
				throw new ArgumentException(string.Format("redirected request uri must have a '/' at the end of the Uri: {0}", str));
			}
			string str1 = string.Concat(originalRequestHostName, str.Substring(num + 1));
			return new Uri(str1);
		}

		public static NameValueCollection GetQueryParameters(Uri requestUri)
		{
			return HttpUtility.ParseQueryString(requestUri.Query);
		}

		public static int GetRequestHeaderLength(NameValueCollection requestHeaders, string httpVerb, string urlString)
		{
			int length = 0;
			if (requestHeaders != null)
			{
				string[] allKeys = requestHeaders.AllKeys;
				for (int i = 0; i < (int)allKeys.Length; i++)
				{
					string str = allKeys[i];
					length += str.Length;
					string str1 = requestHeaders.Get(str);
					if (str1 != null)
					{
						length += str1.Length;
					}
				}
			}
			if (httpVerb != null)
			{
				length += httpVerb.Length;
			}
			if (urlString != null)
			{
				length += urlString.Length;
			}
			return length;
		}

		public static Uri GetRequestUri(IHttpListenerRequest request)
		{
			return HttpUtilities.GetRequestUriInternal(request.Url, request.RawUrl, request.UserHostName);
		}

		internal static Uri GetRequestUriInternal(Uri listenerRequestUri, string rawUrl, string hostHeader)
		{
			Uri uri;
			if (listenerRequestUri == null)
			{
				throw new InvalidUrlException(string.Format("request.Url is null; RawUrl={0}, HostHeader={1}", rawUrl, hostHeader));
			}
			if (string.IsNullOrEmpty(rawUrl))
			{
				throw new ArgumentException("request.RawUrl is null or empty", "request.RawUrl");
			}
			UriBuilder uriBuilder = new UriBuilder()
			{
				Scheme = listenerRequestUri.Scheme,
				Host = listenerRequestUri.Host
			};
			int? nullable = null;
			if (!HttpUtilities.TryGetPortFromHostHeaderValue(hostHeader, out nullable))
			{
				uriBuilder.Port = listenerRequestUri.Port;
			}
			else if (nullable.HasValue)
			{
				uriBuilder.Port = nullable.Value;
			}
			if (!rawUrl.StartsWith("/"))
			{
				uri = (rawUrl != "*" ? new Uri(rawUrl) : new Uri(string.Concat(uriBuilder.Uri.AbsoluteUri, rawUrl)));
			}
			else
			{
				uri = new Uri(string.Concat(HttpRequestAccessorCommon.TrimEndSlash(uriBuilder.Uri.AbsoluteUri), rawUrl));
			}
			uriBuilder.Path = uri.AbsolutePath;
			if (!string.IsNullOrEmpty(uri.Query))
			{
				uriBuilder.Query = HttpUtilities.QueryWithoutQuestionMark(uri.Query);
			}
			return uriBuilder.Uri;
		}

		public static int GetResponseHeaderLength(IHttpListenerResponse response)
		{
			int length = 0;
			WebHeaderCollection headers = response.Headers;
			if (headers != null)
			{
				string[] allKeys = headers.AllKeys;
				for (int i = 0; i < (int)allKeys.Length; i++)
				{
					string str = allKeys[i];
					length += str.Length;
					string str1 = headers.Get(str);
					if (str1 != null)
					{
						length += str1.Length;
					}
				}
			}
			return length;
		}

		public static string GetSafeUriString(string uriString)
		{
			string[] strArrays = new string[] { "sig" };
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				int length = 0;
				int num = 0;
				while (num < uriString.Length)
				{
					length = uriString.IndexOf(string.Concat(str, "="), num);
					if (length == -1)
					{
						break;
					}
					if (length == 0 || uriString[length - 1] != '&' && uriString[length - 1] != '?')
					{
						num = length + str.Length + 1;
					}
					else
					{
						length = length + str.Length + 1;
						num = uriString.IndexOf('&', length);
						num = (num == -1 ? uriString.Length : num);
						if (length == num)
						{
							continue;
						}
						uriString = string.Concat(uriString.Substring(0, length), "XXXXX", uriString.Substring(num));
						num = length;
					}
				}
			}
			return uriString;
		}

		public static string GetSnapshotQueryParameterStringForUrl(DateTime snapshot)
		{
			return string.Format("?{0}={1}", "snapshot", HttpUtilities.PathEncode(HttpUtilities.ConvertSnapshotDateTimeToHttpString(snapshot)));
		}

		public static string GetStringForHttpListenerException(HttpListenerException hle)
		{
			return string.Format("HttpListenerException ErrorCode: {0}, Win32ErrorCode: {1}, Exception: {2}", hle.ErrorCode, hle.NativeErrorCode, hle.GetLogString());
		}

		private static bool IsExtendedAsciiRestrictedChar(char c)
		{
			if (c != '\u007F' && c != '\u0081' && c != '\u008D' && c != '\u008F' && c != '\u0090' && c != '\u009D')
			{
				return false;
			}
			return true;
		}

		public static bool IsHttpRestrictedCodepoint(int codePoint)
		{
			if (codePoint > 31 && codePoint < 127)
			{
				return false;
			}
			if (HttpUtilities.IsValidUnicodeChar(codePoint))
			{
				if (codePoint >= 0 && codePoint <= 31 || codePoint == 127 || codePoint == 129 || codePoint == 141 || codePoint == 143 || codePoint == 144 || codePoint == 157)
				{
					return true;
				}
				if (codePoint >= 63745 && codePoint <= 65501 && HttpUtilities.PercentEncodeUtf8ContainsRestrictedChar(codePoint))
				{
					return true;
				}
			}
			else if (HttpUtilities.PercentEncodeUtf8ContainsRestrictedChar(codePoint))
			{
				return true;
			}
			return false;
		}

		private static bool IsValidUnicodeChar(int codePoint)
		{
			if (codePoint <= 127)
			{
				return true;
			}
			if (32 <= codePoint && codePoint <= 127 || 160 <= codePoint && codePoint <= 55295 || 63744 <= codePoint && codePoint <= 64975 || 65008 <= codePoint && codePoint <= 65519 || 65536 <= codePoint && codePoint <= 131069 || 131072 <= codePoint && codePoint <= 196605 || 196608 <= codePoint && codePoint <= 262141 || 262144 <= codePoint && codePoint <= 327677 || 327680 <= codePoint && codePoint <= 393213 || 393216 <= codePoint && codePoint <= 458749 || 458752 <= codePoint && codePoint <= 524285 || 524288 <= codePoint && codePoint <= 589821 || 589824 <= codePoint && codePoint <= 655357 || 655360 <= codePoint && codePoint <= 720893 || 720896 <= codePoint && codePoint <= 786429 || 786432 <= codePoint && codePoint <= 851965 || 851968 <= codePoint && codePoint <= 917501 || 921600 <= codePoint && codePoint <= 983037)
			{
				return true;
			}
			return false;
		}

		public static string ObfuscateSourceUri(string sourceUri)
		{
			string originalString;
			try
			{
				Uri uri = new Uri(sourceUri);
				UriBuilder uriBuilder = new UriBuilder(uri);
				NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uri.Query);
				nameValueCollection.Remove("sk");
				nameValueCollection.Remove("sig");
				uriBuilder.Query = nameValueCollection.ToString();
				originalString = uriBuilder.Uri.OriginalString;
			}
			catch (Exception exception)
			{
				originalString = sourceUri;
			}
			return originalString;
		}

		public static string PathEncode(string path)
		{
			string str = HttpUtility.UrlEncode(path);
			str = str.Replace(HttpUtilities.EncodedSlash.ToUpperInvariant(), "/");
			str = str.Replace(HttpUtilities.EncodedSlash.ToLowerInvariant(), "/");
			return str.Replace(HttpUtilities.EncodedSpaceByHttpUtilityUrlEncode, HttpUtilities.EncodedSpace);
		}

		private static bool PercentEncodeUtf8ContainsRestrictedChar(int unicodeChar)
		{
			if (unicodeChar <= 127)
			{
				return HttpUtilities.IsExtendedAsciiRestrictedChar((char)(127 & unicodeChar));
			}
			if (unicodeChar <= 2047)
			{
				char chr = 'À';
				char chr1 = '\u0080';
				chr = (char)(chr | (char)((unicodeChar & 1984) >> 6));
				if (HttpUtilities.IsExtendedAsciiRestrictedChar((char)(chr1 | (char)(unicodeChar & 63))))
				{
					return true;
				}
				return HttpUtilities.IsExtendedAsciiRestrictedChar(chr);
			}
			if (unicodeChar <= 65535)
			{
				char chr2 = 'à';
				char chr3 = '\u0080';
				char chr4 = '\u0080';
				chr2 = (char)(chr2 | (char)((unicodeChar & 61440) >> 12));
				chr3 = (char)(chr3 | (char)((unicodeChar & 4032) >> 6));
				if (HttpUtilities.IsExtendedAsciiRestrictedChar((char)(chr4 | (char)(unicodeChar & 63))) || HttpUtilities.IsExtendedAsciiRestrictedChar(chr3))
				{
					return true;
				}
				return HttpUtilities.IsExtendedAsciiRestrictedChar(chr2);
			}
			if (unicodeChar > 2097151)
			{
				return false;
			}
			char chr5 = 'ð';
			char chr6 = '\u0080';
			char chr7 = '\u0080';
			char chr8 = '\u0080';
			chr5 = (char)(chr5 | (char)((unicodeChar & 1835008) >> 18));
			chr6 = (char)(chr6 | (char)((unicodeChar & 258048) >> 12));
			chr7 = (char)(chr7 | (char)((unicodeChar & 4032) >> 6));
			if (HttpUtilities.IsExtendedAsciiRestrictedChar((char)(chr8 | (char)(unicodeChar & 63))) || HttpUtilities.IsExtendedAsciiRestrictedChar(chr7) || HttpUtilities.IsExtendedAsciiRestrictedChar(chr6))
			{
				return true;
			}
			return HttpUtilities.IsExtendedAsciiRestrictedChar(chr5);
		}

		public static string QueryParameterNameEncode(string queryParamName)
		{
			return HttpUtility.UrlEncode(queryParamName);
		}

		public static string QueryParameterValueEncode(string queryParamValue)
		{
			return HttpUtilities.QueryParameterNameEncode(queryParamValue);
		}

		private static string QueryWithoutQuestionMark(string query)
		{
			NephosAssertionException.Assert(query[0] == "?"[0], "Uri.Query should include the question mark with the query parameters");
			return query.Substring(1);
		}

		public static bool StatusCodeIndicatesNoResponseBody(HttpStatusCode statusCode)
		{
			HttpStatusCode httpStatusCode = statusCode;
			switch (httpStatusCode)
			{
				case HttpStatusCode.Continue:
				case HttpStatusCode.SwitchingProtocols:
				{
					return true;
				}
				default:
				{
					if (httpStatusCode == HttpStatusCode.NoContent || httpStatusCode == HttpStatusCode.NotModified)
					{
						return true;
					}
					return false;
				}
			}
		}

		public static bool StringIsIPAddress(string address)
		{
			IPAddress pAddress;
			return IPAddress.TryParse(address, out pAddress);
		}

		[SuppressMessage("Anvil.RdUsage!TimeUtc", "27102", Justification="We eventually set the kind using DateTime.SpecifyKind.")]
		public static bool TryGetDateTimeFromHttpString(string dateString, out DateTime? result)
		{
			DateTime dateTime;
			result = null;
			bool flag = DateTime.TryParseExact(dateString, "R", null, DateTimeStyles.None, out dateTime);
			if (!flag)
			{
				flag = DateTime.TryParseExact(dateString, HttpUtilities.RFC850DateTimePattern, null, DateTimeStyles.None, out dateTime);
			}
			if (!flag)
			{
				return false;
			}
			result = new DateTime?(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
			return true;
		}

		public static bool TryGetPortFromHostHeaderValue(string hostHeaderValue, out int? port)
		{
			port = null;
			if (hostHeaderValue == null)
			{
				return false;
			}
			int num = hostHeaderValue.LastIndexOf(':');
			if (num == -1)
			{
				return true;
			}
			string str = hostHeaderValue.Substring(num + 1);
			try
			{
				port = new int?(Convert.ToInt32(str));
				return true;
			}
			catch (FormatException formatException)
			{
			}
			catch (OverflowException overflowException)
			{
			}
			return false;
		}

		[SuppressMessage("Anvil.RdUsage!TimeUtc", "27102", Justification="We eventually set the kind using DateTime.SpecifyKind.")]
		public static bool TryGetRoundTripFormatDateTimeFromHttpString(string dateString, out DateTime? result)
		{
			DateTime dateTime;
			result = null;
			if (!DateTime.TryParseExact(dateString, "O", null, DateTimeStyles.RoundtripKind, out dateTime))
			{
				return false;
			}
			result = new DateTime?(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
			return true;
		}

		[SuppressMessage("Anvil.RdUsage!TimeUtc", "27102", Justification="We eventually set the kind using DateTime.SpecifyKind.")]
		public static bool TryGetSnapshotDateTimeFromHttpString(string dateString, out DateTime? result)
		{
			DateTime dateTime;
			result = null;
			if (!DateTime.TryParseExact(dateString, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff'Z'", null, DateTimeStyles.None, out dateTime))
			{
				return false;
			}
			result = new DateTime?(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
			return true;
		}
	}
}