using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class SummaryResult
	{
		private string nextMarker;

		private long count;

		private long totalMetadataSize;

		public long Count
		{
			get
			{
				return this.count;
			}
			set
			{
				this.count = value;
			}
		}

		public string NextMarker
		{
			get
			{
				return this.nextMarker;
			}
			set
			{
				this.nextMarker = value;
			}
		}

		public long TotalMetadataSize
		{
			get
			{
				return this.totalMetadataSize;
			}
			set
			{
				this.totalMetadataSize = value;
			}
		}

		public SummaryResult()
		{
		}

		public SummaryResult(long count, long totalMetadataSize, string nextMarker)
		{
			this.count = count;
			this.totalMetadataSize = totalMetadataSize;
			this.nextMarker = nextMarker;
		}

		public static T DecodeMarker<T>(string encodedMarker)
		{
			T t;
			if (string.IsNullOrEmpty(encodedMarker))
			{
				return default(T);
			}
			try
			{
				using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(encodedMarker)))
				{
					t = (T)(new BinaryFormatter()).Deserialize(memoryStream);
				}
			}
			catch (Exception exception)
			{
				throw new InvalidMarkerException("Invalid marker", exception);
			}
			return t;
		}

		public static string EncodeMarker<T>(T markerType)
		{
			string base64String;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				(new BinaryFormatter()).Serialize(memoryStream, markerType);
				base64String = Convert.ToBase64String(memoryStream.GetBuffer());
			}
			return base64String;
		}
	}
}