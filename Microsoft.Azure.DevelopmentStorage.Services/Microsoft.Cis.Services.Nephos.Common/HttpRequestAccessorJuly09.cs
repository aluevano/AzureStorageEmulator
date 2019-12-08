using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class HttpRequestAccessorJuly09
	{
		public static string ConstructCopySourceResource(string accountName, string containerName, string blobName, DateTime? snapshot)
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
			string str = (snapshot.HasValue ? HttpUtilities.GetSnapshotQueryParameterStringForUrl(snapshot.Value) : string.Empty);
			return string.Concat(HttpUtilities.PathEncode(stringBuilder.ToString()), str);
		}

		public static string ConstructCopySourceResource(string accountName, string containerName, string blobName)
		{
			return HttpRequestAccessorJuly09.ConstructCopySourceResource(accountName, containerName, blobName, null);
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
			Uri uri = HttpRequestAccessorJuly09.ConstructHostStyleAccountUri(hostSuffix, accountName);
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
			return HttpRequestAccessorJuly09.ConstructUriFromUriAndString(uri, stringBuilder.ToString());
		}

		public static Uri ConstructNephosUri(Uri endpoint, NephosUriComponents uriComponents, bool pathStyleUri)
		{
			NephosAssertionException.Assert(endpoint != null);
			NephosAssertionException.Assert(uriComponents != null);
			if (!pathStyleUri)
			{
				return HttpRequestAccessorJuly09.ConstructHostStyleNephosUri(endpoint, uriComponents);
			}
			return HttpRequestAccessorJuly09.ConstructPathStyleNephosUri(endpoint, uriComponents);
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
			return HttpRequestAccessorJuly09.ConstructUriFromUriAndString(endpoint, stringBuilder.ToString());
		}

		private static Uri ConstructStorageDomainStyleNephosUri(Uri storageDomain, NephosUriComponents uriComponents)
		{
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
			return HttpRequestAccessorJuly09.ConstructUriFromUriAndString(storageDomain, stringBuilder.ToString());
		}

		public static Uri ConstructStorageDomainUri(Uri endpoint, NephosUriComponents uriComponents)
		{
			NephosAssertionException.Assert(endpoint != null);
			NephosAssertionException.Assert(uriComponents != null);
			return HttpRequestAccessorJuly09.ConstructStorageDomainStyleNephosUri(endpoint, uriComponents);
		}

		private static Uri ConstructUriFromUriAndString(Uri endpoint, string path)
		{
			return new Uri(endpoint, HttpUtilities.PathEncode(path));
		}

		private static string[] GetAccountUriPathSubStrings(Uri uri, int count)
		{
			return HttpRequestAccessorJuly09.GetUriPathSubStrings(uri, count, StringSplitOptions.RemoveEmptyEntries);
		}

		public static void GetContainerNameAndRemainingPart(Uri uri, bool uriIsPathStyle, out string containerName, out string remainingPart)
		{
			containerName = null;
			remainingPart = null;
			int num = (uriIsPathStyle ? 1 : 0);
			string[] uriPathSubStrings = HttpRequestAccessorJuly09.GetUriPathSubStrings(uri, num + 2);
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
			string[] uriPathSubStrings = HttpRequestAccessorJuly09.GetUriPathSubStrings(uri, 2);
			NephosUriComponents nephosUriComponent1 = nephosUriComponent;
			if ((int)uriPathSubStrings.Length <= 0 || string.IsNullOrEmpty(uriPathSubStrings[0]))
			{
				str = null;
			}
			else
			{
				str = uriPathSubStrings[0];
			}
			nephosUriComponent1.ContainerName = str;
			NephosUriComponents nephosUriComponent2 = nephosUriComponent;
			if ((int)uriPathSubStrings.Length <= 1 || string.IsNullOrEmpty(uriPathSubStrings[1]))
			{
				str1 = null;
			}
			else
			{
				str1 = uriPathSubStrings[1];
			}
			nephosUriComponent2.RemainingPart = str1;
			nephosUriComponent.IsRemainingPartPresentButEmpty = ((int)uriPathSubStrings.Length <= 1 || uriPathSubStrings[1] == null ? false : uriPathSubStrings[1].Length == 0);
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
			string[] uriPathSubStrings = HttpRequestAccessorJuly09.GetUriPathSubStrings(uri, 3);
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
			if ((int)uriPathSubStrings.Length <= 1 || string.IsNullOrEmpty(uriPathSubStrings[1]))
			{
				str1 = null;
			}
			else
			{
				str1 = uriPathSubStrings[1];
			}
			nephosUriComponent2.ContainerName = str1;
			NephosUriComponents nephosUriComponent3 = nephosUriComponent;
			if ((int)uriPathSubStrings.Length <= 2 || string.IsNullOrEmpty(uriPathSubStrings[2]))
			{
				str2 = null;
			}
			else
			{
				str2 = uriPathSubStrings[2];
			}
			nephosUriComponent3.RemainingPart = str2;
			nephosUriComponent.IsRemainingPartPresentButEmpty = ((int)uriPathSubStrings.Length <= 2 || uriPathSubStrings[2] == null ? false : uriPathSubStrings[2].Length == 0);
			return nephosUriComponent;
		}

		public static NephosUriComponents GetNephosUriComponents(Uri uri, string[] hostSuffixes, bool allowPathStyleUri, out bool uriIsPathStyle)
		{
			NephosUriComponents nephosUriComponent = null;
			if (HttpRequestAccessorJuly09.TryGetNephosHostStyleUriComponents(uri, hostSuffixes, out nephosUriComponent))
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
			return HttpRequestAccessorJuly09.GetNephosPathStyleUriComponents(uri);
		}

		public static NephosUriComponents GetNephosUriComponents(Uri uri, string[] hostSuffixes, bool allowPathStyleUri)
		{
			bool flag;
			return HttpRequestAccessorJuly09.GetNephosUriComponents(uri, hostSuffixes, allowPathStyleUri, out flag);
		}

		public static NephosUriComponents GetNephosUriComponents(IHttpListenerRequest request, string[] hostSuffixes, bool allowPathStyleUri)
		{
			return HttpRequestAccessorJuly09.GetNephosUriComponents(HttpUtilities.GetRequestUri(request), hostSuffixes, allowPathStyleUri);
		}

		public static NephosUriComponents GetNephosUriComponents(HttpWebRequest request, string[] hostSuffixes, bool allowPathStyleUri)
		{
			return HttpRequestAccessorJuly09.GetNephosUriComponents(request.Address, hostSuffixes, allowPathStyleUri);
		}

		private static string[] GetUriPathSubStrings(Uri uri, int count)
		{
			return HttpRequestAccessorJuly09.GetUriPathSubStrings(uri, count, StringSplitOptions.None);
		}

		private static string[] GetUriPathSubStrings(Uri uri, int count, StringSplitOptions splitOptions)
		{
			string str = HttpRequestAccessor.NormalizeLocalPathForDotNet4(uri.LocalPath);
			return str.Substring(1).Split(HttpRequestAccessorCommon.uriDelimiter, count, splitOptions);
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
						uriComponents = HttpRequestAccessorJuly09.GetNephosHostStyleUriComponents(uri, str2);
						return true;
					}
				}
			}
			return false;
		}
	}
}