using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	internal interface IBlobOperationWithResponseContentMeasurementEvent
	{
		long ResponseBytesWritten
		{
			get;
			set;
		}
	}
}