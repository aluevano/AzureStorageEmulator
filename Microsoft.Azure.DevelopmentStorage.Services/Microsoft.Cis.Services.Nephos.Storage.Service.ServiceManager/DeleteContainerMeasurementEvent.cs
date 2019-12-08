using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class DeleteContainerMeasurementEvent : BlobOperationMeasurementEvent<DeleteContainerMeasurementEvent>
	{
		public DeleteContainerMeasurementEvent() : base("DeleteContainer")
		{
		}
	}
}