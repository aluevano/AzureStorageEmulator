using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	internal interface IBlobOperationWithRequestContentMeasurementEvent
	{
		long RequestBytesRead
		{
			get;
			set;
		}
	}
}