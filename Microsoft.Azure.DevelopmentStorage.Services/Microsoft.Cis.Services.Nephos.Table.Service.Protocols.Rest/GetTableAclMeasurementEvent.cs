using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class GetTableAclMeasurementEvent : TableOperationMeasurementEvent<GetTableAclMeasurementEvent>
	{
		public GetTableAclMeasurementEvent() : base("GetTableAcl")
		{
		}
	}
}