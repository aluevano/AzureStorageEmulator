using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class MessageHashFunctions
	{
		public static string ComputeMac(string canonicalizedString, byte[] key)
		{
			string str;
			HMAC hMAC = HMACCryptoCache.Instance.Acquire(key);
			try
			{
				str = MessageHashFunctions.ComputeMacWithSpecificAlgorithm(hMAC, canonicalizedString);
			}
			finally
			{
				HMACCryptoCache.Instance.Release(hMAC);
			}
			return str;
		}

		public static string ComputeMac(RequestContext requestContext, NephosUriComponents uriComponents, byte[] key)
		{
			string str;
			HMAC hMAC = HMACCryptoCache.Instance.Acquire(key);
			try
			{
				str = MessageHashFunctions.ComputeMacWithSpecificAlgorithm(hMAC, requestContext, uriComponents);
			}
			finally
			{
				HMACCryptoCache.Instance.Release(hMAC);
			}
			return str;
		}

		public static string ComputeMac(HttpWebRequest request, NephosUriComponents uriComponents, byte[] key)
		{
			string str;
			string str1 = MessageCanonicalizer.CanonicalizeHttpRequest(request, uriComponents);
			HMAC hMAC = HMACCryptoCache.Instance.Acquire(key);
			try
			{
				str = MessageHashFunctions.ComputeMacWithSpecificAlgorithm(hMAC, str1);
			}
			finally
			{
				HMACCryptoCache.Instance.Release(hMAC);
			}
			return str;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="0#canonicalized")]
		public static string ComputeMacWithSpecificAlgorithm(HMAC algorithm, string canonicalizedString)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(canonicalizedString);
			return Convert.ToBase64String(algorithm.ComputeHash(bytes));
		}

		public static string ComputeMacWithSpecificAlgorithm(HMAC algorithm, RequestContext requestContext, NephosUriComponents uriComponents)
		{
			string str = MessageCanonicalizer.CanonicalizeHttpRequest(requestContext, uriComponents, false);
			return MessageHashFunctions.ComputeMacWithSpecificAlgorithm(algorithm, str);
		}
	}
}