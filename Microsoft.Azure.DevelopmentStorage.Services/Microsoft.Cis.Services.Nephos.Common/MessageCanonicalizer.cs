using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Canonicalizer", Justification="While canonicalizer may not be a word in the dictionary, its meaning is clear, fits perfectly to describe its function, and is common in programming circles.")]
	public static class MessageCanonicalizer
	{
		private readonly static string[] queryDelimiters;

		private readonly static TimeSpan emptyMetadataValueAlerTimeSpan;

		static MessageCanonicalizer()
		{
			MessageCanonicalizer.queryDelimiters = new string[] { "?", "&" };
			MessageCanonicalizer.emptyMetadataValueAlerTimeSpan = TimeSpan.FromDays(1);
		}

		public static string CanonicalizeHttpRequest(HttpWebRequest request, NephosUriComponents uriComponents)
		{
			if (!MessageCanonicalizer.IsVersionBeforeSep09(request.Headers))
			{
				return MessageCanonicalizer.CanonicalizeHttpRequestVer1(uriComponents, request);
			}
			return MessageCanonicalizer.CanonicalizeHttpRequestDefault(request.Address, uriComponents, request.Method, request.ContentType, request.Headers, false, null);
		}

		public static string CanonicalizeHttpRequest(Uri uri, NephosUriComponents uriComponents, string method, NameValueCollection headers, bool multipleConditionalHeadersEnabled = false)
		{
			if (!MessageCanonicalizer.IsVersionBeforeSep09(headers["x-ms-version"]))
			{
				return MessageCanonicalizer.CanonicalizeHttpRequestVer1(uri, uriComponents, method, headers, false, null, multipleConditionalHeadersEnabled);
			}
			return MessageCanonicalizer.CanonicalizeHttpRequestDefault(uri, uriComponents, method, headers["Content-Type"], headers, false, null);
		}

		public static string CanonicalizeHttpRequest(RequestContext requestContext, NephosUriComponents uriComponents, bool isFileService = false)
		{
			if (!MessageCanonicalizer.IsVersionBeforeSep09(requestContext.RequestHeaders))
			{
				return MessageCanonicalizer.CanonicalizeHttpRequestVer1(uriComponents, requestContext, isFileService);
			}
			return MessageCanonicalizer.CanonicalizeHttpRequestDefault(requestContext.RequestUrl, uriComponents, requestContext.HttpMethod, requestContext.RequestContentType, requestContext.RequestHeaders, isFileService, requestContext.RequestRawUrlString);
		}

		public static string CanonicalizeHttpRequestDefault(Uri address, NephosUriComponents uriComponents, string method, string contentType, NameValueCollection headers, bool isFileService = false, string rawUrl = null)
		{
			MessageCanonicalizer.CanonicalizedString canonicalizedString = new MessageCanonicalizer.CanonicalizedString(method);
			canonicalizedString.AppendCanonicalizedElement(MessageCanonicalizer.GetContentMD5(headers));
			canonicalizedString.AppendCanonicalizedElement(contentType);
			string str = null;
			if (HttpRequestAccessorCommon.GetHeaderValues(headers, "x-ms-date").Count <= 0)
			{
				string[] values = headers.GetValues("Date");
				str = (values == null || (int)values.Length == 0 ? string.Empty : values[0]);
			}
			else
			{
				str = null;
			}
			canonicalizedString.AppendCanonicalizedElement(str);
			ArrayList arrayLists = new ArrayList();
			foreach (string key in headers.Keys)
			{
				if (key == null || !key.ToLowerInvariant().StartsWith("x-ms-", StringComparison.Ordinal))
				{
					continue;
				}
				arrayLists.Add(key.ToLowerInvariant());
			}
			arrayLists.Sort();
			foreach (string arrayList in arrayLists)
			{
				string canonicalizedHeaderValue = MessageCanonicalizer.GetCanonicalizedHeaderValue(headers, arrayList);
				if (string.IsNullOrEmpty(canonicalizedHeaderValue) && !isFileService && MessageCanonicalizer.IsVersionBeforeFeb16(headers["x-ms-version"]))
				{
					continue;
				}
				StringBuilder stringBuilder = new StringBuilder(arrayList);
				stringBuilder.Append(":");
				stringBuilder.Append(canonicalizedHeaderValue);
				canonicalizedString.AppendCanonicalizedElement(stringBuilder.ToString());
			}
			canonicalizedString.AppendCanonicalizedElement(MessageCanonicalizer.GetCanonicalizedResource(address, uriComponents, rawUrl));
			return canonicalizedString.Value;
		}

		public static string CanonicalizeHttpRequestVer1(Uri uri, NephosUriComponents uriComponents, string method, NameValueCollection headers, bool isFileService = false, string rawUrl = null, bool multipleConditionalHeadersEnabled = false)
		{
			return MessageCanonicalizer.CanonicalizeHttpRequestVer1(uri, uriComponents, method, MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "Content-Encoding", multipleConditionalHeadersEnabled), MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "Content-Language", multipleConditionalHeadersEnabled), MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "Content-Length", multipleConditionalHeadersEnabled), MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "Content-MD5", multipleConditionalHeadersEnabled), MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "Content-Type", multipleConditionalHeadersEnabled), MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "Date", multipleConditionalHeadersEnabled), MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "If-Modified-Since", multipleConditionalHeadersEnabled), MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "If-Match", multipleConditionalHeadersEnabled), MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "If-None-Match", multipleConditionalHeadersEnabled), MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "If-Unmodified-Since", multipleConditionalHeadersEnabled), MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, "Range", multipleConditionalHeadersEnabled), headers, isFileService, rawUrl, multipleConditionalHeadersEnabled);
		}

		public static string CanonicalizeHttpRequestVer1(NephosUriComponents uriComponents, RequestContext requestContext, bool isFileService = false)
		{
			return MessageCanonicalizer.CanonicalizeHttpRequestVer1(requestContext.RequestUrl, uriComponents, requestContext.HttpMethod, requestContext.RequestHeaders, isFileService, requestContext.RequestRawUrlString, requestContext.MultipleConditionalHeadersEnabled);
		}

		public static string CanonicalizeHttpRequestVer1(NephosUriComponents uriComponents, HttpWebRequest request)
		{
			string canonicalizedHeaderValue;
			Uri address = request.Address;
			NephosUriComponents nephosUriComponent = uriComponents;
			string method = request.Method;
			string str = MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "Content-Encoding");
			string canonicalizedHeaderValue1 = MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "Content-Language");
			if (request.Headers["Content-Length"] != null)
			{
				canonicalizedHeaderValue = MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "Content-Length");
			}
			else
			{
				canonicalizedHeaderValue = (request.ContentLength < (long)0 ? string.Empty : Convert.ToString(request.ContentLength));
			}
			return MessageCanonicalizer.CanonicalizeHttpRequestVer1(address, nephosUriComponent, method, str, canonicalizedHeaderValue1, canonicalizedHeaderValue, MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "Content-MD5"), (request.Headers["Content-Type"] != null ? MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "Content-Type") : request.ContentType), MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "Date"), MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "If-Modified-Since"), MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "If-Match"), MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "If-None-Match"), MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "If-Unmodified-Since"), MessageCanonicalizer.GetCanonicalizedHeaderValue(request.Headers, "Range"), request.Headers, false, null, false);
		}

		public static string CanonicalizeHttpRequestVer1(Uri address, NephosUriComponents uriComponents, string method, string contentEncoding, string contentLanguage, string contentLength, string contentMD5, string contentType, string date, string ifModifiedSince, string ifMatch, string ifNoneMatch, string ifUnmodifiedSince, string range, NameValueCollection headers, bool isFileService = false, string rawUrl = null, bool multipleConditionalHeadersEnabled = false)
		{
			NephosAssertionException.Assert(address != null);
			NephosAssertionException.Assert(uriComponents != null);
			NephosAssertionException.Assert(!string.IsNullOrEmpty(method));
			NephosAssertionException.Assert(headers != null);
			MessageCanonicalizer.CanonicalizedString canonicalizedString = new MessageCanonicalizer.CanonicalizedString(method);
			canonicalizedString.AppendCanonicalizedElement(contentEncoding ?? string.Empty);
			canonicalizedString.AppendCanonicalizedElement(contentLanguage ?? string.Empty);
			if (contentLength == null || !MessageCanonicalizer.IsVersionBeforeFeb15(headers) && contentLength.Equals("0"))
			{
				canonicalizedString.AppendCanonicalizedElement(string.Empty);
			}
			else
			{
				canonicalizedString.AppendCanonicalizedElement(contentLength);
			}
			canonicalizedString.AppendCanonicalizedElement(contentMD5 ?? string.Empty);
			canonicalizedString.AppendCanonicalizedElement(contentType ?? string.Empty);
			if (headers["x-ms-date"] != null)
			{
				canonicalizedString.AppendCanonicalizedElement(string.Empty);
			}
			else
			{
				canonicalizedString.AppendCanonicalizedElement(date ?? string.Empty);
			}
			canonicalizedString.AppendCanonicalizedElement(ifModifiedSince ?? string.Empty);
			canonicalizedString.AppendCanonicalizedElement(ifMatch ?? string.Empty);
			canonicalizedString.AppendCanonicalizedElement(ifNoneMatch ?? string.Empty);
			canonicalizedString.AppendCanonicalizedElement(ifUnmodifiedSince ?? string.Empty);
			canonicalizedString.AppendCanonicalizedElement(range ?? string.Empty);
			ArrayList arrayLists = new ArrayList();
			foreach (string key in headers.Keys)
			{
				if (key == null || !key.ToLowerInvariant().StartsWith("x-ms-", StringComparison.Ordinal))
				{
					continue;
				}
				arrayLists.Add(key.ToLowerInvariant());
			}
			arrayLists.Sort();
			foreach (string arrayList in arrayLists)
			{
				string canonicalizedHeaderValueVer1 = MessageCanonicalizer.GetCanonicalizedHeaderValueVer1(headers, arrayList, multipleConditionalHeadersEnabled);
				if (string.IsNullOrEmpty(canonicalizedHeaderValueVer1) && !isFileService && MessageCanonicalizer.IsVersionBeforeFeb16(headers["x-ms-version"]))
				{
					continue;
				}
				StringBuilder stringBuilder = new StringBuilder(arrayList);
				stringBuilder.Append(":");
				stringBuilder.Append(canonicalizedHeaderValueVer1);
				canonicalizedString.AppendCanonicalizedElement(stringBuilder.ToString());
			}
			canonicalizedString.AppendCanonicalizedElement(MessageCanonicalizer.GetCanonicalizedResourceVer1(address, uriComponents, rawUrl));
			return canonicalizedString.Value;
		}

		private static string DiscardQueryString(string rawUrl)
		{
			int num = rawUrl.IndexOf("?");
			if (num == -1)
			{
				return rawUrl;
			}
			return rawUrl.Substring(0, num);
		}

		public static string GetCanonicalizedHeaderValue(NameValueCollection headers, string key)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string empty = string.Empty;
			foreach (string headerValue in HttpRequestAccessorCommon.GetHeaderValues(headers, key))
			{
				string str = headerValue.Replace("\r\n", string.Empty);
				stringBuilder.Append(empty);
				stringBuilder.Append(str);
				empty = ",";
			}
			return stringBuilder.ToString();
		}

		private static string GetCanonicalizedHeaderValueVer1(NameValueCollection headers, string key, bool multipleConditionalHeadersEnabled)
		{
			return HttpRequestAccessorCommon.GetHeaderValue(headers, key, multipleConditionalHeadersEnabled);
		}

		public static string GetCanonicalizedResource(Uri address, NephosUriComponents uriComponents, string rawUrl = null)
		{
			NephosAssertionException.Assert(!string.IsNullOrEmpty(uriComponents.AccountName), "Resource account name must be non-empty.");
			StringBuilder stringBuilder = new StringBuilder("/");
			stringBuilder.Append(uriComponents.AccountName);
			string uriStringToSign = MessageCanonicalizer.GetUriStringToSign(address.AbsolutePath, rawUrl);
			stringBuilder.Append(MessageCanonicalizer.GetUriWithoutSecondarySuffix(uriStringToSign, uriComponents));
			string item = HttpUtility.ParseQueryString(address.Query)["comp"];
			if (item != null)
			{
				stringBuilder.Append("?");
				stringBuilder.Append("comp");
				stringBuilder.Append("=");
				stringBuilder.Append(item);
			}
			return stringBuilder.ToString();
		}

		[SuppressMessage("Anvil.Nullptr", "26501", Justification="NephosAssertionException.Assert does not return.")]
		public static string GetCanonicalizedResourceVer1(Uri uri, NephosUriComponents uriComponents, string rawUrl = null)
		{
			NephosAssertionException.Assert((uriComponents == null ? false : !string.IsNullOrEmpty(uriComponents.AccountName)), "Resource account name must be non-empty.");
			StringBuilder stringBuilder = new StringBuilder("/");
			stringBuilder.Append(uriComponents.AccountName);
			string uriStringToSign = MessageCanonicalizer.GetUriStringToSign(uri.AbsolutePath, rawUrl);
			stringBuilder.Append(MessageCanonicalizer.GetUriWithoutSecondarySuffix(uriStringToSign, uriComponents));
			MessageCanonicalizer.CanonicalizedString canonicalizedString = new MessageCanonicalizer.CanonicalizedString(stringBuilder.ToString());
			NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uri.Query);
			NameValueCollection nameValueCollection1 = new NameValueCollection();
			foreach (string key in nameValueCollection.Keys)
			{
				object[] values = nameValueCollection.GetValues(key);
				Array.Sort<object>(values);
				StringBuilder stringBuilder1 = new StringBuilder();
				object[] objArray = values;
				for (int i = 0; i < (int)objArray.Length; i++)
				{
					object obj = objArray[i];
					if (stringBuilder1.Length > 0)
					{
						stringBuilder1.Append(",");
					}
					stringBuilder1.Append(obj.ToString());
				}
				nameValueCollection1.Add((key == null ? key : key.ToLowerInvariant()), stringBuilder1.ToString());
			}
			string[] allKeys = nameValueCollection1.AllKeys;
			Array.Sort<string>(allKeys);
			string[] strArrays = allKeys;
			for (int j = 0; j < (int)strArrays.Length; j++)
			{
				string str = strArrays[j];
				StringBuilder stringBuilder2 = new StringBuilder(string.Empty);
				stringBuilder2.Append(str);
				stringBuilder2.Append(":");
				stringBuilder2.Append(nameValueCollection1[str]);
				canonicalizedString.AppendCanonicalizedElement(stringBuilder2.ToString());
			}
			return canonicalizedString.Value;
		}

		public static string GetContentMD5(NameValueCollection headers)
		{
			string empty = string.Empty;
			ArrayList headerValues = HttpRequestAccessorCommon.GetHeaderValues(headers, "Content-MD5");
			if (headerValues.Count > 1)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { "Content-MD5" };
				throw new HttpRequestDuplicateHeaderException(string.Format(invariantCulture, "The given HTTP request header contains more than one {0} values.", objArray));
			}
			if (headerValues.Count == 1)
			{
				empty = (string)headerValues[0];
			}
			return empty;
		}

		public static string GetNephosOrStandardDateString(NameValueCollection headers)
		{
			string str = null;
			str = (HttpRequestAccessorCommon.GetHeaderValues(headers, "x-ms-date").Count <= 0 ? headers.GetValues("Date")[0] : MessageCanonicalizer.GetCanonicalizedHeaderValue(headers, "x-ms-date"));
			return str;
		}

		private static string GetUriStringToSign(string absolutePath, string rawUrl)
		{
			string str = absolutePath;
			if (rawUrl == null)
			{
				return str;
			}
			str = MessageCanonicalizer.SanitizeRawUrl(rawUrl);
			return str;
		}

		private static string GetUriWithoutSecondarySuffix(string rawUrl, NephosUriComponents uriComponents)
		{
			if (uriComponents.IsUriPathStyle && uriComponents.IsSecondaryAccountAccess && !string.IsNullOrEmpty(rawUrl))
			{
				int length = rawUrl.IndexOf(uriComponents.GetSecondaryAccountName(), 0, StringComparison.OrdinalIgnoreCase);
				if (length == 1)
				{
					length += uriComponents.AccountName.Length;
					rawUrl = rawUrl.Remove(length, "-secondary".Length);
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("removing secondary suffix : {0}", new object[] { rawUrl });
				}
			}
			return rawUrl;
		}

		public static bool IsVersionBeforeFeb15(string xmsVersion)
		{
			if (xmsVersion != null && !VersioningHelper.IsPreFebruary15OrInvalidVersion(xmsVersion))
			{
				return false;
			}
			return true;
		}

		public static bool IsVersionBeforeFeb15(NameValueCollection requestHeaders)
		{
			return MessageCanonicalizer.IsVersionBeforeFeb15(requestHeaders["x-ms-version"]);
		}

		public static bool IsVersionBeforeFeb16(string xmsVersion)
		{
			if (xmsVersion != null && !VersioningHelper.IsPreFebruary16OrInvalidVersion(xmsVersion))
			{
				return false;
			}
			return true;
		}

		public static bool IsVersionBeforeFeb16(NameValueCollection requestHeaders)
		{
			return MessageCanonicalizer.IsVersionBeforeFeb16(requestHeaders["x-ms-version"]);
		}

		public static bool IsVersionBeforeSep09(string xmsVersion)
		{
			if (xmsVersion != null && !VersioningHelper.IsPreSeptember09OrInvalidVersion(xmsVersion))
			{
				return false;
			}
			return true;
		}

		public static bool IsVersionBeforeSep09(NameValueCollection requestHeaders)
		{
			return MessageCanonicalizer.IsVersionBeforeSep09(requestHeaders["x-ms-version"]);
		}

		private static string SanitizeRawUrl(string rawUrl)
		{
			if (rawUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			{
				int num = 0;
				int num1 = 0;
				int num2 = 0;
				while (num2 < rawUrl.Length)
				{
					if (rawUrl[num2] == '/')
					{
						num1++;
					}
					if (num1 != 3)
					{
						num2++;
					}
					else
					{
						num = num2;
						break;
					}
				}
				if (num == 0)
				{
					return "/";
				}
				rawUrl = rawUrl.Substring(num);
			}
			return MessageCanonicalizer.DiscardQueryString(rawUrl);
		}

		public static string UnfoldString(string value)
		{
			if (value == null)
			{
				return value;
			}
			return value.Replace("\r\n", string.Empty);
		}

		private class CanonicalizedString
		{
			private StringBuilder canonicalizedString;

			public string Value
			{
				get
				{
					return this.canonicalizedString.ToString();
				}
			}

			public CanonicalizedString(string initialElement)
			{
				this.canonicalizedString.Append(initialElement);
			}

			public CanonicalizedString()
			{
			}

			public void AppendCanonicalizedElement(string element)
			{
				this.canonicalizedString.Append("\n");
				this.canonicalizedString.Append(element);
			}
		}
	}
}