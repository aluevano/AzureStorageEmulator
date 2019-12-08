using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class BlobOperationMeasurementEvent<T> : NephosBaseOperationMeasurementEvent<T>, IBlobMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	where T : BlobOperationMeasurementEvent<T>
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

		protected BlobOperationMeasurementEvent(string operationName) : base(operationName)
		{
		}

		public override string GetObjectKey()
		{
			if (string.IsNullOrEmpty(this.ContainerName))
			{
				return base.GenerateObjectKeyFrom(new string[] { base.AccountName });
			}
			if (string.IsNullOrEmpty(this.BlobName))
			{
				string[] accountName = new string[] { base.AccountName, this.ContainerName };
				return base.GenerateObjectKeyFrom(accountName);
			}
			string[] strArrays = new string[] { base.AccountName, this.ContainerName, this.BlobName };
			return base.GenerateObjectKeyFrom(strArrays);
		}

		public override string GetObjectType()
		{
			if (this.BlobType == Microsoft.Cis.Services.Nephos.Common.Storage.BlobType.None)
			{
				return string.Empty;
			}
			return BlobTypeStrings.GetString(this.BlobType);
		}
	}
}