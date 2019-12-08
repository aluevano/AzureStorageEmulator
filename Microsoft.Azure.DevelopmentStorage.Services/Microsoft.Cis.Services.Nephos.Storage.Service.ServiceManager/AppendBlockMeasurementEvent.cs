using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class AppendBlockMeasurementEvent : BlockMeasurementEvent<AppendBlockMeasurementEvent>, IBlobOperationWithRequestContentMeasurementEvent
	{
		public AppendBlockMeasurementEvent() : base("AppendBlock", Microsoft.Cis.Services.Nephos.Common.Storage.BlobType.AppendBlob)
		{
		}
	}
}