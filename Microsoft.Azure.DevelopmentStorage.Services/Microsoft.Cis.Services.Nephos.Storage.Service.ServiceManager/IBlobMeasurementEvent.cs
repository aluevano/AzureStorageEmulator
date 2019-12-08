using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	internal interface IBlobMeasurementEvent : INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		string BlobName
		{
			get;
			set;
		}

		Microsoft.Cis.Services.Nephos.Common.Storage.BlobType BlobType
		{
			get;
			set;
		}

		string ContainerName
		{
			get;
			set;
		}
	}
}