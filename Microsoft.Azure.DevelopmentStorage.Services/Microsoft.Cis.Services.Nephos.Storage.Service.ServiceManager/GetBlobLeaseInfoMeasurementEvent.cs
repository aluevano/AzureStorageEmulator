using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetBlobLeaseInfoMeasurementEvent : BlobOperationMeasurementEvent<GetBlobLeaseInfoMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public GetBlobLeaseInfoMeasurementEvent() : base("GetBlobLeaseInfo")
		{
		}
	}
}