using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetBlockListMeasurementEvent : BlobOperationMeasurementEvent<GetBlockListMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public GetBlockListMeasurementEvent() : base("GetBlockList")
		{
		}

		public override string GetObjectType()
		{
			return "BlockBlob";
		}
	}
}