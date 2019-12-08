using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class PutBlockListMeasurementEvent : BlobContentMeasurementEvent<PutBlockListMeasurementEvent>, IBlobOperationWithRequestContentMeasurementEvent
	{
		private long numBlocks;

		[MeasurementEventParameter]
		public long NumBlocks
		{
			get
			{
				return this.numBlocks;
			}
			set
			{
				this.numBlocks = value;
			}
		}

		public PutBlockListMeasurementEvent() : base("PutBlockList")
		{
		}

		public override string GetObjectType()
		{
			return "BlockBlob";
		}
	}
}