using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class OperationContext : OperationContextWithAuthAndAccountContainer
	{
		public Exception BlobErrorToRethrowForDoubleLookup
		{
			get;
			set;
		}

		public string BlobOrFilePathName
		{
			get;
			set;
		}

		public bool IsRequestUsingRootContainer
		{
			get
			{
				return base.ContainerName == "$root";
			}
		}

		public bool IsResourceTypeContainer
		{
			get;
			set;
		}

		public bool IsResourceTypeDirectory
		{
			get;
			set;
		}

		public bool IsUnversionedRequest
		{
			get;
			set;
		}

		public bool ResourceIsAccount
		{
			get
			{
				if (base.AccountName == null || base.ContainerName != null)
				{
					return false;
				}
				return this.BlobOrFilePathName == null;
			}
		}

		public bool ResourceIsBlobOrFilePath
		{
			get
			{
				return this.BlobOrFilePathName != null;
			}
		}

		public bool ResourceIsContainer
		{
			get
			{
				if (base.ContainerName == null)
				{
					return false;
				}
				return this.BlobOrFilePathName == null;
			}
		}

		public string SubResource
		{
			get;
			set;
		}

		public OperationContext()
		{
			base.HttpRequestMeasurementEvent = new HttpBlobRequestProcessedMeasurementEvent();
			this.BlobErrorToRethrowForDoubleLookup = null;
		}

		public OperationContext(TimeSpan elapsedTime) : base(elapsedTime)
		{
			base.HttpRequestMeasurementEvent = new HttpBlobRequestProcessedMeasurementEvent();
			this.BlobErrorToRethrowForDoubleLookup = null;
		}
	}
}