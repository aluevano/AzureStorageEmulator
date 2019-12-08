using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class BlobPreflightRequestMeasurementEvent : BlobOperationMeasurementEvent<BlobPreflightRequestMeasurementEvent>
	{
		public BlobPreflightRequestMeasurementEvent() : base("BlobPreflightRequest")
		{
		}
	}
}