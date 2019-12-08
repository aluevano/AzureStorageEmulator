using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class CopyBlobMeasurementEvent : NephosBaseOperationMeasurementEvent<CopyBlobMeasurementEvent>
	{
		private string sourceUrl;

		private string destinationAccountName;

		private string destinationContainerName;

		private string destinationBlobName;

		[MeasurementEventParameter]
		public string DestinationAccountName
		{
			get
			{
				return this.destinationAccountName;
			}
			set
			{
				this.destinationAccountName = value;
			}
		}

		[MeasurementEventParameter]
		public string DestinationBlobName
		{
			get
			{
				return this.destinationBlobName;
			}
			set
			{
				this.destinationBlobName = value;
			}
		}

		[MeasurementEventParameter]
		public string DestinationContainerName
		{
			get
			{
				return this.destinationContainerName;
			}
			set
			{
				this.destinationContainerName = value;
			}
		}

		[MeasurementEventParameter]
		public string SourceUrl
		{
			get
			{
				return this.sourceUrl;
			}
			set
			{
				this.sourceUrl = value;
			}
		}

		public CopyBlobMeasurementEvent() : base("CopyBlob")
		{
		}

		public override string GetObjectKey()
		{
			string[] destinationAccountName = new string[] { this.DestinationAccountName, this.DestinationContainerName, this.DestinationBlobName };
			return base.GenerateObjectKeyFrom(destinationAccountName);
		}
	}
}