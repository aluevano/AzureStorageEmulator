using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	internal interface IQueueMeasurementEvent : INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		string QueueName
		{
			get;
			set;
		}
	}
}