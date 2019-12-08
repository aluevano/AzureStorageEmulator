using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public static class TableAuthenticationHelper
	{
		private static string AppendStringToCanonicalizedString(StringBuilder canonicalizedString, string stringToAppend)
		{
			canonicalizedString.Append("\n");
			canonicalizedString.Append(stringToAppend);
			return canonicalizedString.ToString();
		}

		public static string CanonicalizeTableHttpRequestForSharedKeyAuth(Uri address, NephosUriComponents uriComponents, string method, string contentType, NameValueCollection headers, string rawUrl)
		{
			StringBuilder stringBuilder = new StringBuilder(method);
			TableAuthenticationHelper.AppendStringToCanonicalizedString(stringBuilder, MessageCanonicalizer.GetContentMD5(headers));
			TableAuthenticationHelper.AppendStringToCanonicalizedString(stringBuilder, contentType);
			TableAuthenticationHelper.AppendStringToCanonicalizedString(stringBuilder, MessageCanonicalizer.GetNephosOrStandardDateString(headers));
			TableAuthenticationHelper.AppendStringToCanonicalizedString(stringBuilder, MessageCanonicalizer.GetCanonicalizedResource(address, uriComponents, rawUrl));
			return stringBuilder.ToString();
		}

		public static string CanonicalizeTableHttpRequestForSharedKeyAuth(Uri address, NephosUriComponents uriComponents, string method, string contentType, NameValueCollection headers)
		{
			return TableAuthenticationHelper.CanonicalizeTableHttpRequestForSharedKeyAuth(address, uriComponents, method, contentType, headers, null);
		}

		public static string GetStringToSignForNephosSharedKeyAuth(RequestContext requestContext, NephosUriComponents uriComponents)
		{
			return TableAuthenticationHelper.CanonicalizeTableHttpRequestForSharedKeyAuth(requestContext.RequestUrl, uriComponents, requestContext.HttpMethod, requestContext.RequestContentType, requestContext.RequestHeaders, requestContext.RequestRawUrlString);
		}

		public static string GetStringToSignForNephosSharedKeyLiteAuth(RequestContext requestContext, NephosUriComponents uriComponents)
		{
			Uri requestUrl = requestContext.RequestUrl;
			StringBuilder stringBuilder = new StringBuilder(MessageCanonicalizer.GetNephosOrStandardDateString(requestContext.RequestHeaders));
			string canonicalizedResource = MessageCanonicalizer.GetCanonicalizedResource(requestUrl, uriComponents, requestContext.RequestRawUrlString);
			TableAuthenticationHelper.AppendStringToCanonicalizedString(stringBuilder, canonicalizedResource);
			return stringBuilder.ToString();
		}
	}
}