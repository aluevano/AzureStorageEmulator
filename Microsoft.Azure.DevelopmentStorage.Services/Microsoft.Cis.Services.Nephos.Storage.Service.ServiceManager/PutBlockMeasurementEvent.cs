using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class PutBlockMeasurementEvent : BlockMeasurementEvent<PutBlockMeasurementEvent>, IBlobOperationWithRequestContentMeasurementEvent
	{
		public PutBlockMeasurementEvent() : base("PutBlock", Microsoft.Cis.Services.Nephos.Common.Storage.BlobType.ListBlob)
		{
		}
	}
}