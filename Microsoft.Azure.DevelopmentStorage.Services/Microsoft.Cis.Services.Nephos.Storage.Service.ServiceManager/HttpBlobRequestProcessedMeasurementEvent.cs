using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class HttpBlobRequestProcessedMeasurementEvent : NephosBaseMeasurementEvent<HttpBlobRequestProcessedMeasurementEvent>, IBlobMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		private string containerName;

		private string blobName;

		[MeasurementEventParameter]
		public string BlobName
		{
			get
			{
				return this.blobName;
			}
			set
			{
				this.blobName = value;
			}
		}

		public virtual Microsoft.Cis.Services.Nephos.Common.Storage.BlobType BlobType
		{
			get;
			set;
		}

		[MeasurementEventParameter]
		public string ContainerName
		{
			get
			{
				return this.containerName;
			}
			set
			{
				this.containerName = value;
			}
		}

		public override string OperationPartitionKey
		{
			get
			{
				if (string.IsNullOrEmpty(this.BlobName))
				{
					return null;
				}
				return this.BlobName;
			}
		}

		public HttpBlobRequestProcessedMeasurementEvent() : base("HttpBlobRequestProcessed")
		{
		}
	}
}