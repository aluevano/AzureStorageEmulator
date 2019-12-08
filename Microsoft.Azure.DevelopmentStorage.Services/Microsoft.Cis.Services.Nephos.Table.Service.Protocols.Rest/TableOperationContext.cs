using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class TableOperationContext : OperationContextWithAuthAndAccountContainer
	{
		private bool resourceIsAccount;

		private string subResource;

		public bool ResourceIsAccount
		{
			get
			{
				return this.resourceIsAccount;
			}
			set
			{
				this.resourceIsAccount = value;
			}
		}

		public bool ResourceIsTable
		{
			get;
			internal set;
		}

		public string SubResource
		{
			get
			{
				return this.subResource;
			}
			set
			{
				this.subResource = value;
			}
		}

		public TableOperationContext()
		{
			base.HttpRequestMeasurementEvent = new HttpTableRequestProcessedMeasurementEvent();
		}

		public TableOperationContext(TimeSpan elapsedTime) : base(elapsedTime)
		{
			base.HttpRequestMeasurementEvent = new HttpTableRequestProcessedMeasurementEvent();
		}
	}
}