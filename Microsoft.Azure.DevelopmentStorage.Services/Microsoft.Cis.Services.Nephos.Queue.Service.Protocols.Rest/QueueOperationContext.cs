using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class QueueOperationContext : OperationContextWithAuthAndAccountContainer
	{
		public string Operation
		{
			get;
			set;
		}

		public bool ResourceIsAccount
		{
			get
			{
				if (this.ResourceIsQueue)
				{
					return false;
				}
				return !this.ResourceIsOperation;
			}
		}

		public bool ResourceIsOperation
		{
			get
			{
				return this.Operation != null;
			}
		}

		public bool ResourceIsQueue
		{
			get
			{
				if (base.ContainerName == null)
				{
					return false;
				}
				return !this.ResourceIsOperation;
			}
		}

		public string SubResource
		{
			get;
			set;
		}

		public QueueOperationContext()
		{
			base.HttpRequestMeasurementEvent = new HttpQueueRequestProcessedMeasurementEvent();
		}

		public QueueOperationContext(TimeSpan elapsedTime) : base(elapsedTime)
		{
			base.HttpRequestMeasurementEvent = new HttpQueueRequestProcessedMeasurementEvent();
		}
	}
}