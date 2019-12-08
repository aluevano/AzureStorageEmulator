using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class ServiceSettingsSerializer
	{
		public ServiceSettingsSerializer()
		{
		}

		public static AnalyticsSettings DeSerialize(byte[] settings)
		{
			if (settings == null)
			{
				return null;
			}
			return (new BinaryFormatter()).Deserialize(new MemoryStream(settings)) as AnalyticsSettings;
		}

		public static byte[] Serialize(AnalyticsSettings settings)
		{
			if (settings == null)
			{
				return null;
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, settings);
			return memoryStream.ToArray();
		}
	}
}