using System;

namespace MeasurementEvents
{
	public interface IMeasurementEvent : IDisposable
	{
		bool Completed
		{
			get;
		}

		TimeSpan Duration
		{
			get;
		}

		TimeSpan ProcessingTime
		{
			get;
		}

		void Complete(MeasurementEventStatus status);

		void Complete(MeasurementEventStatus status, TimeSpan duration);

		void Complete(MeasurementEventStatus status, TimeSpan duration, TimeSpan processingTime);
	}
}