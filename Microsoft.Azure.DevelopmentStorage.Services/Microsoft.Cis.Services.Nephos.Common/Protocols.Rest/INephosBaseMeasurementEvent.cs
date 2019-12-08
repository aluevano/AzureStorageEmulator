using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public interface INephosBaseMeasurementEvent : IMeasurementEvent, IDisposable
	{
		string AccountName
		{
			get;
			set;
		}

		int BatchItemsCount
		{
			get;
			set;
		}

		long ErrorResponseBytes
		{
			get;
			set;
		}

		bool IsAdmin
		{
			get;
			set;
		}

		int ItemsReturnedCount
		{
			get;
			set;
		}

		string OperationName
		{
			get;
		}

		string OperationPartitionKey
		{
			get;
		}

		string OperationStatus
		{
			get;
		}

		RequestOrigin Origin
		{
			get;
			set;
		}

		string GetObjectKey();

		string GetObjectType();
	}
}