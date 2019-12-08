using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Specialized;
using System.Globalization;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public static class AuthenticationManagerHelper
	{
		public static string ExtractKeyNameFromParamsWithConversion(NameValueCollection queryParams)
		{
			string item = queryParams["sk"];
			string str = queryParams["sv"];
			string item1 = queryParams["sk"];
			if (!string.IsNullOrEmpty(str) && str == "2012-02-12" && !string.IsNullOrEmpty(item1) && item1 == "Key1")
			{
				item = "key1";
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("[HACK] Converting KeyName from {0} to {1} for authenticating DSAS/SAS request", new object[] { item1, item });
			}
			return item;
		}

		public static string GetStringToSignForStandardSharedKeyAuth(RequestContext requestContext, NephosUriComponents uriComponents, SupportedAuthScheme requestAuthScheme, bool isFileService = false)
		{
			if (MessageCanonicalizer.IsVersionBeforeSep09(requestContext.RequestHeaders))
			{
				if (requestAuthScheme != SupportedAuthScheme.SharedKey)
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] str = new object[] { requestAuthScheme.ToString() };
					throw new NotSupportedException(string.Format(invariantCulture, "GetStringToSignForDefaultSharedKeyAuth must not be used for {0} auth scheme", str));
				}
				return MessageCanonicalizer.CanonicalizeHttpRequest(requestContext, uriComponents, isFileService);
			}
			if (requestAuthScheme != SupportedAuthScheme.SharedKey && requestAuthScheme != SupportedAuthScheme.SharedKeyLite)
			{
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { requestAuthScheme.ToString() };
				throw new NotSupportedException(string.Format(cultureInfo, "GetStringToSignForDefaultSharedKeyAuth must not be used for {0} auth scheme", objArray));
			}
			if (requestAuthScheme != SupportedAuthScheme.SharedKeyLite)
			{
				return MessageCanonicalizer.CanonicalizeHttpRequest(requestContext, uriComponents, isFileService);
			}
			return MessageCanonicalizer.CanonicalizeHttpRequestDefault(requestContext.RequestUrl, uriComponents, requestContext.HttpMethod, requestContext.RequestContentType, requestContext.RequestHeaders, isFileService, requestContext.RequestRawUrlString);
		}

		public static string GetStringToSignForStandardSignedKeyAuth(RequestContext requestContext, NephosUriComponents uriComponents, SupportedAuthScheme requestAuthScheme, bool isFileService = false)
		{
			return AuthenticationManagerHelper.GetStringToSignForStandardSharedKeyAuth(requestContext, uriComponents, SupportedAuthScheme.SharedKey, isFileService);
		}
	}
}