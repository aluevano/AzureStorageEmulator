using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class BlobContentMeasurementEvent<T> : BlobOperationMeasurementEvent<T>, IBlobContentMeasurementEvent, IBlobMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	where T : BlobContentMeasurementEvent<T>
	{
		private string contentEncoding;

		private string contentLanguage;

		private string cacheControl;

		private string contentDisposition;

		[MeasurementEventParameter]
		public string CacheControl
		{
			get
			{
				return this.cacheControl;
			}
			set
			{
				this.cacheControl = value;
			}
		}

		[MeasurementEventParameter]
		public string ContentDisposition
		{
			get
			{
				return this.contentDisposition;
			}
			set
			{
				this.contentDisposition = value;
			}
		}

		[MeasurementEventParameter]
		public string ContentEncoding
		{
			get
			{
				return this.contentEncoding;
			}
			set
			{
				this.contentEncoding = value;
			}
		}

		[MeasurementEventParameter]
		public string ContentLanguage
		{
			get
			{
				return this.contentLanguage;
			}
			set
			{
				this.contentLanguage = value;
			}
		}

		protected BlobContentMeasurementEvent(string operationName) : base(operationName)
		{
		}
	}
}