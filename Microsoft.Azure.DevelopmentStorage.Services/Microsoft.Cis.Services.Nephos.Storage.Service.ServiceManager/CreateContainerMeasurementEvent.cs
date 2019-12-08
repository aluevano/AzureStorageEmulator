using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class CreateContainerMeasurementEvent : BlobOperationMeasurementEvent<CreateContainerMeasurementEvent>
	{
		public CreateContainerMeasurementEvent() : base("CreateContainer")
		{
		}
	}
}