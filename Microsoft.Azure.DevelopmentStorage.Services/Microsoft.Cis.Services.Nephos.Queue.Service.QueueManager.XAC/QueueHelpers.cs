using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	internal static class QueueHelpers
	{
		public static NameValueCollection DeserializeMetadata(byte[] data)
		{
			if (data == null || (int)data.Length <= 0)
			{
				return new NameValueCollection();
			}
			return QueueHelpers.StringToMetadata(Encoding.Unicode.GetString(data));
		}

		public static bool IsMetadataSame(NameValueCollection m1, NameValueCollection m2)
		{
			return QueueHelpers.MetadataToString(m1) == QueueHelpers.MetadataToString(m2);
		}

		private static string MetadataToString(NameValueCollection metadata)
		{
			StringBuilder stringBuilder = new StringBuilder(metadata.Count * 30);
			ArrayList arrayLists = new ArrayList(metadata.Keys);
			arrayLists.Sort();
			foreach (string arrayList in arrayLists)
			{
				stringBuilder.Append(string.Concat(HttpUtility.UrlEncode(arrayList), "=", HttpUtility.UrlEncode(metadata[arrayList]), "/"));
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder = stringBuilder.Remove(stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		public static byte[] SerializeMetadata(NameValueCollection metadata)
		{
			return Encoding.Unicode.GetBytes(QueueHelpers.MetadataToString(metadata));
		}

		private static NameValueCollection StringToMetadata(string metadataInAString)
		{
			string[] strArrays = metadataInAString.Split(new char[] { '/' });
			NameValueCollection nameValueCollection = new NameValueCollection((int)strArrays.Length);
			string[] strArrays1 = strArrays;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				string str = strArrays1[i];
				string[] strArrays2 = str.Split(new char[] { '=' });
				nameValueCollection[HttpUtility.UrlDecode(strArrays2[0])] = HttpUtility.UrlDecode(strArrays2[1]);
			}
			return nameValueCollection;
		}
	}
}