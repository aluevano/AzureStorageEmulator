using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class BlobTypeStrings
	{
		public const string BlockBlob = "BlockBlob";

		public const string PageBlob = "PageBlob";

		public const string AppendBlob = "AppendBlob";

		public static BlobType GetBlobType(string blobTypeString)
		{
			BlobType blobType = BlobType.None;
			if (!string.IsNullOrEmpty(blobTypeString))
			{
				if (blobTypeString.Equals("BlockBlob", StringComparison.OrdinalIgnoreCase))
				{
					blobType = BlobType.ListBlob;
				}
				else if (blobTypeString.Equals("AppendBlob", StringComparison.OrdinalIgnoreCase))
				{
					blobType = BlobType.AppendBlob;
				}
				else if (blobTypeString.Equals("PageBlob", StringComparison.OrdinalIgnoreCase))
				{
					blobType = BlobType.IndexBlob;
				}
			}
			return blobType;
		}

		public static string GetString(BlobType blobType)
		{
			if (blobType == BlobType.ListBlob)
			{
				return "BlockBlob";
			}
			if (blobType == BlobType.AppendBlob)
			{
				return "AppendBlob";
			}
			if (blobType == BlobType.IndexBlob)
			{
				return "PageBlob";
			}
			NephosAssertionException.Fail("Invalid Parameter: blobType type:{0}", new object[] { blobType });
			return null;
		}
	}
}