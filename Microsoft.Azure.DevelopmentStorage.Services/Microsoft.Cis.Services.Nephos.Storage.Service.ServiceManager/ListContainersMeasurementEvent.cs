using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ListContainersMeasurementEvent : BlobOperationMeasurementEvent<ListContainersMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public ListContainersMeasurementEvent() : base("ListContainers")
		{
		}
	}
}