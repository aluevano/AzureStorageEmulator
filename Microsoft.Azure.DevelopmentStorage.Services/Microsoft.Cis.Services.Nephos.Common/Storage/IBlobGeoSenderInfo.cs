using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlobGeoSenderInfo
	{
		byte[] GeoSenderInfo
		{
			get;
			set;
		}
	}
}