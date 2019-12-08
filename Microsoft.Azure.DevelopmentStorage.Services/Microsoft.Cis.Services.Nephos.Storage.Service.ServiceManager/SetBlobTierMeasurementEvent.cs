using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class SetBlobTierMeasurementEvent : BlobContentMeasurementEvent<SetBlobTierMeasurementEvent>
	{
		public SetBlobTierMeasurementEvent() : base("SetBlobTier")
		{
		}
	}
}