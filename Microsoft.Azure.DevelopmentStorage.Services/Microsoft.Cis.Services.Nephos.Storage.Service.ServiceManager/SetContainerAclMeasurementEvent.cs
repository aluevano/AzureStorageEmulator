using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class SetContainerAclMeasurementEvent : BlobOperationMeasurementEvent<SetContainerAclMeasurementEvent>
	{
		public SetContainerAclMeasurementEvent() : base("SetContainerACL")
		{
		}
	}
}