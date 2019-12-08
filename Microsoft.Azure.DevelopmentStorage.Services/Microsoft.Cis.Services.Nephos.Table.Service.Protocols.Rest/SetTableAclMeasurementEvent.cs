using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class SetTableAclMeasurementEvent : TableOperationMeasurementEvent<SetTableAclMeasurementEvent>
	{
		public SetTableAclMeasurementEvent() : base("SetTableAcl")
		{
		}
	}
}