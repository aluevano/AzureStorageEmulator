using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ClearPageMeasurementEvent : BlobOperationMeasurementEvent<ClearPageMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public ClearPageMeasurementEvent() : base("ClearPage")
		{
		}

		public override string GetObjectType()
		{
			return "PageBlob";
		}
	}
}