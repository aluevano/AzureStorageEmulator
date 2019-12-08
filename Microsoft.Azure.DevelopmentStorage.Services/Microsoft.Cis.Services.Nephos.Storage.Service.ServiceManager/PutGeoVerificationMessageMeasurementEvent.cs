using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class PutGeoVerificationMessageMeasurementEvent : BlobContentMeasurementEvent<PutGeoVerificationMessageMeasurementEvent>, IBlobOperationWithRequestContentMeasurementEvent, IBlobOperationWithResponseContentMeasurementEvent
	{
		[MeasurementEventParameter]
		public long MessageSize
		{
			get
			{
				return base.RequestBytesRead;
			}
		}

		[MeasurementEventParameter]
		public long ResponseDataSize
		{
			get
			{
				return base.ResponseBytesWritten;
			}
		}

		public PutGeoVerificationMessageMeasurementEvent() : base("PutGeoVerificationMessageMeasurementEvent")
		{
		}
	}
}