using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class HttpRequestAccessor
	{
		public static string ConstructCopySourceResource(string accountName, string containerName, string blobName)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("/");
			stringBuilder.Append(accountName);
			if (!string.IsNullOrEmpty(containerName))
			{
				stringBuilder.Append("/");
				stringBuilder.Append(containerName);
			}
			if (!string.IsNullOrEmpty(blobName))
			{
				stringBuilder.Append("/");
				stringBuilder.Append(blobName);
			}
			return HttpUtilities.PathEncode(stringBuilder.ToString());
		}

		private static Uri ConstructHostStyleAccountUri(Uri hostSuffix, string accountName)
		{
			NephosAssertionException.Assert(hostSuffix.AbsolutePath == "/", "hostSuffix can't have path.");
			Uri uri = hostSuffix;
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] scheme = new object[] { uri.Scheme, Uri.SchemeDelimiter, accountName, uri.Host, uri.Port };
			return new Uri(string.Format(invariantCulture, "{0}{1}{2}.{3}:{4}/", scheme));
		}

		private static Uri ConstructHostStyleNephosUri(Uri hostSuffix, NephosUriComponents uriComponents)
		{
			if (uriComponents.AccountName == null)
			{
				return hostSuffix;
			}
			string accountName = uriComponents.AccountName;
			if (uriComponents.IsSecondaryAccountAccess)
			{
				accountName = uriComponents.GetSecondaryAccountName();
			}
			Uri uri = HttpRequestAccessor.ConstructHostStyleAccountUri(hostSuffix, accountName);
			StringBuilder stringBuilder = new StringBuilder(string.Empty);
			if (uriComponents.ContainerName != null)
			{
				stringBuilder.Append(uriComponents.ContainerName);
			}
			if (uriComponents.RemainingPart != null)
			{
				if (uriComponents.ContainerName != null)
				{
					stringBuilder.Append("/");
				}
				stringBuilder.Append(uriComponents.RemainingPart);
			}
			return new Uri(uri, stringBuilder.ToString());
		}

		public static Uri ConstructNephosUri(Uri endpoint, NephosUriComponents uriComponents, bool pathStyleUri)
		{
			NephosAssertionException.Assert(endpoint != null);
			NephosAssertionException.Assert(uriComponents != null);
			if (!pathStyleUri)
			{
				return HttpRequestAccessor.ConstructHostStyleNephosUri(endpoint, uriComponents);
			}
			return HttpRequestAccessor.ConstructPathStyleNephosUri(endpoint, uriComponents);
		}

		private static Uri ConstructPathStyleNephosUri(Uri endpoint, NephosUriComponents uriComponents)
		{
			StringBuilder stringBuilder = new StringBuilder(string.Empty);
			if (uriComponents.AccountName != null)
			{
				stringBuilder.Append(uriComponents.AccountName);
				if (uriComponents.IsSecondaryAccountAccess)
				{
					stringBuilder.Append("-secondary");
				}
				if (uriComponents.ContainerName != null)
				{
					stringBuilder.Append("/");
					stringBuilder.Append(uriComponents.ContainerName);
				}
				if (uriComponents.RemainingPart != null)
				{
					stringBuilder.Append("/");
					stringBuilder.Append(uriComponents.RemainingPart);
				}
			}
			return new Uri(endpoint, stringBuilder.ToString());
		}

		public static void GetContainerNameAndRemainingPart(Uri uri, UriStyle uriStyle, out string containerName, out string remainingPart)
		{
			containerName = null;
			remainingPart = null;
			int num = (uriStyle == UriStyle.PathStyle ? 1 : 0);
			string[] uriPathSubStrings = HttpRequestAccessor.GetUriPathSubStrings(uri, num + 2);
			if (uriPathSubStrings != null && (int)uriPathSubStrings.Length > num)
			{
				containerName = uriPathSubStrings[num];
				if ((int)uriPathSubStrings.Length > num + 1)
				{
					remainingPart = uriPathSubStrings[num + 1];
				}
			}
		}

		public static NephosUriComponents GetCopySourceResourceComponents(string resource)
		{
			string str = HttpUtility.UrlDecode(resource);
			if (!str.StartsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			string[] strArrays = new string[] { "/" };
			string[] strArrays1 = str.Split(strArrays, 4, StringSplitOptions.None);
			NephosAssertionException.Assert(((int)strArrays1.Length < 2 || (int)strArrays1.Length > 4 ? false : strArrays1[0] == string.Empty));
			if ((int)strArrays1.Length == 2)
			{
				return new NephosUriComponents(strArrays1[1]);
			}
			if ((int)strArrays1.Length == 3)
			{
				return new NephosUriComponents(strArrays1[1], strArrays1[2]);
			}
			return new NephosUriComponents(strArrays1[1], strArrays1[2], strArrays1[3]);
		}

		public static NephosUriComponents GetNephosHostStyleUriComponents(Uri uri, string accountName)
		{
			string str;
			string str1;
			NephosUriComponents nephosUriComponent = new NephosUriComponents(accountName);
			string[] uriPathSubStrings = HttpRequestAccessor.GetUriPathSubStrings(uri, 2);
			NephosUriComponents nephosUriComponent1 = nephosUriComponent;
			if ((int)uriPathSubStrings.Length > 0)
			{
				str = HttpRequestAccessorCommon.TrimEndSlash(uriPathSubStrings[0]);
			}
			else
			{
				str = null;
			}
			nephosUriComponent1.ContainerName = str;
			NephosUriComponents nephosUriComponent2 = nephosUriComponent;
			if ((int)uriPathSubStrings.Length > 1)
			{
				str1 = HttpRequestAccessorCommon.TrimEndSlash(uriPathSubStrings[1]);
			}
			else
			{
				str1 = null;
			}
			nephosUriComponent2.RemainingPart = str1;
			return nephosUriComponent;
		}

		public static NephosUriComponents GetNephosPathStyleUriComponents(Uri uri)
		{
			string str;
			string str1;
			string str2;
			NephosUriComponents nephosUriComponent = new NephosUriComponents()
			{
				IsUriPathStyle = true
			};
			string[] uriPathSubStrings = HttpRequestAccessor.GetUriPathSubStrings(uri, 3);
			NephosUriComponents nephosUriComponent1 = nephosUriComponent;
			if ((int)uriPathSubStrings.Length > 0)
			{
				str = HttpRequestAccessorCommon.TrimEndSlash(uriPathSubStrings[0]);
			}
			else
			{
				str = null;
			}
			nephosUriComponent1.AccountName = str;
			NephosUriComponents nephosUriComponent2 = nephosUriComponent;
			if ((int)uriPathSubStrings.Length > 1)
			{
				str1 = HttpRequestAccessorCommon.TrimEndSlash(uriPathSubStrings[1]);
			}
			else
			{
				str1 = null;
			}
			nephosUriComponent2.ContainerName = str1;
			NephosUriComponents nephosUriComponent3 = nephosUriComponent;
			if ((int)uriPathSubStrings.Length > 2)
			{
				str2 = HttpRequestAccessorCommon.TrimEndSlash(uriPathSubStrings[2]);
			}
			else
			{
				str2 = null;
			}
			nephosUriComponent3.RemainingPart = str2;
			return nephosUriComponent;
		}

		public static NephosUriComponents GetNephosUriComponents(Uri uri, string[] hostSuffixes, bool allowPathStyleUri, out bool uriIsPathStyle)
		{
			NephosUriComponents nephosUriComponent = null;
			if (HttpRequestAccessor.TryGetNephosHostStyleUriComponents(uri, hostSuffixes, out nephosUriComponent))
			{
				uriIsPathStyle = false;
				return nephosUriComponent;
			}
			if (!allowPathStyleUri)
			{
				uriIsPathStyle = false;
				return null;
			}
			uriIsPathStyle = true;
			return HttpRequestAccessor.GetNephosPathStyleUriComponents(uri);
		}

		public static NephosUriComponents GetNephosUriComponents(Uri uri, string[] hostSuffixes, bool allowPathStyleUri)
		{
			bool flag;
			return HttpRequestAccessor.GetNephosUriComponents(uri, hostSuffixes, allowPathStyleUri, out flag);
		}

		public static NephosUriComponents GetNephosUriComponents(IHttpListenerRequest request, string[] hostSuffixes, bool allowPathStyleUri)
		{
			return HttpRequestAccessor.GetNephosUriComponents(HttpUtilities.GetRequestUri(request), hostSuffixes, allowPathStyleUri);
		}

		public static NephosUriComponents GetNephosUriComponents(HttpWebRequest request, string[] hostSuffixes, bool allowPathStyleUri)
		{
			return HttpRequestAccessor.GetNephosUriComponents(request.Address, hostSuffixes, allowPathStyleUri);
		}

		private static string[] GetUriPathSubStrings(Uri uri, int count)
		{
			string str = HttpRequestAccessor.NormalizeLocalPathForDotNet4(uri.LocalPath);
			return str.Split(HttpRequestAccessorCommon.uriDelimiter, count, StringSplitOptions.RemoveEmptyEntries);
		}

		public static string NormalizeLocalPathForDotNet4(string localPath)
		{
			if (localPath == null)
			{
				throw new ArgumentException("localPath can't be null");
			}
			char[] chrArray = new char[localPath.Length];
			bool flag = false;
			int num = 0;
			int num1 = 0;
			for (int i = 0; i < localPath.Length; i++)
			{
				if (localPath[i] == "/"[0])
				{
					if (num1 > 0)
					{
						num1 = 0;
						if (flag)
						{
							goto Label0;
						}
					}
					int num2 = num;
					num = num2 + 1;
					chrArray[num2] = localPath[i];
					flag = true;
				}
				else if (localPath[i] != "."[0])
				{
					if (num1 > 0)
					{
						for (int j = 0; j < num1; j++)
						{
							int num3 = num;
							num = num3 + 1;
							chrArray[num3] = "."[0];
						}
					}
					int num4 = num;
					num = num4 + 1;
					chrArray[num4] = localPath[i];
					flag = false;
					num1 = 0;
				}
				else
				{
					num1++;
				}
			Label0:
			}
			return new string(chrArray, 0, num);
		}

		private static bool TryGetNephosHostStyleUriComponents(Uri uri, string[] hostSuffixes, out NephosUriComponents uriComponents)
		{
			uriComponents = null;
			string host = uri.Host;
			string[] strArrays = hostSuffixes;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (host.EndsWith(str, StringComparison.OrdinalIgnoreCase))
				{
					string str1 = host.Substring(0, host.Length - str.Length);
					if (str1.Length > 0 && str1[str1.Length - 1] == HttpRequestAccessorCommon.subDomainDelimiterChar && str1.Length > 1)
					{
						string str2 = str1.Substring(0, str1.Length - 1);
						uriComponents = HttpRequestAccessor.GetNephosHostStyleUriComponents(uri, str2);
						return true;
					}
				}
			}
			return false;
		}
	}
}