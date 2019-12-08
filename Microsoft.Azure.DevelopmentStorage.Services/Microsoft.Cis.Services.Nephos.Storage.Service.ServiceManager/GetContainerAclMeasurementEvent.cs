using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetContainerAclMeasurementEvent : BlobOperationMeasurementEvent<GetContainerAclMeasurementEvent>
	{
		public GetContainerAclMeasurementEvent() : base("GetContainerACL")
		{
		}
	}
}