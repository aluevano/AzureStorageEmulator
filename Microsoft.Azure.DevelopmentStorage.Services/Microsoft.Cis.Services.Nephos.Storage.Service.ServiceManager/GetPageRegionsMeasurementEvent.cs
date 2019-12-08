using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetPageRegionsMeasurementEvent : BlobOperationMeasurementEvent<GetPageRegionsMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public GetPageRegionsMeasurementEvent() : base("GetPageRegions")
		{
		}

		public override string GetObjectType()
		{
			return "PageBlob";
		}
	}
}