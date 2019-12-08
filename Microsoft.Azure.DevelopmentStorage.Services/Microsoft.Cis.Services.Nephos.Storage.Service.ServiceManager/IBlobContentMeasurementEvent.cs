using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	internal interface IBlobContentMeasurementEvent : IBlobMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		string CacheControl
		{
			get;
			set;
		}

		string ContentDisposition
		{
			get;
			set;
		}

		string ContentEncoding
		{
			get;
			set;
		}

		string ContentLanguage
		{
			get;
			set;
		}
	}
}